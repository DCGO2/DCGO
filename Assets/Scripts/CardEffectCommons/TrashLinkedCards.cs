using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public partial class CardEffectCommons
{
    public static IEnumerator SelectTrashLinkedCards(Func<Permanent, bool> permanentCondition, Func<CardSource, bool> cardCondition, int maxCount, bool canNoTrash, bool isFromOnly1Permanent, ICardEffect activateClass, string selectString = "Digimon", Func<Permanent, List<CardSource>, IEnumerator> afterSelectionCoroutine = null)
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
                    if (!permanent.TopCard.CanNotBeAffected(activateClass))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool CanSelectCardCondition(CardSource cardSource)
        {
            if (cardCondition == null || cardCondition(cardSource))
            {
                return true;
            }

            return false;
        }

        int permanentSum =
            GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                .Map(player => player.GetBattleAreaPermanents().Filter(CanSelectPermanentCondition))
                .Flat().Count();

        int linkedCardsSum =
                GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                .Map(player => player.GetBattleAreaPermanents().Filter(CanSelectPermanentCondition))
                .Flat()
                .Map(permanent => permanent.LinkedCards.Count(CanSelectCardCondition))
                .Sum();

        int maxLinkDiscardCount = Math.Min(linkedCardsSum, maxCount);
        int linkDiscardedCount = 0;

        void EndSelection() => linkDiscardedCount = linkedCardsSum;
        bool NotSelectYet() => linkDiscardedCount == 0;

        int permanentSelectedCount = 0;

        while (true)
        {
            if (isFromOnly1Permanent)
            {
                if (permanentSelectedCount >= 1)
                {
                    break;
                }
            }

            if (linkDiscardedCount >= maxLinkDiscardCount)
            {
                break;
            }

            if (permanentSum < 1)
            {
                break;
            }
            else
            {
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
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage($"Select 1 {selectString} that will trash digivolution cards.", $"The opponent is selecting 1 {selectString} that will trash digivolution cards.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    Permanent selectedPermanent = permanent;

                    if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                    {
                        int trashMaxCountOfTargetPermanent = Math.Min(
                            maxLinkDiscardCount - linkDiscardedCount,
                            selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition));

                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => canNoTrash && NotSelectYet(),
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select linked cards to trash.",
                                    maxCount: trashMaxCountOfTargetPermanent,
                                    canEndNotMax: trashMaxCountOfTargetPermanent >= 2 && !isFromOnly1Permanent,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: selectedPermanent.LinkedCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select linked cards to trash.", "The opponent is selecting linked cards to trash.");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(new ITrashLinkCards(
                                selectedPermanent,
                                selectedCards,
                                activateClass).TrashLinkCards());

                       linkDiscardedCount += selectedCards.Count;

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
                        if (canNoTrash && NotSelectYet())
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
