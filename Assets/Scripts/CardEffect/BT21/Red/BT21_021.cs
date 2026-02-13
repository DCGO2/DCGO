using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//OmniShoutmon
namespace DCGO.CardEffects.BT21
{
    public class BT21_021 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Cost

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.IsDigimon)
                    {
                        if (targetPermanent.TopCard.EqualsCardName("Shoutmon"))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.IsDigimon)
                    {
                        if (targetPermanent.Level == 4)
                        {
                            if (targetPermanent.TopCard.EqualsTraits("Xros Heart") || targetPermanent.TopCard.EqualsTraits("Hero"))
                            {
                                return true;
                            }
                            return true;
                        }
                    }
                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region DigiXros Name

            if (timing == EffectTiming.None)
            {
                ChangeCardNamesForDigiXrosClass changeCardNamesForDigiXrosClass = new ChangeCardNamesForDigiXrosClass();
                changeCardNamesForDigiXrosClass.SetUpICardEffect("Also treated as [Shoutmon] for a DigiXros", CanUseCondition, card);
                changeCardNamesForDigiXrosClass.SetUpChangeCardNamesForDigiXrosClass(changeCardNames: ChangeCardNames);
                cardEffects.Add(changeCardNamesForDigiXrosClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                List<string> ChangeCardNames(CardSource cardSource, List<string> cardNames)
                {
                    if (cardSource == card)
                    {
                        cardNames.Add("Shoutmon");
                    }

                    return cardNames;
                }
            }

            #endregion

            #region DigiXros

            if (timing == EffectTiming.None)
            {
                AddDigiXrosConditionClass addDigiXrosConditionClass = new AddDigiXrosConditionClass();
                addDigiXrosConditionClass.SetUpICardEffect($"DigiXros", CanUseCondition, card);
                addDigiXrosConditionClass.SetUpAddDigiXrosConditionClass(getDigiXrosCondition: GetDigiXros);
                addDigiXrosConditionClass.SetNotShowUI(true);
                cardEffects.Add(addDigiXrosConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                DigiXrosCondition GetDigiXros(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        DigiXrosConditionElement element = new DigiXrosConditionElement(CanSelectCardCondition, "[Xros Heart] Digimon");

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            if (cardSource != null)
                            {
                                if (cardSource.Owner == card.Owner)
                                {
                                    if (cardSource.IsDigimon)
                                    {
                                        if (cardSource.EqualsCardNameDigiXros("Shoutmon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>() { element };

                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, null, 2);

                        return digiXrosCondition;
                    }

                    return null;
                }
            }

            #endregion

            #region On Deletion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 [Xros]/[Blue Flare] digimon under tamer, then <Save>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[On Deletion] You may place 1 Digimon card with the [Xros Heart]/[Blue Flare] trait from your hand or trash under any of your Tamers. Then,<Save>";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card);
                }

                bool CanSelectTamerCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                           && permanent.IsTamer;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.EqualsTraits("Xros Heart") || cardSource.EqualsTraits("Blue Flare"))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectTamerCondition))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition) || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            bool fromHand = false;
                            bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                            bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                            CardSource selectedCard = null;

                            if (canSelectHand || canSelectTrash)
                            {
                                if (canSelectHand && canSelectTrash)
                                {
                                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                                {
                                    new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                                    new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                                };

                                    string selectPlayerMessage = "From which area do you select a card?";
                                    string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                                    
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                                    
                                    fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;
                                }
                                else
                                {
                                    fromHand = canSelectHand;
                                }

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCard = cardSource;
                                    yield return null;
                                }

                                if (fromHand)
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

                                    selectHandEffect.SetUpCustomMessage("Select 1 card to add as source.", "The opponent is selecting 1 card to add as source.");

                                    yield return StartCoroutine(selectHandEffect.Activate());
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
                                        message: "Select 1 card to add as source.",
                                        maxCount: 1,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Trash,
                                        customRootCardList: null,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                                    selectCardEffect.SetUpCustomMessage("Select 1 card to add as source.", "The opponent is selecting 1 card to add as source.");
                                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                                }
                            }

                            if (selectedCard != null)
                            {
                                Permanent selectedPermanent = null;
                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectTamerCondition));

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectTamerCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    selectedPermanent = permanent;
                                    yield return null;
                                }

                                if (selectedPermanent != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass));
                                }
                            }

                            if (CardEffectCommons.CanActivateSave(hashtable, CanSelectTamerCondition))
                            {
                                yield return ContinuousController.instance.StartCoroutine(
                                    CardEffectCommons.SaveProcess(hashtable, activateClass, card, CanSelectTamerCondition));
                            }
                        }
                    }
                }
            }

            #endregion

            #region End of Attack

            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Xros Heart]/[Blue Flare]/[Hero] Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Attack] You may play 1 card with the [Xros Heart]/[Blue Flare]/[Hero] trait from your hand with the play cost reduced by 5. If you did, delete this Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerOnEndAttack(hashtable, card))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon || cardSource.IsTamer)
                    {
                        if (cardSource.EqualsTraits("Xros Heart") || cardSource.EqualsTraits("Blue Flare") || cardSource.EqualsTraits("Hero"))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedCard = null;
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

                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (selectedCard != null)
                    {
                        #region reduce play cost

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect("Play Cost -5", CanUseCondition1, card);
                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
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
                            if (CardSourceCondition(cardSource))
                            {
                                if (RootCondition(root))
                                {
                                    if (PermanentsCondition(targetPermanents))
                                    {
                                        Cost -= 5;
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
                        {
                            if (cardSource != null)
                            {
                                if (cardSource.Owner == card.Owner)
                                {
                                    return CanSelectCardCondition(cardSource);
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

                        bool isPlayed = false;
                        Permanent thisPermanent = card.PermanentOfThisCard();

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                     cardSources: new List<CardSource>() { selectedCard }, 
                                     activateClass: activateClass, 
                                     payCost: true, 
                                     isTapped: false, 
                                     root: SelectCardEffect.Root.Hand,
                                     activateETB: true));

                        #region release reducing play cost

                        card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);

                        #endregion

                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedCard.PermanentOfThisCard())) isPlayed = true;

                        if (isPlayed)
                        {

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: permanent => permanent == thisPermanent,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }
                }
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return card.PermanentOfThisCard().TopCard.EqualsTraits("Xros Heart");
                }

                cardEffects.Add(CardEffectFactory.RushSelfStaticEffect(isInheritedEffect: true, card: card, condition: Condition));
            }

            #endregion

            return cardEffects;
        }
    }
}