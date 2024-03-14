using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class ChangeBaseCardColorClass : ICardEffect, IChangeBaseCardColorEffect
{
    public void SetUpChangeBaseCardColorClass(Func<CardSource, List<CardColor>, List<CardColor>> ChangeBaseCardColors)
    {
        this.ChangeBaseCardColors = ChangeBaseCardColors;
    }

    Func<CardSource, List<CardColor>, List<CardColor>> ChangeBaseCardColors { get; set; }
    public List<CardColor> GetBaseCardColors(List<CardColor> BaseCardColors, CardSource cardSource)
    {
        if (ChangeBaseCardColors != null)
        {
            if (cardSource != null)
            {
                BaseCardColors = ChangeBaseCardColors(cardSource, BaseCardColors);
            }
        }

        return BaseCardColors;
    }
}