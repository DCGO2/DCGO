using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect that reduces play cost
    public static ChangeCostClass ChangePlayCostStaticEffect<T>(
        T changeValue,
        Func<Permanent, bool> permanentCondition,
        bool isInheritedEffect,
        CardSource card,
        Func<bool> condition,
        bool setFixedCost)
    {
        bool isInt = typeof(T) == typeof(int);
        bool isIntFunc = typeof(T) == typeof(Func<int>);

        if (!isInt && !isIntFunc) return null;

        if (isInt && (int)(object)changeValue == 0) return null;
        if (isIntFunc && changeValue as Func<int> == null) return null;

        int _changeValue() => isInt ? (int)(object)changeValue : (changeValue as Func<int>)();
        bool isUpValue() => !setFixedCost && _changeValue() > 0;

        string effectName()
        {
            if (!setFixedCost)
            {
                return isUpValue() ? $"Play Cost +{_changeValue()}" : $"Play Cost {_changeValue()}";
            }

            return $"Play Cost is {_changeValue()}";
        };

        ChangeCostClass changeCostClass = new ChangeCostClass();
        changeCostClass.SetUpICardEffect(effectName(), CanUseCondition, card);
        changeCostClass.SetUpChangeCostClass(
            changeCostFunc: ChangeCost,
            cardSourceCondition: (card) => true,
            rootCondition: (card) => true,
            isUpDown: isUpDown,
            isCheckAvailability: () => false,
            isChangePayingCost: () => false);

        if (isInheritedEffect)
        {
            changeCostClass.SetIsInheritedEffect(true);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            if (condition == null || condition())
            {
                changeCostClass.SetEffectName(effectName());

                return true;
            }

            return false;
        }

        int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
        {
            if (PermanentsCondition(targetPermanents))
            {
                if (!setFixedCost)
                {
                    Cost += _changeValue();
                }

                else
                {
                    Cost = _changeValue();
                }
            }

            return Cost;
        }

        bool PermanentsCondition(List<Permanent> targetPermanents)
        {
            if (targetPermanents != null)
            {
                if (targetPermanents.Count(PermanentCondition) >= 1)
                {
                    return true;
                }
            }

            return false;
        }

        bool PermanentCondition(Permanent targetPermanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(targetPermanent))
            {
                return permanentCondition == null || permanentCondition(targetPermanent);
            }

            return false;
        }

        bool isUpDown()
        {
            return !setFixedCost;
        }

        return changeCostClass;
    }
    #endregion
}