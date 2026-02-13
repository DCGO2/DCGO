using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class ChangeDPClass : ICardEffect, IChangeDPEffect
{
    Func<Permanent, int, int> _changeDP { get; set; }
    Func<Permanent, bool> _permanentCondition { get; set; }
    Func<bool> _isUpDown { get; set; }
    Func<bool> _isMinusDP { get; set; }
    public void SetUpChangeDPClass(Func<Permanent, int, int> ChangeDP, Func<Permanent, bool> permanentCondition, Func<bool> isUpDown, Func<bool> isMinusDP)
    {
        _changeDP = ChangeDP;
        _permanentCondition = permanentCondition;
        _isUpDown = isUpDown;
        _isMinusDP = isMinusDP;
    }
    public int GetDP(int DP, Permanent permanent)
    {
        if (_changeDP != null)
        {
            if (permanent != null)
            {
                if (permanent.TopCard != null)
                {
                    if (PermanentCondition(permanent))
                    {
                        DP = _changeDP(permanent, DP);
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