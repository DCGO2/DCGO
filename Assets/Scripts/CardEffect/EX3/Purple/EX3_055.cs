using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX3
{
    public class EX3_055 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 3 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Reveal the top 3 cards of your deck. Add 1 purple or red card with the [Free] trait or 1 card with [Imperialdramon] in its name among them to your hand, and trash 1 such card among them. Place the rest at the bottom of your deck in any order.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("Free"))
                    {
                        if (cardSource.CardColors.Contains(CardColor.Red) || cardSource.CardColors.Contains(CardColor.Purple))
                        {
                            return true;
                        }
                    }

                    if (cardSource.ContainsCardName("Imperialdramon"))
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
                            message: "Select 1 purple or red card with the [Free] trait or 1 card with [Imperialdramon] in its name to add to your hand.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 purple or red card with the [Free] trait or 1 card with [Imperialdramon] in its name to discard.",
                            mode: SelectCardEffect.Mode.Discard,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                        activateClass: activateClass
                    ));
                }
            }

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.TopCard.CardColors.Contains(CardColor.Red) && permanent.IsDigimon))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.RetaliationSelfEffect(isInheritedEffect: true, card: card, condition: Condition));
            }

            return cardEffects;
        }
    }
}