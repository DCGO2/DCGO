using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public partial class CardEffectCommons
{
    public static IEnumerator SelectTrashDigivolutionCards(Func<Permanent, bool> permanentCondition, Func<CardSource, bool> cardCondition, int maxCount, bool canNoTrash, bool isFromOnly1Permanent, ICardEffect activateClass, string selectString = "Digimon", Func<Permanent, List<CardSource>, IEnumerator> afterSelectionCoroutine = null, bool canEndNotMax = false)
    {
        if (maxCount <= 0) yield break;
        if (activateClass == null) yield break;
        CardSource card = activateClass.EffectSourceCard;
        if (card == null) yield break;

        bool CanSelectPermanentCondition(Permanent permanent)
        {
            if (IsPermanentExistsOnBattleArea(permanent))
            {
                if (permanentCondition == null || permanentCondition(permanent))
                {
                    return true;
                }
            }

            return false;
        }

        bool CanSelectCardCondition(CardSource cardSource)
        {
            if (!cardSource.CanNotTrashFromDigivolutionCards(activateClass))
            {
                if (cardCondition == null || cardCondition(cardSource))
                {
                    return true;
                }
            }

            return false;
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

        List<CardSource> selectedCards = new List<CardSource>();

        while (true)
        {
            if (isFromOnly1Permanent)
            {
                if (permanentSelectedCount >= 1)
                {
                    break;
                }
            }

            if (digivolutionDiscardedCount >= maxDigivolutionDiscardCount)
            {
                break;
            }

            if (permanentSum < 1)
            {
                break;
            }
            else
            {
                Permanent selectedPermanent = null;

                permanentSelectedCount++;
                maxCount = Math.Min(1, permanentSum);

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

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

                selectPermanentEffect.SetUpCustomMessage($"Select 1 {selectString} that will trash digivolution cards.", $"The opponent is selecting 1 {selectString} that will trash digivolution cards.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    selectedPermanent = permanent;

                    if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                    {
                        int trashMaxCountOfTargetPermanent = Math.Min(
                            maxDigivolutionDiscardCount - digivolutionDiscardedCount,
                            selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition));

                        

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

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
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: selectedPermanent.DigivolutionCards,
                                    canLookReverseCard: false,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUseFaceDown();

                        selectCardEffect.SetUpCustomMessage("Select digivolution cards to trash.", "The opponent is selecting digivolution cards to trash.");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        bool CanTargetCondition_ByPreSelecetedList(List<CardSource> sources, CardSource source)
                        {
                            return !selectedCards.Contains(source);
                        }

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(new ITrashDigivolutionCards(
                                selectedPermanent,
                                selectedCards,
                                activateClass).TrashDigivolutionCards());

                        digivolutionDiscardedCount = selectedCards.Count;

                        if (canNoTrash && NotSelectYet())
                        {
                            EndSelection();
                        }

                        if (afterSelectionCoroutine != null)
                            yield return ContinuousController.instance.StartCoroutine(afterSelectionCoroutine(selectedPermanent, selectedCards));
                    }
                }

                IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                {
                    if (permanents.Count == 0)
                    {
                        if ((canNoTrash && NotSelectYet()) || canEndNotMax)
                        {
                            EndSelection();
                        }
                    }

                    yield return null;
                }
            }
        }
    }

}
