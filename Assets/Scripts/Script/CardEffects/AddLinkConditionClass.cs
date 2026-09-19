using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class AddLinkConditionClass : ICardEffect, IAddLinkConditionEffect
{
    Func<CardSource, LinkCondition> _getLinkCondition { get; set; }
    public void SetUpAddLinkConditionClass(Func<CardSource, LinkCondition> getLinkCondition)
    {
        _getLinkCondition = getLinkCondition;
    }
    public LinkCondition GetLinkCondition(CardSource cardSource)
    {
        if (cardSource != null)
        {
            if (_getLinkCondition != null)
            {
                if (_getLinkCondition(cardSource) != null)
                {
                    return _getLinkCondition(cardSource);
                }
            }
        }

        return null;
    }
}