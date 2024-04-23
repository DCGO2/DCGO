using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class Darkmaledramon_EX4_042 : CEntity_Effect
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

            cardEffects.Add(CardEffectFactory.CanNotBeBlockedStaticSelfEffect(
                defenderCondition: null, 
                isInheritedEffect: false, 
                card: card, 
                condition: Condition, 
                effectName: "Unblockable"));
        }

        if (timing == EffectTiming.None)
        {
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

            bool AttackerCondition(Permanent attacker)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(attacker, card))
                {
                    if (attacker.TopCard.ContainsCardName("Knightmon") || attacker.TopCard.ContainsCardName("Knightsmon"))
                    {
                        if (attacker.IsDigimon)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.CanNotBlockStaticEffect(
                attackerCondition: AttackerCondition, 
                defenderCondition: null, 
                isInheritedEffect: false, 
                card: card, 
                condition: Condition, 
                effectName: "Your Digimons with [Knightmon] or [Knightsmon] in their names are unblockable"));
        }

        return cardEffects;
    }
}
