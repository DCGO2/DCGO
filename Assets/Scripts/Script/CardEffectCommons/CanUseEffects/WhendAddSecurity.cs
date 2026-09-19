using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "when security cards added" effect
    public static bool CanTriggerWhenAddSecurity(Hashtable hashtable, Func<Player, bool> playerCondition)
    {
        return CanTriggerWhenLoseSecurity(hashtable, playerCondition);
    }
    #endregion
}