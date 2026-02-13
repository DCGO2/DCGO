using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class DisableEffectClass : ICardEffect, IDisableCardEffect
{
    Func<ICardEffect, bool> DisableCondition { get; set; }
    public void SetUpDisableEffectClass(Func<ICardEffect, bool> DisableCondition)
    {
        this.DisableCondition = DisableCondition;
    }

    public bool IsDisabled(ICardEffect cardEffect)
    {
        if (DisableCondition(cardEffect))
        {
            return true;
        }

        return false;
    }
}
