using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;

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

    public int GetSAttack(int SAttack, Permanent permanent, int invertValue)
    {
        int changedSAttack = SAttack;

        if (PermanentCondition(permanent))
        {
            changedSAttack = _changeSAttackFunc(permanent, SAttack);

            switch (invertValue)
            {
                case -1:
                    if(changedSAttack < SAttack)
                        changedSAttack = SAttack + Mathf.Abs(changedSAttack - SAttack);
                    break;
                case 1:
                    if (changedSAttack > SAttack)
                        changedSAttack = SAttack - (changedSAttack - SAttack);
                    break;
            }
        }

        return changedSAttack;
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