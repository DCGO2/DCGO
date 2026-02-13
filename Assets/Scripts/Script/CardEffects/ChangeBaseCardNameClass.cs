using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChangeBaseCardNameClass : ICardEffect, IChangeBaseCardNameEffect
{
    public void SetUpChangeBaseCardNamesClass(Func<CardSource, List<string>, List<string>> changeBaseCardNames)
    {
        this.changeBaseCardNames = changeBaseCardNames;
    }

    Func<CardSource, List<string>, List<string>> changeBaseCardNames { get; set; }

    public List<string> ChangeBaseCardNames(List<string> BaseCardNames, CardSource cardSource)
    {
        if (changeBaseCardNames != null)
        {
            BaseCardNames = changeBaseCardNames(cardSource, BaseCardNames);
        }

        return BaseCardNames;
    }
}