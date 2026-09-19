using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CannotReturnToLibraryClass : ICardEffect, ICannotReturnToLibraryEffect
{
    public void SetUpCannotReturnToLibraryClass(Func<Permanent, bool> permanentCondition, Func<ICardEffect, bool> cardEffectCondition)
    {
        _permanentCondition = permanentCondition;
        _cardEffectCondition = cardEffectCondition;
    }

    Func<Permanent, bool> _permanentCondition { get; set; }
    Func<ICardEffect, bool> _cardEffectCondition { get; set; }

    public bool CannotReturnToLibrary(Permanent permanent, ICardEffect cardEffect)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (_permanentCondition != null)
                {
                    if (_permanentCondition(permanent))
                    {
                        if (_cardEffectCondition != null)
                        {
                            if (_cardEffectCondition(cardEffect))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }
}