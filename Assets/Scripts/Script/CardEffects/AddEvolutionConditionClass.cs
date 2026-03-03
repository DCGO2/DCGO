using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class AddDigivolutionRequirementClass : ICardEffect, IAddDigivolutionRequirementEffect
{
    Func<Permanent, CardSource, CardEffectCommons.IgnoreRequirement, bool, int> _getEvoCost { get; set; }
    public void SetUpAddDigivolutionRequirementClass(Func<Permanent, CardSource, CardEffectCommons.IgnoreRequirement, bool, int> getEvoCost)
    {
        _getEvoCost = getEvoCost;
    }

    public int GetEvoCost(Permanent permanent, CardSource cardSource, CardEffectCommons.IgnoreRequirement ignore, bool isCheckAvailability)
    {
        if (permanent != null && cardSource != null)
        {
            if (permanent.TopCard != null)
            {
                if (_getEvoCost != null)
                {
                    return _getEvoCost(permanent, cardSource, ignore, isCheckAvailability);
                }
            }
        }

        return -1;
    }
}