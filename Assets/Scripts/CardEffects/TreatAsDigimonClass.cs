using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TreatAsDigimonClass : ICardEffect, ITreatAsDigimonEffect
{
    Func<Permanent, bool> PermanentCondition { get; set; }
    public void SetUpTreatAsDigimonClass(Func<Permanent, bool> permanentCondition)
    {
        this.PermanentCondition = permanentCondition;
    }

    public bool IsDigimon(Permanent permanent)
    {
        if (PermanentCondition != null && permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (PermanentCondition(permanent))
                {
                    return true;
                }
            }
        }

        return false;
    }
}