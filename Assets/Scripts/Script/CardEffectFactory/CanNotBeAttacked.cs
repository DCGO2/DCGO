using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect that oneself can't be attacked
    public static CanNotAttackTargetDefendingPermanentClass CanNotBeAttackedSelfStaticEffect(
        Func<Permanent, bool> attackerCondition,
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

        bool DefenderCondition(Permanent attacker)
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

        return CanNotAttackStaticEffect(
            attackerCondition: attackerCondition,
            defenderCondition: DefenderCondition,
            isInheritedEffect: isInheritedEffect,
            card: card,
            condition: CanUseCondition,
            effectName: effectName);
    }
    #endregion
}