using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT6_014 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            cardEffects.Add(CardEffectFactory.BlitzSelfEffect(isInheritedEffect: false, card: card, condition: null, isWhenDigivolving: true));
        }

        return cardEffects;
    }
}
