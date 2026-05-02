using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// LadyDevimon
namespace DCGO.CardEffects.BT25
{
    public class BT25_083 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.HasTSTraits || permanent.TopCard.HasText("Three Musketeers");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, false, card, null, level: 4));
            }
            #endregion

            #region OP/WD Shared

            string SharedEffectName = "By place 1 [Three Musketeers] trait card from hand or trash as any digimon bottom digivolution card, draw 1";

            string SharedEffectDescription(string tag) => $"[{tag}] By placing 1 [Three Musketeers] trait card from your hand or trash as any of your Digimon's bottom digivolution cards, <Draw 1>";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool CanSelectCardCondition(CardSource cardSource) => cardSource.HasThreeMusketeersTraits;
                bool CanSelectDigimonCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectDigimonCondition) && (canSelectHand || canSelectTrash))
                {

                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                    if (canSelectHand) selectionElements.Add(new SelectionElement<int>("Hand", 1, 0));
                    if (canSelectTrash) selectionElements.Add(new SelectionElement<int>("Trash", 2, 0));
                    selectionElements.Add(new SelectionElement<int>("Dont take one", 3, 1));

                    string selectPlayerMessage = "Which place will you take a [Three Musketeers] trait card from?";
                    string notSelectPlayerMessage = "The opponent is choosing an area to take a [Three Musketeers] trait card from.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                    bool dontTake = GManager.instance.userSelectionManager.SelectedIntValue == 3;
                    bool fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;

                    if (!dontTake)
                    {
                        #region Select 3M card to add digivolution source
                        CardSource selectedCard = null;
                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        if (fromHand)
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, CanSelectCardCondition));

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

                            selectHandEffect.SetUpCustomMessage("Select 1 card to add to digivolution sources.", "The opponent is selecting 1 card to add to digivolution sources.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                            yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                        }
                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, CanSelectCardCondition));

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to add to digivolution sources",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }
                        #endregion

                        if (selectedCard != null && CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectDigimonCondition))
                        {
                            #region Select digimon to add digivolution source

                            Permanent selectedPermanent = null;
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, CanSelectDigimonCondition));

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectDigimonCondition,
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

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to gain digivolution source.", "The opponent is selecting 1 Digimon to gain digivolution source.");
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());


                            #endregion

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass));
                                yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                            }
                        }
                    }

                }

            }

            CardEffectFactory.ActivateClassesForSharedEffects
                    (ref cardEffects, timing, card,
                        SharedEffectName,
                        SharedActivateCoroutine,
                        SharedEffectDescription,
                        optional: false,
                        isSkippable: true,
                        onPlay: true,
                        whenDigivolving: true);

            #endregion

            #region WD/WA Shared

            string SharedEffectName1 = "By trashing 1 option card from any digimon digivolution cards, use 1 [Three Musketeers] trait option card in trash for 3 reduced cost";

            string SharedHashString = "BT25_083_WD_WA";

            string SharedEffectDescription1(string tag) => $"[{tag}] [Once Per Turn] By trashing 1 Option card from any of your Digimon's digivolution cards, you may use 1 [Three Musketeers] trait Option card from your trash with the cost reduced by 3.";

            bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass) => CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectDigimonCondition);

            bool CanSelectDigimonCondition(Permanent permanent) =>
                CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                && permanent.DigivolutionCards.Exists(CanSelectOptionCard);

            bool CanSelectOptionCard(CardSource cardSource) => cardSource.IsOption;

            IEnumerator SharedActivateCoroutine1(Hashtable hashtable, ActivateClass activateClass)
            {
                bool hasUsed = false;
                bool CanSelect3MOptionCard(CardSource cardSource) => cardSource.IsOption && cardSource.HasThreeMusketeersTraits;
                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectDigimonCondition))
                {
                    #region Select digimon to trash option card from digivolution source

                    Permanent selectedPermanent = null;
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, CanSelectDigimonCondition));

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectDigimonCondition,
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

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to trash 1 option in sources", "The opponent is selecting 1 Digimon to trash 1 option in sources");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());


                    #endregion

                    if (selectedPermanent != null)
                    {
                        #region Select option card to trash from digivolution source
                        CardSource selectedCard = null;
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectOptionCard,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "1 option digivolution card to trash.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.DigivolutionCards,
                            customRootCardList: selectedPermanent.DigivolutionCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        selectCardEffect.SetUpCustomMessage("Select 1 option digivolution card to trash.", "The opponent is selecting 1 option digivolution card to trash.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Trashed Card");
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        #endregion

                        if (selectedCard != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new ITrashDigivolutionCards(selectedPermanent, new List<CardSource>() { selectedCard }, activateClass).TrashDigivolutionCards());
                            #region Select 3M option card in trash to use with reduced cost
                            CardSource selectedOption = null;
                            SelectCardEffect selectCardEffect1 = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect1.SetUp(
                                canTargetCondition: CanSelect3MOptionCard,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine1,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 [Three Musketeers] option to use.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            IEnumerator SelectCardCoroutine1(CardSource cardSource)
                            {
                                selectedOption = cardSource;
                                yield return null;
                            }

                            selectCardEffect1.SetUpCustomMessage("Select 1 [Three Musketeers] option to use.", "The opponent is selecting 1 [Three Musketeers] option to use.");
                            selectCardEffect1.SetUpCustomMessage_ShowCard("Selected Card");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect1.Activate());

                            if (selectedOption != null)
                            {
                                hasUsed = true;

                                #region reduce play cost
                                ChangeCostClass changeCostClass = new ChangeCostClass();
                                changeCostClass.SetUpICardEffect($"Play Cost -3", CanUseCondition1, card);
                                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CanSelect3MOptionCard, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                                Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                                card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                                ICardEffect GetCardEffect(EffectTiming _timing)
                                {
                                    if (_timing == EffectTiming.None)
                                    {
                                        return changeCostClass;
                                    }

                                    return null;
                                }

                                bool CanUseCondition1(Hashtable hashtable)
                                {
                                    return true;
                                }

                                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                                {
                                    if (CanSelect3MOptionCard(cardSource))
                                    {
                                        if (RootCondition(root))
                                        {
                                            if (PermanentsCondition(targetPermanents))
                                            {
                                                Cost -= 3;
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

                                bool RootCondition(SelectCardEffect.Root root)
                                {
                                    return true;
                                }

                                bool isUpDown()
                                {
                                    return true;
                                }
                                #endregion

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                                    cardSources: new List<CardSource>() { selectedOption },
                                    activateClass: activateClass,
                                    payCost: true,
                                    root: SelectCardEffect.Root.Trash));
                            }

                            #endregion

                        }
                    }
                }
                if (!hasUsed) activateClass.RemoveUse();
            }

            CardEffectFactory.ActivateClassesForSharedEffects
                    (ref cardEffects, timing, card,
                        SharedEffectName1,
                        SharedActivateCoroutine1,
                        SharedEffectDescription1,
                        maxCountPerTurn: 1,
                        hashValue: SharedHashString,
                        optional: false,
                        isSkippable: true,
                        whenDigivolving: true,
                        whenAttacking: true,
                        additionalActivateCondition: AdditionalActivateCondition);

            #endregion

            #region Inherited
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 level 4 or lower [Three Musketeers] Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("BT25_083_OD");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "You may play 1 level 4 or lower Digimon card with [Three Musketeers] in its text from your trash without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletionInherited(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.HasLevel &&
                        cardSource.Level <= 4 &&
                        cardSource.HasText("Three Musketeers")
                        && cardSource.IsDigimon;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 level 4 or lower [Three Musketeers] digimon to play.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.PlayForFree,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 level 4 or lower [Three Musketeers] digimon to play.", "The opponent is selecting 1 level 4 or lower [Three Musketeers] digimon to play.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                }
            }
            #endregion

            return cardEffects;
        }

    }
}