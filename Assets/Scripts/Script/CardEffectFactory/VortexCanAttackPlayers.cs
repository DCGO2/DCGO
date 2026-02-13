using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect that oneself can't attack
    public static VortexCanAttackPlayersClass VortexCanAttackPlayersSelfStaticEffect(
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

        return VortexCanAttackPlayersStaticEffect(
            attackerCondition: AttackerCondition,
            isInheritedEffect: isInheritedEffect,
            card: card,
            condition: CanUseCondition,
            effectName: effectName);
    }
    #endregion

    #region Static effect that can't attack
    public static VortexCanAttackPlayersClass VortexCanAttackPlayersStaticEffect(
        Func<Permanent, bool> attackerCondition,
        bool isInheritedEffect,
        CardSource card,
        Func<bool> condition,
        string effectName)
    {
        VortexCanAttackPlayersClass vortexCanAttackPlayersClass = new VortexCanAttackPlayersClass();
        vortexCanAttackPlayersClass.SetUpICardEffect(effectName, CanUseCondition, card);
        vortexCanAttackPlayersClass.SetUpVortexCanAttackPlayersClass(attackerCondition: AttackerCondition);

        if (isInheritedEffect)
        {
            vortexCanAttackPlayersClass.SetIsInheritedEffect(true);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return condition == null || condition();
        }

        bool AttackerCondition(Permanent attacker)
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(attacker))
            {
                if (!attacker.TopCard.CanNotBeAffected(vortexCanAttackPlayersClass))
                {
                    if (attackerCondition == null || attackerCondition(attacker))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        return vortexCanAttackPlayersClass;
    }
    #endregion
}