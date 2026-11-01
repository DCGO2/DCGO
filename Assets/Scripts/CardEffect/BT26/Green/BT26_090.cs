using System;
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
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
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
                int reduceCost = 0;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By suspending this Tamer, use 1 [TS] Option card from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[End of Your Turn] By suspending this Tamer, you may use 1 Option card with the [TS] trait from your hand. For each memory your opponent has, reduce this effect's paid cost by 1.";

                bool CanSelectOptionCardCondition(CardSource cardSource, ICardEffect effect)
                {
                    reduceCost = card.Owner.Enemy.MemoryForPlayer;

                    return cardSource.IsOption
                        && cardSource.HasTSTraits
                        && !cardSource.CanNotPlayThisOption
                        && cardSource.Owner.MaxMemoryCost >= cardSource.GetCostItself - reduceCost;
                }

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
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
                            CardSource selectedCard = null;

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectOptionCardConditionBound,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCard = cardSource;
                                yield return null;
                            }

                            selectHandEffect.SetUpCustomMessage("Select 1 [TS] Option card to use.", "The opponent is selecting 1 [TS] Option card to use.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");
                            yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                            if (selectedCard != null)
                            {
                                ChangeCostClass changeCostClass = new ChangeCostClass();
                                changeCostClass.SetUpICardEffect($"Use Cost -{reduceCost}", _ => true, card);
                                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: PlayCondition, rootCondition: _ => true, isUpDown: () => true, isCheckAvailability: () => false, isChangePayingCost: () => true);
                                Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                                card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                                ICardEffect GetCardEffect(EffectTiming _timing)
                                    => _timing == EffectTiming.None ? changeCostClass : null;

                                bool PlayCondition(CardSource cs) => cs == selectedCard;

                                int ChangeCost(CardSource cs, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                                {
                                    if (PlayCondition(cs))
                                    {
                                        cost -= reduceCost;
                                    }

                                    return cost;
                                }

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                                    cardSources: new List<CardSource>() { selectedCard },
                                    activateClass: activateClass,
                                    payCost: true,
                                    root: SelectCardEffect.Root.Hand));

                                card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);
                            }
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
