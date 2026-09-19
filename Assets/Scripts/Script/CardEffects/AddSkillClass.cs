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
        // Callers probe this wrapper against every card being evaluated on the field (e.g. "would
        // this granted-effect apply to card X?"), not just the card it actually applies to. Only
        // adopt `card` as our own identity when we actually matched it -- otherwise a probe against
        // an unrelated card (that fails _cardSourceCondition) would permanently overwrite this
        // wrapper's real EffectSourceCard with that unrelated card, and since later probes read
        // EffectSourceCard back to decide what to pass in next, the corruption never self-heals.
        if (_cardSourceCondition(card))
        {
            getCardEffect = _getEffects(card, getCardEffect, timing);
            SetEffectSourceCard(card);
        }

        return getCardEffect;
    }
}
