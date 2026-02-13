using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Reveal cards from top of deck and process for all of them
    public static IEnumerator RevealDeckTopCardsAndProcessForAll(
        int revealCount,
        SimplifiedSelectCardConditionClass simplifiedSelectCardCondition,
        RemainingCardsPlace remainingCardsPlace,
        ICardEffect activateClass,
        Func<List<CardSource>, IEnumerator> revealedCardsCoroutine = null,
        List<CardSource> refSelectedCards = null,
        bool isOpponentDeck = false)
    {
        if (revealCount <= 0) yield break;
        if (activateClass == null) yield break;
        CardSource effectSourceCard = activateClass.EffectSourceCard;
        if (effectSourceCard == null) yield break;
        Player selectPlayer = effectSourceCard.Owner;
        if (selectPlayer == null) yield break;
        Player revealPlayer = isOpponentDeck ? selectPlayer.Enemy : selectPlayer;
        if (revealPlayer == null) yield break;
        if (revealPlayer.LibraryCards.Count == 0) yield break;

        if (simplifiedSelectCardCondition == null) yield break;

        RevealLibraryClass revealLibrary = new RevealLibraryClass(revealPlayer, revealCount);

        yield return ContinuousController.instance.StartCoroutine(revealLibrary.RevealLibrary());

        List<CardSource> selectedCards = revealLibrary.RevealedCards.Filter(simplifiedSelectCardCondition.CanTargetCondition);

        List<CardSource> remainingCards = revealLibrary.RevealedCards.Filter(cardSource => !selectedCards.Contains(cardSource));

        switch (simplifiedSelectCardCondition.Mode)
        {
            case SelectCardEffect.Mode.AddHand:
                {
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(selectedCards, false, activateClass));

                    if (selectedCards.Count >= 1)
                    {
                        string log = "";

                        log += $"\nCard{Utils.PluralFormSuffix(selectedCards.Count)} added to hand:";

                        foreach (CardSource cardSource in selectedCards)
                        {
                            log += $"\n{cardSource.BaseENGCardNameFromEntity}({cardSource.CardID})";
                        }

                        log += "\n";

                        PlayLog.OnAddLog?.Invoke(log);
                    }
                }
                break;

            case SelectCardEffect.Mode.Discard:
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(selectedCards, "Discarded Cards", true, true));
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCards(selectedCards));

                    if (selectedCards.Count >= 1)
                    {
                        string log = "";

                        log += $"\nTrash Card{Utils.PluralFormSuffix(selectedCards.Count)}:";

                        foreach (CardSource cardSource in selectedCards)
                        {
                            log += $"\n{cardSource.BaseENGCardNameFromEntity}({cardSource.CardID})";
                        }

                        log += "\n";

                        PlayLog.OnAddLog?.Invoke(log);
                    }
                }
                break;

            case SelectCardEffect.Mode.Custom:
                if (simplifiedSelectCardCondition.SelectCardCoroutine != null)
                {
                    foreach (CardSource selectedCard in selectedCards)
                    {
                        yield return ContinuousController.instance.StartCoroutine(simplifiedSelectCardCondition.SelectCardCoroutine(selectedCard));
                    }
                }
                break;
        }

        if (revealedCardsCoroutine != null)
        {
            yield return ContinuousController.instance.StartCoroutine(revealedCardsCoroutine(revealLibrary.RevealedCards));
        }

        switch (remainingCardsPlace)
        {
            case RemainingCardsPlace.DeckBottom:
                yield return ContinuousController.instance.StartCoroutine(ReturnRevealedCardsToLibraryBottom(remainingCards, activateClass));
                break;

            case RemainingCardsPlace.DeckTop:
                yield return ContinuousController.instance.StartCoroutine(ReturnRevealedCardsToLibraryTop(remainingCards, activateClass));
                break;

            case RemainingCardsPlace.Trash:
                {
                    yield return ContinuousController.instance.StartCoroutine(TrashRevealedCards(remainingCards, activateClass));

                    if (remainingCards.Count >= 1)
                    {
                        string log = "";

                        log += $"\nTrash Card{Utils.PluralFormSuffix(remainingCards.Count)}:";

                        foreach (CardSource cardSource in remainingCards)
                        {
                            log += $"\n{cardSource.BaseENGCardNameFromEntity}({cardSource.CardID})";
                        }

                        log += "\n";

                        PlayLog.OnAddLog?.Invoke(log);
                    }
                }
                break;

            case RemainingCardsPlace.AddHand:
                {
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(remainingCards, false, activateClass));

                    if (remainingCards.Count >= 1)
                    {
                        string log = "";

                        log += $"\nTrash Card{Utils.PluralFormSuffix(remainingCards.Count)}:";

                        foreach (CardSource cardSource in remainingCards)
                        {
                            log += $"\n{cardSource.BaseENGCardNameFromEntity}({cardSource.CardID})";
                        }

                        log += "\n";

                        PlayLog.OnAddLog?.Invoke(log);
                    }
                }
                break;

            case RemainingCardsPlace.DeckTopOrBottom:
                if (remainingCards.Count >= 2)
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(remainingCards, "Remaining Cards", true, true));
                }

                yield return ContinuousController.instance.StartCoroutine(ReturnRevealedCardsToLibraryTopOrBottom(remainingCards, activateClass));
                break;
        }

        if (refSelectedCards != null)
        {
            foreach (CardSource selectedCard in selectedCards)
            {
                refSelectedCards.Add(selectedCard);
            }
        }

        revealLibrary.RevealedCards.ForEach(cardSource => cardSource.IsBeingRevealed = false);
    }
    #endregion

    #region Reveal cards from top of deck and select them
    public static IEnumerator SimplifiedRevealDeckTopCardsAndSelect(
        int revealCount,
        SimplifiedSelectCardConditionClass[] simplifiedSelectCardConditions,
        RemainingCardsPlace remainingCardsPlace,
        ICardEffect activateClass,
        Func<List<CardSource>, CardSource, bool> canTargetCondition_ByPreSelecetedList = null,
        Func<List<CardSource>, bool> canEndSelectCondition = null,
        bool canNoSelect = false,
        bool canEndNotMax = false,
        bool isSendAllCardsToSamePlace = false,
        bool isOpponentDeck = false,
        Func<List<CardSource>, IEnumerator> revealedCardsCoroutine = null)
    {
        if (revealCount <= 0) yield break;
        if (activateClass == null) yield break;
        CardSource effectSourceCard = activateClass.EffectSourceCard;
        if (effectSourceCard == null) yield break;
        Player revealPlayer = effectSourceCard.Owner;
        if (revealPlayer == null) yield break;
        if (revealPlayer.LibraryCards.Count == 0) yield break;

        if (simplifiedSelectCardConditions == null) yield break;
        if (simplifiedSelectCardConditions.Length == 0) yield break;

        SelectCardConditionClass[] selectCardConditions = simplifiedSelectCardConditions.Map(selectCardConditionTuple =>
        new SelectCardConditionClass(
            canTargetCondition: selectCardConditionTuple.CanTargetCondition,
            canTargetCondition_ByPreSelecetedList: canTargetCondition_ByPreSelecetedList,
            canEndSelectCondition: canEndSelectCondition,
            canNoSelect: canNoSelect,
            selectCardCoroutine: selectCardConditionTuple.SelectCardCoroutine,
            message: selectCardConditionTuple.Message,
            maxCount: selectCardConditionTuple.MaxCount,
            canEndNotMax: canEndNotMax,
            mode: selectCardConditionTuple.Mode
        ));

        yield return ContinuousController.instance.StartCoroutine(RevealDeckTopCardsAndSelect(
            revealCount: revealCount,
            selectCardConditions: selectCardConditions,
            remainingCardsPlace: remainingCardsPlace,
            activateClass: activateClass,
            isSendAllCardsToSamePlace: isSendAllCardsToSamePlace,
            isOpponentDeck: isOpponentDeck,
            revealedCardsCoroutine: revealedCardsCoroutine
        ));
    }

    public static IEnumerator RevealDeckTopCardsAndSelect(
        int revealCount,
        SelectCardConditionClass[] selectCardConditions,
        RemainingCardsPlace remainingCardsPlace,
        ICardEffect activateClass,
        bool canNoAction = false,
        bool isSendAllCardsToSamePlace = false,
        bool isOpponentDeck = false,
        Func<List<CardSource>, IEnumerator> revealedCardsCoroutine = null)
    {
        if (revealCount <= 0) yield break;
        if (activateClass == null) yield break;
        CardSource effectSourceCard = activateClass.EffectSourceCard;
        if (effectSourceCard == null) yield break;
        Player selectPlayer = effectSourceCard.Owner;
        if (selectPlayer == null) yield break;
        Player revealPlayer = isOpponentDeck ? selectPlayer.Enemy : selectPlayer;
        if (revealPlayer == null) yield break;
        if (revealPlayer.LibraryCards.Count == 0) yield break;
        if (selectCardConditions.Length == 0) yield break;

        RevealLibraryClass revealLibrary = new RevealLibraryClass(revealPlayer, revealCount);

        yield return ContinuousController.instance.StartCoroutine(revealLibrary.RevealLibrary());

        List<CardSource> revealedCards = revealLibrary.RevealedCards.Clone();

        List<CardSource> remainingCards = revealedCards.Clone();

        bool doAction = true;

        // バーニングスタークラッシャー(BT10-096)
        // ブレイジング・メモリーブースト
        if (selectCardConditions.Length >= 2)
        {
            if (canNoAction)
            {
                List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Select cards", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Not select", value : false, spriteIndex: 1),
                        };

                string selectPlayerMessage = "Will you select cards?";
                string notSelectPlayerMessage = "The opponent is wheter to select cards.";

                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: selectPlayer, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                doAction = GManager.instance.userSelectionManager.SelectedBoolValue;
            }
        }

        if (doAction)
        {
            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

            foreach(SelectCardConditionClass selectCondition in selectCardConditions)
            {
                int maxCount = Math.Min(selectCondition.MaxCount, revealedCards.Count(selectCondition.CanTargetCondition));

                if (maxCount >= 1)
                {
                    selectCardEffect.SetUp(
                        canTargetCondition: selectCondition.CanTargetCondition,
                        canTargetCondition_ByPreSelecetedList: selectCondition.CanTargetCondition_ByPreSelecetedList,
                        canEndSelectCondition: selectCondition.CanEndSelectCondition,
                        canNoSelect: () => selectCondition.CanNoSelect,
                        selectCardCoroutine: selectCondition.SelectCardCoroutine,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        message: selectCondition.Message,
                        maxCount: maxCount,
                        canEndNotMax: selectCondition.CanEndNotMax,
                        isShowOpponent: true,
                        mode: selectCondition.Mode,
                        root: SelectCardEffect.Root.Library,
                        customRootCardList: revealedCards,
                        canLookReverseCard: true,
                        selectPlayer: selectPlayer,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                }

                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                {
                    foreach (CardSource cardSource in cardSources)
                    {
                        revealedCards.Remove(cardSource);
                    }

                    yield return null;
                }
            }

            remainingCards = revealedCards.Clone();

            /*SelectCardConditionClass selectCardCondition = selectCardConditions[0];

            int maxCount = Math.Min(selectCardCondition.MaxCount, revealedCards.Count(selectCardCondition.CanTargetCondition));
            //TODO: Clean this up - MBunch
            if (maxCount >= 1)
            {
                selectCardEffect.SetUp(
                canTargetCondition: selectCardCondition.CanTargetCondition,
                canTargetCondition_ByPreSelecetedList: selectCardCondition.CanTargetCondition_ByPreSelecetedList,
                canEndSelectCondition: selectCardCondition.CanEndSelectCondition,
                canNoSelect: () => selectCardCondition.CanNoSelect,
                selectCardCoroutine: selectCardCondition.SelectCardCoroutine,
                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                message: selectCardCondition.Message,
                maxCount: maxCount,
                canEndNotMax: selectCardCondition.CanEndNotMax,
                isShowOpponent: true,
                mode: selectCardCondition.Mode,
                root: SelectCardEffect.Root.Custom,
                customRootCardList: revealedCards,
                canLookReverseCard: true,
                selectPlayer: selectPlayer,
                cardEffect: activateClass);

                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                {
                    foreach (CardSource cardSource in cardSources)
                    {
                        remainingCards = revealedCards.Filter(cardSource => !cardSources.Contains(cardSource));
                    }

                    yield return null;
                }
            }

            remainingCards = revealedCards.Clone();

            if (selectCardConditions.Length >= 2)
            {
                SelectCardConditionClass selectCardCondition1 = selectCardConditions[1];

                maxCount = Math.Min(selectCardCondition1.MaxCount, revealedCards.Count(selectCardCondition1.CanTargetCondition));

                if (maxCount >= 1)
                {
                    selectCardEffect.SetUp(
                canTargetCondition: selectCardCondition1.CanTargetCondition,
                canTargetCondition_ByPreSelecetedList: selectCardCondition1.CanTargetCondition_ByPreSelecetedList,
                canEndSelectCondition: selectCardCondition1.CanEndSelectCondition,
                canNoSelect: () => selectCardCondition1.CanNoSelect,
                selectCardCoroutine: selectCardCondition1.SelectCardCoroutine,
                afterSelectCardCoroutine: AfterSelectCardCoroutine1,
                message: selectCardCondition1.Message,
                maxCount: maxCount,
                canEndNotMax: selectCardCondition1.CanEndNotMax,
                isShowOpponent: true,
                mode: selectCardCondition1.Mode,
                root: SelectCardEffect.Root.Custom,
                customRootCardList: revealedCards,
                canLookReverseCard: true,
                selectPlayer: selectPlayer,
                cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
                    {
                        remainingCards = revealedCards.Filter(cardSource => !cardSources.Contains(cardSource));

                        yield return null;
                    }
                }
            }*/
        }

        if (isSendAllCardsToSamePlace)
        {
            remainingCards = revealLibrary.RevealedCards.Clone();
        }

        switch (remainingCardsPlace)
        {
            case RemainingCardsPlace.DeckBottom:
                yield return ContinuousController.instance.StartCoroutine(ReturnRevealedCardsToLibraryBottom(remainingCards, activateClass));
                break;

            case RemainingCardsPlace.DeckTop:
                yield return ContinuousController.instance.StartCoroutine(ReturnRevealedCardsToLibraryTop(remainingCards, activateClass));
                break;

            case RemainingCardsPlace.Trash:
                yield return ContinuousController.instance.StartCoroutine(TrashRevealedCards(remainingCards, activateClass));
                break;

            case RemainingCardsPlace.AddHand:
                yield return ContinuousController.instance.StartCoroutine(AddRevealedCardsToHand(remainingCards, activateClass));
                break;

            case RemainingCardsPlace.DeckTopOrBottom:
                if (remainingCards.Count >= 2)
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(
                        remainingCards,
                        "Remaining Cards",
                        true,
                        true));
                }

                yield return ContinuousController.instance.StartCoroutine(ReturnRevealedCardsToLibraryTopOrBottom(remainingCards, activateClass));
                break;
        }

        if (revealedCardsCoroutine != null)
        {
            yield return ContinuousController.instance.StartCoroutine(revealedCardsCoroutine(revealLibrary.RevealedCards));
        }

        revealLibrary.RevealedCards.ForEach(cardSource => cardSource.IsBeingRevealed = false);
    }
    #endregion

    #region Return revealed cards to the bottom of deck by any order
    public static IEnumerator ReturnRevealedCardsToLibraryBottom(List<CardSource> remainingCards, ICardEffect activateClass)
    {
        if (remainingCards.Count == 0) yield break;
        if (activateClass == null) yield break;
        CardSource effectSourceCard = activateClass.EffectSourceCard;
        if (effectSourceCard == null) yield break;
        Player selectPlayer = effectSourceCard.Owner;
        if (selectPlayer == null) yield break;

        if (remainingCards.Count == 1)
        {
            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(remainingCards));

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(remainingCards, "Deck Bottom Card", true, true));
        }

        else
        {
            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

            selectCardEffect.SetUp(
                canTargetCondition: (cardSource) => true,
                canTargetCondition_ByPreSelecetedList: null,
                canEndSelectCondition: null,
                canNoSelect: () => false,
                selectCardCoroutine: null,
                afterSelectCardCoroutine: AfterSelectCardCoroutine1,
                message: "Specify the order to place the card at the bottom of the deck\n(cards will be placed back to the bottom of the deck so that cards with lower numbers are on top).",
                maxCount: remainingCards.Count,
                canEndNotMax: false,
                isShowOpponent: false,
                mode: SelectCardEffect.Mode.Custom,
                root: SelectCardEffect.Root.Custom,
                customRootCardList: remainingCards,
                canLookReverseCard: true,
                selectPlayer: selectPlayer,
                cardEffect: activateClass);

            selectCardEffect.SetNotShowCard();
            selectCardEffect.SetNotAddLog();
            selectCardEffect.SetIsDeckBottom();
            selectCardEffect.SetUseFaceDown();

            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

            static IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
            {
                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(cardSources));

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(cardSources,
                "Deck Bottom Cards", true, true));
            }
        }
    }
    #endregion

    #region Return revealed cards to the top of deck by any order
    static IEnumerator ReturnRevealedCardsToLibraryTop(List<CardSource> remainingCards, ICardEffect activateClass)
    {
        if (remainingCards.Count == 0) yield break;
        if (activateClass == null) yield break;
        CardSource effectSourceCard = activateClass.EffectSourceCard;
        if (effectSourceCard == null) yield break;
        Player selectPlayer = effectSourceCard.Owner;
        if (selectPlayer == null) yield break;

        if (remainingCards.Count == 1)
        {
            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryTopCards(remainingCards));

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(remainingCards, "Deck Top Card", true, true));
        }

        else
        {
            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

            selectCardEffect.SetUp(
        canTargetCondition: (cardSource) => true,
        canTargetCondition_ByPreSelecetedList: null,
        canEndSelectCondition: null,
        canNoSelect: () => false,
        selectCardCoroutine: null,
        afterSelectCardCoroutine: AfterSelectCardCoroutine1,
        message: "Specify the order to place the card at the top of the deck\n(cards will be placed back to the top of the deck so that cards with lower numbers are on top).",
        maxCount: remainingCards.Count,
        canEndNotMax: false,
        isShowOpponent: false,
        mode: SelectCardEffect.Mode.Custom,
        root: SelectCardEffect.Root.Custom,
        customRootCardList: remainingCards,
        canLookReverseCard: true,
        selectPlayer: selectPlayer,
        cardEffect: activateClass);

            selectCardEffect.SetNotShowCard();
            selectCardEffect.SetNotAddLog();
            selectCardEffect.SetIsDeckTop();
            selectCardEffect.SetUseFaceDown();

            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

            static IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
            {
                List<CardSource> topCards = cardSources.Clone();

                topCards.Reverse();

                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryTopCards(topCards));

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(cardSources, "Deck Top Cards", true, true));
            }
        }
    }
    #endregion

    #region Trash revealed cards
    static IEnumerator TrashRevealedCards(List<CardSource> remainingCards, ICardEffect activateClass)
    {
        if (remainingCards.Count == 0) yield break;
        if (activateClass == null) yield break;
        CardSource effectSourceCard = activateClass.EffectSourceCard;
        if (effectSourceCard == null) yield break;
        Player selectPlayer = effectSourceCard.Owner;
        if (selectPlayer == null) yield break;

        yield return ContinuousController.instance.StartCoroutine(new ITrashDeckCards(remainingCards, activateClass).TrashDeckCards());

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(remainingCards, "Discarded Cards", true, true));
    }
    #endregion

    #region Add revealed cards to hand
    static IEnumerator AddRevealedCardsToHand(List<CardSource> remainingCards, ICardEffect activateClass)
    {
        if (remainingCards.Count == 0) yield break;
        if (activateClass == null) yield break;
        CardSource effectSourceCard = activateClass.EffectSourceCard;
        if (effectSourceCard == null) yield break;
        Player selectPlayer = effectSourceCard.Owner;
        if (selectPlayer == null) yield break;

        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(remainingCards, false, activateClass));

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(remainingCards, "Card added to hand", true, true));
    }
    #endregion

    #region Return revealed cards to the top or bottom of deck by any order
    static IEnumerator ReturnRevealedCardsToLibraryTopOrBottom(List<CardSource> remainingCards, ICardEffect activateClass)
    {
        if (remainingCards.Count == 0) yield break;
        if (activateClass == null) yield break;
        CardSource effectSourceCard = activateClass.EffectSourceCard;
        if (effectSourceCard == null) yield break;
        Player selectPlayer = effectSourceCard.Owner;
        if (selectPlayer == null) yield break;

        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Deck Top", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Deck Bottom", value : false, spriteIndex: 1),
                        };

        string selectPlayerMessage = "To which area do you place cards?";
        string notSelectPlayerMessage = "The opponent is choosing to which area to place cards.";

        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: selectPlayer, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

        bool toTop = GManager.instance.userSelectionManager.SelectedBoolValue;

        if (toTop)
        {
            yield return ContinuousController.instance.StartCoroutine(ReturnRevealedCardsToLibraryTop(remainingCards, activateClass));
        }
        else
        {
            yield return ContinuousController.instance.StartCoroutine(ReturnRevealedCardsToLibraryBottom(remainingCards, activateClass));
        }
    }
    #endregion
}

