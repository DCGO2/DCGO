using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class ImmuneFromDPMinusClass : ICardEffect, IImmuneFromDPMinusEffect
{
    Func<Permanent, bool> _permanentCondition { get; set; }
    Func<ICardEffect, bool> _cardEffectCondition { get; set; }
    public void SetUpImmuneFromDPMinusClass(Func<Permanent, bool> permanentCondition, Func<ICardEffect, bool> cardEffectCondition)
    {
        _permanentCondition = permanentCondition;
        _cardEffectCondition = cardEffectCondition;
    }

    public bool ImmuneFromDPMinus(Permanent permanent, ICardEffect cardEffect)
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