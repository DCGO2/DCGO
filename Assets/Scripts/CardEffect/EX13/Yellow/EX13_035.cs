using System;
using System.Collections;
using System.Collections.Generic;

// KingEtemon
namespace DCGO.CardEffects.EX13
{
    public class EX13_035 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.ContainsCardName("Sukamon")
                        || targetPermanent.TopCard.ContainsCardName("Etemon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null,
                    level: 5));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName = "May play up to 2 [Chuumon]/[Sukamon]/[Etemon] in name Digimon with up to 6 total play cost from hand or trash";

            string SharedEffectDescription(string tag)
                => $"[{tag}] You may play up to 2 Digimon cards with [Chuumon], [Sukamon] or [Etemon] in their names and up to 6 total play cost from your hand or trash without paying the costs. By returning 10 such cards from your trash to the bottom of the deck, add 6 to the play cost maximum.";

            bool IsChuumonSukamonEtemonCard(CardSource cardSource)
                => cardSource.IsDigimon
                    && (cardSource.ContainsCardName("Chuumon")
                        || cardSource.ContainsCardName("Sukamon")
                        || cardSource.ContainsCardName("Etemon"));

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                int totalCost = 6;
                bool stop = false;
                List<CardSource> allSelectedCards = new List<CardSource>();

                #region By returning 10 cards from trash to the bottom of the deck, add 6 to the play cost maximum
                if (CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, IsChuumonSukamonEtemonCard) >= 10)
                {
                    GManager.instance.userSelectionManager.SetBoolSelection(
                        selectionElements: new List<SelectionElement<bool>>()
                        {
                            new(message: "Return 10 cards", value: true, spriteIndex: 0),
                            new(message: "Don't return", value: false, spriteIndex: 1),
                        },
                        selectPlayer: card.Owner,
                        selectPlayerMessage: "Return 10 cards from your trash to the bottom of the deck to add 6 to the play cost maximum?",
                        notSelectPlayerMessage: "The opponent is choosing whether to return cards from their trash to the bottom of the deck.");

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    if (GManager.instance.userSelectionManager.SelectedBoolValue)
                    {
                        List<CardSource> returnCards = new List<CardSource>();

                        SelectCardEffect selectReturnCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectReturnCardEffect.SetUp(
                            canTargetCondition: IsChuumonSukamonEtemonCard,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectReturnCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 10 cards to return to the bottom of the deck.",
                            maxCount: 10,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        IEnumerator SelectReturnCardCoroutine(CardSource cardSource)
                        {
                            returnCards.Add(cardSource);
                            yield return null;
                        }

                        selectReturnCardEffect.SetUpCustomMessage("Select 10 cards to return to the bottom of the deck.", "The opponent is selecting 10 cards to return to the bottom of the deck.");
                        selectReturnCardEffect.SetUpCustomMessage_ShowCard("Selected Cards");

                        yield return ContinuousController.instance.StartCoroutine(selectReturnCardEffect.Activate());

                        if (returnCards.Count == 10)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ReturnRevealedCardsToLibraryBottom(
                                remainingCards: returnCards,
                                activateClass: activateClass));

                            totalCost += 6;
                        }
                    }
                }
                #endregion

                #region May play up to 2 Digimon cards from hand or trash
                bool CanSelectCardCondition(CardSource cardSource)
                    => IsChuumonSukamonEtemonCard(cardSource)
                        && cardSource.HasPlayCost
                        && cardSource.GetCostItself <= totalCost
                        && allSelectedCards.Count < 2
                        && !allSelectedCards.Contains(cardSource)
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);

                bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                {
                    int sumCost = cardSource.GetCostItself;
                    foreach (CardSource cardSource1 in cardSources) sumCost += cardSource1.GetCostItself;
                    return sumCost <= totalCost;
                }

                bool CanEndSelectCardCondition(List<CardSource> cards)
                {
                    int sumCost = 0;
                    foreach (CardSource source in cards) sumCost += source.GetCostItself;
                    return sumCost <= totalCost;
                }

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    allSelectedCards.Add(cardSource);
                    totalCost -= cardSource.GetCostItself;

                    yield return null;
                }

                IEnumerator AfterSelectCardCoroutine(List<CardSource> selectedCards)
                {
                    if (selectedCards.Count == 0)
                    {
                        stop = true;
                    }

                    yield return null;
                }

                while (!stop)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    if (!canSelectHand && !canSelectTrash) break;

                    if (canSelectHand && canSelectTrash)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new(message: "From hand", value: 1, spriteIndex: 0),
                            new(message: "From trash", value: 2, spriteIndex: 0),
                            new(message: "Don't select", value: 3, spriteIndex: 1),
                        };

                        GManager.instance.userSelectionManager.SetIntSelection(
                            selectionElements: selectionElements,
                            selectPlayer: card.Owner,
                            selectPlayerMessage: "From which area will you select a card?",
                            notSelectPlayerMessage: "The opponent is choosing from which area to select a card.");

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                    }
                    else
                    {
                        GManager.instance.userSelectionManager.SetInt(canSelectHand ? 1 : 2);
                    }

                    int selectedArea = GManager.instance.userSelectionManager.SelectedIntValue;

                    if (selectedArea == 3) break;

                    int remainingCount = 2 - allSelectedCards.Count;

                    if (selectedArea == 1)
                    {
                        int maxCount = Math.Min(remainingCount, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, CanSelectCardCondition));

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                            canEndSelectCondition: CanEndSelectCardCondition,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: true,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage(
                            $"Select up to {remainingCount} Digimon cards with up to {totalCost} total play cost to play from your hand.",
                            $"The opponent is selecting up to {remainingCount} Digimon cards with up to {totalCost} total play cost to play from their hand.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Selected cards");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                    }
                    else
                    {
                        int maxCount = Math.Min(remainingCount, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, CanSelectCardCondition));

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                            canEndSelectCondition: CanEndSelectCardCondition,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            message: $"Select up to {remainingCount} Digimon cards with up to {totalCost} total play cost to play from your trash.",
                            maxCount: maxCount,
                            canEndNotMax: true,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage(
                            $"Select up to {remainingCount} Digimon cards with up to {totalCost} total play cost to play from your trash.",
                            $"The opponent is selecting up to {remainingCount} Digimon cards with up to {totalCost} total play cost to play from their trash.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Selected cards");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }
                }

                if (allSelectedCards.Count >= 1)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: allSelectedCards,
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Hand,
                        activateETB: true));
                }
                #endregion
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            #endregion

            #region All Turns
            if (timing == EffectTiming.None)
            {
                bool IsSukamonOrEtemonDigimon(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent)
                        && (permanent.TopCard.ContainsCardName("Sukamon") || permanent.TopCard.ContainsCardName("Etemon"));

                bool PermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool Condition()
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.MatchConditionPermanentCount(IsSukamonOrEtemonDigimon) >= 3;

                cardEffects.Add(CardEffectFactory.ChangeSAttackStaticEffect(
                    permanentCondition: PermanentCondition,
                    changeValue: -1,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition));

                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(
                    permanentCondition: PermanentCondition,
                    changeValue: -3000,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition,
                    effectName: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
