using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CanNotAffectedClass : ICardEffect, ICanNotAffectedEffect
{
    Func<CardSource, bool> CardCondition { get; set; }
    Func<ICardEffect, bool> SkillCondition { get; set; }
    public void SetUpCanNotAffectedClass(Func<CardSource, bool> CardCondition, Func<ICardEffect, bool> SkillCondition)
    {
        this.CardCondition = CardCondition;
        this.SkillCondition = SkillCondition;
    }

    public bool CanNotAffect(CardSource cardSource, ICardEffect cardEffect)
    {
        if(cardSource != null && cardEffect != null)
        {
            if(CardCondition != null && SkillCondition != null)
            {
                if(CardCondition(cardSource) && SkillCondition(cardEffect))
                {
                    return true;
                }
            }
        }

        return false;
    }
}