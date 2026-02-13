using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class CannotAddSecurityClass : ICardEffect, ICannotAddSecurityEffect
{
    public void SetUpCannotAddSecurityClass(Func<Player, bool> PlayerCondition, Func<ICardEffect, bool> CardEffectCondition)
    {
        this.PlayerCondition = PlayerCondition;
        this.CardEffectCondition = CardEffectCondition;
    }

    Func<Player, bool> PlayerCondition { get; set; }
    Func<ICardEffect, bool> CardEffectCondition { get; set; }

    public bool cannotAddSecurity(Player player, ICardEffect cardEffect)
    {
        if (player != null)
        {
            if (PlayerCondition != null)
            {
                if (CardEffectCondition != null)
                {
                    if (PlayerCondition(player))
                    {
                        if (CardEffectCondition(cardEffect))
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