using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DontBattleSecurityDigimonClass : ICardEffect, IDontBattleSecurityDigimonEffect
{
    public void SetUpDontBattleSecurityDigimonClass(Func<CardSource, bool> CardSourceCondition)
    { 
        this.CardSourceCondition = CardSourceCondition;
    }

    Func<CardSource, bool> CardSourceCondition { get; set; }

    public bool DontBattleSecurityDigimon(CardSource cardSource)
    {
        if(CardSourceCondition != null)
        {
            if(cardSource != null)
            {
                if(CardSourceCondition(cardSource))
                {
                    return true;
                }
            }
        }

        return false;
    }
}