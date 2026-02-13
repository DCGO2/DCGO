using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CanNotUnsuspendClass : ICardEffect, ICanNotUnsuspendEffect
{
    Func<Permanent, bool> PermanentCondition { get; set; }
    public void SetUpCanNotUntapClass(Func<Permanent, bool> PermanentCondition)
    {
        this.PermanentCondition = PermanentCondition;
    }

    public bool CanNotUnsuspend(Permanent permanent)
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