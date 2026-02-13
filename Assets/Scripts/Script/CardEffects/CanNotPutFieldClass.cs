using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CanNotPutFieldClass : ICardEffect, ICanNotPutFieldEffect
{
    Func<CardSource, bool> _cardCondition = null;
    Func<ICardEffect, bool> _cardEffectCondition = null;
    public void SetUpCanNotPutFieldClass(Func<CardSource, bool> cardCondition, Func<ICardEffect, bool> cardEffectCondition)
    {
        _cardCondition = cardCondition;
        _cardEffectCondition = cardEffectCondition;
    }

    public bool CanNotPutField(CardSource cardSource, ICardEffect cardEffect)
    {
        if (cardSource != null)
        {
            if (_cardCondition != null && _cardEffectCondition != null)
            {
                if (_cardCondition(cardSource) && _cardEffectCondition(cardEffect))
                {
                    return true;
                }
            }
        }

        return false;
    }
}