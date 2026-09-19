using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT8_071 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            CannotReduceCostClass cannotReduceCostClass = new CannotReduceCostClass();
            cannotReduceCostClass.SetUpICardEffect("Players can't reduce play costs", CanUseCondition, card);
            cannotReduceCostClass.SetUpCannotReduceCostClass(
                playerCondition: PlayerCondition,
                targetPermanentsCondition: TargetPermanentsCondition,
                cardCondition: CardCondition);
            cardEffects.Add(cannotReduceCostClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            bool PlayerCondition(Player player)
            {
                return true;
            }

            bool TargetPermanentsCondition(List<Permanent> targetPermanents)
            {
                if (targetPermanents == null)
                {
                    return true;
                }

                else
                {
                    if (targetPermanents.Count((permanent) => permanent != null) == 0)
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CardCondition(CardSource cardSource)
            {
                return cardSource.HasPlayCost;
            }
        }

        return cardEffects;
    }
}
