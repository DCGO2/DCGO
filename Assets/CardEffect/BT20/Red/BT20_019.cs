using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace DCGO.CardEffects.BT20
{
    public class BT20_019 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                #region When Digivolving
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Become unaffected by opponents effects, then can attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] if [Jesmon]/[X Antibody] is in this Digimon's digivoulution cards, for the turn, 1 of your Digimon isn't affected by your opponent's effects. THne, 1 of your Digimon may attack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                    {
                        return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return true;
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent this_card = card.PermanentOfThisCard();

                    if (this_card != null)
                    {
                        bool CanSelectCondition(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                            {
                                return true;
                            }
                            return false;
                        }

                        if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.EqualsCardName("Jesmon") || cardSource.EqualsCardName("X Antibody")) >= 1)
                        {
                            #region select immunity recipient
                            {
                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                                    canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's effects.", CanUseConditionImmunity, card);
                                    canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                                    card.Owner.UntilEachTurnEndEffects.Add(GetCardEffect);

                                    bool CanUseConditionImmunity(Hashtable hashtable)
                                    {
                                        return true;
                                    }

                                    bool CardCondition(CardSource cardSource)
                                    {
                                        if (CardEffectCommons.IsExistOnBattleAreaDigimon(cardSource))
                                        {
                                            if (card.Owner == cardSource.Owner)
                                            {
                                                return true;
                                            }
                                        }

                                        return false;
                                    }

                                    bool SkillCondition (ICardEffect cardEffect)
                                    {
                                        if (CardEffectCommons.IsOpponentEffect(cardEffect, card))
                                        {
                                            return true;
                                        }

                                        return false;
                                    }

                                    ICardEffect GetCardEffect (EffectTiming _timing)
                                    {
                                        if (_timing == EffectTiming.None)
                                        {
                                            return canNotAffectedClass;
                                        }
                                        return null;
                                    }

                                    yield return null;
                                }

                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectCondition));

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that be immune from your opponent's effects.", "Opponent is selecting on Digimon that will be immune from your effects.");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                                
                            }
                            #endregion
                        }

                        {
                            #region Select attacker
                            {
                                bool CanSelectAttackPermanentCondition(Permanent permanent)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                                    {
                                        if (permanent.CanAttack(activateClass))
                                        {
                                            if (card.Owner.Enemy.GetBattleAreaDigimons().Count((enemyDigimon) => permanent.CanAttackTargetDigimon(enemyDigimon, activateClass)) >= 1)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                    return false;
                                }

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                    selectAttackEffect.SetUp(
                                        attacker: permanent,
                                        canAttackPlayerCondition: () => true,
                                        defenderCondition: (permanent) => true,
                                        cardEffect: activateClass);

                                    yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                                }

                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectAttackPermanentCondition));

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectAttackPermanentCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that be immune from your opponent's effects.", "Opponent is selecting on Digimon that will be immune from your effects.");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                            }
                            #endregion
                        }

                        CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                        canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's effects", CanUseCondition1, card);
                        canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                        this_card.UntilOpponentTurnEndEffects.Add((_timing) => canNotAffectedClass);

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            return CardEffectCommons.IsPermanentExistsOnBattleArea(this_card);
                        }

                        bool CardCondition(CardSource cardSource)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnBattleArea(this_card))
                            {
                                if (cardSource == this_card.TopCard)
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        bool SkillCondition(ICardEffect cardEffect)
                        {
                            if (cardEffect != null)
                            {
                                if (cardEffect.EffectSourceCard != null)
                                {
                                    if (cardEffect.EffectSourceCard.Owner == card.Owner.Enemy)
                                    {
                                        return true;
                                    }
                                }
                            }
                            return false;
                        }
                    }
                }
                #endregion
            }


            if (timing == EffectTiming.None)
            {
                #region Your Turn
                {
                    ActivateClass activateClass = new ActivateClass();
                    activateClass.SetUpICardEffect("Select digimon to gain piercing and can attack unsuspended digimon", CanUseCondition, card);
                    activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                    cardEffects.Add(activateClass);

                    String EffectDiscription ()
                    {
                        return "[Your Turn] All of your Digimon with [Sistermon] in their names or the [Royal Knight] trait gain <Piercing> and can also attack your opponent's unsuspended Digimon."
                    }

                    bool CanUseCondition(Hashtable hashtable)
                    {
                        if (CardEffectCommons.IsExistOnBattleArea(card))
                        {
                            if (CardEffectCommons.IsOpponentTurn(card))
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    bool PermanentCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                        {
                            if (permanent.DigivolutionCards.Count((card) => card.ContainsCardName("Sistermon") || card.HasRoyalKnightTraits) >= 1)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    IEnumerator ActivateCoroutine (Hashtable hashtable)
                    {

                        List<Permanent> permanents = card.Owner.GetBattleAreaDigimons();

                        if (permanents.Count >= 1)
                        {
                            foreach (Permanent permanent in permanents)
                            {
                                yield return CardEffectCommons.GainPierce(targetPermanent: permanent, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass, true);
                            }
                        }

                        CanAttackTargetDefendingPermanentClass canAttackTargetDefendingPermanentClass = new CanAttackTargetDefendingPermanentClass();
                        canAttackTargetDefendingPermanentClass.SetUpICardEffect($"Can attack to unsuspended Digimon", CanUseCondition1, card);
                        canAttackTargetDefendingPermanentClass.SetUpCanAttackTargetDefendingPermanentClass(attackerCondition: PermanentCondition, defenderCondition: DefenderCondition, cardEffectCondition: CardEffectCondition);
                        card.Owner.UntilOpponentTurnEndEffects.Add((_timing) => canAttackTargetDefendingPermanentClass);

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            return true;
                        }

                        bool DefenderCondition(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                            {
                                if (!permanent.IsSuspended)
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        bool CardEffectCondition(ICardEffect cardEffect)
                        {
                            return true;
                        }
                    }

                    
                }
                #endregion

                #region Inherit
                {
                    ActivateClass activateClass = new ActivateClass();
                    activateClass.SetUpICardEffect("Select digimon to gain piercing and can attack unsuspended digimon", CanUseCondition, card);
                    activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                    cardEffects.Add(activateClass);

                    String EffectDiscription()
                    {
                        return "[Your Turn] While this Digimon is [Jesmon GX], all of your Digimon gain <Piercing> and can also attack your opponent's unsuspended Digimon.";
                    }

                    bool CanUseCondition(Hashtable hashtable)
                    {
                        if (CardEffectCommons.IsExistOnBattleArea(card))
                        {
                            if (CardEffectCommons.IsOpponentTurn(card))
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    bool PermanentCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                        {
                            if (permanent.TopCard.CardNames.Contains("Jesmon GX"))
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    IEnumerator ActivateCoroutine(Hashtable hashtable)
                    {

                        List<Permanent> permanents = card.Owner.GetBattleAreaDigimons();

                        if (permanents.Count >= 1)
                        {
                            foreach (Permanent permanent in permanents)
                            {
                                yield return CardEffectCommons.GainPierce(targetPermanent: permanent, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass, true);
                            }
                        }

                        CanAttackTargetDefendingPermanentClass canAttackTargetDefendingPermanentClass = new CanAttackTargetDefendingPermanentClass();
                        canAttackTargetDefendingPermanentClass.SetUpICardEffect($"Can attack to unsuspended Digimon", CanUseCondition1, card);
                        canAttackTargetDefendingPermanentClass.SetUpCanAttackTargetDefendingPermanentClass(attackerCondition: PermanentCondition, defenderCondition: DefenderCondition, cardEffectCondition: CardEffectCondition);
                        card.Owner.UntilOpponentTurnEndEffects.Add((_timing) => canAttackTargetDefendingPermanentClass);

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            return true;
                        }

                        bool DefenderCondition(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                            {
                                if (!permanent.IsSuspended)
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        bool CardEffectCondition(ICardEffect cardEffect)
                        {
                            return true;
                        }
                    }
                }
                #endregion
                
            }

            return cardEffects;
        }
    }
}