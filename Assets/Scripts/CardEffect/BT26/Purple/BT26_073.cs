using System.Collections;
using System.Collections.Generic;

// Aegiochusmon: Dark
namespace DCGO.CardEffects.BT26
{
    public class BT26_073 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Aegiomon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region Assembly
            if (timing == EffectTiming.None)
            {
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect("Assembly", CanUseCondition, card);
                addAssemblyConditionClass.SetUpAddAssemblyConditionClass(getAssemblyCondition: GetAssembly);
                addAssemblyConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAssemblyConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        AssemblyConditionElement element = new AssemblyConditionElement(CanSelectCardCondition);

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            return cardSource.IsDigimon
                                && cardSource.HasLevel
                                && cardSource.Level <= 4
                                && (cardSource.HasText("Chronomon")
                                    || cardSource.HasTSTraits);
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            element: element,
                            CanTargetCondition_ByPreSelecetedList: null,
                            selectMessage: "Lv.4 or lower w/[Chronomon] in text or w/[TS] trait",
                            elementCount: 1,
                            reduceCost: 2);

                        return assemblyCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Shared OP / WD
            string SharedEffectName = "By deleting this Digimon or returning 1 [Shaman]/[TS] from trash to deck bottom, delete 1 opponent's Lv5 or lower Digimon";

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] By deleting this Digimon or returning 1 [Shaman] or [TS] trait card from your trash to the bottom of the deck, delete 1 of your opponent's level 5 or lower Digimon.";
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Conditions
                bool CanDeleteSelf()
                {
                    return card.PermanentOfThisCard().CanBeDestroyedBySkill(activateClass);
                }

                bool CanReturnCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("Shaman")
                        || cardSource.HasTSTraits;
                }

                bool CanReturnFromTrash()
                {
                    return CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanReturnCardCondition);
                }

                bool CanDeleteCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasLevel
                        && permanent.Level <= 5;
                }
                #endregion

                bool hasPaidCost = false;

                #region Select Cost to Pay
                List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                if (CanDeleteSelf()) selectionElements.Add(new SelectionElement<int>(message: "Delete this Digimon", value: 1, spriteIndex: 0));
                if (CanReturnFromTrash()) selectionElements.Add(new SelectionElement<int>(message: "Return 1 [Shaman]/[TS] from trash to deck bottom", value: 2, spriteIndex: 0));
                selectionElements.Add(new SelectionElement<int>(message: "Don't use effect", value: 3, spriteIndex: 1));

                string selectPlayerMessage = "Choose a cost to pay:";
                string notSelectPlayerMessage = "The opponent is choosing a cost to pay.";

                GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                int chosenOption = GManager.instance.userSelectionManager.SelectedIntValue;
                #endregion

                #region Pay Cost
                if (chosenOption == 1)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        hasPaidCost = true;
                        yield return null;
                    }
                }
                else if (chosenOption == 2)
                {
                    CardSource selectedTrashCard = null;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanReturnCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 [Shaman] or [TS] card to return to deck bottom",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 [Shaman] or [TS] card to return to deck bottom.", "The opponent is selecting 1 [Shaman] or [TS] card to return to deck bottom.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Returned Card");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedTrashCard = cardSource;
                        yield return null;
                    }

                    if (selectedTrashCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(new List<CardSource>() { selectedTrashCard }));

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { selectedTrashCard }, "Deck Bottom Card", true, true));

                        hasPaidCost = true;
                    }
                }
                #endregion

                #region Delete Opponent's Level 5 or Lower
                if (hasPaidCost && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanDeleteCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanDeleteCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
                #endregion
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

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [TS] trait card with play cost 5 or less from hand or trash without paying cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Deletion] You may play 1 [TS] trait card with a play cost of 5 or less from your hand or trash without paying the cost.";
                }

                bool CanPlayCondition(CardSource cardSource)
                {
                    return cardSource.HasTSTraits
                        && cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 5
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card, activateClass)
                        && (CardEffectCommons.HasMatchConditionOwnersHand(card, CanPlayCondition)
                            || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanPlayCondition));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanPlayCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanPlayCondition);

                    if (canSelectHand || canSelectTrash)
                    {
                        #region Location Selection
                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<int>> selectionElements1 = new List<SelectionElement<int>>()
                            {
                                new (message: $"From hand", value : 1, spriteIndex: 0),
                                new (message: $"From trash", value : 2, spriteIndex: 0),
                                new (message: $"Don't play", value: 3, spriteIndex: 1)
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

                        SelectCardEffect.Root root = GManager.instance.userSelectionManager.SelectedIntValue == 1 ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                        bool doSelect = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                        #endregion

                        if (doSelect)
                        {
                            #region Hand/Trash Card Selection & Play
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                                canTargetCondition: CanPlayCondition,
                                root,
                                activateClass,
                                payCost: false
                            ));
                            #endregion
                        }
                    }
                }
            }
            #endregion

            #region Inherited Effect
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