#region Select conditions class
public class SimplifiedSelectCardConditionClass
{
    public SimplifiedSelectCardConditionClass(Func<CardSource, bool> canTargetCondition,
        string message,
        SelectCardEffect.Mode mode,
        int maxCount,
        Func<CardSource, IEnumerator> selectCardCoroutine)
    {
        _canTargetCondition = canTargetCondition;
        _selectCardCoroutine = selectCardCoroutine;
        _message = message;
        _maxCount = maxCount;
        _mode = mode;
    }
    Func<CardSource, bool> _canTargetCondition = null;
    Func<CardSource, IEnumerator> _selectCardCoroutine = null;
    string _message = null;
    int _maxCount = 0;
    SelectCardEffect.Mode _mode = SelectCardEffect.Mode.Custom;
    public Func<CardSource, bool> CanTargetCondition { get { return _canTargetCondition; } }
    public Func<CardSource, IEnumerator> SelectCardCoroutine { get { return _selectCardCoroutine; } }
    public string Message { get { return _message; } }
    public int MaxCount { get { return _maxCount; } }
    public SelectCardEffect.Mode Mode { get { return _mode; } }
}

public class SelectCardConditionClass
{
    public SelectCardConditionClass(Func<CardSource, bool> canTargetCondition,
        Func<List<CardSource>, CardSource, bool> canTargetCondition_ByPreSelecetedList,
        Func<List<CardSource>, bool> canEndSelectCondition,
        bool canNoSelect,
        Func<CardSource, IEnumerator> selectCardCoroutine,
        string message,
        int maxCount,
        bool canEndNotMax,
        SelectCardEffect.Mode mode)
    {
        _canTargetCondition = canTargetCondition;
        _canTargetCondition_ByPreSelecetedList = canTargetCondition_ByPreSelecetedList;
        _canEndSelectCondition = canEndSelectCondition;
        _canNoSelect = canNoSelect;
        _selectCardCoroutine = selectCardCoroutine;
        _message = message;
        _maxCount = maxCount;
        _canEndNotMax = canEndNotMax;
        _mode = mode;
    }
    Func<CardSource, bool> _canTargetCondition = null;
    Func<List<CardSource>, CardSource, bool> _canTargetCondition_ByPreSelecetedList = null;
    Func<List<CardSource>, bool> _canEndSelectCondition = null;
    bool _canNoSelect = false;
    Func<CardSource, IEnumerator> _selectCardCoroutine = null;
    string _message = null;
    int _maxCount = 0;
    bool _canEndNotMax = false;
    SelectCardEffect.Mode _mode = SelectCardEffect.Mode.Custom;
    public Func<CardSource, bool> CanTargetCondition { get { return _canTargetCondition; } }
    public Func<List<CardSource>, CardSource, bool> CanTargetCondition_ByPreSelecetedList { get { return _canTargetCondition_ByPreSelecetedList; } }
    public Func<List<CardSource>, bool> CanEndSelectCondition { get { return _canEndSelectCondition; } }
    public bool CanNoSelect { get { return _canNoSelect; } }
    public Func<CardSource, IEnumerator> SelectCardCoroutine { get { return _selectCardCoroutine; } }
    public string Message { get { return _message; } }
    public int MaxCount { get { return _maxCount; } }
    public bool CanEndNotMax { get { return _canEndNotMax; } }
    public SelectCardEffect.Mode Mode { get { return _mode; } }
}
#endregion

