using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChangeCardColorClass : ICardEffect, IChangeCardColorEffect
{
    public void SetUpChangeCardColorClass(Func<CardSource, List<CardColor>, List<CardColor>> ChangeCardColors)
    {
        this.ChangeCardColors = ChangeCardColors;
    }

    Func<CardSource, List<CardColor>, List<CardColor>> ChangeCardColors { get; set; }
    public List<CardColor> GetCardColors(List<CardColor> CardColors, CardSource cardSource)
    {
        if (ChangeCardColors != null)
        {
            if (cardSource != null)
            {
                CardColors = ChangeCardColors(cardSource, CardColors);
            }
        }

        return CardColors;
    }
}