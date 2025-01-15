using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ST15_04 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Reveal the top card of deck", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Play] Reveal the top card of your deck. If it's a black card, add it to your hand. Trash the rest.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.CardColors.Contains(CardColor.Black);
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
                    revealCount: 1,
                    simplifiedSelectCardCondition:
                    new SimplifiedSelectCardConditionClass(
                        canTargetCondition: CanSelectCardCondition,
                        message: "",
                        mode: SelectCardEffect.Mode.AddHand,
                        maxCount: -1,
                        selectCardCoroutine: null),
                    remainingCardsPlace: RemainingCardsPlace.Trash,
                    activateClass: activateClass
                ));
            }
        }

        return cardEffects;
    }
}
