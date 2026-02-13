using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect that oneself can't attack
    public static CanNotAttackTargetDefendingPermanentClass CanNotAttackSelfStaticEffect(
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

        return CanNotAttackStaticEffect(
            attackerCondition: AttackerCondition,
            defenderCondition: defenderCondition,
            isInheritedEffect: isInheritedEffect,
            card: card,
            condition: CanUseCondition,
            effectName: effectName);
    }
    #endregion

    #region Static effect that can't attack
    public static CanNotAttackTargetDefendingPermanentClass CanNotAttackStaticEffect(
        Func<Permanent, bool> attackerCondition,
        Func<Permanent, bool> defenderCondition,
        bool isInheritedEffect,
        CardSource card,
        Func<bool> condition,
        string effectName)
    {
        CanNotAttackTargetDefendingPermanentClass canNotAttackClass = new CanNotAttackTargetDefendingPermanentClass();
        canNotAttackClass.SetUpICardEffect(effectName, CanUseCondition, card);
        canNotAttackClass.SetUpCanNotAttackTargetDefendingPermanentClass(attackerCondition: AttackerCondition, defenderCondition: DefenderCondition);

        if (isInheritedEffect)
        {
            canNotAttackClass.SetIsInheritedEffect(true);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return condition == null || condition();
        }

        bool AttackerCondition(Permanent attacker)
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(attacker))
            {
                if (!attacker.TopCard.CanNotBeAffected(canNotAttackClass))
                {
                    if (attackerCondition == null || attackerCondition(attacker))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool DefenderCondition(Permanent defender)
        {
            return defenderCondition == null || defenderCondition(defender);
        }

        return canNotAttackClass;
    }
    #endregion
}