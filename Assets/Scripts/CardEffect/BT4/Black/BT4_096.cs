using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT4_096 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnStartTurn)
        {
            cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Reveal the top 3 cards of deck", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Play] Reveal the top 3 cards of your deck. If all of the revealed cards are black, gain 1 memory. Place the cards on top of your deck in any order.";
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
                                    revealCount: 3,
                                    simplifiedSelectCardCondition:
                                    new SimplifiedSelectCardConditionClass(
                                            canTargetCondition: (cardSource) => false,
                                            message: "",
                                            mode: SelectCardEffect.Mode.Custom,
                                            maxCount: -1,
                                            selectCardCoroutine: null),
                                    remainingCardsPlace: RemainingCardsPlace.DeckTop,
                                    activateClass: activateClass,
                                    revealedCardsCoroutine: RevealedCardsCoroutine
                                ));

                IEnumerator RevealedCardsCoroutine(List<CardSource> revealedCards)
                {
                    if (revealedCards.Every(cardSource => cardSource.CardColors.Contains(CardColor.Black)))
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                    }
                }
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
        }

        return cardEffects;
    }
}
