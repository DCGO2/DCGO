using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class WarGreymon_ST1_11 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            int count()
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    return card.PermanentOfThisCard().DigivolutionCards.Count / 2;
                }

                return 0;
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

            cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect<Func<int>>(
                changeValue: () => count(), 
                isInheritedEffect: false, 
                card: card, 
                condition: Condition));
        }

        return cardEffects;
    }
}
