using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT12
{
    public class BT12_007 : CEntity_Effect
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
                    return "[On Play] Reveal the top 4 cards of your deck. Add all of the [Takato Matsuki] cards among them to your hand. Place the remaining cards at the bottom of your deck in any order.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardNames.Contains("Takato Matsuki"))
                    {
                        return true;
                    }

                    if (cardSource.CardNames.Contains("TakatoMatsuki"))
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

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (card.PermanentOfThisCard().TopCard.ContainsCardName("Growlmon"))
                            {
                                return true;
                            }

                            if (card.PermanentOfThisCard().TopCard.ContainsCardName("Gallantmon"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 2000, isInheritedEffect: true, card: card, condition: Condition));
            }

            return cardEffects;
        }
    }
}