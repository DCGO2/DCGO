using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CanNotBeDestroyedByBattleClass : ICardEffect, ICanNotBeDestroyedByBattleEffect
{
    public void SetUpCanNotBeDestroyedByBattleClass(Func<Permanent, Permanent, Permanent, CardSource, bool> canNotBeDestroyedByBattleCondition, Func<Permanent, bool> permanentCondition)
    {
        _canNotBeDestroyedByBattleCondition = canNotBeDestroyedByBattleCondition;
        _permanentCondition = permanentCondition;
    }

    Func<Permanent, Permanent, Permanent, CardSource, bool> _canNotBeDestroyedByBattleCondition = null;
    Func<Permanent, bool> _permanentCondition = null;

    public bool CanNotBeDestroyedByBattle(Permanent permanent, Permanent attackingPermanent, Permanent defendingPermanent, CardSource defendingCard)
    {
        if (_canNotBeDestroyedByBattleCondition != null)
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
            {
                if (PermanentCondition(permanent))
                {
                    if (_canNotBeDestroyedByBattleCondition(permanent, attackingPermanent, defendingPermanent, defendingCard))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public bool PermanentCondition(Permanent permanent)
    {
        if (_permanentCondition != null)
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
            {
                if (_permanentCondition(permanent))
                {
                    return true;
                }
            }
        }

        return false;
    }
}