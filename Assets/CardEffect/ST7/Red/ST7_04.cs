using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class ST7_04 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
        }

        if (timing == EffectTiming.None)
        {
            bool DefenderCondition(Permanent permanent)
            {
                return permanent == null;
            }

            bool Condition()
            {
                return CardEffectCommons.IsOwnerTurn(card);
            }

            cardEffects.Add(CardEffectFactory.CanNotAttackSelfStaticEffect(
                defenderCondition: DefenderCondition, 
                isInheritedEffect: false, 
                card: card, 
                condition: Condition,
                effectName: "Can't Attack to player"));
        }

        return cardEffects;
    }
}
