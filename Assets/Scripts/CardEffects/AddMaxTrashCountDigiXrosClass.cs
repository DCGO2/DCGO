using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AddMaxTrashCountDigiXrosClass : ICardEffect, IAddMaxTrashCountDigiXrosEffect
{
    Func<CardSource, int> _getMaxTrashCount { get; set; }
    public void SetUpAddMaxTrashCountDigiXrosClass(Func<CardSource, int> getMaxTrashCount)
    {
        _getMaxTrashCount = getMaxTrashCount;
    }
    public int GetMaxTrashCount(CardSource cardSource)
    {
        if (cardSource != null)
        {
            if (_getMaxTrashCount != null)
            {
                return _getMaxTrashCount(cardSource);
            }
        }

        return 0;
    }
}