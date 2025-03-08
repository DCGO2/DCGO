using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects
{
    public class BT20_019 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Become unaffected by opponents effects, then can attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] if [Jesmon]/[X Antibody] is in this Digimon's digivoulution cards, for the turn, 1 of your Digimon isn't affected by you opponent's effects. THne, 1 of your Digimon may attack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                    {
                        return CardEffectCommons.IsExistOnBattleArea(card);
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                    {
                        if (CardEffectCommons.IsExistOnBattleArea(card))
                        {
                            return true;
                        }
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

                        List<Permanent> selectedPermanents = new List<Permanent>();
                        if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.EqualsCardName("Jesmon") || cardSource.EqualsCardName("X Antibody")) >= 1)
                        {
                            #region select immunity recipient
                            {

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
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that be immune from your opponent's effects.", "Opponent is selecting on Digimon that will be immune from your effects.");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                                {
                                    selectedPermanents = permanents;
                                    yield return null;
                                }

                                foreach (Permanent permanent in selectedPermanents)
                                {
                                    Permanent selectedPermanent = permanent;

                                    if (selectedPermanent != null)
                                    {
                                        if (selectedPermanent.CanAttack(activateClass))
                                        {
                                            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                            selectAttackEffect.SetUp(
                                                attacker: selectedPermanent,
                                                canAttackPlayerCondition: () => false,
                                                defenderCondition: (permanent) => true,
                                                cardEffect: activateClass);

                                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                                        }
                                    }
                                }
                            }
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
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that be immune from your opponent's effects.", "Opponent is selecting on Digimon that will be immune from your effects.");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                                {
                                    selectedPermanents = permanents;
                                    yield return null;
                                }

                                foreach (Permanent permanent in selectedPermanents)
                                {
                                    Permanent selectedPermanent = permanent;

                                    if (selectedPermanent != null)
                                    {
                                        if (selectedPermanent.CanAttack(activateClass))
                                        {
                                            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                            selectAttackEffect.SetUp(
                                                attacker: selectedPermanent,
                                                canAttackPlayerCondition: () => false,
                                                defenderCondition: (permanent) => true,
                                                cardEffect: activateClass);

                                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                                        }
                                    }
                                }

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
            }

            #region Alliance

            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return true;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return null;
                }
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnStartTurn)
            {

                CanAttackTargetDefendingPermanentClass canAttackTargetDefendingPermanentClass = new CanAttackTargetDefendingPermanentClass();
                canAttackTargetDefendingPermanentClass.SetUpICardEffect("Gain Piercing and can attack unsuspended Digimon", CanUseCondition, card);
                canAttackTargetDefendingPermanentClass.SetUpCanAttackTargetDefendingPermanentClass(attackerCondition: PermanentCondition, defenderCondition: DefenderCondition, cardEffectCondition: CardEffectCondition);
                card.Owner.UntilOpponentTurnEndEffects.Add((_timing) => canAttackTargetDefendingPermanentClass);

                string EffectDiscription ()
                {
                    return "All of you DIgimon with [Sistermon] in their names or the [Royal Knight] trait gain <Piercing> and can also attack unsuspended Digimon.";
                }

                bool CanUseCondition (Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn (card))
                        {  
                            return true;
                        }
                    }

                    return false;
                }

                bool PermanentCondition (Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        return (permanent.TopCard.ContainsCardName("Sistermon") || permanent.TopCard.HasRoyalKnightTraits);
                    }
                    return false;
                }
            
                bool DefenderCondition (Permanent permanent)
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
            
                bool CardEffectCondition (ICardEffect cardEffect)
                {
                    return true;
                }

            }
            #endregion


            return cardEffects;
        }
    }
}