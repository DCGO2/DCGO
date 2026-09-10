using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Bebydomon
namespace DCGO.CardEffects.EX13
{
    public class EX13_005 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region When Attacking - ESS
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May play/use 1 [Dracomon]/[Examon] text card from hand for 1 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("EX13_005_ESS_WA");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] [Once Per Turn] You may play or use 1 card with [Dracomon] or [Examon] in its text from your hand with the cost reduced by 1.";

                bool CanSelectCardCondition(CardSource cardSource)
                    => (cardSource.HasText("Dracomon") || cardSource.HasText("Examon"))
                        && CardEffectCommons.CanPlayOrUse(cardSource, activateClass, fixedCost: Math.Max(0, cardSource.GetCostItself - 1));

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedCard = null;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectCardCondition,
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

                    selectHandEffect.SetUpCustomMessage("Select 1 card to play/use.", "The opponent is selecting 1 card to play/use.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    if (selectedCard != null)
                    {
                        IEnumerator ReduceCost(string type)
                        {
                            if (card.Owner.CanReduceCost(null, card))
                            {
                                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                            }

                            Hashtable costHashtable = new Hashtable
                            {
                                { "CardEffect", activateClass }
                            };

                            ChangeCostClass changeCostClass = new ChangeCostClass();
                            changeCostClass.SetUpICardEffect($"{type} cost: -1", CanUseCostCondition, card);
                            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CostCardSourceCondition, rootCondition: CostRootCondition, isUpDown: IsUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                            card.Owner.UntilCalculateFixedCostEffect.Add(_ => changeCostClass);

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(costHashtable));

                            bool CanUseCostCondition(Hashtable effectHashtable)
                                => true;

                            int ChangeCost(CardSource costCard, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                            {
                                if (CostCardSourceCondition(costCard)
                                    && CostRootCondition(root)
                                    && CostPermanentsCondition(targetPermanents))
                                {
                                    cost -= 1;
                                }

                                return cost;
                            }

                            bool CostPermanentsCondition(List<Permanent> targetPermanents)
                                => targetPermanents == null || targetPermanents.Count(targetPermanent => targetPermanent != null) == 0;

                            bool CostCardSourceCondition(CardSource costCard)
                                => costCard != null
                                    && costCard.Owner == card.Owner;

                            bool CostRootCondition(SelectCardEffect.Root root)
                                => true;

                            bool IsUpDown()
                                => true;
                        }

                        if (selectedCard.IsOption)
                        {
                            yield return ContinuousController.instance.StartCoroutine(ReduceCost("Use"));
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                                cardSources: new List<CardSource>() { selectedCard },
                                activateClass: activateClass,
                                payCost: true,
                                root: SelectCardEffect.Root.Hand));
                        }
                        else
                        {
                            yield return ContinuousController.instance.StartCoroutine(ReduceCost("Play"));
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: new List<CardSource>() { selectedCard },
                                activateClass: activateClass,
                                payCost: true,
                                isTapped: false,
                                root: SelectCardEffect.Root.Hand,
                                activateETB: true));
                        }
                    }

                    if (selectedCard == null) activateClass.RemoveUse();
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
