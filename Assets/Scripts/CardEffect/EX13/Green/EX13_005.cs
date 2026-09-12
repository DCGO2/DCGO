using System;
using System.Collections;
using System.Collections.Generic;

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
                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect("Play/Use Cost -1", _ => true, card);
                        changeCostClass.SetUpChangeCostClass(
                            changeCostFunc: ChangeCost, cardSourceCondition: _ => true, rootCondition: _ => true,
                            isUpDown: () => true, isCheckAvailability: () => false, isChangePayingCost: () => true);

                        ICardEffect GetCardEffect(EffectTiming _timing) => _timing == EffectTiming.None ? changeCostClass : null;
                        card.Owner.UntilCalculateFixedCostEffect.Add(GetCardEffect);

                        int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                            => cost - 1;

                        if (selectedCard.IsOption)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                                cardSources: new List<CardSource>() { selectedCard },
                                activateClass: activateClass,
                                payCost: true,
                                root: SelectCardEffect.Root.Hand));
                        }
                        else
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: new List<CardSource>() { selectedCard },
                                activateClass: activateClass,
                                payCost: true,
                                isTapped: false,
                                root: SelectCardEffect.Root.Hand,
                                activateETB: true));
                        }

                        card.Owner.UntilCalculateFixedCostEffect.Remove(GetCardEffect);
                    }

                    if (selectedCard == null) activateClass.RemoveUse();
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
