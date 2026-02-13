using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CanNotDigivolveClass : ICardEffect, ICanNotDigivolveEffect
{
    Func<Permanent, bool> _PermanentCondition { get; set; }
    Func<CardSource, bool> _CardCondition { get; set; }
    public void SetUpCanNotEvolveClass(Func<Permanent, bool> permanentCondition, Func<CardSource, bool> cardCondition)
    {
        _PermanentCondition = permanentCondition;
        _CardCondition = cardCondition;
    }

    public bool CanNotEvolve(Permanent permanent, CardSource cardSource)
    {
        if (permanent != null && cardSource != null)
        {
            if (permanent.TopCard != null)
            {
                if (_PermanentCondition != null && _CardCondition != null)
                {
                    if (_PermanentCondition(permanent) && _CardCondition(cardSource))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}