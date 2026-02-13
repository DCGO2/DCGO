using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT5_021 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            CannotReduceCostClass cannotReduceCostClass = new CannotReduceCostClass();
            cannotReduceCostClass.SetUpICardEffect("Opponent can't reduce digivolution costs", CanUseCondition, card);
            cannotReduceCostClass.SetUpCannotReduceCostClass(
                playerCondition: PlayerCondition,
                targetPermanentsCondition: TargetPermanentsCondition,
                cardCondition: CardCondition);
            cardEffects.Add(cannotReduceCostClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOpponentTurn(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool PlayerCondition(Player player)
            {
                if (player == card.Owner.Enemy)
                {
                    return true;
                }

                return false;
            }

            bool TargetPermanentsCondition(List<Permanent> targetPermanents)
            {
                if (targetPermanents != null)
                {
                    if (targetPermanents.Count((permanent) => permanent != null) >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CardCondition(CardSource cardSource)
            {
                return cardSource.IsPermanent;
            }
        }

        return cardEffects;
    }
}
