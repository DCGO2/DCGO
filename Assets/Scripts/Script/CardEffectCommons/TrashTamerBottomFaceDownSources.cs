using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class CardEffectCommons
{
    public static IEnumerator TrashTamerBottomFaceDownSources(
        CardSource card,
        ICardEffect cardEffect,
        int totalTrashCount = 0,
        IEnumerator successProcess = null,
        IEnumerator failureProcess = null)
    {
        // Collect the whole plan first and trash only once all are chosen, so backing out never leaves a partial payment.
        Dictionary<Permanent, int> trashPlan = new Dictionary<Permanent, int>();

        bool FaceDownCards(CardSource cardSource) => cardSource.IsFaceDown;

        bool TamerWithFaceDownSource(Permanent permanent)
            => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                && permanent.DigivolutionCards.Any(FaceDownCards)
                && !permanent.ImmuneFromStackTrashing(cardEffect);

        int PlannedCount(Permanent permanent)
            => trashPlan.TryGetValue(permanent, out int count) ? count : 0;

        int UnplannedFaceDownCount(Permanent permanent)
            => permanent.DigivolutionCards.Count(FaceDownCards) - PlannedCount(permanent);

        bool TamerWithUnplannedFaceDownSource(Permanent permanent)
            => TamerWithFaceDownSource(permanent)
                && UnplannedFaceDownCount(permanent) > 0;

        int UnplannedFaceDownCountUnderTamers()
            => card.Owner.GetBattleAreaPermanents()
                .Where(TamerWithFaceDownSource)
                .Sum(UnplannedFaceDownCount);

        bool useOPT = false;

        if (totalTrashCount <= 2)
        {
            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
            int maxCount = Math.Min(totalTrashCount, CardEffectCommons.MatchConditionPermanentCount(TamerWithFaceDownSource));

            selectPermanentEffect.SetUp(
                selectPlayer: card.Owner,
                canTargetCondition: TamerWithFaceDownSource,
                canTargetCondition_ByPreSelecetedList: null,
                canEndSelectCondition: CanEndSelectCondition,
                maxCount: maxCount,
                canNoSelect: true,
                canEndNotMax: true,
                selectPermanentCoroutine: null,
                afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                mode: SelectPermanentEffect.Mode.Custom,
                cardEffect: cardEffect);

            selectPermanentEffect.SetUpCustomMessage("Select all Tamer(s) to trash bottom face-down cards from.", "The opponent is selecting Tamer(s) to trash bottom face-down cards from.");

            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

            bool CanEndSelectCondition(List<Permanent> permanents)
            {
                return permanents.Count == totalTrashCount
                    || (permanents.Count > 0 && permanents[0].DigivolutionCards.Count(FaceDownCards) >= totalTrashCount);
            }

            IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
            {
                if (permanents.Count == 1)
                {
                    useOPT = true;
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanents[0], trashCount: totalTrashCount, isFromTop: false, activateClass: cardEffect, cardCondition: FaceDownCards));
                }
                else if (permanents.Count == totalTrashCount)
                {
                    useOPT = true;
                    foreach (Permanent selectedPermanent in permanents)
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedPermanent, trashCount: 1, isFromTop: false, activateClass: cardEffect, cardCondition: FaceDownCards));
                }
            }
        }
        else
        {
            int remainingTrashCount = totalTrashCount;

            while (remainingTrashCount > 0 && UnplannedFaceDownCountUnderTamers() >= remainingTrashCount)
            {
                Permanent selectedTamer = null;

                if (CardEffectCommons.MatchConditionPermanentCount(TamerWithUnplannedFaceDownSource) == 1)
                {
                    selectedTamer = card.Owner.GetBattleAreaPermanents().Find(TamerWithUnplannedFaceDownSource);
                }
                else
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: TamerWithUnplannedFaceDownSource,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: cardEffect);

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

                //Backing out cancels the whole payment, nothing has been trashed yet
                if (selectedTamer != null)
                {
                    int availableCount = UnplannedFaceDownCount(selectedTamer);
                    int otherTamersCount = UnplannedFaceDownCountUnderTamers() - availableCount;

                    // Never let the player take so few that the other Tamers can't cover the rest.
                    int maxTrashCount = Math.Min(remainingTrashCount, availableCount);
                    int minTrashCount = Math.Max(1, remainingTrashCount - otherTamersCount);
                    int trashCount = maxTrashCount;

                    if (maxTrashCount > minTrashCount)
                    {
                        List<SelectionElement<int>> countElements = new List<SelectionElement<int>>();

                        for (int i = maxTrashCount; i >= minTrashCount; i--)
                        {
                            countElements.Add(new(message: $"Trash {i}", value: i, spriteIndex: 0));
                        }

                        countElements.Add(new(message: "Don't trash", value: 0, spriteIndex: 1));

                        GManager.instance.userSelectionManager.SetIntSelection(
                            selectionElements: countElements,
                            selectPlayer: card.Owner,
                            selectPlayerMessage: "How many face-down cards will you trash from this Tamer?",
                            notSelectPlayerMessage: "The opponent is choosing how many face-down cards to trash.");

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        trashCount = GManager.instance.userSelectionManager.SelectedIntValue;
                    }

                    if (trashCount > 0)
                    {
                        trashPlan[selectedTamer] = PlannedCount(selectedTamer) + trashCount;
                        remainingTrashCount -= trashCount;
                    }
                }

                if (remainingTrashCount == 0)
                {
                    useOPT = true;

                    foreach (KeyValuePair<Permanent, int> plannedTrash in trashPlan)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                            targetPermanent: plannedTrash.Key,
                            trashCount: plannedTrash.Value,
                            isFromTop: false,
                            activateClass: cardEffect,
                            cardCondition: FaceDownCards));
                    }
                }
            }
        }

        if (successProcess != null && useOPT) yield return ContinuousController.instance.StartCoroutine(successProcess);

        if (failureProcess != null && !useOPT) yield return ContinuousController.instance.StartCoroutine(failureProcess);
    }
}
