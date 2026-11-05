using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Makoto Kuonji
namespace DCGO.CardEffects.BT27
{
    public class BT27_097 : CEntity_Effect
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
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By suspending this Tamer, place top 2 deck cards face down under this Tamer, then may use 1 purple [BEATBREAK] Option", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] When a Digimon attacks, by suspending this Tamer, you may place the top 2 cards of your deck face down under this Tamer. Then, if it's your turn, by trashing 4 bottom face-down cards from under any of your Tamers, you may use 1 purple [BEATBREAK] trait Option card from your trash with the use cost reduced by 3.";

                const int TrashCount = 4;
                const int ReduceCost = 3;

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
                        && cardSource.HasBeatBreakTraits
                        && cardSource.HasOptionColor(CardColor.Purple)
                        && !cardSource.CanNotPlayThisOption
                        && cardSource.Owner.MaxMemoryCost >= cardSource.GetCostItself - ReduceCost;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, permanent => permanent.IsDigimon);

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

                    GManager.instance.userSelectionManager.SetIntSelection(
                        selectionElements: selectionElements,
                        selectPlayer: card.Owner,
                        selectPlayerMessage: "Will you suspend this tamer to place the top 2 cards from your deck under this Tamer face down?",
                        notSelectPlayerMessage: "The opponent is choosing whether to place the top 2 cards from their deck under their Tamer face down.");

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    int answer = GManager.instance.userSelectionManager.SelectedIntValue;

                    if (answer == 3) yield break;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                        new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass,
                        SuccessProcess,
                        null));

                    IEnumerator SuccessProcess(List<Permanent> suspendedPermaments)
                    {
                        // Take(2) so a deck with fewer than 2 cards doesn't throw.
                        List<CardSource> topCards = card.Owner.LibraryCards.Take(2).ToList();

                        if (answer == 1 && topCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(topCards, activateClass, isFacedown: true));
                        }

                        if (!CardEffectCommons.IsOwnerTurn(card)
                            || FaceDownCountUnderTamers() < TrashCount
                            || !CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectOptionCondition)) yield break;

                        #region Cost - Trash 4 bottom face-down cards from under any of your Tamers
                        List<SelectionElement<bool>> useElements = new List<SelectionElement<bool>>()
                        {
                            new(message: "Trash 4 and use an Option", value: true, spriteIndex: 0),
                            new(message: "Don't trash", value: false, spriteIndex: 1),
                        };

                        GManager.instance.userSelectionManager.SetBoolSelection(
                            selectionElements: useElements,
                            selectPlayer: card.Owner,
                            selectPlayerMessage: "Will you trash 4 face-down cards from under your Tamers to use an Option card?",
                            notSelectPlayerMessage: "The opponent is choosing whether to trash face-down cards to use an Option card.");

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        if (!GManager.instance.userSelectionManager.SelectedBoolValue) yield break;

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
                                    canNoSelect: false,
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
                                List<SelectionElement<int>> countElements = new List<SelectionElement<int>>();

                                for (int i = maxTrashCount; i >= minTrashCount; i--)
                                {
                                    countElements.Add(new(message: $"Trash {i}", value: i, spriteIndex: 0));
                                }

                                GManager.instance.userSelectionManager.SetIntSelection(
                                    selectionElements: countElements,
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

                        #region May use 1 purple [BEATBREAK] Option from trash with the cost reduced by 3
                        if (!CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectOptionCondition)) yield break;

                        CardSource selectedCard = null;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectOptionCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 Option card to use.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 Option card to use.", "The opponent is selecting 1 Option card to use.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Used Card");

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        if (selectedCard != null)
                        {
                            ChangeCostClass changeCostClass = new ChangeCostClass();
                            changeCostClass.SetUpICardEffect($"Use Cost -{ReduceCost}", _ => true, card);
                            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: PlayCondition, rootCondition: _ => true, isUpDown: () => true, isCheckAvailability: () => false, isChangePayingCost: () => true);
                            Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                            card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                            ICardEffect GetCardEffect(EffectTiming _timing)
                                => _timing == EffectTiming.None ? changeCostClass : null;

                            bool PlayCondition(CardSource cardSource) => cardSource == selectedCard;

                            int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                            {
                                if (PlayCondition(cardSource))
                                {
                                    cost -= ReduceCost;
                                }

                                return cost;
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                                cardSources: new List<CardSource>() { selectedCard },
                                activateClass: activateClass,
                                payCost: true,
                                root: SelectCardEffect.Root.Trash));

                            card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);
                        }
                        #endregion
                    }
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
