using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect that can't be deleted
    public static CanNotBeDestroyedClass CanNotBeDestroyedStaticEffect(Func<Permanent, bool> permanentCondition, bool isInheritedEffect, CardSource card, Func<bool> condition, string effectName)
    {
        CanNotBeDestroyedClass canNotBeDestroyedClass = new CanNotBeDestroyedClass();
        canNotBeDestroyedClass.SetUpICardEffect(effectName, CanUseCondition, card);
        canNotBeDestroyedClass.SetUpCanNotBeDestroyedClass(permanentCondition: PermanentCondition);

        if (isInheritedEffect)
        {
            canNotBeDestroyedClass.SetIsInheritedEffect(true);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return condition == null || condition();
        }

        bool PermanentCondition(Permanent permanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
            {
                if (!permanent.TopCard.CanNotBeAffected(canNotBeDestroyedClass))
                {
                    if (permanentCondition == null || permanentCondition(permanent))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        return canNotBeDestroyedClass;
    }
    #endregion
}