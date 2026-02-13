using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.LM
{
    public class LM_010 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play
            if(timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Tamers can't unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Suspend 1 Tamer. None of your opponent's Tamers can unsuspend until the end of their turn.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                    {
                        return permanent.IsTamer;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
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
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer to suspend.", "The opponent is selecting 1 Tamer to suspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        foreach(Permanent permanent in card.Owner.Enemy.GetBattleAreaPermanents())
                        {
                            if(permanent.IsTamer)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCantUnsuspendUntilOpponentTurnEnd(
                                    targetPermanent: permanent,
                                    activateClass: activateClass
                                ));
                            }
                        }
                    }
                }
            }
            

            #endregion

            #region All Turns - DP Increase
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                    {
                        if (permanent.IsTamer)
                        {
                            if (permanent.IsSuspended)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                int DPIncrease()
                {
                    return CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition) * 1000;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: DPIncrease(), isInheritedEffect: false, card: card, condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}