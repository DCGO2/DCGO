using System.Collections;
using System.Collections.Generic;

public class EX5_042 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnDestroyedAnyone)
        {
            cardEffects.Add(CardEffectFactory.FortitudeSelfEffect(isInheritedEffect: false, card: card, condition: null));
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Reveal the top card of deck", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Play] Reveal the top card of your deck. If that card is a level 5 or lower Digimon card with ÅÉFortitudeÅÑ, play it without paying the cost. Add the rest to your hand.";
            }

            bool CardCondition(CardSource cardSource)
            {
                if (cardSource.IsDigimon)
                {
                    if (cardSource.HasFortitude)
                    {
                        if (cardSource.Level <= 5)
                        {
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                            {
                                if (cardSource.HasLevel)
                                {
                                    return true;
                                }
                            }
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
                    if (card.Owner.LibraryCards.Count >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                List<CardSource> selectedCards = new List<CardSource>();

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndProcessForAll(
                                    revealCount: 1,
                                    simplifiedSelectCardCondition:
                                    new SimplifiedSelectCardConditionClass(
                                            canTargetCondition: CardCondition,
                                            message: "",
                                            mode: SelectCardEffect.Mode.Custom,
                                            maxCount: -1,
                                            selectCardCoroutine: SelectCardCoroutine),
                                    remainingCardsPlace: RemainingCardsPlace.AddHand,
                                    activateClass: activateClass
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

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Reveal the top card of deck", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Reveal the top card of your deck. If that card is a level 5 or lower Digimon card with ÅÉFortitudeÅÑ, play it without paying the cost. Add the rest to your hand.";
            }

            bool CardCondition(CardSource cardSource)
            {
                if (cardSource.IsDigimon)
                {
                    if (cardSource.HasFortitude)
                    {
                        if (cardSource.Level <= 5)
                        {
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                            {
                                if (cardSource.HasLevel)
                                {
                                    return true;
                                }
                            }
                        }
                    }
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
                List<CardSource> selectedCards = new List<CardSource>();

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndProcessForAll(
                                    revealCount: 1,
                                    simplifiedSelectCardCondition:
                                    new SimplifiedSelectCardConditionClass(
                                            canTargetCondition: CardCondition,
                                            message: "",
                                            mode: SelectCardEffect.Mode.Custom,
                                            maxCount: -1,
                                            selectCardCoroutine: SelectCardCoroutine),
                                    remainingCardsPlace: RemainingCardsPlace.AddHand,
                                    activateClass: activateClass
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

        if (timing == EffectTiming.None)
        {
            bool CanUseCondition()
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

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    if (permanent.HasFortitude)
                    {
                        if (permanent.HasNoDigivolutionCards)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.RushStaticEffect(
                permanentCondition: PermanentCondition,
                isInheritedEffect: false,
                card: card,
                condition: CanUseCondition
            ));
        }

        return cardEffects;
    }
}