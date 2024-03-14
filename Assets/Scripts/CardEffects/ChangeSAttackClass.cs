using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChangeSAttackClass : ICardEffect, IChangeSAttackEffect
{
    public void SetUpChangeSAttackClass(Func<Permanent, int, int> changeSAttackFunc, Func<Permanent, bool> permanentCondition, Func<CalculateOrder> isUpDown)
    {
        _changeSAttackFunc = changeSAttackFunc;
        _permanentCondition = permanentCondition;
        _isUpDown = isUpDown;
    }

    Func<Permanent, int, int> _changeSAttackFunc = null;
    Func<Permanent, bool> _permanentCondition = null;
    Func<CalculateOrder> _isUpDown = null;

    public int GetSAttack(int SAttack, Permanent permanent)
    {
        if (PermanentCondition(permanent))
        {
            SAttack = _changeSAttackFunc(permanent, SAttack);
        }

        return SAttack;
    }

    public CalculateOrder isUpDown()
    {
        if (_isUpDown != null)
        {
            return _isUpDown();
        }

        return CalculateOrder.UpToConstant;
    }

    public bool PermanentCondition(Permanent permanent)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (_permanentCondition != null)
                {
                    if (_permanentCondition(permanent))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}