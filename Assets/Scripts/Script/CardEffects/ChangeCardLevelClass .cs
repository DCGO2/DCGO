using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;

public class ChangeCardLevelClass : ICardEffect, IChangeCardLevelEffect
{
    Func<CardSource, int, int> GetLevel { get; set; } = null;
    public void SetUpChangeCardLevelClass(Func<CardSource, int, int> GetLevel)
    {
        this.GetLevel = GetLevel;
    }

    public int GetCardLevel(int level, CardSource card)
    {
        if (card != null)
        {
            if (GetLevel != null)
            {
                level = GetLevel(card, level);
            }
        }

        return level;
    }
}