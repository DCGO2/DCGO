using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT5_053 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            int count()
            {
                return card.Owner.GetBattleAreaDigimons().Count((permanent) => permanent.IsSuspended && permanent != card.PermanentOfThisCard());
            }

            bool Condition()
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (count() >= 1)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect<Func<int>>(changeValue: () => 2000 * count(), isInheritedEffect: false, card: card, condition: Condition));
        }

        return cardEffects;
    }
}
