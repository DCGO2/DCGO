using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CanNotBeDestroyedBySkillClass : ICardEffect, ICanNotBeDestroyedBySkillEffect
{
    public void SetUpCanNotBeDestroyedBySkillClass(Func<Permanent, ICardEffect, bool> canNotBeDestroyedCondition)
    {
        _canNotBeDestroyedCondition = canNotBeDestroyedCondition;
    }

    Func<Permanent, ICardEffect, bool> _canNotBeDestroyedCondition { get; set; }

    public bool CanNotBeDestroyedBySkill(Permanent permanent, ICardEffect cardEffect)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (cardEffect != null)
                {
                    if (_canNotBeDestroyedCondition != null)
                    {
                        if (_canNotBeDestroyedCondition(permanent, cardEffect))
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