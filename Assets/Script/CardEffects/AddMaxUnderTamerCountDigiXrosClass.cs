using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AddMaxUnderTamerCountDigiXrosClass : ICardEffect, IAddMaxUnderTamerCountDigiXrosEffect
{
    Func<CardSource, int> _getMaxUnderTamerCount { get; set; }
    public void SetUpAddMaxUnderTamerCountDigiXrosClass(Func<CardSource, int> getMaxUnderTamerCount)
    {
        _getMaxUnderTamerCount = getMaxUnderTamerCount;
    }
    public int getMaxUnderTamerCount(CardSource cardSource)
    {
        if (cardSource != null)
        {
            if (_getMaxUnderTamerCount != null)
            {
                return _getMaxUnderTamerCount(cardSource);
            }
        }

        return 0;
    }
}