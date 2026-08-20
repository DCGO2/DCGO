using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Queen of Thorns
namespace DCGO.CardEffects.BT26
{
    public class BT26_098 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Reduce Use Cost

            bool SharedCanSelectTamerCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.HasFaceDownDigivolutionCards;

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reduce Use Cost -2", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "When this card would be used, by trashing the bottom face-down card from under any of your Tamers, reduce the cost by 2.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, cardSource => cardSource == card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, SharedCanSelectTamerCondition);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, SharedCanSelectTamerCondition))
                    {
                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: SharedCanSelectTamerCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer to trash the bottom face-down card from.", "The opponent is selecting 1 Tamer to trash the bottom face-down card from.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedPermanent != null)
                        {
                            CardSource cardToTrash = selectedPermanent.DigivolutionCards.Last(x => x.IsFaceDown);
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsAndProcessAccordingToResult(
                                targetPermanent: selectedPermanent,
                                targetDigivolutionCards: new List<CardSource>() { cardToTrash },
                                activateClass: activateClass,
                                successProcess: SuccessProcess,
                                failureProcess: null));

                            IEnumerator SuccessProcess(List<CardSource> trashedCards)
                            {
                                ChangeCostClass changeCostClass = new ChangeCostClass();
                                changeCostClass.SetUpICardEffect("Use Cost -2", _ => true, card);
                                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: _ => true, isUpDown: () => true, isCheckAvailability: () => false, isChangePayingCost: () => true);
                                card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                                int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                                    => CardSourceCondition(cardSource) ? cost - 2 : cost;

                                bool CardSourceCondition(CardSource cardSource) => cardSource != null && cardSource == card;
                            }
                        }
                    }
                }
            }

            #endregion

            #region Main

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 1 [Sunflowmon] and 1 [Lilamon] from trash as 1 [Lalamon]'s bottom sources, may digivolve into [Rosemon] without cost or requirements", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] By placing 1 [Sunflowmon] and 1 [Lilamon] from your trash as 1 of your [Lalamon]'s bottom digivolution cards, that Digimon may digivolve into [Rosemon] in the hand, ignoring digivolution requirements and without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool IsLalamon(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsCardName("Lalamon");

                bool IsSunflowmon(CardSource cardSource) => cardSource.EqualsCardName("Sunflowmon");

                bool IsLilamon(CardSource cardSource) => cardSource.EqualsCardName("Lilamon");

                bool IsRosemon(CardSource cardSource) => cardSource.EqualsCardName("Rosemon");

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (!(CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsSunflowmon) && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsLilamon))) yield break;

                    Permanent selectedPermanent = null;

                    if (CardEffectCommons.MatchConditionOwnersPermanentCount(card, IsLalamon) > 1)
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsLalamon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 [Lalamon] to gain digivolution sources.", "The opponent is selecting 1 [Lalamon] to gain digivolution sources.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    if (CardEffectCommons.MatchConditionOwnersPermanentCount(card, IsLalamon) == 1)
                        selectedPermanent = card.Owner.GetBattleAreaDigimons().Find(x => IsLalamon(x));

                    if (selectedPermanent != null)
                    {
                        List<CardSource> selectedTrashCards = new List<CardSource>();

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedTrashCards.Add(cardSource);
                            yield return null;
                        }

                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsSunflowmon))
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: IsSunflowmon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 [Sunflowmon] card.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 [Sunflowmon] card to place under [Lalamon].", "The opponent is selecting a [Sunflowmon] card to place under [Lalamon].");
                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }

                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsLilamon) && selectedTrashCards.Find(IsSunflowmon) != null)
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: IsLilamon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 [Lilamon] card.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 [Lilamon] card to place under [Lalamon].", "The opponent is selecting a [Lilamon] card to place under [Lalamon].");
                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }

                        if (selectedTrashCards.Find(IsSunflowmon) != null && selectedTrashCards.Find(IsLilamon) != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(
                                addedDigivolutionCards: selectedTrashCards,
                                cardEffect: activateClass));

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: selectedPermanent,
                                cardCondition: IsRosemon,
                                payCost: false,
                                reduceCostTuple: null,
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: 1,
                                isHand: true,
                                activateClass: activateClass,
                                successProcess: null));
                        }
                    }
                }
            }

            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Lalamon]/[Yoshino Fujieda] from hand or trash, then add this to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Security] You may play 1 [Lalamon] or [Yoshino Fujieda] from your hand or trash without paying the cost. Then, add this card to the hand.";

                bool CanSelectCardCondition(CardSource cardSource)
                    => (cardSource.EqualsCardName("Lalamon") || cardSource.EqualsCardName("Yoshino Fujieda"))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);

                bool CanUseCondition(Hashtable hashtable) => CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canPlayFromHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                    bool canPlayFromTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    if (canPlayFromHand || canPlayFromTrash)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                        if (canPlayFromHand) selectionElements.Add(new SelectionElement<int>("Play from hand", 1, 0));
                        if (canPlayFromTrash) selectionElements.Add(new SelectionElement<int>("Play from trash", 2, 0));
                        selectionElements.Add(new SelectionElement<int>("Don't play a card", 3, 1));

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: "Will you play a [Lalamon] or [Yoshino Fujieda]?", notSelectPlayerMessage: "The opponent is choosing whether to play a [Lalamon] or [Yoshino Fujieda].");
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool playFromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                        bool dontPlay = GManager.instance.userSelectionManager.SelectedIntValue == 3;

                        if (!dontPlay)
                        {
                            CardSource selectedCard = null;
                            SelectCardEffect.Root selectedRoot = SelectCardEffect.Root.None;

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCard = cardSource;
                                yield return null;
                            }

                            if (playFromHand)
                            {
                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectCardCondition,
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

                                selectHandEffect.SetUpCustomMessage("Select 1 [Lalamon] or [Yoshino Fujieda] to play.", "The opponent is selecting 1 [Lalamon] or [Yoshino Fujieda] to play.");
                                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                                selectedRoot = SelectCardEffect.Root.Hand;
                            }
                            else
                            {
                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 [Lalamon] or [Yoshino Fujieda] to play.",
                                    maxCount: 1,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage("Select 1 [Lalamon] or [Yoshino Fujieda] to play.", "The opponent is selecting 1 [Lalamon] or [Yoshino Fujieda] to play.");
                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                                selectedRoot = SelectCardEffect.Root.Trash;
                            }

                            if (selectedCard != null && selectedRoot != SelectCardEffect.Root.None)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: new List<CardSource>() { selectedCard },
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: selectedRoot,
                                    activateETB: true));
                            }
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
