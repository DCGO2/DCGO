using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChangeCardLevelForAssemblyClass : ICardEffect, IChangeCardLevelForAssemblyEffect
{
    public void SetUpChangeCardLevelForAssemblyClass(Func<CardSource, List<int>, List<int>> changeCardLevel)
    {
        this.changeCardLevel = changeCardLevel;
    }

    Func<CardSource, List<int>, List<int>> changeCardLevel { get; set; }

    public List<int> ChangeCardLevelForAssembly(List<int> Level, CardSource cardSource)
    {
        if (changeCardLevel != null)
        {
            Level = changeCardLevel(cardSource, Level);
        }

        return Level;
    }
}