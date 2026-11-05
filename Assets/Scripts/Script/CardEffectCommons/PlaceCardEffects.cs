using System;
using System.Collections;
using System.Collections.Generic;

public partial class CardEffectCommons
{
    public static IEnumerator PlaceFromHandOrTrashToDrawAndGainMemory(
        CardSource card,
        Func<CardSource, bool> canSelectCardCondition,
        ICardEffect cardEffect,
        Func<Permanent, bool> CanSelectPermanentCondition = null,
        string customMessage = null,
        int drawCount = 1,
        int memoryGain = 1,
        bool optional = true,
        bool faceDown = false,
        bool toTop = false,
        int from1Hand2TrashOr3Both = 1)
    {
        bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, canSelectCardCondition);
        bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, canSelectCardCondition);

        if (canSelectHand || canSelectTrash)
        {
            bool fromHand = false;
            bool fromTrash = false;
            bool dontPlace = false;

            if (canSelectHand && canSelectTrash && from1Hand2TrashOr3Both == 3)
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

                fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                fromTrash = GManager.instance.userSelectionManager.SelectedIntValue == 2;
                dontPlace = GManager.instance.userSelectionManager.SelectedIntValue == 3;
            }
            else if (canSelectHand && !canSelectTrash && from1Hand2TrashOr3Both == 1)
            {
                fromHand = true;
            }
            else if (!canSelectHand && canSelectTrash && from1Hand2TrashOr3Both == 2)
            {
                fromTrash = true;
            }

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

            if (!dontPlace)
            {
                CardSource selectedCard = null;

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCard = cardSource;

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
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Custom,
                        cardEffect: cardEffect);

                    selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");
                    selectHandEffect.SetUpCustomMessage("Select 1 card to place under a permanent.", "The opponent is selecting 1 card to place under a permanent.");

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
                        afterSelectCardCoroutine: null,
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
                    selectCardEffect.SetUpCustomMessage("Select 1 card to place under a permanent.", "The opponent is selecting 1 card to place under a permanent.");

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
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: cardEffect);

                        selectPermanentEffect.SetUpCustomMessage($"Select 1 permanent that will get a card placed under.", $"The opponent is selecting 1 permanent that will get a card placed under.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            if (toTop)
                            {
                                yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsTop(new List<CardSource>() { selectedCard }, cardEffect));
                            }
                            else
                            {
                                yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, cardEffect));
                            }

                            if (drawCount > 0) yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, drawCount, cardEffect).Draw());
                            if (memoryGain > 0 && card.Owner.CanAddMemory(cardEffect)) yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(memoryGain, cardEffect));
                        }
                    }
                }
            }
        }
    }
}