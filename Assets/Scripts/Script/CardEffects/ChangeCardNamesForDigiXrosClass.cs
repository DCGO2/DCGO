using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChangeCardNamesForDigiXrosClass : ICardEffect, IChangeCardNamesForDigiXrosEffect
{
    public void SetUpChangeCardNamesForDigiXrosClass(Func<CardSource, List<string>, List<string>> changeCardNames)
    {
        this.changeCardNames = changeCardNames;
    }

    Func<CardSource, List<string>, List<string>> changeCardNames { get; set; }

    public List<string> ChangeCardNamesForDigiXros(List<string> CardNames, CardSource cardSource)
    {
        if (changeCardNames != null)
        {
            CardNames = changeCardNames(cardSource, CardNames);
        }

        return CardNames;
    }
}