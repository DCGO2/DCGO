using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Copipemon
namespace DCGO.CardEffects.EX10
{
    public class EX10_038 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel2 && targetPermanent.TopCard.HasAppmonTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Link

            if (timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.LinkEffect(card));
            }

            #endregion

            #region Link Condition

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasAppmonTraits;
                }
                cardEffects.Add(CardEffectFactory.AddSelfLinkConditionStaticEffect(permanentCondition: PermanentCondition, linkCost: 1, card: card));
            }

            #endregion

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 4 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "Reveal the top 3 cards of your deck. Add 1 card with the [Appmon] trait and 1 card with the [Leviathan] trait among them to the hand. Return the rest to the bottom of the deck.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.HasAppmonTraits;
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    return cardSource.HasLeviathanTraits;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && card.Owner.LibraryCards.Count >= 1;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 card with [Appmon] trait",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition1,
                            message: "Select 1 card with [Leviathan] trait.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                        activateClass: activateClass
                    ));
                }
            }

            #endregion

            #region Link Effect

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 1 linked card, return 1 [appmon] digimon from trash to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsLinkedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] By trashing 1 of this Digimon's link cards, you may return 1 [Appmon] trait Digimon card from your trash to the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && HasLinkedCards(card.PermanentOfThisCard().TopCard.PermanentOfThisCard())
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CardCondition);
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasAppmonTraits;
                }

                bool HasLinkedCards(Permanent permanent)
                {
                    return !permanent.HasNoLinkCards;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent thisCardPermament = card.PermanentOfThisCard().TopCard.PermanentOfThisCard();

                    if (HasLinkedCards(thisCardPermament))
                    {
                        CardSource selectedCard = null;

                        #region Select Link Card

                        int maxCount = Math.Min(1, thisCardPermament.LinkedCards.Count);
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: _ => true,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 linked card to trash.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: thisCardPermament.LinkedCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }
                        selectCardEffect.SetUpCustomMessage("Select 1 linked card to trash.", "The opponent is selecting 1 linked card to trash.");
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        #endregion

                        if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashLinkCardsAndProcessAccordingToResult(
                            targetPermanent: thisCardPermament,
                            targetLinkCards: new List<CardSource>() { selectedCard },
                            activateClass: activateClass,
                            successProcess: SuccessProcess,
                            failureProcess: null));

                        IEnumerator SuccessProcess(List<CardSource> trashedLinkCards)
                        {
                            if (trashedLinkCards.Any() && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CardCondition))
                            {
                                #region Select Trash Digimon

                                int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, CardCondition));
                                SelectCardEffect selectCardEffect1 = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect1.SetUp(
                                            canTargetCondition: CardCondition,
                                            canTargetCondition_ByPreSelecetedList: null,
                                            canEndSelectCondition: null,
                                            canNoSelect: () => true,
                                            selectCardCoroutine: null,
                                            afterSelectCardCoroutine: null,
                                            message: "Select 1 [Appmon] trait digimon to return to hand",
                                            maxCount: maxCount1,
                                            canEndNotMax: false,
                                            isShowOpponent: true,
                                            mode: SelectCardEffect.Mode.AddHand,
                                            root: SelectCardEffect.Root.Trash,
                                            customRootCardList: null,
                                            canLookReverseCard: true,
                                            selectPlayer: card.Owner,
                                            cardEffect: activateClass);

                                selectCardEffect1.SetUpCustomMessage("Select 1 [Appmon] trait digimon to return to hand", "The opponent is selecting 1 [Appmon] trait digimon to return to hand");
                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect1.Activate());

                                #endregion
                            }
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}