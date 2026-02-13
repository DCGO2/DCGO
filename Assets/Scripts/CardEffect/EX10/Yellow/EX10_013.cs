using System;
using System.Collections;
using System.Collections.Generic;

// Lucemon
namespace DCGO.CardEffects.EX10
{
    public class EX10_013 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel2 && targetPermanent.TopCard.EqualsCardName("Cupimon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 5, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Blocker

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Move digimon to battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Breeding] [When Digivolving] This Digimon may move.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBreedingAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBreedingAreaDigimon(card)
                        && card.PermanentOfThisCard().CanMove;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.CanMove)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.MovePermanent(card.Owner.GetBreedingAreaPermanents()[0].PermanentFrame));
                    }
                }
            }

            #endregion

            #region End of your turn

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By returning 5 [lucemon] in text cards from trash to bottom of deck, digivolve into [Lucemon: Chaos Mode] in trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Your Turn] By returning 5 cards with [Lucemon] in their texts from your trash to the bottom of the deck, this Digimon may digivolve into [Lucemon: Chaos Mode] in the trash without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsLucemon)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsChaosMode);
                }

                bool IsLucemon(CardSource cardSource)
                {
                    return cardSource.HasText("Lucemon");
                }

                bool IsChaosMode(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Lucemon: Chaos Mode")
                        && cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsLucemon))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        #region Select 5 [Lucemon] in text cards from trash to bottom of deck

                        int maxCount = Math.Min(5, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, IsLucemon));

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: IsLucemon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 5 [Lucemon] in text cards to bottom deck",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        selectCardEffect.SetUpCustomMessage("Select 5 [Lucemon] in text cards to bottom deck", "Your opponent is selecting 5 [Lucemon] in text cards to bottom deck");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Selected Cards");
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        #endregion

                        if (selectedCards.Count == 5)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ReturnRevealedCardsToLibraryBottom(
                                remainingCards: selectedCards,
                                activateClass: activateClass));

                            if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsChaosMode)) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                    targetPermanent: card.PermanentOfThisCard(),
                                    cardCondition: IsChaosMode,
                                    payCost: false,
                                    reduceCostTuple: null,
                                    fixedCostTuple: null,
                                    ignoreDigivolutionRequirementFixedCost: -1,
                                    isHand: false,
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
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: true, card: card, condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}
