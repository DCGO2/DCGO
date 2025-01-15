using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class P_009 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            bool Condition()
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (card.PermanentOfThisCard().TopCard.HasGreymonName)
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
