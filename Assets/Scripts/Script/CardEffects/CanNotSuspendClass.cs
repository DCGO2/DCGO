using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class CanNotSuspendClass : ICardEffect, ICanNotSuspendEffect
{
    Func<Permanent, bool> PermanentCondition { get; set; }
    public void SetUpCanNotSuspendClass(Func<Permanent, bool> PermanentCondition)
    {
        this.PermanentCondition = PermanentCondition;
    }

    public bool CanNotSuspend(Permanent permanent)
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