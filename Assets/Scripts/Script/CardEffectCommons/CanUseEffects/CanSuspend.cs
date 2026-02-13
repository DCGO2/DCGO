using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can activate effects by suspending oneself
    public static bool CanActivateSuspendCostEffect(CardSource card, bool includeBreeding = false)
    {
        return CanActivatePermanentSuspendCostEffect(card.PermanentOfThisCard(), includeBreeding);
    }
    #endregion

    #region Can activate effects by suspending permanent
    public static bool CanActivatePermanentSuspendCostEffect(Permanent permanent, bool includeBreeding = false)
    {
        if (IsPermanentExistsOnBattleArea(permanent))
        {
            if (!permanent.IsSuspended && permanent.CanSuspend)
            {
                return true;
            }
        }

        if (includeBreeding)
        {
            if (IsPermanentExistsOnBreedingArea(permanent))
            {
                if (!permanent.IsSuspended && permanent.CanSuspend)
                {
                    return true;
                }
            }
        }

        return false;
    }
    #endregion
}