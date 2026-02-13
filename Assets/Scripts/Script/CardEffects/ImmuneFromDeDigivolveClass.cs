using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class ImmuneFromDeDigivolveClass : ICardEffect, IImmuneFromDeDigivolveEffect
{
    Func<Permanent, bool> PermanentCondition { get; set; }
    public void SetUpImmuneFromDeDigivolveClass(Func<Permanent, bool> PermanentCondition)
    {
        this.PermanentCondition = PermanentCondition;
    }

    public bool ImmuneDeDigivolve(Permanent permanent)
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