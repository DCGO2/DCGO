using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VortexCanAttackPlayersClass : ICardEffect, IVortexCanAttackPlayersEffect
{
    Func<Permanent, bool> _attackerCondition = null;
    public void SetUpVortexCanAttackPlayersClass(Func<Permanent, bool> attackerCondition)
    {
        _attackerCondition = attackerCondition;
    }

    public bool VortexCanAttackPlayersPermanent(Permanent attacker)
    {
        return CardEffectCommons.IsPermanentExistsOnBattleArea(attacker)
            && (_attackerCondition == null || _attackerCondition(attacker));
    }
}