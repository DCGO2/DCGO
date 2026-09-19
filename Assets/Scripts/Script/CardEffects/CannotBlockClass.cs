using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CannotBlockClass : ICardEffect, ICannotBlockEffect
{
    Func<Permanent, Permanent, bool> _permanentsCondition = null;
    public void SetUpCannotBlockClass(Func<Permanent, Permanent, bool> permanentsCondition)
    {
        _permanentsCondition = permanentsCondition;
    }

    public bool CannotBlock(Permanent attackingPermanent, Permanent defendingPermanent)
    {
        if (CardEffectCommons.IsPermanentExistsOnBattleArea(attackingPermanent))
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(defendingPermanent))
            {
                if (_permanentsCondition != null)
                {
                    if (_permanentsCondition(attackingPermanent, defendingPermanent))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}