using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect that reduces digivolution cost
    public static ChangeCostClass ChangeDigivolutionCostStaticEffect<T>(
        T changeValue,
        Func<Permanent, bool> permanentCondition,
        Func<CardSource, bool> cardCondition,
        Func<SelectCardEffect.Root, bool> rootCondition,
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
                return isUpValue() ? $"Digivolution Cost +{_changeValue()}" : $"Digivolution Cost {_changeValue()}";
            }

            return $"Digivolution Cost is {_changeValue()}";
        };

        ChangeCostClass changeCostClass = new ChangeCostClass();
        changeCostClass.SetUpICardEffect("", CanUseCondition, card);
        changeCostClass.SetUpChangeCostClass(
            changeCostFunc: ChangeCost,
            cardSourceCondition: CardCondition,
            rootCondition: RootCondition,
            isUpDown: isUpDown,
            isCheckAvailability: () => false,
            isChangePayingCost: () => true);

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
            if (CardCondition(cardSource))
            {
                if (RootCondition(root))
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
            if (CardEffectCommons.IsPermanentExistsOnField(targetPermanent))
            {
                return permanentCondition == null || permanentCondition(targetPermanent);
            }

            return false;
        }

        bool CardCondition(CardSource cardSource)
        {
            if (cardSource != null)
            {
                return cardCondition == null || cardCondition(cardSource);
            }

            return false;
        }

        bool RootCondition(SelectCardEffect.Root root)
        {
            return rootCondition == null || rootCondition(root);
        }

        bool isUpDown()
        {
            return !setFixedCost;
        }

        return changeCostClass;
    }
    #endregion
}