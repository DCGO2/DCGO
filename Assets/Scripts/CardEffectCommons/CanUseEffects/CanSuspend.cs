using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can activate effects by suspending oneself
    public static bool CanActivateSuspendCostEffect(CardSource card)
    {
        return CanActivatePermanentSuspendCostEffect(card.PermanentOfThisCard());
    }
    #endregion

    #region Can activate effects by suspending permanent
    public static bool CanActivatePermanentSuspendCostEffect(Permanent permanent)
    {
        if (IsPermanentExistsOnBattleArea(permanent))
        {
            if (!permanent.IsSuspended && permanent.CanSuspend)
            {
                return true;
            }
        }

        return false;
    }
    #endregion
}