using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect that changes one's own DP
    public static ChangeDPClass ChangeSelfDPStaticEffect<T>(
        T changeValue,
        bool isInheritedEffect,
        CardSource card,
        Func<bool> condition,
        bool isLinkedEffect = false)
    {
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

        return ChangeTargetDPStaticEffect(
            targetPermanent: card.PermanentOfThisCard(),
            changeValue: changeValue,
            isInheritedEffect: isInheritedEffect,
            card: card,
            condition: CanUseCondition,
            isLinkedEffect: isLinkedEffect);
    }
    #endregion

    #region Static effect that changes DP
    public static ChangeDPClass ChangeTargetDPStaticEffect<T>(
        Permanent targetPermanent,
        T changeValue,
        bool isInheritedEffect,
        CardSource card,
        Func<bool> condition,
        bool isLinkedEffect = false)
    {
        bool PermanentCondition(Permanent permanent)
        {
            return permanent == targetPermanent;
        }

        return ChangeDPStaticEffect(
            permanentCondition: PermanentCondition,
            changeValue: changeValue,
            isInheritedEffect: isInheritedEffect,
            card: card,
            condition: condition,
            effectName: null,
            isLinkedEffect: isLinkedEffect
        );
    }

    public static ChangeDPClass ChangeDPStaticEffect<T>(
        Func<Permanent, bool> permanentCondition,
        T changeValue,
        bool isInheritedEffect,
        CardSource card,
        Func<bool> condition,
        Func<string> effectName,
        bool isLinkedEffect = false)
    {
        bool isInt = typeof(T) == typeof(int);
        bool isIntFunc = typeof(T) == typeof(Func<int>);

        if (!isInt && !isIntFunc) return null;

        if (isInt && (int)(object)changeValue == 0) return null;
        if (isIntFunc && changeValue as Func<int> == null) return null;

        int _changeValue() => isInt ? (int)(object)changeValue : (changeValue as Func<int>)();
        bool isUpValue() => _changeValue() > 0;
        string EffectName()
        {
            if (effectName != null)
            {
                return effectName();
            }

            return isUpValue() ? $"DP +{_changeValue()}" : $"DP {_changeValue()}";
        }

        ChangeDPClass changeDPClass = new ChangeDPClass();
        changeDPClass.SetUpICardEffect("", CanUseCondition, card);
        changeDPClass.SetUpChangeDPClass(ChangeDP: ChangeDP, permanentCondition: PermanentCondition, isUpDown: _isUpDown, isMinusDP: () => !isUpValue());
        changeDPClass.SetIsInheritedEffect(isInheritedEffect);
        changeDPClass.SetIsLinkedEffect(isLinkedEffect);

        bool CanUseCondition(Hashtable hashtable)
        {
            if (condition == null || condition())
            {
                changeDPClass.SetEffectName(EffectName());

                return true;
            }

            return false;
        }

        int ChangeDP(Permanent permanent, int DP)
        {
            if (PermanentCondition(permanent))
            {
                DP += _changeValue();
            }

            return DP;
        }

        bool PermanentCondition(Permanent permanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
            {
                if (!permanent.TopCard.CanNotBeAffected(changeDPClass))
                {
                    if (permanentCondition == null || permanentCondition(permanent))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool _isUpDown()
        {
            return true;
        }

        return changeDPClass;
    }
    #endregion
}
