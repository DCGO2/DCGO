using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect of [Iceclad] on oneself
    public static IcecladClass IcecladSelfStaticEffect(bool isInheritedEffect, CardSource card, Func<bool> condition)
    {
        bool PermanentCondition(Permanent permanent) => permanent == card.PermanentOfThisCard();

        bool CanUseCondition()
        {
            if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
            {
                if (condition == null || condition())
                {
                    return true;
                }
            }

            return false;
        }

        return IcecladStaticEffect(permanentCondition: PermanentCondition, isInheritedEffect: isInheritedEffect, card: card, condition: CanUseCondition);
    }
    #endregion

    #region Static effect of [Iceclad]
    public static IcecladClass IcecladStaticEffect(Func<Permanent, bool> permanentCondition, bool isInheritedEffect, CardSource card, Func<bool> condition)
    {
        string effectName = "Iceclad";

        IcecladClass icecladClass = new IcecladClass();
        icecladClass.SetUpICardEffect(effectName, CanUseCondition, card);
        icecladClass.SetUpIcecladClass(PermanentCondition: PermanentCondition);

        if (isInheritedEffect)
        {
            icecladClass.SetIsInheritedEffect(true);
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

        return icecladClass;
    }
    #endregion
}