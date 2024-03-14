using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class AddDigivolutionRequirementClass : ICardEffect, IAddDigivolutionRequirementEffect
{
    Func<Permanent, CardSource, bool, int> _getEvoCost { get; set; }
    public void SetUpAddDigivolutionRequirementClass(Func<Permanent, CardSource, bool, int> getEvoCost)
    {
        _getEvoCost = getEvoCost;
    }

    public int GetEvoCost(Permanent permanent, CardSource cardSource, bool isCheckAvailability)
    {
        if (permanent != null && cardSource != null)
        {
            if (permanent.TopCard != null)
            {
                if (_getEvoCost != null)
                {
                    return _getEvoCost(permanent, cardSource, isCheckAvailability);
                }
            }
        }

        return -1;
    }
}