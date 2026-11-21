using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public partial class CardEffectCommons
{
    public static IEnumerator SelectTrashDigivolutionCards(
        Func<Permanent, bool> permanentCondition,
        Func<CardSource, bool> cardCondition,
        int maxCount,
        bool canNoTrash,
        bool isFromOnly1Permanent,
        ICardEffect activateClass,
        string selectString = "Digimon",
        Func<Permanent, List<CardSource>, IEnumerator> afterSelectionCoroutine = null,
        bool canEndNotMax = false)
    {
        if (maxCount <= 0) yield break;
        if (activateClass == null) yield break;

        CardSource card = activateClass.EffectSourceCard;
        if (card == null) yield break;

        // Store selections by permanent.
        Dictionary<Permanent, List<CardSource>> trashPlan =
            new Dictionary<Permanent, List<CardSource>>();

        // Preserve the order in which permanents were first selected.
        List<Permanent> trashPlanOrder = new List<Permanent>();

        bool IsAlreadyPlanned(CardSource cardSource)
        {
            return trashPlan.Values.Any(cards => cards.Contains(cardSource));
        }

        bool CanSelectCardCondition(CardSource cardSource)
        {
            return !IsAlreadyPlanned(cardSource)
                && !cardSource.CanNotTrashFromDigivolutionCards(activateClass)
                && (cardCondition == null
                    || cardCondition(cardSource));
        }

        bool CanSelectPermanentCondition(Permanent permanent)
        {
            return IsPermanentExistsOnBattleArea(permanent)
                && (permanentCondition == null
                    || permanentCondition(permanent))
                && (isFromOnly1Permanent
                    || permanent.DigivolutionCards.Any(CanSelectCardCondition));
        }

        int digivolutionCardsSum =
            GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                .Map(player => player.GetBattleAreaPermanents())
                .Flat()
                .Map(permanent => permanent.DigivolutionCards.Count(cardSource =>
                    !cardSource.CanNotTrashFromDigivolutionCards(activateClass)
                    && (cardCondition == null || cardCondition(cardSource))))
                .Sum();

        int maxDigivolutionDiscardCount = Math.Min(
            digivolutionCardsSum,
            maxCount);

        int digivolutionDiscardedCount = 0;

        void EndSelection()
        {
            digivolutionDiscardedCount = maxDigivolutionDiscardCount;
        }

        bool NotSelectYet()
        {
            return digivolutionDiscardedCount == 0;
        }

        int permanentSelectedCount = 0;

        while (true)
        {
            if (isFromOnly1Permanent && permanentSelectedCount >= 1)
                break;

            if (digivolutionDiscardedCount >= maxDigivolutionDiscardCount)
                break;

            // Recalculate available permanents because previously planned
            // cards are no longer eligible for selection.
            int permanentSum =
                GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                    .Map(player => player.GetBattleAreaPermanents()
                        .Filter(CanSelectPermanentCondition))
                    .Flat()
                    .Count();

            if (permanentSum < 1)
                break;

            Permanent selectedPermanent = null;

            permanentSelectedCount++;
            maxCount = Math.Min(1, permanentSum);

            SelectPermanentEffect selectPermanentEffect =
                GManager.instance.GetComponent<SelectPermanentEffect>();

            selectPermanentEffect.SetUp(
                selectPlayer: card.Owner,
                canTargetCondition: CanSelectPermanentCondition,
                canTargetCondition_ByPreSelecetedList: null,
                canEndSelectCondition: null,
                maxCount: maxCount,
                canNoSelect: canNoTrash && NotSelectYet(),
                canEndNotMax: canEndNotMax,
                selectPermanentCoroutine: SelectPermanentCoroutine,
                afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                mode: SelectPermanentEffect.Mode.Custom,
                cardEffect: activateClass);

            selectPermanentEffect.SetUpCustomMessage(
                $"Select 1 {selectString} that will trash digivolution cards.",
                $"The opponent is selecting 1 {selectString} that will trash digivolution cards.");

            yield return ContinuousController.instance.StartCoroutine(
                selectPermanentEffect.Activate());

            IEnumerator SelectPermanentCoroutine(Permanent permanent)
            {
                selectedPermanent = permanent;

                int availableCardCount =
                    selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition);

                if (availableCardCount >= 1)
                {
                    int trashMaxCountOfTargetPermanent = Math.Min(
                        maxDigivolutionDiscardCount - digivolutionDiscardedCount,
                        availableCardCount);

                    // Collect this selection separately before adding it
                    // to the overall trash plan.
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect =
                        GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                        canEndSelectCondition: null,
                        canNoSelect: () => canNoTrash && NotSelectYet(),
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select digivolution cards to trash.",
                        maxCount: trashMaxCountOfTargetPermanent,
                        canEndNotMax: trashMaxCountOfTargetPermanent >= 2
                            && !isFromOnly1Permanent,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.DigivolutionCards,
                        customRootCardList: selectedPermanent.DigivolutionCards,
                        canLookReverseCard: false,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUseFaceDown();

                    selectCardEffect.SetUpCustomMessage(
                        "Select digivolution cards to trash.",
                        "The opponent is selecting digivolution cards to trash.");

                    yield return ContinuousController.instance.StartCoroutine(
                        selectCardEffect.Activate());

                    bool CanTargetCondition_ByPreSelecetedList(
                        List<CardSource> sources,
                        CardSource source)
                    {
                        return !selectedCards.Contains(source);
                    }

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);
                        yield return null;
                    }

                    // Add the new selections to the existing plan.
                    if (selectedCards.Count > 0)
                    {
                        if (!trashPlan.ContainsKey(selectedPermanent))
                        {
                            trashPlan[selectedPermanent] = new List<CardSource>();
                            trashPlanOrder.Add(selectedPermanent);
                        }

                        trashPlan[selectedPermanent].AddRange(selectedCards);

                        digivolutionDiscardedCount += selectedCards.Count;
                    }

                    if (canNoTrash && NotSelectYet())
                    {
                        EndSelection();
                    }
                }
            }

            IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
            {
                if (permanents.Count == 0
                    && ((canNoTrash && NotSelectYet())
                        || canEndNotMax))
                {
                    EndSelection();
                }

                yield return null;
            }
        }

        // All selections are complete. Execute the trash plan.
        foreach (Permanent selectedPermanent in trashPlanOrder)
        {
            List<CardSource> selectedCards = trashPlan[selectedPermanent];

            yield return ContinuousController.instance.StartCoroutine(
                new ITrashDigivolutionCards(
                    selectedPermanent,
                    selectedCards,
                    activateClass).TrashDigivolutionCards());

            // Run the callback after this permanent's selected cards
            // have been trashed.
            if (afterSelectionCoroutine != null)
            {
                yield return ContinuousController.instance.StartCoroutine(
                    afterSelectionCoroutine(selectedPermanent, selectedCards));
            }
        }
    }
}