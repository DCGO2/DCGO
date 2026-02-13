using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect that treat as Digimon
    public static TreatAsDigimonClass TreatAsDigimonStaticEffect(Func<Permanent, bool> permanentCondition, bool isInheritedEffect, CardSource card, Func<bool> condition)
    {
        string effectName = "Also treat as Digimon";

        TreatAsDigimonClass treatAsDigimonClass = new TreatAsDigimonClass();
        treatAsDigimonClass.SetUpICardEffect(effectName, CanUseCondition, card);
        treatAsDigimonClass.SetUpTreatAsDigimonClass(permanentCondition: PermanentCondition);

        if (isInheritedEffect)
        {
            treatAsDigimonClass.SetIsInheritedEffect(true);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return condition == null || condition();
        }

        bool PermanentCondition(Permanent permanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
            {
                if (permanentCondition == null || permanentCondition(permanent))
                {
                    return true;
                }
            }

            return false;
        }

        return treatAsDigimonClass;
    }
    #endregion
}