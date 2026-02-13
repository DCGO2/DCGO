using System.Collections;
using System.Collections.Generic;

// Nanimon
namespace DCGO.CardEffects.EX9
{
    public class EX9_028 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution condition

            if (timing == EffectTiming.None)
            {
                bool Condition(Permanent permanent)
                {
                    return permanent.TopCard.IsLevel3 && permanent.TopCard.EqualsTraits("DM");
                }
                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(Condition, 2, false, card, null));
            }

            #endregion

            #region End of Your Turn

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By Placing 3 [Ver.4] digimon FD as this digimon bottom sources, digivolve into a [Ver.4] digimon in hand or trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Digivolve_EX9-028");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Your Turn] [Once Per Turn] By placing 3 Digimon cards with the [Ver.4] trait from your trash face down as this Digimon's bottom digivolution cards, it may digivolve into a Digimon card with the [Ver.4] trait in the hand or trash.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) && 
                           CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.MatchConditionOwnersCardCountInTrash(card,CanSelectCardCondition) >= 3;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && 
                           cardSource.EqualsTraits("Ver.4");
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    if (cardSource.EqualsTraits("Ver.4"))
                    {
                        if (cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, false, activateClass))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        message: "Select 3 cards to add as source",
                        maxCount: 3,
                        canEndNotMax: true,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 3 cards to add as source.", "The opponent is selecting 3 cards to add as source.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Selected Cards");
                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        selectedCards.AddRange(cardSources);
                        yield return null;
                    }

                    if (selectedCards.Count == 3)
                    {
                        foreach (var selectedCard in selectedCards)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddExecutingCard(selectedCard));
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass, isFacedown: true));
                        }

                        bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition1);
                        bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition1);

                        if (canSelectHand || canSelectTrash)
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


                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                    targetPermanent: card.PermanentOfThisCard(),
                                    cardCondition: CanSelectCardCondition1,
                                    payCost: true,
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

            #endregion

            #region ESS

            if (timing == EffectTiming.None)
            {
                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.Owner == card.Owner.Enemy;
                }

                bool Condition()
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

                cardEffects.Add(CardEffectFactory.ChangeSecurityDigimonCardDPStaticEffect(
                    cardCondition: CardCondition,
                    changeValue: -3000,
                    isInheritedEffect: true,
                    card: card,
                    condition: Condition,
                    effectName: "Opponent's Security Digimon gains DP -3000"));
            }

            #endregion

            return cardEffects;
        }
    }
}