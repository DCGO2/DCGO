using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CanNotPlayClass : ICardEffect, ICanNotPlayCardEffect
{
    Func<CardSource, bool> _cardCondition = null;
    public void SetUpCanNotPlayClass(Func<CardSource, bool> cardCondition)
    {
        _cardCondition = cardCondition;
    }

    public bool CanNotPlay(CardSource cardSource)
    {
        if (_cardCondition != null)
        {
            if (cardSource != null)
            {
                if (_cardCondition(cardSource))
                {
                    return true;
                }
            }
        }

        return false;
    }
}