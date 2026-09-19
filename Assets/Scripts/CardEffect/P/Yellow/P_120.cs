using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class P_120 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
        {
            cardEffects.Add(CardEffectFactory.BarrierSelfEffect(isInheritedEffect: false, card: card, condition: null));
        }

        if (timing == EffectTiming.None)
        {
            bool CardCondition(CardSource cardSource)
            {
                return cardSource.Owner == card.Owner.Enemy;
            }

            bool Condition()
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

            cardEffects.Add(CardEffectFactory.ChangeSecurityDigimonCardDPStaticEffect(
                cardCondition: CardCondition,
                changeValue: -2000,
                isInheritedEffect: true,
                card: card,
                condition: Condition,
                effectName: "Opponent's Security Digimon gains DP -2000"));
        }

        return cardEffects;
    }
}
