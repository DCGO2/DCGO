using System.Collections.Generic;
using System;
public class ChangeCostClass : ICardEffect, IChangeCostEffect
{
    Func<CardSource, int, SelectCardEffect.Root, List<Permanent>, int> _changeCostFunc { get; set; }
    Func<CardSource, bool> _cardSourceCondition { get; set; }
    Func<SelectCardEffect.Root, bool> _rootCondition { get; set; }
    Func<bool> _isUpDown { get; set; }
    Func<bool> _isCheckAvailability { get; set; }
    Func<bool> _isChangePayingCost { get; set; }
    public void SetUpChangeCostClass(Func<CardSource, int, SelectCardEffect.Root, List<Permanent>, int> changeCostFunc, Func<CardSource, bool> cardSourceCondition, Func<SelectCardEffect.Root, bool> rootCondition, Func<bool> isUpDown, Func<bool> isCheckAvailability, Func<bool> isChangePayingCost)
    {
        _changeCostFunc = changeCostFunc;
        _cardSourceCondition = cardSourceCondition;
        _rootCondition = rootCondition;
        _isUpDown = isUpDown;
        _isCheckAvailability = isCheckAvailability;
        _isChangePayingCost = isChangePayingCost;
    }
    public int GetCost(int cost, CardSource cardSource, SelectCardEffect.Root root, List<Permanent> targetPermanents)
    {
        if (cardSource != null)
        {
            if (CardCondition(cardSource))
            {
                if (_changeCostFunc != null)
                {
                    if (_rootCondition != null)
                    {
                        if (_rootCondition(root))
                        {
                            int newCost = _changeCostFunc(cardSource, cost, root, targetPermanents);

                            if (IsUpDown())
                            {
                                if (newCost < cost)
                                {
                                    if (!cardSource.Owner.CanReduceCost(targetPermanents, cardSource))
                                    {
                                        newCost = cost;
                                    }
                                }
                            }

                            cost = newCost;
                        }
                    }
                }
            }
        }

        return cost;
    }

    public bool CardCondition(CardSource cardSource)
    {
        if (_cardSourceCondition != null)
        {
            if (_cardSourceCondition(cardSource))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsUpDown()
    {
        if (_isUpDown != null)
        {
            return _isUpDown();
        }

        return false;
    }

    public bool IsCheckAvailability()
    {
        if (_isCheckAvailability != null)
        {
            return _isCheckAvailability();
        }

        return false;
    }

    public bool IsChangePayingCost()
    {
        if (_isChangePayingCost != null)
        {
            return _isChangePayingCost();
        }

        return true;
    }
}