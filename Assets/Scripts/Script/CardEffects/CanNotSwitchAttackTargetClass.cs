using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class CanNotSwitchAttackTargetClass : ICardEffect, ICanNotSwitchAttackTargetEffect
{
    Func<Permanent, bool> PermanentCondition { get; set; }
    public void SetUpCanNotSwitchAttackTargetClass(Func<Permanent, bool> PermanentCondition)
    {
        this.PermanentCondition = PermanentCondition;
    }

    public bool CanNotBeSwitchAttackTarget(Permanent permanent)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (PermanentCondition != null)
                {
                    if (PermanentCondition(permanent))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}