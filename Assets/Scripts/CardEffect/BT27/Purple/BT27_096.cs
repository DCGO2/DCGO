using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Keenan Crier
namespace DCGO.CardEffects.BT27
{
    public class BT27_096 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Turn
            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnDiscardHand)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By suspending this Tamer, place top 2 deck cards face down under this Tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] When hands are trashed from, by suspending this Tamer, you may place the top 2 cards of your deck face down under this Tamer.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnTrashHand(hashtable, null, cardSource => true);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
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
                        // Take(2) so a deck with fewer than 2 cards doesn't throw.
                        List<CardSource> topCards = card.Owner.LibraryCards.Take(2).ToList();

                        if (answer == 1 && topCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(topCards, activateClass, isFacedown: true));
                        }
                    }
                }
            }
            #endregion

            #region Main - OPT
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 5 face-down cards from under your Tamers to use 1 [Ravemon] text Option from trash for free", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT27_096_Main");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] [Once Per Turn] By trashing 5 bottom face-down cards from under any of your Tamers, you may use 1 Option card with [Ravemon] in its text from your trash without paying the cost.";

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

                bool CanSelectOptionCondition(CardSource cardSource)
                    => cardSource.IsOption
                        && cardSource.HasText("Ravemon")
                        && !cardSource.CanNotPlayThisOption;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && FaceDownCountUnderTamers() >= TrashCount
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectOptionCondition);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    #region Cost - Trash 5 bottom face-down cards from under any of your Tamers
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
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: remainingTrashCount == TrashCount,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

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

                    if (remainingTrashCount > 0)
                    {
                        activateClass.RemoveUse();
                        yield break;
                    }
                    #endregion

                    #region May use 1 [Ravemon] text Option from trash
                    if (!CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectOptionCondition)
                        || remainingTrashCount != 0) yield break;

                    CardSource selectedCard = null;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectOptionCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 Option card with [Ravemon] in its text to use.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 Option card with [Ravemon] in its text to use.", "The opponent is selecting 1 Option card to use.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Used Card");

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    if (selectedCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                            cardSources: new List<CardSource>() { selectedCard },
                            activateClass: activateClass,
                            payCost: false,
                            root: SelectCardEffect.Root.Trash));
                    }
                    #endregion
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
