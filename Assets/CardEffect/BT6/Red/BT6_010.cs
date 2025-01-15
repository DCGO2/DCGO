using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;


public class BT6_010 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnDetermineDoSecurityCheck)
        {
            bool Condition()
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("Hybrid"))
                        {
                            return true;
                        }

                        if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("Ten Warriors"))
                        {
                            return true;
                        }

                        if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("TenWarriors"))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: true, card: card, condition: Condition));
        }

        return cardEffects;
    }
}
