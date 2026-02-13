using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect that adds one's own card's digivolution requirement
    public static AddDigivolutionRequirementClass AddSelfDigivolutionRequirementStaticEffect(Func<Permanent, bool> permanentCondition, int digivolutionCost, bool ignoreDigivolutionRequirement, CardSource card, Func<bool> condition, string effectName = null, Func<CardSource, bool> cardCondition = null, Func<int> costEquation = null)
    {
        bool CanUseCondition()
        {
            return condition == null || condition();
        }

        return AddDigivolutionRequirementStaticEffect(
            permanentCondition: permanentCondition,
            cardCondition: cardCondition ?? ((cardSource) => cardSource == card),
            ignoreDigivolutionRequirement: ignoreDigivolutionRequirement,
            digivolutionCost: digivolutionCost,
            isInheritedEffect: false,
            card: card,
            condition: CanUseCondition,
            effectName: effectName ?? "Can digivolve to this card",
            costEquation: costEquation);
    }
    #endregion

    #region Static effect that adds digivolution requirement
    public static AddDigivolutionRequirementClass AddDigivolutionRequirementStaticEffect(Func<Permanent, bool> permanentCondition, Func<CardSource, bool> cardCondition, bool ignoreDigivolutionRequirement, int digivolutionCost, bool isInheritedEffect, CardSource card, Func<bool> condition, string effectName, Func<int> costEquation = null)
    {
        AddDigivolutionRequirementClass addDigivolutionRequirementClass = new AddDigivolutionRequirementClass();
        addDigivolutionRequirementClass.SetUpICardEffect(effectName, CanUseCondition, card);
        addDigivolutionRequirementClass.SetUpAddDigivolutionRequirementClass(getEvoCost: GetEvoCost);

        if (isInheritedEffect)
        {
            addDigivolutionRequirementClass.SetIsInheritedEffect(true);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return condition == null || condition();
        }

        int GetEvoCost(Permanent permanent, CardSource cardSource, bool checkAvailability)
        {
            // �i�������𖳎����Đi�����悤�Ƃ��Ă���̂ɁA�i�������𖳎��ł��Ȃ��ꍇ
            if (ignoreDigivolutionRequirement && !cardSource.Owner.CanIgnoreDigivolutionRequirement(permanent, cardSource))
            {
                return -1;
            }

            if (CardCondition(cardSource) && PermanentCondition(permanent))
            {
                return costEquation != null ? costEquation() : digivolutionCost;
            }

            return -1;
        }

        bool PermanentCondition(Permanent permanent)
        {
            if (permanent != null)
            {
                if (permanent.TopCard != null)
                {
                    if (permanentCondition == null || permanentCondition(permanent))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool CardCondition(CardSource cardSource)
        {
            if (cardSource != null)
            {
                if (cardCondition == null || cardCondition(cardSource))
                {
                    return true;
                }
            }

            return false;
        }

        return addDigivolutionRequirementClass;
    }
    #endregion
}