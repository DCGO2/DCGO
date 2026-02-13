using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT8_059 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            CannotIgnoreDigivolutionConditionClass cannotIgnoreDigivolutionConditionClass = new CannotIgnoreDigivolutionConditionClass();
            cannotIgnoreDigivolutionConditionClass.SetUpICardEffect("Players can't ignore digivolution requirements", CanUseCondition, card);
            cannotIgnoreDigivolutionConditionClass.SetUpCannotIgnoreDigivolutionConditionClass(IgnoreDigivolutionCondition: IgnoreDigivolutionCondition);

            cardEffects.Add(cannotIgnoreDigivolutionConditionClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    return true;
                }

                return false;
            }



            bool IgnoreDigivolutionCondition(Player player, Permanent targetPermanent, CardSource cardSource)
            {
                return true;
            }
        }

        return cardEffects;
    }
}
