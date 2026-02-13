using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CanNotBeDestroyedClass : ICardEffect, ICanNotBeDestroyedEffect
{
    Func<Permanent, bool> _permanentCondition { get; set; }
    public void SetUpCanNotBeDestroyedClass(Func<Permanent, bool> permanentCondition)
    {
        _permanentCondition = permanentCondition;
    }

    public bool CanNotBeDestroyed(Permanent permanent)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (_permanentCondition != null)
                {
                    if (_permanentCondition(permanent))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}