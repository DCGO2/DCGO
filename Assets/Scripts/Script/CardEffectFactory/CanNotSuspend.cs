using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect that can't suspend
    public static CanNotSuspendClass CantSuspendStaticEffect(Func<Permanent, bool> permanentCondition, bool isInheritedEffect, CardSource card,
    Func<bool> condition, string effectName)
    {
        CanNotSuspendClass canNotUnsuspendClass = new CanNotSuspendClass();
        canNotUnsuspendClass.SetUpICardEffect(effectName, CanUseCondition, card);
        canNotUnsuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCondition);

        if (isInheritedEffect)
        {
            canNotUnsuspendClass.SetIsInheritedEffect(true);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return condition == null || condition();
        }

        bool PermanentCondition(Permanent permanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
            {
                if (!permanent.TopCard.CanNotBeAffected(canNotUnsuspendClass))
                {
                    if (permanentCondition == null || permanentCondition(permanent))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        return canNotUnsuspendClass;
    }
    #endregion
}