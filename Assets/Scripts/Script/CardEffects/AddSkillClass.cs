using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AddSkillClass : ICardEffect, IAddSkillEffect
{
    Func<CardSource, bool> _cardSourceCondition = null;
    Func<CardSource, List<ICardEffect>, EffectTiming, List<ICardEffect>> _getEffects = null;
    EffectTiming? _limitedTiming = null;
    public void SetUpAddSkillClass(Func<CardSource, bool> cardSourceCondition, Func<CardSource, List<ICardEffect>, EffectTiming, List<ICardEffect>> getEffects, EffectTiming? limitTiming = null)
    {
        _cardSourceCondition = cardSourceCondition;
        _getEffects = getEffects;
        _limitedTiming = limitTiming;
    }

    public bool ShouldAddEffect(EffectTiming timing)
    {
        if (_limitedTiming == null)
            return true;

        return timing == _limitedTiming;
    }
    public List<ICardEffect> GetCardEffect(CardSource card, List<ICardEffect> getCardEffect, EffectTiming timing)
    {
        if (_cardSourceCondition(card))
        {
            getCardEffect = _getEffects(card, getCardEffect, timing);
        }

        SetEffectSourceCard(card);

        return getCardEffect;
    }
}
