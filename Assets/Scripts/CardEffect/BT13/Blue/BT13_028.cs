using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT13
{
    public class BT13_028 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Your 1 [Jellymon] digivolves into this card", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Hand][Main] If you have [Kiyoshiro Higashimitarai], by placing 1 [TeslaJellymon] from your hand as 1 of your [Jellymon]'s bottom digivolution card, that Digimon digivolves into this card for a digivolution cost of 3, ignoring digivolution requirements.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.CardNames.Contains("TeslaJellymon");
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.CardNames.Contains("Jellymon"))
                        {
                            if (!permanent.IsToken)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Contains(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.TopCard.CardNames.Contains("Kiyoshiro Higashimitarai") || permanent.TopCard.CardNames.Contains("KiyoshiroHigashimitarai")))
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.HandCards.Contains(card))
                    {
                        bool added = false;

                        Permanent selectedPermanent = null;

                        if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                CardSource selectedCard = null;

                                int maxCount = 1;

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

                                selectHandEffect.SetUpCustomMessage("Select 1 card to place at the bottom of digivolution cards.", "The opponent is selecting 1 card to place at the bottom of digivolution cards.");

                                yield return StartCoroutine(selectHandEffect.Activate());

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCard = cardSource;

                                    yield return null;
                                }

                                if (selectedCard != null)
                                {
                                    maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectPermanentCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: true,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: SelectPermanentCoroutine,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 [Jellymon] that will get a digivolution card.", "The opponent is selecting 1 [Jellymon] that will get a digivolution card.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        selectedPermanent = permanent;

                                        if (selectedPermanent != null)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass));

                                            added = true;
                                        }
                                    }
                                }
                            }
                        }

                        if (added)
                        {
                            if (selectedPermanent != null)
                            {
                                if (card.Owner.HandCards.Contains(card))
                                {
                                    #region ignore digivolution requirements

                                    AddDigivolutionRequirementClass addEvolutionConditionClass = new AddDigivolutionRequirementClass();
                                    addEvolutionConditionClass.SetUpICardEffect("Ignore Digivolution requirements", CanUseCondition1, card);
                                    addEvolutionConditionClass.SetUpAddDigivolutionRequirementClass(getEvoCost: GetEvoCost);
                                    Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                                    card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                                    ICardEffect GetCardEffect(EffectTiming _timing)
                                    {
                                        if (_timing == EffectTiming.None)
                                        {
                                            return addEvolutionConditionClass;
                                        }

                                        return null;
                                    }

                                    bool CanUseCondition1(Hashtable hashtable)
                                    {
                                        return true;
                                    }

                                    int GetEvoCost(Permanent permanent, CardSource cardSource, bool checkAvailability)
                                    {
                                        if (card.Owner.CanIgnoreDigivolutionRequirement(permanent, cardSource))
                                        {
                                            if (CardSourceCondition(cardSource) && PermanentCondition(permanent))
                                            {
                                                return 3;
                                            }
                                        }

                                        return -1;
                                    }

                                    bool PermanentCondition(Permanent targetPermanent)
                                    {
                                        return targetPermanent == selectedPermanent;
                                    }

                                    bool CardSourceCondition(CardSource cardSource)
                                    {
                                        return cardSource == card;
                                    }

                                    #endregion

                                    if (card.CanPlayCardTargetFrame(selectedPermanent.PermanentFrame, true, activateClass))
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(new PlayCardClass(
                                            cardSources: new List<CardSource>() { card },
                                            hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                            payCost: true,
                                            targetPermanent: selectedPermanent,
                                            isTapped: false,
                                            root: SelectCardEffect.Root.Hand,
                                            activateETB: true).PlayCard());
                                    }

                                    #region release ignore digivolution requirements

                                    card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);

                                    #endregion
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return cards from trash to the bottom of deck to unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Unsuspend_BT13_028");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Attack][Once Per Turn] By returning 3 cards with [Jellymon] in their text from your trash at the bottom of the deck in any order, unsuspend this Digimon.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.HasText("Jellymon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.TrashCards.Count(CanSelectCardCondition) >= 3)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.TrashCards.Count(CanSelectCardCondition) >= 3)
                    {
                        int maxCount = 3;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                        canTargetCondition: (cardSource) => CanSelectCardCondition(cardSource),
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => false,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        message: "Select cards to place at the bottom of the deck\n(cards will be placed back to the bottom of the deck so that cards with lower numbers are on top).",
                        maxCount: maxCount,
                        canEndNotMax: false,
                        isShowOpponent: false,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                        selectCardEffect.SetNotShowCard();
                        selectCardEffect.SetNotAddLog();

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count == 3)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(cardSources));

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(cardSources, "Deck Bottom Cards", true, true));

                                if (CardEffectCommons.IsExistOnBattleArea(card))
                                {
                                    Permanent selectedPermanent = card.PermanentOfThisCard();

                                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass).Unsuspend());
                                }
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}