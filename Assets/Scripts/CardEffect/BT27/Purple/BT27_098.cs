using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

<<<<<<< Updated upstream
// Akihiro Kurata — BT27-098
=======
// Akihiro Kurata
>>>>>>> Stashed changes
namespace DCGO.CardEffects.BT27
{
    public class BT27_098 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region All Turns
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
<<<<<<< Updated upstream
                activateClass.SetUpICardEffect("Suspend: place top 2 deck cards face down under this Tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    "[All Turns] When any Digimon are deleted, by suspending this Tamer, you may place the top 2 cards of your deck face down under this Tamer.");
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, p => p.IsDigimon, activateClass);
=======
                activateClass.SetUpICardEffect("By suspending this Tamer, place top 2 deck cards face down under this Tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] When any Digimon are deleted, by suspending this Tamer, you may place the top 2 cards of your deck face down under this Tamer.";

                bool AnyDigimonDeletedCondition(Permanent permanent) => permanent.IsDigimon;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, AnyDigimonDeletedCondition, activateClass);
>>>>>>> Stashed changes

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);

<<<<<<< Updated upstream
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent> { card.PermanentOfThisCard() },
                            activateClass: activateClass,
                            successProcess: SuccessProcess,
                            failureProcess: null));

                    IEnumerator SuccessProcess(List<Permanent> suspendedPermanents)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>
                        {
                            new(message: $"Suspend and place 2 cards from deck under this card", value: 1, spriteIndex: 0),
                            new(message: $"Only suspend without placing cards", value: 2, spriteIndex: 0),
                            new(message: $"No", value: 3, spriteIndex: 1)
                        };

                        string selectPlayerMessage = "Will you suspend this tamer to place the top 2 cards from your deck under this Tamer face down?";
                        string notSelectPlayerMessage = "The opponent is choosing whether to place the top 2 cards from their deck under their Tamer face down.";

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        int answer = GManager.instance.userSelectionManager.SelectedIntValue;

                        if (answer != 3)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                                new List<Permanent>() { card.PermanentOfThisCard() },
                                activateClass,
                                SuccessProcess,
                                null));
                        }

                        IEnumerator SuccessProcess(List<Permanent> suspendedPermaments)
                        {
                            if (answer == 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                                        new List<CardSource> { card.Owner.LibraryCards[0], card.Owner.LibraryCards[1] }, activateClass, isFacedown: true));
                            }
