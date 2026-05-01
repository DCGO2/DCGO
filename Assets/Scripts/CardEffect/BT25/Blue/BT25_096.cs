using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Mirage Beast Knight
namespace DCGO.CardEffects.BT25
{
    public class BT25_096 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Reduce Play Cost

            bool SharedCanSelectTamerCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.HasFaceDownDigivolutionCards;

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reduce Play Cost -2", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetHashString("PlayCost-2_BT21_093");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "When this card would be used, by trashing the bottom face-down card from under any of your Tamers, reduce the use cost by 2.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, cardSource => cardSource == card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, SharedCanSelectTamerCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, SharedCanSelectTamerCondition))
                    {
                        Permanent selectedPermanent = null;

                        #region Select Tamer to trash bottom face down digivolution card
                        if (CardEffectCommons.MatchConditionOwnersPermanentCount(card, SharedCanSelectTamerCondition) > 1)
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, SharedCanSelectTamerCondition));

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: SharedCanSelectTamerCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
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

                            selectPermanentEffect.SetUpCustomMessage("Select 1 tamer to trash bottom face down digivolution card.", "The opponent is selecting 1 tamer to trash bottom face down digivolution card.");
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        }
                        if (CardEffectCommons.MatchConditionOwnersPermanentCount(card, SharedCanSelectTamerCondition) == 1)
                            selectedPermanent = card.Owner.GetBattleAreaPermanents().FirstOrDefault(permanent => SharedCanSelectTamerCondition(permanent));
                        #endregion

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
                                if (card.Owner.CanReduceCost(null, card)) ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                                ChangeCostClass changeCostClass = new ChangeCostClass();
                                changeCostClass.SetUpICardEffect("Play Cost -2", CanUseCondition1, card);
                                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                                card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                                bool CanUseCondition1(Hashtable hashtable) => true;

                                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                                {
                                    if (CardSourceCondition(cardSource))
                                    {
                                        if (RootCondition(root))
                                        {
                                            if (PermanentsCondition(targetPermanents))
                                            {
                                                Cost -= 2;
                                            }
                                        }
                                    }

                                    return Cost;
                                }

                                bool PermanentsCondition(List<Permanent> targetPermanents)
                                {
                                    if (targetPermanents == null)
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                bool CardSourceCondition(CardSource cardSource)
                                    => cardSource != null
                                        && cardSource == card;

                                bool RootCondition(SelectCardEffect.Root root) => true;

                                bool isUpDown() => true;
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -2", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => true, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnHand(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, SharedCanSelectTamerCondition);
                }

                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource))
                    {
                        if (RootCondition(root))
                        {
                            Cost -= 2;
                        }
                    }

                    return Cost;
                }

                bool CardSourceCondition(CardSource cardSource)
                    => cardSource != null && cardSource == card;

                bool RootCondition(SelectCardEffect.Root root) => true;

                bool isUpDown() => true;
            }

            #endregion

            #region Main

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 1 [Gaogamon] and 1 [MachGaogamon] from trash as 1 [Gaomon] bottom sources, may digivolve into [MirageGaogamon] without cost or digivole requirements", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] By placing 1 [Gaogamon] and 1 [MachGaogamon] from your trash as 1 of your [Gaomon]'s bottom digivolution cards, that Digimon may digivolve into [MirageGaogamon] in the hand, ignoring digivolution requirements and without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    #region Conditions

                    bool IsGaomon(Permanent permanent)
                        => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                            && permanent.TopCard.EqualsCardName("Gaomon");

                    bool IsGaogamon(CardSource cardSource)
                        => cardSource.EqualsCardName("Gaogamon");

                    bool IsMachGaogamon(CardSource cardSource)
                        => cardSource.EqualsCardName("MachGaogamon");

                    bool isMirageGaogamon(CardSource cardSource)
                        => cardSource.EqualsCardName("MirageGaogamon")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);

                    #endregion

                    if (!(CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsGaogamon) && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsMachGaogamon))) yield break;
                    Permanent selectedPermanent = null;

                    #region Select Gaomon
                    if (CardEffectCommons.MatchConditionOwnersPermanentCount(card, IsGaomon) > 1)
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, IsGaomon));

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsGaomon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
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

                        selectPermanentEffect.SetUpCustomMessage("Select 1 [Gaomon] to gain digivolution sources", "The opponent is selecting 1 [Gaomon] to gain digivolution sources.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                    if (CardEffectCommons.MatchConditionOwnersPermanentCount(card, IsGaomon) == 1)
                        selectedPermanent = card.Owner.GetBattleAreaDigimons().Find(IsGaomon);
                    #endregion

                    if (selectedPermanent != null)
                    {
                        List<CardSource> selectedTrashCards = new List<CardSource>();

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedTrashCards.Add(cardSource);
                            yield return null;
                        }

                        #region Select Gaogamon
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsGaogamon))
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, IsGaogamon));

                            selectCardEffect.SetUp(
                                canTargetCondition: IsGaogamon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 [Gaogamon] card",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 [Gaogamon] card to place under [Gaomon].", "The opponent is selecting a [Gaogamon] card to place under [Gaomon].");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Selected [Gaogamon]");
                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }
                        #endregion

                        #region Select MachGaogamon
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsMachGaogamon) && selectedTrashCards.Find(IsGaogamon))
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, IsMachGaogamon));

                            selectCardEffect.SetUp(
                                canTargetCondition: IsMachGaogamon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 [MachGaogamon] card",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 [MachGaogamon] card to place under [Gaomon].", "The opponent is selecting a [MachGaogamon] card to place under [Gaomon].");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Selected [MachGaogamon]");
                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }
                        #endregion

                        if (selectedTrashCards.Find(IsGaogamon) && selectedTrashCards.Find(IsMachGaogamon))
                        {
                            yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(
                                addedDigivolutionCards: selectedTrashCards,
                                cardEffect: activateClass));

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: selectedPermanent,
                                cardCondition: isMirageGaogamon,
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
                activateClass.SetUpICardEffect($"Play 1 [Gaomon]/[Thomas H. Norstein] from hand or trash, then add this to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Security] You may play 1 [Gaomon] or [Thomas H. Norstein] from your hand or trash without paying the cost. Then, add this card to the hand.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return (cardSource.EqualsCardName("Gaomon") || cardSource.EqualsCardName("Thomas H. Norstein"))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

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
                        selectionElements.Add(new SelectionElement<int>("Dont play a card", 3, 1));


                        string selectPlayerMessage = "Will you play a [Gaomon] or [Thomas H. Norstein]?";
                        string notSelectPlayerMessage = "The opponent is choosing whether to play a [Gaomon] or [Thomas H. Norstein]";


                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
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
                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, CanSelectCardCondition));
                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectHandEffect.SetUpCustomMessage("Select 1 [Gaomon] or [Thomas H. Norstein] to play.", "The opponent is selecting 1 [Gaomon] or [Thomas H. Norstein] to play.");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");
                                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                                selectedRoot = SelectCardEffect.Root.Hand;
                            }
                            else
                            {
                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, CanSelectCardCondition));
                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                            canTargetCondition: CanSelectCardCondition,
                                            canTargetCondition_ByPreSelecetedList: null,
                                            canEndSelectCondition: null,
                                            canNoSelect: () => true,
                                            selectCardCoroutine: SelectCardCoroutine,
                                            afterSelectCardCoroutine: null,
                                            message: "Select 1 [Gaomon] or [Thomas H. Norstein] to play",
                                            maxCount: maxCount,
                                            canEndNotMax: false,
                                            isShowOpponent: true,
                                            mode: SelectCardEffect.Mode.Custom,
                                            root: SelectCardEffect.Root.Trash,
                                            customRootCardList: null,
                                            canLookReverseCard: true,
                                            selectPlayer: card.Owner,
                                            cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage("Select 1 [Gaomon] or [Thomas H. Norstein] to play.", "The opponent is selecting 1 [Gaomon] or [Thomas H. Norstein] to play.");
                                selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");
                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                                selectedRoot = SelectCardEffect.Root.Trash;
                            }

                            if (selectedCard != null && selectedRoot != SelectCardEffect.Root.None)
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: new List<CardSource>() { selectedCard },
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: selectedRoot,
                                    activateETB: true
                                ));



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
