using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class IgnoreColorConditionClass : ICardEffect, IIgnoreColorConditionEffect
{
    Func<CardSource, bool> _cardCondition = null;
    public void SetUpIgnoreColorConditionClass(Func<CardSource, bool> cardCondition)
    {
        _cardCondition = cardCondition;
    }

    public bool IgnoreColorCondition(CardSource cardSource)
    {
        if (cardSource != null)
        {
            if (_cardCondition != null)
            {
                if (_cardCondition(cardSource))
                {
                    return true;
                }
            }
        }

        return false;
    }
}