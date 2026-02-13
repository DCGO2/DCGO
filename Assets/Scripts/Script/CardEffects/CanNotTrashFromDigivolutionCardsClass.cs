using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CanNotTrashFromDigivolutionCardsClass : ICardEffect, ICanNotTrashFromDigivolutionCardsEffect
{
    public void SetUpCanNotTrashFromDigivolutionCardsClass(Func<CardSource, bool> CardCondition, Func<ICardEffect, bool> CardEffectCondition)
    {
        this.CardCondition = CardCondition;
        this.CardEffectCondition = CardEffectCondition;
    }

    Func<CardSource, bool> CardCondition { get; set; }
    Func<ICardEffect, bool> CardEffectCondition { get; set; }

    public bool CanNotTrashFromDigivolutionCards(CardSource cardSource, ICardEffect cardEffect)
    {
        if (cardSource != null)
        {
            if (CardEffectCondition != null)
            {
                if (CardCondition(cardSource))
                {
                    if (CardEffectCondition(cardEffect))
                    {
                        return !cardSource.IsFlipped;
                    }
                }
            }
        }

        return false;
    }
}