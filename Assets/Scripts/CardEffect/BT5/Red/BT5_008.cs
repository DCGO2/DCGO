using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT5_008 : CEntity_Effect
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
                        return true;
                    }
                }

                return false;
            }

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                {
                    if (permanent != card.PermanentOfThisCard())
                    {
                        if (permanent.TopCard.CardNames.Contains("Gaossmon"))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(
                permanentCondition: PermanentCondition,
                changeValue: 3000,
                isInheritedEffect: false,
                card: card,
                condition: Condition,
                effectName: () => "Your other [Gaossmon]s gain DP +3000"));
        }

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
                return player == card.Owner.Enemy;
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
