using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CanNotBeRemovedClass : ICardEffect, ICanNotBeRemovedEffect
{
    Func<Permanent, bool> _permanentCondition { get; set; }
    public void SetUpCanNotBeRemovedClass(Func<Permanent, bool> permanentCondition)
    {
        _permanentCondition = permanentCondition;
    }

    public bool CanNotBeRemoved(Permanent permanent)
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