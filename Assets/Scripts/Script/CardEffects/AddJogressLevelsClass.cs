using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AddJogressLevelsClass : ICardEffect, IAddJogressLevelsEffect
{
    Func<CardSource, Permanent, List<int>> _getJogressLevels { get; set; }
    public void SetUpAddJogressLevelsClass(Func<CardSource, Permanent, List<int>> getJogressLevels)
    {
        _getJogressLevels = getJogressLevels;
    }

    public List<int> GetJogressLevels(CardSource cardSource, Permanent permanent)
    {
        if (cardSource != null)
        {
            if (permanent != null)
            {
                if (permanent.TopCard != null)
                {
                    if (_getJogressLevels != null)
                    {
                        return _getJogressLevels(cardSource, permanent);
                    }
                }
            }
        }

        return null;
    }
}