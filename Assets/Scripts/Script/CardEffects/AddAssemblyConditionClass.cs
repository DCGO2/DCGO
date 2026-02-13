using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AddAssemblyConditionClass : ICardEffect, IAddAssemblyConditionEffect
{
    Func<CardSource, AssemblyCondition> _getAssemblyCondition { get; set; }
    public void SetUpAddAssemblyConditionClass(Func<CardSource, AssemblyCondition> getAssemblyCondition)
    {
        _getAssemblyCondition = getAssemblyCondition;
    }
    public AssemblyCondition GetAssemblyCondition(CardSource cardSource)
    {
        if (cardSource != null)
        {
            if (_getAssemblyCondition != null)
            {
                if (_getAssemblyCondition(cardSource) != null)
                {
                    return _getAssemblyCondition(cardSource);
                }
            }
        }

        return null;
    }
}