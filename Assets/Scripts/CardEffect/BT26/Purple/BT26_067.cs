using System;
using System.Collections;
using System.Collections.Generic;

// Wizardmon
namespace DCGO.CardEffects.BT26
{
    public class BT26_067 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 3));
            }
            #endregion

            #region Shared On Play / When Digivolving - Draw & Trash

            string SharedEffectName()
                => "Draw 1 and trash 1 card in hand";

            string SharedEffectDescription(string tag)
                => $"[{tag}] <Draw 1> and trash 1 card in your hand.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                if (card.Owner.HandCards.Count >= 1)
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: _ => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                }
            }

            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            #region End of Your Turn
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By returning this Digimon to deck bottom, play 1 red/blue [Iliad] Digimon from trash for 4 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[End of Your Turn] If you have a blue or yellow Digimon, by returning this Digimon to the bottom of the deck, you may play 1 red or blue Digimon card with the [Iliad] trait from your trash with the cost reduced by 4.";

                bool HasBlueOrYellowDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.CardColors.Contains(CardColor.Blue) || permanent.TopCard.CardColors.Contains(CardColor.Yellow));

                bool CanSelectTrashCardCondition(CardSource cardSource)
                    => cardSource.IsDigimon
                        && cardSource.EqualsTraits("Iliad")
                        && (cardSource.CardColors.Contains(CardColor.Red) || cardSource.CardColors.Contains(CardColor.Blue))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, true, activateClass, fixedCost: cardSource.GetCostItself - 4);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.HasMatchConditionPermanent(HasBlueOrYellowDigimonCondition);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.PermanentOfThisCard() != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).DeckBounce());

                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectTrashCardCondition))
                        {
                            CardSource selectedCard = null;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectTrashCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 red/blue [Iliad] Digimon card to play.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCard = cardSource;
                                yield return null;
                            }

                            selectCardEffect.SetUpCustomMessage("Select 1 red/blue [Iliad] Digimon card to play.", "The opponent is selecting 1 red/blue [Iliad] Digimon card to play.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            if (selectedCard != null)
                            {
                                int reduceCost = 4;

                                ChangeCostClass changeCostClass = new ChangeCostClass();
                                changeCostClass.SetUpICardEffect($"Play Cost -{reduceCost}", _ => true, card);
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

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: new List<CardSource> { selectedCard },
                                    activateClass: activateClass,
                                    payCost: true,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Trash,
                                    activateETB: true));

                                card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);
                            }
                        }
                    }
                }
            }
            #endregion

            #region Inherit - Retaliation
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.RetaliationSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
