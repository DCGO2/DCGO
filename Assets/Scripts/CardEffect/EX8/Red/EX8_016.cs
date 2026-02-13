using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX8
{
    public class EX8_016 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.ContainsTraits("Dinosaur") || targetPermanent.TopCard.ContainsCardName("Tyrannomon"))
                    {
                        if (targetPermanent.TopCard.HasLevel)
                        {
                            if (targetPermanent.Level == 5)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Security Attack +1
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Fortitude
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.FortitudeSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region On Play/When Digivolving Shared

            bool CanSelectPermanentForSuspendCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnBattleArea(permanent) && permanent.IsDigimon;
            }

            bool CanSelectLowestDPSuspendedCondition(Permanent permanent)
            {
                return CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy, (permanent) => permanent.IsSuspended);
            }

            bool CanActivateOnPlayWhenDigivolvingCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }
            
            #endregion
            
            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend and delete an opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateOnPlayWhenDigivolvingCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] You may suspend 1 Digimon. Then, delete 1 of your opponent's suspended Digimon with the lowest DP.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card) &&
                           CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentForSuspendCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentForSuspendCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectLowestDPSuspendedCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectLowestDPSuspendedCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectLowestDPSuspendedCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend and delete an opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateOnPlayWhenDigivolvingCondition, ActivateCoroutine, -1, false,
                    EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return
                        "[When Digivolving] You may suspend 1 Digimon. Then, delete 1 of your opponent's suspended Digimon with the lowest DP.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card) &&
                           CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentForSuspendCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentForSuspendCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    }
                    
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectLowestDPSuspendedCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectLowestDPSuspendedCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectLowestDPSuspendedCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            #region Opponent's Turn
            if (timing == EffectTiming.None)
            {
                bool AttackerCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        return true;                    
                    }

                    return false;
                }

                bool DefenderCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.IsSuspended)
                        {
                            return false;
                        }
                    }

                    return true;
                }

                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            if (card.PermanentOfThisCard().IsSuspended)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                string effectName = "While this Digimon is suspended, all of your opponent's Digimon can only attack suspended Digimon.";

                cardEffects.Add(CardEffectFactory.CanNotAttackStaticEffect(
                    attackerCondition: AttackerCondition,
                    defenderCondition: DefenderCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition,
                    effectName: effectName
                ));
            }      
            #endregion

            return cardEffects;
        }
    }
}