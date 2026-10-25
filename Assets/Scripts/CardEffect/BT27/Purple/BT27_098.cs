using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Akihiro Kurata
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
                activateClass.SetUpICardEffect("By suspending this Tamer, place top 2 deck cards face down under this Tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] When any Digimon are deleted, by suspending this Tamer, you may place the top 2 cards of your deck face down under this Tamer.";

                bool AnyDigimonDeletedCondition(Permanent permanent) => permanent.IsDigimon;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, AnyDigimonDeletedCondition, activateClass);

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
                        if (answer == 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                                    new List<CardSource> { card.Owner.LibraryCards[0], card.Owner.LibraryCards[1] }, activateClass, isFacedown: true));
                        }
                    }
                }
            }
            #endregion

            #region End of All Turns
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
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
