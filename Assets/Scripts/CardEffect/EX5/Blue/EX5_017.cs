using System.Collections;
using System.Collections.Generic;


public class EX5_017 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            static bool PermanentCondition(Permanent targetPermanent)
            {
                if (targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 3)
                {
                    if (targetPermanent.TopCard.CardTraits.Contains("Light Fang"))
                    {
                        return true;
                    }

                    if (targetPermanent.TopCard.CardTraits.Contains("LightFung"))
                    {
                        return true;
                    }

                    if (targetPermanent.TopCard.CardTraits.Contains("Night Claw"))
                    {
                        return true;
                    }

                    if (targetPermanent.TopCard.CardTraits.Contains("NightClaw"))
                    {
                        return true;
                    }
                }
                return false;
            }

            cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Reveal the top 3 cards of deck", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Play] Reveal the top 3 cards of your deck. Add 1 card with the [Night Claw] trait and 1 card with the [Light Fang]/[Galaxy] trait among them to the hand. Return the rest to the bottom of the deck.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.CardTraits.Contains("Night Claw"))
                {
                    return true;
                }

                if (cardSource.CardTraits.Contains("NightClaw"))
                {
                    return true;
                }

                return false;
            }

            bool CanSelectCardCondition1(CardSource cardSource)
            {
                if (cardSource.CardTraits.Contains("Light Fang"))
                {
                    return true;
                }

                if (cardSource.CardTraits.Contains("LightFung"))
                {
                    return true;
                }

                if (cardSource.CardTraits.Contains("Galaxy"))
                {
                    return true;
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
                    if (card.Owner.LibraryCards.Count >= 1)
                    {
                        return true;
                    }
                }

                return false;
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
                            message: "Select 1 card with the [Night Claw] trait.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition1,
                            message: "Select 1 card with the [Light Fang]/[Galaxy] trait.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                    },
                    remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                    activateClass: activateClass
                ));
            }
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Reveal the top 3 cards of deck", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Reveal the top 3 cards of your deck. Add 1 card with the [Night Claw] trait and 1 card with the [Light Fang]/[Galaxy] trait among them to the hand. Return the rest to the bottom of the deck.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.CardTraits.Contains("Night Claw"))
                {
                    return true;
                }

                if (cardSource.CardTraits.Contains("NightClaw"))
                {
                    return true;
                }

                return false;
            }

            bool CanSelectCardCondition1(CardSource cardSource)
            {
                if (cardSource.CardTraits.Contains("Light Fang"))
                {
                    return true;
                }

                if (cardSource.CardTraits.Contains("LightFung"))
                {
                    return true;
                }

                if (cardSource.CardTraits.Contains("Galaxy"))
                {
                    return true;
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.LibraryCards.Count >= 1)
                    {
                        return true;
                    }
                }

                return false;
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
                            message: "Select 1 card with the [Night Claw] trait.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition1,
                            message: "Select 1 card with the [Light Fang]/[Galaxy] trait.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                    },
                    remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                    activateClass: activateClass
                ));
            }
        }

        if (timing == EffectTiming.None)
        {
            bool Condition()
            {
                if (CardEffectCommons.IsOpponentTurn(card))
                {
                    return true;
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 2000, isInheritedEffect: true, card: card, condition: Condition));
        }

        return cardEffects;
    }
}
