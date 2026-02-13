using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect of [Rush] on oneself
    public static RushClass RushSelfStaticEffect(bool isInheritedEffect, CardSource card, Func<bool> condition)
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

        return RushStaticEffect(permanentCondition: PermanentCondition, isInheritedEffect: isInheritedEffect, card: card, condition: CanUseCondition);
    }
    #endregion

    #region Static effect of [Rush]
    public static RushClass RushStaticEffect(Func<Permanent, bool> permanentCondition, bool isInheritedEffect, CardSource card, Func<bool> condition)
    {
        string effectName = "Rush";

        RushClass rushClass = new RushClass();
        rushClass.SetUpICardEffect(effectName, CanUseCondition, card);
        rushClass.SetUpRushClass(PermanentCondition: PermanentCondition);

        if (isInheritedEffect)
        {
            rushClass.SetIsInheritedEffect(true);
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

        return rushClass;
    }
    #endregion
}