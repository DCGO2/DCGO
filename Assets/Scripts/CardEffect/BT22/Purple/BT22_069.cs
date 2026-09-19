using System.Collections;
using System.Collections.Generic;

//BT22 Lunamon
namespace DCGO.CardEffects.BT22
{
    public class BT22_069 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.Level == 2 && (permanent.TopCard.HasCSTraits || permanent.TopCard.HasLightFangOrNightClawTraits);
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 0, false, card, null));
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal top 3, Add Night Claw + Galaxy/Light Fang", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[On Play] Reveal the top 3 cards of your deck. Add 1 card with the [Night Claw] trait and 1 card with the [Light Fang] or [Galaxy] trait among them to the hand. Return the rest to the bottom of the deck.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPlay(hashtable, card))
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
                        return true;
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource, int searchOption)
                {
                    if (searchOption == 1)
                    {
                        return cardSource.EqualsTraits("Night Claw");
                    }
                    if (searchOption == 2)
                    {
                        return cardSource.EqualsTraits("Light Fang") || cardSource.EqualsTraits("Galaxy");
                    }

                    return false;
                }
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition: cardsource => CanSelectCardCondition(cardsource, 1),
                            message: "Select 1 card with [Night Claw] in its traits.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition: cardsource => CanSelectCardCondition(cardsource, 2),
                            message: "Select 1 card with [Light Fang] or [Galaxy] in its traits.",
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

            #region Inherit

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place the top card of this Digimon at the bottom of digivolution cards to Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("ReturnDigivolutionCards_BT22_069");
                cardEffects.Add(activateClass);

                string EffectDiscription() =>
                    "[Main] [Once Per Turn] By placing this [Night Claw] or [Light Fang] trait Digimon's top stacked card as its bottom digivolution card, <Draw 1>.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.PermanentOfThisCard().DigivolutionCards.Count >= 1
                        && card.PermanentOfThisCard().TopCard.HasLightFangOrNightClawTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card) && card.PermanentOfThisCard().DigivolutionCards.Count >= 1)
                    {
                        CardSource topCard = card.PermanentOfThisCard().TopCard;

                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { topCard }, activateClass));
                        if (card.Owner.LibraryCards.Count >= 1) yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}