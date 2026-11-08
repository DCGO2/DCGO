using System;
using System.Collections;
using System.Collections.Generic;

public partial class CardEffectCommons
{
    public static IEnumerator PlaceFromHandOrTrashToDrawAndGainMemory(
        CardSource card,
        ICardEffect cardEffect,
        Func<CardSource, bool> canSelectCardCondition = null,
        Func<Permanent, bool> CanSelectPermanentCondition = null,
        string customMessage = null,
        int drawCount = 1,
        int memoryGain = 1,
        int opponentDrawCount = 0,
        bool optional = true,
        bool faceDown = false,
        bool toTop = false,
        int from1Hand2TrashOr3Both = 1,
        IEnumerator successProcess = null,
        IEnumerator failureProcess = null)
    {
        canSelectCardCondition ??= cardSource => true;

        bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, canSelectCardCondition) && from1Hand2TrashOr3Both != 2;
        bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, canSelectCardCondition) && from1Hand2TrashOr3Both != 1;

        bool decline = false;

        if (canSelectHand || canSelectTrash)
        {
            bool fromHand = false;
            bool fromTrash = false;
            bool dontPlace = false;

            if (canSelectHand && canSelectTrash)
            {
                List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                {
                    new(message: "From hand", value: 1, spriteIndex: 0),
                    new(message: "From trash", value: 2, spriteIndex: 0),
                };
                if (optional) selectionElements.Add(new(message: "Don't place", value: 3, spriteIndex: 1));

                string selectPlayerMessage = "From which area do you select a card?";
                string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                fromTrash = GManager.instance.userSelectionManager.SelectedIntValue == 2;
                dontPlace = GManager.instance.userSelectionManager.SelectedIntValue == 3;
            }
            else if (canSelectHand)
            {
                fromHand = true;
            }
            else
            {
                fromTrash = true;
            }

            if (!dontPlace)
            {
                CardSource selectedCard = null;

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCard = cardSource;

                    yield return null;
                }

                IEnumerator AfterSelectCardCoroutine(List<CardSource> selectedCards)
                {
                    if (selectedCards.Count == 0) decline = true;

                    yield return null;
                }

                if (fromHand)
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: canSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: optional,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        mode: SelectHandEffect.Mode.Custom,
                        cardEffect: cardEffect);

                    selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");
                    selectHandEffect.SetUpCustomMessage(customMessage ?? "Select 1 card to place under a permanent.", "The opponent is selecting 1 card to place under a permanent.");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                }
                else if (fromTrash)
                {
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: canSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => optional,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        message: "Select 1 card to place under a permanent.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: cardEffect);

                    selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");
                    selectCardEffect.SetUpCustomMessage(customMessage ?? "Select 1 card to place under a permanent.", "The opponent is selecting 1 card to place under a permanent.");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                }

                if (selectedCard != null)
                {
                    Permanent selectedPermanent = null;

                    if (CanSelectPermanentCondition == null)
                    {
                        selectedPermanent = card.PermanentOfThisCard();
                    }
                    else
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: optional,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: cardEffect);

                        selectPermanentEffect.SetUpCustomMessage($"Select 1 permanent that will get a card placed under.", $"The opponent is selecting 1 permanent that will get a card placed under.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> selectedPermanents)
                        {
                            if (selectedPermanents.Count == 0) decline = true;

                            yield return null;
                        }
                    }

                    if (selectedPermanent != null)
                    {
                        if (toTop)
                        {
                            yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsTop(new List<CardSource>() { selectedCard }, cardEffect, faceDown));
                        }
                        else
                        {
                            yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, cardEffect, isFacedown: faceDown));
                        }

                        if (drawCount > 0) yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, drawCount, cardEffect).Draw());
                        if (opponentDrawCount > 0) yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner.Enemy, opponentDrawCount, cardEffect).Draw());
                        if (memoryGain > 0 && card.Owner.CanAddMemory(cardEffect)) yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(memoryGain, cardEffect));
                        if (successProcess != null) yield return ContinuousController.instance.StartCoroutine(successProcess);
                    }
                }
            }
            else if (optional) decline = true;
        }

        if (failureProcess != null && decline) yield return ContinuousController.instance.StartCoroutine(failureProcess);
    }
}
