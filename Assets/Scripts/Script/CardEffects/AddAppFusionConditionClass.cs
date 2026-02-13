using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class AddAppFusionConditionClass : ICardEffect, IAddAppFusionConditionEffect
{
    Func<CardSource, AppFusionCondition> _getAppFusionCondition { get; set; }
    public void SetUpAddAppFusionConditionClass(Func<CardSource, AppFusionCondition> getAppFusionCondition)
    {
        _getAppFusionCondition = getAppFusionCondition;
    }
    public AppFusionCondition GetAppFusionCondition(CardSource cardSource)
    {
        if (cardSource != null)
        {
            if (_getAppFusionCondition != null)
            {
                if (_getAppFusionCondition(cardSource) != null)
                {
                    return _getAppFusionCondition(cardSource);
                }
            }
        }

        return null;
    }
}