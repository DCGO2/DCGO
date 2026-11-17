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

        bool CanSelectPermanentCondition(Permanent permanent)
        {
            return IsPermanentExistsOnBattleArea(permanent)
                && (permanentCondition == null
                    || permanentCondition(permanent))
                && (isFromOnly1Permanent
                    || permanent.DigivolutionCards.Any(CanSelectCardCondition));
        }

        bool CanSelectCardCondition(CardSource cardSource)
        {
            return !cardSource.CanNotTrashFromDigivolutionCards(activateClass)
                && (cardCondition == null
                    || cardCondition(cardSource));
        }

        int permanentSum =
            GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                .Map(player => player.GetBattleAreaPermanents().Filter(CanSelectPermanentCondition))
                .Flat().Count();

        int digivolutionCardsSum =
            GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                .Map(player => player.GetBattleAreaPermanents().Filter(CanSelectPermanentCondition))
                .Flat()
                .Map(permanent => permanent.DigivolutionCards.Count(CanSelectCardCondition))
                .Sum();

        int maxDigivolutionDiscardCount = Math.Min(digivolutionCardsSum, maxCount);
        int digivolutionDiscardedCount = 0;

        void EndSelection() => digivolutionDiscardedCount = maxDigivolutionDiscardCount;
        bool NotSelectYet() => digivolutionDiscardedCount == 0;

        int permanentSelectedCount = 0;

        // Store selections by permanent. Nothing is trashed until selection is complete.
        Dictionary<Permanent, List<CardSource>> trashPlan =
            new Dictionary<Permanent, List<CardSource>>();

        while (true)
        {
            if (isFromOnly1Permanent && permanentSelectedCount >= 1) break;

            if (digivolutionDiscardedCount >= maxDigivolutionDiscardCount) break;

            if (permanentSum < 1) break;
            else
            {
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

                    if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                    {
                        int trashMaxCountOfTargetPermanent = Math.Min(
                            maxDigivolutionDiscardCount - digivolutionDiscardedCount,
                            selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition));

                        // Keep this permanent's selections separate from all other permanents.
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
                            canEndNotMax: trashMaxCountOfTargetPermanent >= 2 && !isFromOnly1Permanent,
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

                        // Add the selection to the plan. Do not trash yet.
                        if (selectedCards.Count > 0)
                        {
                            trashPlan[selectedPermanent] = selectedCards;
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
        }

        // Selection is now complete. Execute the entire trash plan.
        foreach (KeyValuePair<Permanent, List<CardSource>> plannedTrash in trashPlan)
        {
            Permanent selectedPermanent = plannedTrash.Key;
            List<CardSource> selectedCards = plannedTrash.Value;

            yield return ContinuousController.instance.StartCoroutine(
                new ITrashDigivolutionCards(
                    selectedPermanent,
                    selectedCards,
                    activateClass).TrashDigivolutionCards());

            if (afterSelectionCoroutine != null)
            {
                yield return ContinuousController.instance.StartCoroutine(
                    afterSelectionCoroutine(selectedPermanent, selectedCards));
            }
        }
    }
}
