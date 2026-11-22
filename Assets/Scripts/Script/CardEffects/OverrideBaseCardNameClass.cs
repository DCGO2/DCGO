using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OverrideBaseCardNameClass : ICardEffect, IOverrideBaseCardNameEffect
{
    public void SetUpOverrideBaseCardNamesClass(Func<CardSource, List<string>, List<string>> changeBaseCardNames)
    {
        this.changeBaseCardNames = changeBaseCardNames;
    }

    Func<CardSource, List<string>, List<string>> changeBaseCardNames { get; set; }

    public List<string> OverrideBaseCardNames(List<string> BaseCardNames, CardSource cardSource)
    {
        if (changeBaseCardNames != null)
        {
            BaseCardNames = changeBaseCardNames(cardSource, BaseCardNames);
        }

        return BaseCardNames;
    }
}