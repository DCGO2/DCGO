using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX2
{
    public class EX2_053 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 3 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("Reveal_EX2_053");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play][Once Per Turn] If one of your [Mother D-Reaper]s has 5 or more digivolution cards, reveal the top 3 cards of your deck. You may play 1 card with [D-Reaper] in its traits and a play cost of 10 or less among them without paying its memory cost. Place the remaining cards at the top of your deck in any order.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                    {
                        if (cardSource.CardTraits.Contains("D-Reaper"))
                        {
                            if (cardSource.GetCostItself <= 10)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        foreach (Permanent permanent in card.Owner.GetBattleAreaPermanents())
                        {
                            if (permanent.DigivolutionCards.Count >= 5)
                            {
                                if (permanent.TopCard.CardNames.Contains("Mother D-Reaper"))
                                {
                                    return true;
                                }

                                if (permanent.TopCard.CardNames.Contains("MotherD-Reaper"))
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
                    List<CardSource> selectedCards = new List<CardSource>();

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 card with [D-Reaper] in its traits and a play cost of 10 or less.",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: 1,
                            selectCardCoroutine: SelectCardCoroutine),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckTop,
                        activateClass: activateClass,
                        canNoSelect: true
                    ));

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: selectedCards,
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Library,
                        activateETB: true));
                }
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 3 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("Reveal_EX2_053");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking][Once Per Turn] If one of your [Mother D-Reaper]s has 5 or more digivolution cards, reveal the top 3 cards of your deck. You may play 1 card with [D-Reaper] in its traits and a play cost of 10 or less among them without paying its memory cost. Place the remaining cards at the top of your deck in any order.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                    {
                        if (cardSource.CardTraits.Contains("D-Reaper"))
                        {
                            if (cardSource.GetCostItself <= 10)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        foreach (Permanent permanent in card.Owner.GetBattleAreaPermanents())
                        {
                            if (permanent.DigivolutionCards.Count >= 5)
                            {
                                if (permanent.TopCard.CardNames.Contains("Mother D-Reaper"))
                                {
                                    return true;
                                }

                                if (permanent.TopCard.CardNames.Contains("MotherD-Reaper"))
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
                    List<CardSource> selectedCards = new List<CardSource>();

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 card with [D-Reaper] in its traits and a play cost of 10 or less.",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: 1,
                            selectCardCoroutine: SelectCardCoroutine),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckTop,
                        activateClass: activateClass,
                        canNoSelect: true
                    ));

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: selectedCards,
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Library,
                        activateETB: true));
                }
            }

            return cardEffects;
        }
    }
}