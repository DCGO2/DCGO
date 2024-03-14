using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect that can't be deleted by battle
    public static CanNotBeDestroyedByBattleClass CanNotBeDestroyedByBattleStaticEffect(Func<Permanent, Permanent, Permanent, CardSource, bool> canNotBeDestroyedByBattleCondition, Func<Permanent, bool> permanentCondition, bool isInheritedEffect, CardSource card, Func<bool> condition, string effectName)
    {
        CanNotBeDestroyedByBattleClass canNotBeDestroyedByBattleClass = new CanNotBeDestroyedByBattleClass();
        canNotBeDestroyedByBattleClass.SetUpICardEffect(effectName, CanUseCondition, card);
        canNotBeDestroyedByBattleClass.SetUpCanNotBeDestroyedByBattleClass(canNotBeDestroyedByBattleCondition, permanentCondition: PermanentCondition);

        if (isInheritedEffect)
        {
            canNotBeDestroyedByBattleClass.SetIsInheritedEffect(true);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return condition == null || condition();
        }

        bool PermanentCondition(Permanent permanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
            {
                if (!permanent.TopCard.CanNotBeAffected(canNotBeDestroyedByBattleClass))
                {
                    if (permanentCondition == null || permanentCondition(permanent))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        return canNotBeDestroyedByBattleClass;
    }
    #endregion
}