using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect that changes one's own SAttack
    public static ChangeSAttackClass ChangeSelfSAttackStaticEffect<T>(
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

        return ChangeTargetSAttackStaticEffect(
            targetPermanent: card.PermanentOfThisCard(),
            changeValue: changeValue,
            isInheritedEffect: isInheritedEffect,
            card: card,
            condition: CanUseCondition,
            isLinkedEffect: isLinkedEffect);
    }
    #endregion

    #region Static effect that changes SAttack
    public static ChangeSAttackClass ChangeTargetSAttackStaticEffect<T>(
        Permanent targetPermanent,
        T changeValue,
        bool isInheritedEffect,
        CardSource card,
        Func<bool> condition,
        string hashstring = null,
        bool isLinkedEffect = false)
    {
        bool PermanentCondition(Permanent permanent)
        {
            return permanent == targetPermanent;
        }

        return ChangeSAttackStaticEffect(
            permanentCondition: PermanentCondition,
            changeValue: changeValue,
            isInheritedEffect: isInheritedEffect,
            card: card,
            condition: condition,
            hashstring: hashstring,
            isLinkedEffect: isLinkedEffect
        );
    }

    public static ChangeSAttackClass ChangeSAttackStaticEffect<T>(
        Func<Permanent, bool> permanentCondition,
        T changeValue,
        bool isInheritedEffect,
        CardSource card,
        Func<bool> condition,
        string hashstring = null,
        bool isLinkedEffect = false)
    {
        bool isInt = typeof(T) == typeof(int);
        bool isIntFunc = typeof(T) == typeof(Func<int>);

        if (!isInt && !isIntFunc) return null;

        if (isInt && (int)(object)changeValue == 0) return null;
        if (isIntFunc && changeValue as Func<int> == null) return null;

        int _changeValue() => isInt ? (int)(object)changeValue : (changeValue as Func<int>)();
        bool isUpValue() => _changeValue() > 0;
        string effectName() => isUpValue() ? $"Security Attack +{_changeValue()}" : $"Security Attack {_changeValue()}";

        ChangeSAttackClass changeSAttackClass = new ChangeSAttackClass();
        changeSAttackClass.SetUpICardEffect("", CanUseCondition, card);
        changeSAttackClass.SetUpChangeSAttackClass(changeSAttackFunc: ChangeSAttack, permanentCondition: PermanentCondition, isUpDown: _isUpDown);

        if (hashstring != null)
        {
            changeSAttackClass.SetHashString(hashstring);
        }

        if (isInheritedEffect)
        {
            changeSAttackClass.SetIsInheritedEffect(true);
        }

        if (isLinkedEffect)
        {
            changeSAttackClass.SetIsLinkedEffect(true);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            if (condition == null || condition())
            {
                changeSAttackClass.SetEffectName(effectName());

                return true;
            }

            return false;
        }

        int ChangeSAttack(Permanent permanent, int SAttack)
        {
            if (PermanentCondition(permanent))
            {
                SAttack += _changeValue();
            }

            return SAttack;
        }

        bool PermanentCondition(Permanent permanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
            {
                if (!permanent.TopCard.CanNotBeAffected(changeSAttackClass))
                {
                    if (permanentCondition == null || permanentCondition(permanent))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        CalculateOrder _isUpDown()
        {
            return CalculateOrder.UpDownValue;
        }

        return changeSAttackClass;
    }
    #endregion


    #region Static effect that inverts one's own SAttack
    public static InvertSAttackClass InvertSelfSAttackStaticEffect<T>(
        T changeValue,
        bool isInheritedEffect,
        CardSource card,
        Func<bool> condition)
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

        return InvertTargetSAttackStaticEffect(
            targetPermanent: card.PermanentOfThisCard(),
            changeValue: changeValue,
            isInheritedEffect: isInheritedEffect,
            card: card,
            condition: CanUseCondition);
    }
    #endregion

    #region Static effect that inverts SAttack
    public static InvertSAttackClass InvertTargetSAttackStaticEffect<T>(
        Permanent targetPermanent,
        T changeValue,
        bool isInheritedEffect,
        CardSource card,
        Func<bool> condition)
    {
        bool PermanentCondition(Permanent permanent)
        {
            return permanent == targetPermanent;
        }

        return InvertSAttackStaticEffect(
            permanentCondition: PermanentCondition,
            changeValue: changeValue,
            isInheritedEffect: isInheritedEffect,
            card: card,
            condition: condition
        );
    }

    public static InvertSAttackClass InvertSAttackStaticEffect<T>(
        Func<Permanent, bool> permanentCondition,
        T changeValue,
        bool isInheritedEffect,
        CardSource card,
        Func<bool> condition)
    {
        bool isInt = typeof(T) == typeof(int);
        bool isIntFunc = typeof(T) == typeof(Func<int>);

        if (!isInt && !isIntFunc) return null;

        if (isInt && (int)(object)changeValue == 0) return null;
        if (isIntFunc && changeValue as Func<int> == null) return null;

        int _changeValue() => isInt ? (int)(object)changeValue : (changeValue as Func<int>)();
        bool isUpValue() => _changeValue() > 0;
        string effectName() => isUpValue() ? $"<Security A. +> are turned into <Security A. ->" : $"<Security A. -> are turned into <Security A. +>";

        InvertSAttackClass invertSAttackClass = new InvertSAttackClass();
        invertSAttackClass.SetUpICardEffect("", CanUseCondition, card);
        invertSAttackClass.SetUpChangeSAttackClass(changeInvertFunc: InvertValue, permanentCondition: PermanentCondition);
        invertSAttackClass.SetIsInheritedEffect(isInheritedEffect);
        invertSAttackClass.SetHashString($"InvertSecA_{card.CardID}");
        invertSAttackClass.SetIsBackgroundProcess(true);

        bool CanUseCondition(Hashtable hashtable)
        {
            if (condition == null || condition())
            {
                invertSAttackClass.SetEffectName(effectName());

                return true;
            }

            return false;
        }

        int InvertValue(Permanent permanent, int invertValue)
        {
            if (PermanentCondition(permanent))
            {
                invertValue += _changeValue();
            }

            return invertValue;
        }

        bool PermanentCondition(Permanent permanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
            {
                if (!permanent.TopCard.CanNotBeAffected(invertSAttackClass))
                {
                    if (permanentCondition == null || permanentCondition(permanent))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        return invertSAttackClass;
    }
    #endregion
}