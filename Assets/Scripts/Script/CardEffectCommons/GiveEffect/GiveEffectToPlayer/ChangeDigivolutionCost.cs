using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Player gains effect to change digivolution cost until calculatintg fixed cost
    public static Func<EffectTiming, ICardEffect> ChangeDigivolutionCostPlayerEffect(
        Func<Permanent, bool> permanentCondition,
        Func<CardSource, bool> cardCondition,
        Func<SelectCardEffect.Root, bool> rootCondition,
        int changeValue,
        bool setFixedCost,
        ICardEffect activateClass)
    {
        if (activateClass == null) return null;
        if (activateClass.EffectSourceCard == null) return null;

        bool Condition()
        {
            return true;
        }

        bool PermanentCondition(Permanent permanent)
        {
            return permanentCondition == null || permanentCondition(permanent);
        }

        bool CardCondition(CardSource cardSource)
        {
            return cardCondition == null || cardCondition(cardSource);
        }

        bool RootCondition(SelectCardEffect.Root root)
        {
            return rootCondition == null || rootCondition(root);
        }

        ChangeCostClass changeCostClass = CardEffectFactory.ChangeDigivolutionCostStaticEffect(
            changeValue: changeValue,
            permanentCondition: PermanentCondition,
            cardCondition: CardCondition,
            rootCondition: RootCondition,
            isInheritedEffect: false,
            card: activateClass.EffectSourceCard,
            condition: Condition,
            setFixedCost: setFixedCost);

        return GetCardEffectByEffectTiming(timing: EffectTiming.None, cardEffect: changeCostClass);
    }
    #endregion
}