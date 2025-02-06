using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX2
{
    public class EX2_030 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 4 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Reveal 4 cards from the top of your deck. Add all black Tamer cards among them to your hand. Place the remaining cards at the bottom of your deck in any order.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsTamer)
                    {
                        if (cardSource.CardColors.Contains(CardColor.Black))
                        {
                            return true;
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
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndProcessForAll(
                                        revealCount: 4,
                                        simplifiedSelectCardCondition:
                                        new SimplifiedSelectCardConditionClass(
                                                canTargetCondition: CanSelectCardCondition,
                                                message: "",
                                                mode: SelectCardEffect.Mode.AddHand,
                                                maxCount: -1,
                                                selectCardCoroutine: null),
                                        remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                                        activateClass: activateClass
                                    ));
                }
            }

            return cardEffects;
        }
    }
}