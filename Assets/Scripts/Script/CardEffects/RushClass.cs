using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;

public class RushClass : ICardEffect, IRushEffect
{
    public void SetUpRushClass(Func<Permanent, bool> PermanentCondition)
    {
        this.PermanentCondition = PermanentCondition;
    }

    Func<Permanent, bool> PermanentCondition { get; set; }

    public bool HasRush(Permanent permanent)
    {
        if (PermanentCondition != null)
        {
            if (permanent != null)
            {
                if (permanent.TopCard != null)
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