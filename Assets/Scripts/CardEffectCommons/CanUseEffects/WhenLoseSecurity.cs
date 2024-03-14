using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "when security cards lose" effect
    public static bool CanTriggerWhenLoseSecurity(Hashtable hashtable, Func<Player, bool> playerCondition)
    {
        Player Player = GetPlayerFromHashtable(hashtable);

        if (Player != null)
        {
            if (playerCondition == null || playerCondition(Player))
            {
                return true;
            }
        }

        return false;
    }
    #endregion
}