using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AddDigiXrosConditionClass : ICardEffect, IAddDigiXrosConditionEffect
{
    Func<CardSource, DigiXrosCondition> _getDigiXrosCondition { get; set; }
    public void SetUpAddDigiXrosConditionClass(Func<CardSource, DigiXrosCondition> getDigiXrosCondition)
    {
        _getDigiXrosCondition = getDigiXrosCondition;
    }
    public DigiXrosCondition GetDigiXrosCondition(CardSource cardSource)
    {
        if (cardSource != null)
        {
            if (_getDigiXrosCondition != null)
            {
                if (_getDigiXrosCondition(cardSource) != null)
                {
                    return _getDigiXrosCondition(cardSource);
                }
            }
        }

        return null;
    }
}