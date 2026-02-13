using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect that can't be removed
    public static CanNotBeRemovedClass CanNotBeRemovedStaticEffect(Func<Permanent, bool> permanentCondition, bool isInheritedEffect, CardSource card, Func<bool> condition, string effectName)
    {
        CanNotBeRemovedClass canNotBeRemovedClass = new CanNotBeRemovedClass();
        canNotBeRemovedClass.SetUpICardEffect(effectName, CanUseCondition, card);
        canNotBeRemovedClass.SetUpCanNotBeRemovedClass(permanentCondition: PermanentCondition);

        if (isInheritedEffect)
        {
            canNotBeRemovedClass.SetIsInheritedEffect(true);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return condition == null || condition();
        }

        bool PermanentCondition(Permanent permanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
            {
                if (!permanent.TopCard.CanNotBeAffected(canNotBeRemovedClass))
                {
                    if (permanentCondition == null || permanentCondition(permanent))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        return canNotBeRemovedClass;
    }
    #endregion
}