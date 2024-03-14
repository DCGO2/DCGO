using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Ignore battle Security Effect condition
    public static bool CanUseIgnoreBattle(Hashtable hashtable, CardSource card)
    {
        return CanTriggerOptionMainEffect(hashtable, card);
    }
    #endregion
}