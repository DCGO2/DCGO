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

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Become unaffected by opponents effects, then can attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] if [Jesmon]/[X Antibody] is in this Digimon's digivoulution cards, for the turn, 1 of your Digimon isn't affected by your opponent's effects. Then, 1 of your Digimon may attack.";
                }

                bool SourceCondition(CardSource source)
                {
                    return source.EqualsCardName("Jesmon") || source.EqualsCardName("X Antibody");
                }

                bool CanSelectCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent this_card = card.PermanentOfThisCard();

                    if (this_card != null)
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count(SourceCondition) >= 1)
                        {
                            #region select immunity recipient
                            {
                                Permanent immunityPermanent = null;

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

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will be immune from your opponent's effects.", "Opponent is selecting 1 Digimon that will be immune from your effects.");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    immunityPermanent = permanent;
                                    yield return null;
                                }

                                if(immunityPermanent != null)
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

                                    bool SkillCondition(ICardEffect cardEffect)
                                    {
                                        if (CardEffectCommons.IsOpponentEffect(cardEffect, card))
                                        {
                                            return true;
                                        }

                                        return false;
                                    }

                                    ICardEffect GetCardEffect(EffectTiming _timing)
                                    {
                                        if (_timing == EffectTiming.None)
                                        {
                                            return canNotAffectedClass;
                                        }
                                        return null;
                                    }

                                }
                            }
                            #endregion
                        }

                        #region Select attacker
                        {
                            bool CanSelectAttackPermanentCondition(Permanent permanent)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                                {
                                    if (permanent.CanAttack(activateClass))
                                    {
                                        return true;
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
                }
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.None)
            {
                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("[Your Turn] All of your Digimon with [Sistermon] or the [Royal Knight] trait gain <Piercing> and can attack your opponent's unsuspended Digimon.", CanUseCondition, card);
                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                cardEffects.Add(addSkillClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
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
                        if (permanent.TopCard.ContainsCardName("Sistermon") || permanent.TopCard.HasRoyalKnightTraits)
                            return true;
                    }

                    return false;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return PermanentCondition(cardSource.PermanentOfThisCard());
                }

                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                {
                    if (_timing == EffectTiming.OnDetermineDoSecurityCheck)
                    {
                        bool Condition()
                        {
                            return CardSourceCondition(cardSource);
                        }

                        cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: Condition));
                    }

                    return cardEffects;
                }

                CanAttackTargetDefendingPermanentClass canAttackTargetDefendingPermanentClass = new CanAttackTargetDefendingPermanentClass();
                canAttackTargetDefendingPermanentClass.SetUpICardEffect($"Can attack to unsuspended Digimon", CanUseCondition1, card);
                canAttackTargetDefendingPermanentClass.SetUpCanAttackTargetDefendingPermanentClass(attackerCondition: PermanentCondition, defenderCondition: DefenderCondition, cardEffectCondition: CardEffectCondition);
                cardEffects.Add(canAttackTargetDefendingPermanentClass);

                bool CanUseCondition1(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
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
            #endregion

            #region Inherit
            if(timing == EffectTiming.None)
            {
                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("[Your Turn] While this Digimon is [Jesmon GX], all of your Digimon gain <Piercing> and can also attack your opponent's unsuspended Digimon.", CanUseCondition, card);
                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                addSkillClass.SetIsInheritedEffect(true);
                cardEffects.Add(addSkillClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return card.PermanentOfThisCard().TopCard.EqualsCardName("Jesmon GX");
                        }
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return PermanentCondition(cardSource.PermanentOfThisCard());
                }

                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                {
                    if (_timing == EffectTiming.OnDetermineDoSecurityCheck)
                    {
                        bool Condition()
                        {
                            return CardSourceCondition(cardSource);
                        }

                        cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: Condition));
                    }

                    return cardEffects;
                }

                CanAttackTargetDefendingPermanentClass canAttackTargetDefendingPermanentClass = new CanAttackTargetDefendingPermanentClass();
                canAttackTargetDefendingPermanentClass.SetUpICardEffect($"Can attack to unsuspended Digimon", CanUseCondition1, card);
                canAttackTargetDefendingPermanentClass.SetUpCanAttackTargetDefendingPermanentClass(attackerCondition: PermanentCondition, defenderCondition: DefenderCondition, cardEffectCondition: CardEffectCondition);
                canAttackTargetDefendingPermanentClass.SetIsInheritedEffect(true);
                cardEffects.Add(canAttackTargetDefendingPermanentClass);

                bool CanUseCondition1(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
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
            #endregion

            return cardEffects;
        }
    }
}