=======
                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass: activateClass,
                        successProcess: SuccessProcess,
                        failureProcess: null));

                    IEnumerator SuccessProcess(List<Permanent> _permanents)
                    {
                        List<CardSource> topCards = card.Owner.LibraryCards.Take(2).ToList();

                        if (topCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(topCards, activateClass, isFacedown: true));
>>>>>>> Stashed changes
                        }
                    }
                }
            }
            #endregion

            #region End of All Turns
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
<<<<<<< Updated upstream
                activateClass.SetUpICardEffect("Trash 5 Tamer sources to play [Gizmon]/[Belphemon] in name from hand/trash for free, then place this Tamer under it", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[End of All Turns] By trashing 5 bottom face-down cards from under any of your Tamers, you may play 1 Digimon card with [Gizmon] or [Belphemon] in its name from your hand or trash without paying the cost. After, place this Tamer as its bottom digivolution card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    var faceDownSourceCount = card.Owner.GetBattleAreaPermanents()
                        .Filter(TamerWithOneOrMoreFaceDownSource)
                        .Sum(permanent => permanent.DigivolutionCards.Count(CanSelectTrashSourceCardCondition));

                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && faceDownSourceCount >= 5;
                }

                bool TamerWithOneOrMoreFaceDownSource(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.DigivolutionCards.Any(CanSelectTrashSourceCardCondition);
                }

                bool CanSelectTrashSourceCardCondition(CardSource cardSource)
                {
                    return cardSource.IsFaceDown;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasPlayCost
                        && (cardSource.ContainsCardName("Gizmon")
                            || cardSource.ContainsCardName("Belphemon"))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool trashed = false;
                    const int targetTrashAmount = 5;

                    SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                    int maxCount1 = Math.Min(targetTrashAmount, CardEffectCommons.MatchConditionPermanentCount(TamerWithOneOrMoreFaceDownSource));

                    selectPermanentEffect1.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: TamerWithOneOrMoreFaceDownSource,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: CanEndSelectCondition,
                        maxCount: maxCount1,
                        canNoSelect: true,
                        canEndNotMax: true,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect1.SetUpCustomMessage(
                        $"Select Tamer(s) to trash bottom face-down cards from (Total: {targetTrashAmount})",
                        $"The opponent is selecting Tamer(s) to trash bottom face-down cards from (Total: {targetTrashAmount})"
                    );

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                    bool CanEndSelectCondition(List<Permanent> permanents)
                    {
                        if (permanents.Count > 0)
                        {
                            int faceDownCount = 0;
                            foreach (Permanent permanent in permanents)
                            {
                                faceDownCount += permanent.DigivolutionCards.Count(CanSelectTrashSourceCardCondition);
                            }

                            return faceDownCount >= targetTrashAmount;
                        }

                        return false;
                    }

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        if (permanents == null || permanents.Count == 0) yield break;

                        int remainingTrashCount = targetTrashAmount;
                        List<Permanent> validTargets = new List<Permanent>(permanents);

                        while (remainingTrashCount > 0 && validTargets.Count > 0)
                        {
                            if (validTargets.Count == 1)
                            {
                                Permanent finalTarget = validTargets[0];
                                int availableCards = finalTarget.DigivolutionCards.Count(CanSelectTrashSourceCardCondition);
                                int trashThisAmount = Math.Min(remainingTrashCount, availableCards);

                                if (trashThisAmount > 0)
                                {
                                    trashed = true;

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                                        targetPermanent: finalTarget,
                                        trashCount: trashThisAmount,
                                        isFromTop: false,
                                        activateClass: activateClass,
                                        CanSelectTrashSourceCardCondition
                                    ));

                                    remainingTrashCount -= trashThisAmount;
                                }

                                break;
                            }

                            bool allTargetsHaveExactlyOneLeft = validTargets.All(p => p.DigivolutionCards.Count(CanSelectTrashSourceCardCondition) == 1);

                            if (allTargetsHaveExactlyOneLeft)
                            {
                                foreach (Permanent target in new List<Permanent>(validTargets))
                                {
                                    if (remainingTrashCount <= 0) break;

                                    trashed = true;

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                                        targetPermanent: target,
                                        trashCount: 1,
                                        isFromTop: false,
                                        activateClass: activateClass,
                                        CanSelectTrashSourceCardCondition
                                    ));

                                    remainingTrashCount--;
                                }

                                break;
                            }

                            Permanent selectedTarget = null;
                            SelectPermanentEffect selectPermanentEffectLoop = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffectLoop.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: permanent => validTargets.Contains(permanent) && permanent.DigivolutionCards.Count(CanSelectTrashSourceCardCondition) > 0,
