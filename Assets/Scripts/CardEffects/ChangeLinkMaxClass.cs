using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;

public class ChangeLinkMaxClass : ICardEffect, IChangeLinkMaxEffect
{
    public void SetUpChangeLinkMaxClass(Func<Permanent, int, int> changeLinkMaxFunc, Func<Permanent, bool> permanentCondition, Func<CalculateOrder> isUpDown)
    {
        _changeLinkMaxFunc = changeLinkMaxFunc;
        _permanentCondition = permanentCondition;
        _isUpDown = isUpDown;
    }

    Func<Permanent, int, int> _changeLinkMaxFunc = null;
    Func<Permanent, bool> _permanentCondition = null;
    Func<CalculateOrder> _isUpDown = null;

    public int GetLinkMax(int LinkMax, Permanent permanent, int invertValue)
    {
        int changedLinkMax = LinkMax;

        if (PermanentCondition(permanent))
        {
            changedLinkMax = _changeLinkMaxFunc(permanent, LinkMax);

            switch (invertValue)
            {
                case -1:
                    if(changedLinkMax < LinkMax)
                        changedLinkMax = LinkMax + Mathf.Abs(changedLinkMax - LinkMax);
                    break;
                case 1:
                    if (changedLinkMax > LinkMax)
                        changedLinkMax = LinkMax - (changedLinkMax - LinkMax);
                    break;
            }
        }

        return changedLinkMax;
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