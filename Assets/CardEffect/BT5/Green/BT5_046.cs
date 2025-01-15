using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;


public class BT5_046 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnDeclaration)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Reveal the top card of deck", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] <Digi-Burst 1> (Trash 1 of this Digimon's digivolution cards to activate the effect below.) - Reveal the top card of your deck. Add it to your hand if it's a green Digimon card. Otherwise, place it at the bottom of your deck.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.IsDigimon)
                {
                    if (cardSource.CardColors.Contains(CardColor.Green))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (new IDigiBurst(card.PermanentOfThisCard(), 1, activateClass).CanDigiBurst())
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(new IDigiBurst(card.PermanentOfThisCard(), 1, activateClass).DigiBurst());

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndProcessForAll(
                                    revealCount: 1,
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
