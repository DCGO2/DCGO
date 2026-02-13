using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CanNotSelectBySkillClass : ICardEffect, ICanNotSelectBySkillEffect
{
    Func<Permanent, bool> PermanentCondition { get; set; }
    Func<ICardEffect, bool> CardEffectCondition { get; set; }
    public void SetUpCanNotSelectBySkillClass(Func<Permanent, bool> PermanentCondition, Func<ICardEffect, bool> CardEffectCondition)
    {
        this.PermanentCondition = PermanentCondition;
        this.CardEffectCondition = CardEffectCondition;
    }

    public bool CanNotSelectBySkill(Permanent permanent, ICardEffect cardEffect)
    {
        if (permanent != null && permanent.TopCard != null && cardEffect != null && cardEffect.EffectSourceCard != null)
        {
            if (PermanentCondition != null && CardEffectCondition != null)
            {
                if (PermanentCondition(permanent) && CardEffectCondition(cardEffect))
                {
                    return true;
                }
            }
        }

        return false;
    }
}