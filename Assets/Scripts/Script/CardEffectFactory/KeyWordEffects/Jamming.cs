using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect of [Jamming] on oneself
    public static CanNotBeDestroyedByBattleClass JammingSelfStaticEffect(bool isInheritedEffect, CardSource card, Func<bool> condition, bool isLinkedEffect = false)
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

        return JammingStaticEffect(permanentCondition: PermanentCondition, isInheritedEffect: isInheritedEffect, card: card, condition: CanUseCondition, isLinkedEffect: isLinkedEffect);
    }
    #endregion

    #region Static effect of [Jamming]
    public static CanNotBeDestroyedByBattleClass JammingStaticEffect(Func<Permanent, bool> permanentCondition, bool isInheritedEffect, CardSource card, Func<bool> condition, bool isLinkedEffect = false)
    {
        string effectName = "Jamming";

        bool CanUseCondition()
        {
            return condition == null || condition();
        }

        bool CanNotBeDestroyedByBattleCondition(Permanent permanent, Permanent AttackingPermanent, Permanent DefendingPermanent, CardSource DefendingCard)
        {
            if (permanent == AttackingPermanent)
            {
                if (DefendingCard != null)
                {
                    if (DefendingCard == GManager.instance.attackProcess.SecurityDigimon)
                    {
                        return true;
                    }
                }
            }

            return false;
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

        return CanNotBeDestroyedByBattleStaticEffect(
            canNotBeDestroyedByBattleCondition: CanNotBeDestroyedByBattleCondition,
            permanentCondition: PermanentCondition,
            isInheritedEffect: isInheritedEffect,
            card: card,
            condition: CanUseCondition,
            effectName: effectName,
            isLinkedEffect: isLinkedEffect
        );
    }
    #endregion
}