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
                activateClass.SetUpICardEffect("Reduce Use Cost -2", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
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
                        if (CardEffectCommons.HasMatchConditionPermanent(SharedCanSelectTamerCondition))
                        {
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

                            selectPermanentEffect.SetUpCustomMessage("Select 1 tamer to trash bottom face down digivolution card.", "The opponent is selecting 1 tamer to trash bottom face down digivolution card.");
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
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
                                changeCostClass.SetUpICardEffect("Play Use -2", CanUseCondition1, card);
                                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                                card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                                bool CanUseCondition1(Hashtable hashtable) => true;

                                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                                {
                                    if (CardSourceCondition(cardSource)
                                    && RootCondition(root)
                                    && PermanentsCondition(targetPermanents))
                                    {
                                        Cost -= 2;
                                    }

                                    return Cost;
                                }

                                bool PermanentsCondition(List<Permanent> targetPermanents)
                                {
                                    return targetPermanents == null
                                            || targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0;
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
                changeCostClass.SetUpICardEffect("Play Use -2", CanUseCondition, card);
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
                    if (CardSourceCondition(cardSource)
                    && RootCondition(root))
                    {
                        Cost -= 2;
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
                activateClass.SetUpICardEffect("By placing 1 [Gaogamon] and 1 [MachGaogamon] from trash as 1 [Gaomon]'s bottom sources, may digivolve into [MirageGaogamon] without cost or requirements", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] By placing 1 [Gaogamon] and 1 [MachGaogamon] from your trash as 1 of your [Gaomon]'s bottom digivolution cards, that Digimon may digivolve into [MirageGaogamon] in the hand, ignoring digivolution requirements and without paying the cost.";

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

                    bool IsMirageGaogamon(CardSource cardSource)
                        => cardSource.EqualsCardName("MirageGaogamon");

                    #endregion

                    if (CardEffectCommons.HasMatchConditionPermanent(IsGaomon)
                    && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsGaogamon)
                    && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsMachGaogamon))
                    {
                        Permanent selectedPermanent = null;

                        #region Select Gaomon
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsGaomon,
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

                        selectPermanentEffect.SetUpCustomMessage("Select 1 [Gaomon] to gain digivolution sources", "The opponent is selecting 1 [Gaomon] to gain digivolution sources.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
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
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: IsGaogamon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 [Gaogamon] card",
                                maxCount: 1,
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
                            #endregion

                            #region Select MachGaogamon
                            SelectCardEffect selectCardEffect1 = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect1.SetUp(
                                canTargetCondition: IsMachGaogamon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 [MachGaogamon] card",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect1.SetUpCustomMessage("Select 1 [MachGaogamon] card to place under [Gaomon].", "The opponent is selecting a [MachGaogamon] card to place under [Gaomon].");
                            selectCardEffect1.SetUpCustomMessage_ShowCard("Selected [MachGaogamon]");
                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect1.Activate());
                            #endregion

                            if (selectedTrashCards.Count == 2)
                            {
                                List<CardSource> digivolutionCards = new List<CardSource>();

                                SelectCardEffect selectCardEffect2 = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect2.SetUp(
                                    canTargetCondition: (cardSource) => true,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                    message: "Specify the order to place the cards in the digivolution cards\n(cards will be placed so that cards with lower numbers are on top).",
                                    maxCount: selectedTrashCards.Count,
                                    canEndNotMax: false,
                                    isShowOpponent: false,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: selectedTrashCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect2.SetUpCustomMessage_ShowCard("Digivolution Cards");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect2.Activate());

                                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                                {
                                    digivolutionCards = cardSources.Clone();

                                    yield return null;
                                }

                                yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(digivolutionCards, activateClass));

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                    targetPermanent: selectedPermanent,
                                    cardCondition: IsMirageGaogamon,
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
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Gaomon]/[Thomas H. Norstein] from hand or trash, then add this to hand", CanUseCondition, card);
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
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    if (canSelectHand || canSelectTrash)
                    {
                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<int>> selectionElements1 = new List<SelectionElement<int>>()
                            {
                                new (message: $"From hand", value : 1, spriteIndex: 0),
                                new (message: $"From trash", value : 2, spriteIndex: 1),
                                new (message: $"Don't play", value: 3, spriteIndex: 2)
                            };

                            string selectPlayerMessage1 = "From which area will you play a card?";
                            string notSelectPlayerMessage1 = "The opponent is choosing from which area to select a card.";

                            GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage1, notSelectPlayerMessage: notSelectPlayerMessage1);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetInt(canSelectHand ? 1 : 2);
                        }
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool doPlay = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                        SelectCardEffect.Root root = GManager.instance.userSelectionManager.SelectedIntValue == 1 ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;

                        if (doPlay)
                        {
                            #region Hand/Trash Card Selection & Play
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                                canTargetCondition: CanSelectCardCondition,
                                root,
                                activateClass,
                                payCost: false
                            ));
                            #endregion
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