#region In which area the remaining cards are placed
public enum RemainingCardsPlace
{
    DeckBottom,
    DeckTop,
    DeckTopOrBottom,
    Trash,
    AddHand,
    None,
}
#endregion

#region Reveal deck cards
public class RevealLibraryClass
{
    public RevealLibraryClass(Player player, int revealCount)
    {
        _player = player;
        _revealCount = revealCount;
    }
    Player _player = null;
    int _revealCount = 0;
    List<CardSource> _revealedCards = new List<CardSource>();
    public List<CardSource> RevealedCards => _revealedCards.Clone();

    public IEnumerator RevealLibrary()
    {
        _revealedCards = new List<CardSource>();

        for (int i = 0; i < _revealCount; i++)
        {
            if (_player.LibraryCards.Count > i)
            {
                _revealedCards.Add(_player.LibraryCards[i]);
            }
        }

        if (RevealedCards.Count >= 1)
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(RevealedCards, "Revealed Cards", true, true));
        }

        #region ログ追加
        if (RevealedCards.Count >= 1)
        {
            string log = "";

            log += $"\nRevealed Cards:";

            foreach (CardSource cardSource in RevealedCards)
            {
                if (cardSource != null)
                {
                    log += $"\n{cardSource.BaseENGCardNameFromEntity}({cardSource.CardID})";
                }
            }

            log += "\n";

            PlayLog.OnAddLog?.Invoke(log);
        }
        #endregion

        yield return new WaitForSeconds(0.5f);

        _revealedCards.ForEach(cardSource => cardSource.IsBeingRevealed = true);
    }
}
#endregion