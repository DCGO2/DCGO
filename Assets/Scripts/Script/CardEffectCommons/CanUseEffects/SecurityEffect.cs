using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger [Security] effect
    public static bool CanTriggerSecurityEffect(Hashtable hashtable, CardSource card)
    {
        return CanTriggerOptionMainEffect(hashtable, card);
    }
    #endregion
}