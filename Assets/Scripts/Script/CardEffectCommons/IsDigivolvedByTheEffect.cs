using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    public static bool IsDigivolvedByTheEffect(Permanent permanent, CardSource cardSource, ICardEffect cardEffect)
    {
        if (IsPermanentExistsOnBattleArea(permanent))
        {
            if (permanent.TopCard == cardSource)
            {
                if (permanent.DigivolvingEffect == cardEffect)
                {
                    return true;
                }
            }
        }

        return false;
    }
}