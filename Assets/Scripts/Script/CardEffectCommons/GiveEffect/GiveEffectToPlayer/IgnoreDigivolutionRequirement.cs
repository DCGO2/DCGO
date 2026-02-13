using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Player gains effect to ignore digivolution requirements and have digivolution cost fixed until calculatintg fixed cost
    public static Func<EffectTiming, ICardEffect> GainIgnoreDigivolutionRequirementPlayerEffect(Func<Permanent, bool> permanentCondition, Func<CardSource, bool> cardCondition, bool ignoreDigivolutionRequirement, int digivolutionCost, ICardEffect activateClass)
    {
        if (activateClass == null) return null;
        if (activateClass.EffectSourceCard == null) return null;

        CardSource card = activateClass.EffectSourceCard;

        bool PermanentCondition(Permanent permanent)
        {
            return permanentCondition == null || permanentCondition(permanent);
        }

        bool CardCondition(CardSource cardSource)
        {
            return cardCondition == null || cardCondition(cardSource);
        }

        AddDigivolutionRequirementClass addDigivolutionRequirementClass = CardEffectFactory.AddDigivolutionRequirementStaticEffect(
            permanentCondition: PermanentCondition,
            cardCondition: CardCondition,
            ignoreDigivolutionRequirement: ignoreDigivolutionRequirement,
            digivolutionCost: digivolutionCost,
            isInheritedEffect: false,
            card: card,
            condition: null,
            effectName: "Ignore Digivolution requirements and change digivolution cost");

        return GetCardEffectByEffectTiming(timing: EffectTiming.None, cardEffect: addDigivolutionRequirementClass);
    }
    #endregion
}