using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect that changes origin DP
    public static ChangeBaseDPClass ChangeBaseDPStaticEffect<T>(Permanent targetPermanent, T changeValue, bool isInheritedEffect, CardSource card, Func<bool> condition)
    {
        bool isInt = typeof(T) == typeof(int);
        bool isIntFunc = typeof(T) == typeof(Func<int>);

        if (!isInt && !isIntFunc) return null;

        if (isInt && (int)(object)changeValue == 0) return null;
        if (isIntFunc && changeValue as Func<int> == null) return null;

        int _changeValue() => isInt ? (int)(object)changeValue : (changeValue as Func<int>)();
        string effectName() => $"Origin DP is {_changeValue()}";

        ChangeBaseDPClass changeBaseDPClass = new ChangeBaseDPClass();
        changeBaseDPClass.SetUpICardEffect("", CanUseCondition, card);
        changeBaseDPClass.SetUpChangeBaseDPClass(changeDPFunc: ChangeDP, permanentCondition: PermanentCondition, isUpDownFunc: _isUpDown, isMinusDPFunc: () => false);
        changeBaseDPClass.SetIsInheritedEffect(isInheritedEffect);

        bool CanUseCondition(Hashtable hashtable)
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(targetPermanent))
            {
                if (condition == null || condition())
                {
                    changeBaseDPClass.SetEffectName(effectName());

                    return true;
                }
            }

            return false;
        }

        int ChangeDP(Permanent permanent, int DP)
        {
            if (PermanentCondition(permanent))
            {
                DP = _changeValue();
            }

            return DP;
        }

        bool PermanentCondition(Permanent permanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
            {
                if (!permanent.TopCard.CanNotBeAffected(changeBaseDPClass))
                {
                    if (permanent == targetPermanent)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool _isUpDown()
        {
            return false;
        }

        return changeBaseDPClass;
    }
    #endregion
}