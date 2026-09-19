using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class ImmuneStackReturnToLibraryClass : ICardEffect, IImmuneFromStackReturnToLibraryEffect
{
    Func<Permanent, bool> PermanentCondition { get; set; }
    Func<ICardEffect, bool> EffectCondition { get; set; }
    public void SetUpImmuneFromStackReturnToLibraryClass(Func<Permanent, bool> PermanentCondition, Func<ICardEffect, bool> EffectCondition)
    {
        this.PermanentCondition = PermanentCondition;
        this.EffectCondition = EffectCondition;
    }

    public bool ImmuneStackReturnToLibrary(Permanent permanent, ICardEffect effect)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (EffectCondition != null)
                {
                    if (!EffectCondition(effect))
                    {
                        return false;
                    }
                }

                if (PermanentCondition != null)
                {
                    if (!PermanentCondition(permanent))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        return false;
    }
}
