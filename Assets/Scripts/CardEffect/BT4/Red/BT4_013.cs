using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT4_013 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            bool Condition()
            {
                return card.Owner.HandCards.Contains(card);
            }

            bool PermanentCondition(Permanent targetPermanent)
            {
                return targetPermanent.TopCard.CardColors.Contains(CardColor.Red) && targetPermanent.IsTamer;
            }

            cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: Condition));
        }

        if (timing == EffectTiming.None)
        {
            bool Condition()
            {
                return CardEffectCommons.IsOwnerTurn(card);
            }

            cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 3000, isInheritedEffect: false, card: card, condition: Condition));
        }

        return cardEffects;
    }
}
