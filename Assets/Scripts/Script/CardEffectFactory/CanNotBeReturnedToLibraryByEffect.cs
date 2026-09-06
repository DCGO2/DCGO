using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect that can't have its stack returned to the library by effect
    public static ImmuneStackReturnToLibraryClass CanNotBeReturnedToLibraryBySkillStaticEffect(Func<Permanent, bool> permanentCondition, Func<ICardEffect, bool> cardEffectCondition, bool isInheritedEffect, CardSource card, Func<bool> condition, string effectName)
    {
        ImmuneStackReturnToLibraryClass canNotBeReturnedToLibraryBySkillClass = new ImmuneStackReturnToLibraryClass();
        canNotBeReturnedToLibraryBySkillClass.SetUpICardEffect(effectName, CanUseCondition, card);
        canNotBeReturnedToLibraryBySkillClass.SetUpImmuneFromStackReturnToLibraryClass(PermanentCondition, CardEffectCondition);

        if (isInheritedEffect)
        {
            canNotBeReturnedToLibraryBySkillClass.SetIsInheritedEffect(true);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return condition == null || condition();
        }

        bool PermanentCondition(Permanent permanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnField(permanent))
            {
                if (!permanent.TopCard.CanNotBeAffected(canNotBeReturnedToLibraryBySkillClass))
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

        return canNotBeReturnedToLibraryBySkillClass;
    }
    #endregion
}
