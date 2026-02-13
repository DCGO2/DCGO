using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CanNotAttackTargetDefendingPermanentClass : ICardEffect, ICanNotAttackTargetDefendingPermanentEffect
{
    Func<Permanent, bool> _attackerCondition = null;
    Func<Permanent, bool> _defenderCondition = null;
    public void SetUpCanNotAttackTargetDefendingPermanentClass(Func<Permanent, bool> attackerCondition, Func<Permanent, bool> defenderCondition)
    {
        _attackerCondition = attackerCondition;
        _defenderCondition = defenderCondition;
    }

    public bool CanNotAttackTargetDefendingPermanent(Permanent attacker, Permanent defender)
    {
        if (CardEffectCommons.IsPermanentExistsOnBattleArea(attacker))
        {
            if (_attackerCondition == null || _attackerCondition(attacker))
            {
                if (_defenderCondition == null || _defenderCondition(defender))
                {
                    return true;
                }
            }
        }

        return false;
    }
}