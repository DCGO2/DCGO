using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT5_029 : CEntity_Effect
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
                        if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.ContainsCardName("WereGarurumon")) >= 1)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.JammingSelfStaticEffect(isInheritedEffect: false, card: card, condition: Condition));
        }

        if (timing == EffectTiming.None)
        {
            bool Condition()
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.PermanentOfThisCard().TopCard.ContainsCardName("Omnimon") || card.PermanentOfThisCard().TopCard.HasGarurumonName)
                    {
                        return true;
                    }
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 1000, isInheritedEffect: true, card: card, condition: Condition));
        }

        return cardEffects;
    }
}
