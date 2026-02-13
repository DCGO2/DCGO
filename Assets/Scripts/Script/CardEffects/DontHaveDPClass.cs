using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DontHaveDPClass : ICardEffect, IDontHaveDPEffect
{
    Func<Permanent, bool> PermanentCondition { get; set; }
    public void SetUpDontHaveDPClass(Func<Permanent, bool> PermanentCondition)
    {
        this.PermanentCondition = PermanentCondition;
    }

    public bool DontHaveDP(Permanent permanent)
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