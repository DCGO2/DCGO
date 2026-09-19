using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChangeCardNamesClass : ICardEffect, IChangeCardNamesEffect
{
    public void SetUpChangeCardNamesClass(Func<CardSource, List<string>, List<string>> changeCardNames)
    {
        _changeCardNames = changeCardNames;
    }

    Func<CardSource, List<string>, List<string>> _changeCardNames = null;

    public List<string> ChangeCardNames(List<string> cardNames, CardSource cardSource)
    {
        if (_changeCardNames != null)
        {
            cardNames = _changeCardNames(cardSource, cardNames);
        }

        return cardNames;
    }
}