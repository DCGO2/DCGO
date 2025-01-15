using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT2_029 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            bool Condition()
            {
                return CardEffectCommons.IsOwnerTurn(card);
            }

            bool DefenderCondition(Permanent defender)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(defender, card))
                {
                    if (defender.HasNoDigivolutionCards)
                    {
                        return true;
                    }
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.CanNotBeBlockedStaticSelfEffect(
                defenderCondition: DefenderCondition,
                isInheritedEffect: false,
                card: card,
                condition: Condition,
                effectName: "Can't be Blocked by a Digimon with no digivolution cards"));
        }

        return cardEffects;
    }
}
