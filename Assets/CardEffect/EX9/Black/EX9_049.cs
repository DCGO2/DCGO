using System.Collections;
using System.Collections.Generic;

// Sukamon
namespace DCGO.CardEffects.EX9
{
    public class EX9_049 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Cost Reduction

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("DM") && targetPermanent.TopCard.IsLevel3;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition,
                    digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region End of Your Turn

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 3 Ver3 digimon flipped in source, digivolve into a ver3", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Digivolve~EX9_049");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Your Turn] [Once Per Turn] By placing 3 Digimon cards with the [Ver.3] trait from your trash face down as this Digimon's bottom digivolution cards, it may digivolve into a Digimon card with the [Ver.3] trait in the hand or trash.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.EqualsTraits("Ver.3"))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    if (cardSource.EqualsTraits("Ver.3"))
                    {
                        if (cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, false, activateClass))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanSelectPermanentHandCondition(Permanent permanent)
                {
                    foreach (CardSource cardSource in card.Owner.HandCards)
                    {
                        if (cardSource.EqualsTraits("Ver.3"))
                        {
                            if (cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, false, activateClass))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentTrashCondition(Permanent permanent)
                {
                    foreach (CardSource cardSource in card.Owner.TrashCards)
                    {
                        if (cardSource.EqualsTraits("Ver.3"))
                        {
                            if (cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, false, activateClass))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool ValidDigivolveIntoHand(CardSource source)
                {
                    return source.EqualsTraits("Ver.3");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 3 Ver.3 digimon to add as bottom digivolution card",
                            maxCount: 3,
                            canEndNotMax: true,
                            isShowOpponent: false,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: card.Owner.TrashCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass
                        );

                        selectCardEffect.SetUpCustomMessage("Select 3 cards to add as bottom digivolution card.", "The opponent is selecting 3 cards to add as bottom digivolution card.");
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        if (selectedCards.Count == 3)
                        {
                            foreach (var selectedCard in selectedCards)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddExecutingCard(selectedCard));
                                selectedCard.SetReverse();
                                yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass));
                            }

                            bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition1);
                            bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition1);

                            if (canSelectHand || canSelectTrash)
                            {
                                List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                                {
                                    new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                                    new SelectionElement<bool>(message: $"No ", value : false, spriteIndex: 1),
                                };

                                string selectPlayerMessage = "Will you digivolve?";
                                string notSelectPlayerMessage = "The opponent is choosing to digivolve";
                                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                                bool isDigivolving = GManager.instance.userSelectionManager.SelectedBoolValue;

                                if (isDigivolving)
                                {
                                    if (canSelectHand && canSelectTrash)
                                    {
                                        List<SelectionElement<bool>> selectionElements1 = new List<SelectionElement<bool>>()
                                        {
                                            new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                                            new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                                        };

                                        string selectPlayerMessage1 = "From which area do you select a card?";
                                        string notSelectPlayerMessage1 = "The opponent is choosing from which area to select a card.";

                                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage1, notSelectPlayerMessage: notSelectPlayerMessage1);
                                    }
                                    else
                                    {
                                        GManager.instance.userSelectionManager.SetBool(canSelectHand);
                                    }

                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                                    bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;
                                    CardSource selectedCard = null;

                                    IEnumerator SelectCardCoroutine1(CardSource cardSource)
                                    {
                                        selectedCard = cardSource;
                                        yield return null;
                                    }

                                    if (fromHand)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                            targetPermanent: card.PermanentOfThisCard(),
                                            cardCondition: ValidDigivolveIntoHand,
                                            payCost: false,
                                            reduceCostTuple: null,
                                            fixedCostTuple: null,
                                            ignoreDigivolutionRequirementFixedCost: -1,
                                            isHand: fromHand,
                                            activateClass: activateClass,
                                            successProcess: null));
                                    }
                                    else
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                            targetPermanent: card.PermanentOfThisCard(),
                                            cardCondition: ValidDigivolveIntoHand,
                                            payCost: false,
                                            reduceCostTuple: null,
                                            fixedCostTuple: null,
                                            ignoreDigivolutionRequirementFixedCost: -1,
                                            isHand: fromHand,
                                            activateClass: activateClass,
                                            successProcess: null));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Blocker - ESS

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: true, card: card, condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}