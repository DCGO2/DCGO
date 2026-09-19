using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CannotReduceCostClass : ICardEffect, ICannotReduceCostEffect
{
    public void SetUpCannotReduceCostClass(Func<Player, bool> playerCondition, Func<List<Permanent>, bool> targetPermanentsCondition,
    Func<CardSource, bool> cardCondition)
    {
        _playerCondition = playerCondition;
        _targetPermanentsCondition = targetPermanentsCondition;
        _cardCondition = cardCondition;
    }

    Func<Player, bool> _playerCondition = null;
    Func<List<Permanent>, bool> _targetPermanentsCondition = null;
    Func<CardSource, bool> _cardCondition = null;

    public bool CannotReduceCost(Player player, List<Permanent> targetPermanents, CardSource cardSource)
    {
        if (_playerCondition != null)
        {
            if (player != null)
            {
                if (_playerCondition(player))
                {
                    if (_targetPermanentsCondition != null)
                    {
                        if (_targetPermanentsCondition(targetPermanents))
                        {
                            if (cardSource != null)
                            {
                                if (_cardCondition != null)
                                {
                                    if (_cardCondition(cardSource))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return false;
    }
}