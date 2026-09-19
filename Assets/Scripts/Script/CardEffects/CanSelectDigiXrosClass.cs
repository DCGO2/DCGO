using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;

public class CanSelectDigiXrosClass : ICardEffect, ICanSelectDigiXrosEffect
{
    public void SetUpCanSelectDigiXrosClass(Func<CardSource, Permanent, bool> CanSelectCondition)
    {
        this.CanSelectCondition = CanSelectCondition;
    }

    Func<CardSource, Permanent, bool> CanSelectCondition { get; set; }

    public bool CanSelect(CardSource cardSource, Permanent permanent)
    {
        if (CanSelectCondition != null)
        {
            if (permanent != null)
            {
                if (permanent.TopCard != null)
                {
                    if (cardSource != null)
                    {
                        if (CanSelectCondition(cardSource, permanent))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
}