using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT6_053 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
        }

        if (timing == EffectTiming.None)
        {
            bool PermanentCondition(Permanent permanent)
            {
                return permanent == card.PermanentOfThisCard();
            }

            bool CardEffectCondition(ICardEffect cardEffect)
            {
                return CardEffectCommons.IsOpponentEffect(cardEffect, card);
            }

            bool CanUseCondition()
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOpponentTurn(card))
                    {
                        return true;
                    }
                }

                return false;
            }

            string effectName = "Can't have DP reduced by opponent's effect";

            cardEffects.Add(CardEffectFactory.ImmuneFromDPMinusStaticEffect(
                permanentCondition: PermanentCondition,
                cardEffectCondition: CardEffectCondition,
                isInheritedEffect: false,
                card: card,
                condition: CanUseCondition,
                effectName: effectName
            ));
        }

        return cardEffects;
    }
}
