using System.Collections.Generic;
using System;
public class ChangeLinkCostClass : ICardEffect, IChangeLinkCostEffect
{
    Func<CardSource, Permanent, int, SelectCardEffect.Root, int> _changeCostFunc { get; set; }
    Func<CardSource, bool> _cardSourceCondition { get; set; }
    Func<Permanent, bool> _permanentCondition { get; set; }
    Func<SelectCardEffect.Root, bool> _rootCondition { get; set; }
    Func<bool> _isUpDown { get; set; }
    public void SetUpChangeLinkCostClass(Func<CardSource, Permanent, int, SelectCardEffect.Root, int> changeCostFunc, Func<CardSource, bool> cardSourceCondition, Func<Permanent, bool> permanentCondition, Func<SelectCardEffect.Root, bool> rootCondition, Func<bool> isUpDown)
    {
        _changeCostFunc = changeCostFunc;
        _cardSourceCondition = cardSourceCondition;
        _permanentCondition = permanentCondition;
        _rootCondition = rootCondition;
        _isUpDown = isUpDown;
    }
    public int GetCost(int cost, CardSource cardSource, Permanent permanent, SelectCardEffect.Root root)
    {
        if (cardSource != null
            && CardCondition(cardSource)
            && PermanentCondition(permanent)
            && _changeCostFunc != null
            && _rootCondition != null
            && _rootCondition(root))
        {
            int newCost = _changeCostFunc(cardSource, permanent, cost, root);

            if (IsUpDown()
                && newCost < cost)
            {
                newCost = cost;
            }

            cost = newCost;
        }

        return cost;
    }

    public bool CardCondition(CardSource cardSource)
    {
        return _cardSourceCondition != null
            && _cardSourceCondition(cardSource);
    }

    public bool PermanentCondition(Permanent permanent)
    {
        return _permanentCondition != null
            && _permanentCondition(permanent);
    }

    public bool IsUpDown()
    {
        return _isUpDown != null
            && _isUpDown();
    }
}