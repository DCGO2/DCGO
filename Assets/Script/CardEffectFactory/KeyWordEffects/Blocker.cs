using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect of [Blocker] on oneself

    public static BlockerClass BlockerSelfStaticEffect(bool isInheritedEffect, CardSource card, Func<bool> condition, bool isLinkedEffect = false)
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

        return BlockerStaticEffect(permanentCondition: PermanentCondition, isInheritedEffect: isInheritedEffect, card: card, condition: CanUseCondition, isLinkedEffect: isLinkedEffect);
    }

    #endregion

    #region Static effect of [Blocker]

    public static BlockerClass BlockerStaticEffect(Func<Permanent, bool> permanentCondition, bool isInheritedEffect, CardSource card, Func<bool> condition, bool isLinkedEffect = false)
    {
        string effectName = "Blocker";

        BlockerClass blockerClass = new BlockerClass();
        blockerClass.SetUpICardEffect(effectName, CanUseCondition, card);
        blockerClass.SetUpBlockerClass(PermanentCondition: PermanentCondition);

        if (isInheritedEffect)
        {
            blockerClass.SetIsInheritedEffect(true);
        }

        if (isLinkedEffect)
        {
            blockerClass.SetIsLinkedEffect(true);
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

        return blockerClass;
    }

    #endregion
}