using System;
using System.Collections;
using System.Collections.Generic;

//Rasielmon
namespace DCGO.CardEffects.LM
{
    public class LM_042 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.IsLevel5)
                    {
                        if (targetPermanent.TopCard.EqualsTraits("Angel") || targetPermanent.TopCard.EqualsTraits("Archangel"))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, false, card: card, condition: null));
            }

            #endregion

            #region Sec +1

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(
                changeValue: 1,
                isInheritedEffect: false,
                card: card,
                condition: null));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Suspend 1 of your opponent's Digimon or Tamers. Then, until their turn ends, 1 of their Digimon or Tamers can't activate [When Digivolving] effects or unsuspend.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPlay(hashtable, card))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanentConditionSuspend))
                        {
                            if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanentConditionStopWD))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanSelectPermanentConditionSuspend(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        if (permanent.IsTamer || permanent.IsDigimon)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanSelectPermanentConditionStopWD(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        if (permanent.IsTamer || permanent.IsDigimon)
                        {
                            if (!permanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentConditionSuspend));
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentConditionSuspend,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or Tamer to suspend", "The opponent is selecting 1 Digimon or tamer to suspend");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    Permanent selectedPermanent = null;
                    int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentConditionStopWD));
                    SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect1.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentConditionStopWD,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect1.SetUpCustomMessage("Select 1 Digimon or Tamer to disable WD effects", "The opponent is selecting 1 Digimon or tamer to disable WD effects");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCantUnsuspendUntilOpponentTurnEnd(
                                targetPermanent: selectedPermanent,
                                activateClass: activateClass
                            ));

                    DisableEffectClass invalidationClass = new DisableEffectClass();
                    invalidationClass.SetUpICardEffect("Ignore [When Digivolving] Effect", hashtableDebuff => true, card);
                    invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
                    selectedPermanent.UntilOwnerTurnEndEffects.Add(_ => invalidationClass);

                    bool InvalidateCondition(ICardEffect cardEffect)
                    {
                        if (selectedPermanent.TopCard != null)
                        {
                            if (cardEffect != null)
                            {
                                if (cardEffect.EffectSourceCard != null)
                                {
                                    if (isExistOnField(cardEffect.EffectSourceCard))
                                    {
                                        if (cardEffect.EffectSourceCard.PermanentOfThisCard() == selectedPermanent)
                                        {
                                            if (cardEffect.IsWhenDigivolving)
                                            {
                                                if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        return false;
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Suspend 1 of your opponent's Digimon or Tamers. Then, until their turn ends, 1 of their Digimon or Tamers can't activate [When Digivolving] effects or unsuspend.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanentConditionSuspend))
                        {
                            if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanentConditionStopWD))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanSelectPermanentConditionSuspend(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        if (permanent.IsTamer || permanent.IsDigimon)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanSelectPermanentConditionStopWD(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        if (permanent.IsTamer || permanent.IsDigimon)
                        {
                            if (!permanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentConditionSuspend));
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentConditionSuspend,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or Tamer to suspend", "The opponent is selecting 1 Digimon or tamer to suspend");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    Permanent selectedPermanent = null;
                    int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentConditionStopWD));
                    SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect1.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentConditionStopWD,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect1.SetUpCustomMessage("Select 1 Digimon or Tamer to disable WD effects", "The opponent is selecting 1 Digimon or tamer to disable WD effects");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCantUnsuspendUntilOpponentTurnEnd(
                                targetPermanent: selectedPermanent,
                                activateClass: activateClass
                            ));

                    DisableEffectClass invalidationClass = new DisableEffectClass();
                    invalidationClass.SetUpICardEffect("Ignore [When Digivolving] Effect", hashtableDebuff => true, card);
                    invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
                    selectedPermanent.UntilOwnerTurnEndEffects.Add(_ => invalidationClass);

                    bool InvalidateCondition(ICardEffect cardEffect)
                    {
                        if (selectedPermanent.TopCard != null)
                        {
                            if (cardEffect != null)
                            {
                                if (cardEffect.EffectSourceCard != null)
                                {
                                    if (isExistOnField(cardEffect.EffectSourceCard))
                                    {
                                        if (cardEffect.EffectSourceCard.PermanentOfThisCard() == selectedPermanent)
                                        {
                                            if (cardEffect.IsWhenDigivolving)
                                            {
                                                if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        return false;
                    }
                }
            }

            #endregion

            #region On Deletion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place as the bottom security card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] Place this card as the bottom security card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanActivateOnDeletion(card))
                    {
                        if (card.Owner.CanAddSecurity(activateClass))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(card, toTop: false));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}