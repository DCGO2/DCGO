using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class AddBurstDigivolutionConditionClass : ICardEffect, IAddBurstDigivolutionConditionEffect
{
    Func<CardSource, BurstDigivolutionCondition> _getBurstDigivolutionCondition { get; set; }
    public void SetUpAddBurstDigivolutionConditionClass(Func<CardSource, BurstDigivolutionCondition> getBurstDigivolutionCondition)
    {
        _getBurstDigivolutionCondition = getBurstDigivolutionCondition;
    }
    public BurstDigivolutionCondition GetBurstDigivolutionCondition(CardSource cardSource)
    {
        if (cardSource != null)
        {
            if (_getBurstDigivolutionCondition != null)
            {
                if (_getBurstDigivolutionCondition(cardSource) != null)
                {
                    return _getBurstDigivolutionCondition(cardSource);
                }
            }
        }

        return null;
    }
}