using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChangeDPDeleteEffectMaxDPClass : ICardEffect, IChangeDPDeleteEffectMaxDPEffect
{
    Func<int, ICardEffect, int> _changeMaxDP = null;
    public void SetUpChangeDPDeleteEffectMaxDPClass(Func<int, ICardEffect, int> changeMaxDP)
    {
        _changeMaxDP = changeMaxDP;
    }

    public int GetMaxDP(int maxDP, ICardEffect cardEffect)
    {
        if (cardEffect != null)
        {
            if (_changeMaxDP != null)
            {
                maxDP = _changeMaxDP(maxDP, cardEffect);
            }
        }

        return maxDP;
    }
}