=======
                activateClass.SetUpICardEffect("Trash 5 face-down cards from under your Tamers to play [Gizmon]/[Belphemon] in name from hand or trash for free, then place this Tamer under it", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[End of All Turns] By trashing 5 bottom face-down cards from under any of your Tamers, you may play 1 Digimon card with [Gizmon] or [Belphemon] in its name from your hand or trash without paying the cost. After, place this Tamer as its bottom digivolution card.";

                const int TrashCount = 5;

                bool FaceDownCards(CardSource cardSource) => cardSource.IsFaceDown;

                bool TamerWithFaceDownSource(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.DigivolutionCards.Any(FaceDownCards)
                        && !permanent.ImmuneFromStackTrashing(activateClass);

                int FaceDownCountUnderTamers()
                    => card.Owner.GetBattleAreaPermanents()
                        .Where(TamerWithFaceDownSource)
                        .Sum(permanent => permanent.DigivolutionCards.Count(FaceDownCards));

                bool PlayCardCondition(CardSource cardSource)
                    => cardSource.IsDigimon
                        && (cardSource.ContainsCardName("Gizmon") || cardSource.ContainsCardName("Belphemon"));

                bool CanPlayFromHandCondition(CardSource cardSource)
                    => PlayCardCondition(cardSource)
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, SelectCardEffect.Root.Hand);

                bool CanPlayFromTrashCondition(CardSource cardSource)
                    => PlayCardCondition(cardSource)
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, SelectCardEffect.Root.Trash);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && FaceDownCountUnderTamers() >= TrashCount;

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    #region Cost - Trash 5 bottom face-down cards from under any of your Tamers
                    // Each step picks 1 Tamer and how many to trash from it. Every selection finishes before the next one starts,
                    // so no selection is opened from inside another selection's callback.
                    int remainingTrashCount = TrashCount;

                    while (remainingTrashCount > 0 && FaceDownCountUnderTamers() >= remainingTrashCount)
                    {
                        Permanent selectedTamer = null;

                        if (CardEffectCommons.MatchConditionPermanentCount(TamerWithFaceDownSource) == 1)
                        {
                            selectedTamer = card.Owner.GetBattleAreaPermanents().Find(TamerWithFaceDownSource);
                        }
                        else
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: TamerWithFaceDownSource,
>>>>>>> Stashed changes
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
<<<<<<< Updated upstream
                                selectPermanentCoroutine: (p) => { selectedTarget = p; return null; },
=======
                                selectPermanentCoroutine: SelectPermanentCoroutine,
>>>>>>> Stashed changes
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

<<<<<<< Updated upstream
                            selectPermanentEffectLoop.SetUpCustomMessage(
                                $"Select a Tamer to trash up to {remainingTrashCount} cards from\n(You can choose another Tamer after if max is not chosen)",
                                "The opponent is selecting a Tamer to trash cards from"
                            );

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffectLoop.Activate());

                            if (selectedTarget != null)
                            {
                                int availableCards = selectedTarget.DigivolutionCards.Count(CanSelectTrashSourceCardCondition);

                                int maxSelectableNumber = Math.Min(remainingTrashCount, availableCards);
                                int chosenTrashAmount = 1;

                                if (maxSelectableNumber > 1)
                                {
                                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                                    for (int i = 1; i <= maxSelectableNumber; i++)
                                        selectionElements.Add(new SelectionElement<int>(message: $"{i}", value: i, spriteIndex: 0));

                                    string selectPlayerMessage = "Choose how many cards to trash?";
                                    string notSelectPlayerMessage = "The opponent is choosing how many cards to trash.";

                                    if (selectionElements.Count > 1)
                                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                                    else
                                        GManager.instance.userSelectionManager.SetInt(selectionElements[0].Value);

                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                                    chosenTrashAmount = GManager.instance.userSelectionManager.SelectedIntValue;
                                }
                                else
                                {
                                    chosenTrashAmount = maxSelectableNumber;
                                }

                                if (chosenTrashAmount > 0)
                                {
                                    trashed = true;
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                                        targetPermanent: selectedTarget,
                                        trashCount: chosenTrashAmount,
                                        isFromTop: false,
                                        activateClass: activateClass,
                                        CanSelectTrashSourceCardCondition
                                    ));

                                    remainingTrashCount -= chosenTrashAmount;

                                    if (selectedTarget.DigivolutionCards.Count(CanSelectTrashSourceCardCondition) == 0)
                                    {
                                        validTargets.Remove(selectedTarget);
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        yield return null;
                    }

                    if (trashed)
                    {
                        bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                        bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                        if (canSelectHand || canSelectTrash)
                        {
                            SelectCardEffect.Root root;

                            if (canSelectHand && canSelectTrash)
                            {
                                List<SelectionElement<int>> selectionElements1 = new List<SelectionElement<int>>()
                            {
                                new(message: "From hand", value: 1, spriteIndex: 0),
                                new(message: "From trash", value: 2, spriteIndex: 0),
                                new(message: "Don't play", value: 3, spriteIndex: 1),
                            };

                                GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: "From which area will you play a card?", notSelectPlayerMessage: "The opponent is choosing from which area to select a card.");
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                                if (GManager.instance.userSelectionManager.SelectedIntValue == 3) yield break;

                                root = GManager.instance.userSelectionManager.SelectedIntValue == 1 ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                            }
                            else
                            {
                                root = canSelectHand ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                                canTargetCondition: CanSelectCardCondition,
                                root: root,
                                cardEffect: activateClass,
                                payCost: false,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine
                            ));

                            IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                            {
                                if (cardSources.Count == 1)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(new List<Permanent[]>() { new Permanent[] { card.PermanentOfThisCard(), cardSources[0].PermanentOfThisCard() } }, false, activateClass).PlacePermanentToDigivolutionCards());
                                }

                                yield return null;
                            }
                        }
                    }
