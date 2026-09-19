using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;

public class IcecladClass : ICardEffect, IIcecladEffect
{
    public void SetUpIcecladClass(Func<Permanent, bool> PermanentCondition)
    {
        this.PermanentCondition = PermanentCondition;
    }

    Func<Permanent, bool> PermanentCondition { get; set; }

    public bool HasIceclad(Permanent permanent)
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