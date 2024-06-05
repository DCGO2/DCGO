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

        return ChangeTargetSAttackStaticEffect(
            targetPermanent: card.PermanentOfThisCard(),
            changeValue: changeValue,
            isInheritedEffect: isInheritedEffect,
            card: card,
            condition: CanUseCondition);
    }
    #endregion

    #region Static effect that changes SAttack
    public static ChangeSAttackClass ChangeTargetSAttackStaticEffect<T>(
        Permanent targetPermanent,
        T changeValue,
        bool isInheritedEffect,
        CardSource card,
        Func<bool> condition,
        string hashstring = null)
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
            hashstring: hashstring
        );
    }

    public static ChangeSAttackClass ChangeSAttackStaticEffect<T>(
        Func<Permanent, bool> permanentCondition,
        T changeValue,
        bool isInheritedEffect,
        CardSource card,
        Func<bool> condition,
        string hashstring = null)
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
}