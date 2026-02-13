using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ChangeTraitsClass : ICardEffect, IChangeTraitsEffect
{
    public void SetUpChangeTraitsClass(Func<CardSource, List<string>, List<string>> changeeTraits)
    {
        this.changeeTraits = changeeTraits;
    }

    Func<CardSource, List<string>, List<string>> changeeTraits { get; set; }

    public List<string> ChangTraits(List<string> Traits, CardSource cardSource)
    {
        if (changeeTraits != null)
        {
            Traits = changeeTraits(cardSource, Traits);
        }

        return Traits;
    }
}