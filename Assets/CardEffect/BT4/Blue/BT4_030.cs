using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT4_030 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            cardEffects.Add(CardEffectFactory.JammingSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
        }

        if (timing == EffectTiming.None)
        {
            bool CardCondition(CardSource cardSource)
            {
                if (cardSource.CardTraits.Contains("Hybrid") && cardSource.IsDigimon)
                {
                    return true;
                }

                if (cardSource.CardColors.Contains(CardColor.Blue) && cardSource.IsTamer)
                {
                    return true;
                }

                return false;
            }

            bool Condition()
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOpponentTurn(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Some(CardCondition))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.CanNotBeAttackedSelfStaticEffect(
                attackerCondition: null,
                isInheritedEffect: false,
                card: card,
                condition: Condition,
                effectName: "Can't be Attacked"
            ));
        }

        return cardEffects;
    }
}
