using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can unsuspend oneselef
    public static bool CanUnsuspend(Permanent permanent)
    {
        return permanent != null && permanent.TopCard != null && permanent.IsSuspended && permanent.CanUnsuspend;
    }
    #endregion
}