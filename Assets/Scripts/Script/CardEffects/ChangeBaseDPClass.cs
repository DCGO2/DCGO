using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChangeBaseDPClass : ICardEffect, IChangeBaseDPEffect
{
    Func<Permanent, int, int> _changeDPFunc { get; set; }
    Func<Permanent, bool> _permanentCondition { get; set; }
    Func<bool> _isUpDown { get; set; }
    Func<bool> _isMinusDP { get; set; }
    public void SetUpChangeBaseDPClass(Func<Permanent, int, int> changeDPFunc, Func<Permanent, bool> permanentCondition, Func<bool> isUpDownFunc, Func<bool> isMinusDPFunc)
    {
        this._changeDPFunc = changeDPFunc;
        this._permanentCondition = permanentCondition;
        this._isUpDown = isUpDownFunc;
        this._isMinusDP = isMinusDPFunc;
    }
    public int GetDP(int DP, Permanent permanent)
    {
        if (_changeDPFunc != null)
        {
            if (permanent != null)
            {
                if (permanent.TopCard != null)
                {
                    if (PermanentCondition(permanent))
                    {
                        DP = _changeDPFunc(permanent, DP);
                    }
                }
            }
        }

        return DP;
    }

    public bool PermanentCondition(Permanent permanent)
    {
        if (_permanentCondition != null)
        {
            return _permanentCondition(permanent);
        }

        return false;
    }

    public bool IsUpDown()
    {
        if (_isUpDown != null)
        {
            return _isUpDown();
        }

        return false;
    }

    public bool IsMinusDP()
    {
        if (_isMinusDP != null)
        {
            return _isMinusDP();
        }

        return false;
    }
}