using System.Collections;
using System.Collections.Generic;

// Strabimon
namespace DCGO.CardEffects.P
{
    public class P_249 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                cardEffects.Add(CardEffectFactory.StartOfYourMainPhaseClass(
                    card,
                    "Place 1 [Hybrid] under this or a Tamer to digivolve placed into [Hybrid] for 2 less",
                    ActivateCoroutine,
                    EffectDescription(),
                    additionalActivateCondition: AdditionalActivateCondition,
                    optional: false,
                    isSkippable: true
                ));

                string EffectDescription()
                    => "[Start of Your Main Phase] By placing 1 [Hybrid] trait card from your hand or trash as this Digimon’s bottom digivolution card or under any of your Tamers with inherited effects, that Digimon or Tamer may digivolve into a [Hybrid] trait card in the hand with the cost reduced by 2.";

                bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                    => CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition)
                        || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.EqualsTraits("Hybrid");

                bool CanSelectTamerPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.TopCard.HasInheritedEffect;

                IEnumerator ActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
                {
                    bool placed = false;
                    CardSource selectedCard = null;
                    Permanent selectedPermanent = null;


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

                    selectHandEffect.SetUpCustomMessage("Select 1 card to place under another card.",
                        "The opponent is selecting 1 card to place under another card.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");

                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (selectedCard != null)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTamerPermanentCondition))
                        {
                            List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                            {
                                new(message: "Under this Digimon", value: 1, spriteIndex: 0),
                                new(message: "Under Tamer with inherited effects", value: 2, spriteIndex: 0),
                                new(message: "Don't place", value: 3, spriteIndex: 1),
                            };

                            GManager.instance.userSelectionManager.SetIntSelection(
                                selectionElements: selectionElements,
                                selectPlayer: card.Owner,
                                selectPlayerMessage: "To which area will you place a card?",
                                notSelectPlayerMessage: "The opponent is choosing to which area to place a card.");

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            if (GManager.instance.userSelectionManager.SelectedIntValue == 3) yield break;
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetInt(1);
                        }

                        if (GManager.instance.userSelectionManager.SelectedIntValue == 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource> { selectedCard }, activateClass));
                            placed = true;
                        }
                        else
                        {
                            SelectPermanentEffect suspendPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            suspendPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectTamerPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: selectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(suspendPermanentEffect.Activate());

                            IEnumerator selectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;

                                yield return null;
                            }

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource> { selectedCard }, activateClass));
                                placed = true;
                            }
                        }

                        if (placed)
                        {
                            Permanent targetPermanent = selectedPermanent ?? card.PermanentOfThisCard();

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: targetPermanent,
                                cardCondition: CanSelectCardCondition,
                                payCost: true,
                                reduceCostTuple: (reduceCost: 2, reduceCostCardCondition: null),
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: true,
                                activateClass: activateClass,
                                successProcess: null
                            ));
                        }
                    }
                }
            }
            #endregion

            #region On Deletion - ESS
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Tamer card from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Deletion] You may play 1 Tamer card with inherited effects from your hand without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsTamer
                        && cardSource.HasInheritedEffect
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                        canTargetCondition: CanSelectCardCondition,
                        SelectCardEffect.Root.Hand,
                        activateClass,
                        payCost: false
                    ));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
