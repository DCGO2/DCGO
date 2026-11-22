using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CanNotBeDestroyedClass : ICardEffect, ICanNotBeDestroyedEffect
{
    Func<Permanent, bool> _permanentCondition { get; set; }
    bool _canBeDestroyedByBattle { get; set; }
    public void SetUpCanNotBeDestroyedClass(Func<Permanent, bool> permanentCondition, bool canBeDestroyedByBattle = false)
    {
        _permanentCondition = permanentCondition;
        _canBeDestroyedByBattle = canBeDestroyedByBattle;
    }

    public bool CanNotBeDestroyed(Permanent permanent, bool isByBattle)
    {
        if (!isByBattle || !_canBeDestroyedByBattle)
        {
            if (permanent != null)
            {
                if (permanent.TopCard != null)
                {
                    if (_permanentCondition != null)
                    {
                        if (_permanentCondition(permanent))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
}