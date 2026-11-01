using System;
using System.Collections;
using System.Collections.Generic;

// Veedramon
namespace DCGO.CardEffects.EX13
{
    public class EX13_019 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasCSTraits;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 3));
            }
            #endregion

            #region Jamming
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.JammingSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                const int reduceCost = 2;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Tamer card with [Veedramon] in its text from the hand with the cost reduced by 2", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("EX13_019_WA");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] [Once Per Turn] You may play 1 Tamer card with [Veedramon] in its text from your hand with the cost reduced by 2.";

                bool CanSelectHandCardCondition(CardSource cardSource)
                    => cardSource.IsTamer
                        && cardSource.HasText("Veedramon")
                        && CardEffectCommons.CanPlayAsNewPermanent(
                            cardSource: cardSource,
                            payCost: true,
                            cardEffect: activateClass,
                            fixedCost: Math.Max(0, cardSource.GetCostItself - reduceCost));

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectHandCardCondition);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    CardSource selectedCard = null;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectHandCardCondition,
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

                    selectHandEffect.SetUpCustomMessage("Select 1 Tamer card with [Veedramon] in its text to play.", "The opponent is selecting 1 Tamer card to play.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (selectedCard == null)
                    {
                        activateClass.RemoveUse();
                        yield break;
                    }

                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect($"Play Cost -{reduceCost}", _ => true, card);
                    changeCostClass.SetUpChangeCostClass(
                        changeCostFunc: ChangeCost,
                        cardSourceCondition: PlayCondition,
                        rootCondition: _ => true,
                        isUpDown: () => true,
                        isCheckAvailability: () => false,
                        isChangePayingCost: () => true);

                    Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                    card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                    ICardEffect GetCardEffect(EffectTiming _timing)
                        => _timing == EffectTiming.None ? changeCostClass : null;

                    bool PlayCondition(CardSource cardSource) => cardSource == selectedCard;

                    int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                    {
                        if (PlayCondition(cardSource)) cost -= reduceCost;

                        return cost;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: new List<CardSource>() { selectedCard },
                        activateClass: activateClass,
                        payCost: true,
                        isTapped: false,
                        root: SelectCardEffect.Root.Hand,
                        activateETB: true));

                    card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);
                }
            }
            #endregion

            #region Jamming - ESS
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.JammingSelfStaticEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
