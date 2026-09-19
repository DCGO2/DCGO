using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect that oneself is unblockable
    public static CannotBlockClass CanNotBeBlockedStaticSelfEffect(
        Func<Permanent, bool> defenderCondition,
        bool isInheritedEffect,
        CardSource card,
        Func<bool> condition,
        string effectName)
    {
        bool CanUseCondition()
        {
            if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
            {
                if (condition == null || condition())
                {
                    return true;
                }
            }

            return false;
        }

        bool AttackerCondition(Permanent attacker)
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(attacker))
            {
                if (attacker == card.PermanentOfThisCard())
                {
                    return true;
                }
            }

            return false;
        }

        return CanNotBlockStaticEffect(
            attackerCondition: AttackerCondition,
            defenderCondition: defenderCondition,
            isInheritedEffect: isInheritedEffect,
            card: card,
            condition: CanUseCondition,
            effectName: effectName);
    }
    #endregion
}