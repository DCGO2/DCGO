using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect that can't be trashed by effect
    public static ImmuneStackTrashingClass CanNotBeTrashedBySkillStaticEffect(Func<Permanent, bool> permanentCondition, Func<ICardEffect, bool> cardEffectCondition, bool isInheritedEffect, CardSource card, Func<bool> condition, string effectName)
    {
        ImmuneStackTrashingClass canNotBeTrashedBySkillClass = new ImmuneStackTrashingClass();
        canNotBeTrashedBySkillClass.SetUpICardEffect(effectName, CanUseCondition, card);
        canNotBeTrashedBySkillClass.SetUpImmuneFromStackTrashingClass(PermanentCondition, CardEffectCondition);

        if (isInheritedEffect)
        {
            canNotBeTrashedBySkillClass.SetIsInheritedEffect(true);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return condition == null || condition();
        }

        bool PermanentCondition(Permanent permanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnField(permanent))
            {
                if (!permanent.TopCard.CanNotBeAffected(canNotBeTrashedBySkillClass))
                {
                    if (permanentCondition == null || permanentCondition(permanent))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool CardEffectCondition(ICardEffect cardEffect)
        {
            if (cardEffectCondition == null || cardEffectCondition(cardEffect))
            {
                return true;
            }

            return false;
        }

        return canNotBeTrashedBySkillClass;
    }
    #endregion
}