=======
                            selectPermanentEffect.SetUpCustomMessage(
                                $"Select 1 Tamer to trash bottom face-down cards from ({remainingTrashCount} left).",
                                "The opponent is selecting 1 Tamer to trash bottom face-down cards from.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedTamer = permanent;
                                yield return null;
                            }
                        }

                        if (selectedTamer == null) break;

                        int availableCount = selectedTamer.DigivolutionCards.Count(FaceDownCards);
                        int otherTamersCount = FaceDownCountUnderTamers() - availableCount;

                        // Never let the player take so few that the other Tamers can't cover the rest.
                        int maxTrashCount = Math.Min(remainingTrashCount, availableCount);
                        int minTrashCount = Math.Max(1, remainingTrashCount - otherTamersCount);
                        int trashCount = maxTrashCount;

                        if (maxTrashCount > minTrashCount)
                        {
                            List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                            for (int i = maxTrashCount; i >= minTrashCount; i--)
                            {
                                selectionElements.Add(new(message: $"Trash {i}", value: i, spriteIndex: 0));
                            }

                            GManager.instance.userSelectionManager.SetIntSelection(
                                selectionElements: selectionElements,
                                selectPlayer: card.Owner,
                                selectPlayerMessage: "How many face-down cards will you trash from this Tamer?",
                                notSelectPlayerMessage: "The opponent is choosing how many face-down cards to trash.");

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            trashCount = GManager.instance.userSelectionManager.SelectedIntValue;
                        }

                        int countBefore = selectedTamer.DigivolutionCards.Count(FaceDownCards);

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                            targetPermanent: selectedTamer,
                            trashCount: trashCount,
                            isFromTop: false,
                            activateClass: activateClass,
                            cardCondition: FaceDownCards));

                        int trashedCount = countBefore - selectedTamer.DigivolutionCards.Count(FaceDownCards);

                        if (trashedCount <= 0) break;

                        remainingTrashCount -= trashedCount;
                    }

                    if (remainingTrashCount > 0) yield break;
                    #endregion

                    #region Play 1 [Gizmon]/[Belphemon] from hand or trash
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanPlayFromHandCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanPlayFromTrashCondition);

                    if (!canSelectHand && !canSelectTrash) yield break;

                    bool isHand = canSelectHand;

                    if (canSelectHand && canSelectTrash)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new(message: "From hand", value: 1, spriteIndex: 0),
                            new(message: "From trash", value: 2, spriteIndex: 0),
                            new(message: "Don't play", value: 3, spriteIndex: 1),
                        };

                        GManager.instance.userSelectionManager.SetIntSelection(
                            selectionElements: selectionElements,
                            selectPlayer: card.Owner,
                            selectPlayerMessage: "From which area will you play a card?",
                            notSelectPlayerMessage: "The opponent is choosing from which area to select a card.");

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedIntValue == 3) yield break;

                        isHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                    }

                    CardSource selectedCard = null;

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (isHand)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanPlayFromHandCondition,
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

                        selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                    }
                    else
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanPlayFromTrashCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to play.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }

                    if (selectedCard == null) yield break;

                    // Play first, then place: PlayByEffect's afterSelectCardCoroutine runs before the card is on the field.
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: new List<CardSource> { selectedCard },
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: isHand ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash,
                        activateETB: true));
                    #endregion

                    #region Place this Tamer as its bottom digivolution card
                    Permanent playedPermanent = selectedCard.PermanentOfThisCard();
                    Permanent thisTamer = card.PermanentOfThisCard();

                    if (playedPermanent != null && thisTamer != null && playedPermanent.TopCard == selectedCard)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(
                            new List<Permanent[]>() { new Permanent[] { thisTamer, playedPermanent } },
                            false,
                            activateClass).PlacePermanentToDigivolutionCards());
                    }
                    #endregion
>>>>>>> Stashed changes
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
