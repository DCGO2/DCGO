using System.Collections;
using System.Collections.Generic;

// Kanan Yuki
namespace DCGO.CardEffects.BT26
{
    public class BT26_090 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Start of Your Main Phase] If you have 4 or less memory, gain 1 memory.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card)
                        && card.Owner.CanAddMemory(activateClass)
                        && card.Owner.MemoryForPlayer <= 4;

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }
            #endregion

            #region End of Your Turn
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By suspending this Tamer, use 1 [TS] Option card from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[End of Your Turn] By suspending this Tamer, you may use 1 Option card with the [TS] trait from your hand. For each memory your opponent has, reduce this effect's paid cost by 1.";

                bool CanSelectOptionCardCondition(CardSource cardSource, ICardEffect effect)
                    => cardSource.IsOption
                        && cardSource.HasTSTraits
                        && !cardSource.CanNotPlayThisOption
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, true, effect, fixedCost: cardSource.GetCostItself - card.Owner.Enemy.MemoryForPlayer);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass: activateClass,
                        successProcess: SuccessProcess,
                        failureProcess: null));

                    IEnumerator SuccessProcess(List<Permanent> _permanents)
                    {
                        bool CanSelectOptionCardConditionBound(CardSource cardSource) => CanSelectOptionCardCondition(cardSource, activateClass);

                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectOptionCardConditionBound))
                        {
                            int reduceCost = card.Owner.Enemy.MemoryForPlayer;

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                                canTargetCondition: CanSelectOptionCardConditionBound,
                                root: SelectCardEffect.Root.Hand,
                                cardEffect: activateClass,
                                payCost: true,
                                reduceCostTuple: (reduceCost, null)));
                        }
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}
