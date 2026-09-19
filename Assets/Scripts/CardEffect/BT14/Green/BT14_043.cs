using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT14
{
    public class BT14_043 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend your 1 Digimon to suspend opponent's 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By suspending 1 of your Digimon, suspend 1 of your opponent's Digimon.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (CardEffectCommons.CanActivateSuspendCostEffect(permanent.TopCard))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = null;

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 your Digimon to suspend.", "The opponent is selecting 1 Digimon to suspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }
                    }

                    if (selectedPermanent != null)
                    {
                        if (selectedPermanent.TopCard != null)
                        {
                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                if (!selectedPermanent.IsSuspended && selectedPermanent.CanSuspend)
                                {
                                    Permanent suspendTargetPermanent = selectedPermanent;

                                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { suspendTargetPermanent }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                                    if (suspendTargetPermanent.TopCard != null)
                                    {
                                        if (suspendTargetPermanent.IsSuspended)
                                        {
                                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                                            {
                                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition1));

                                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                                selectPermanentEffect.SetUp(
                                                    selectPlayer: card.Owner,
                                                    canTargetCondition: CanSelectPermanentCondition1,
                                                    canTargetCondition_ByPreSelecetedList: null,
                                                    canEndSelectCondition: null,
                                                    maxCount: maxCount,
                                                    canNoSelect: false,
                                                    canEndNotMax: false,
                                                    selectPermanentCoroutine: null,
                                                    afterSelectPermanentCoroutine: null,
                                                    mode: SelectPermanentEffect.Mode.Tap,
                                                    cardEffect: activateClass);

                                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}