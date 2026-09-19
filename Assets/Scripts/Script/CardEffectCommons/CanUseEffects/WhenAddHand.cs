using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "when hand cards added" effect
    public static bool CanTriggerWhenAddHand(Hashtable hashtable, Func<Player, bool> playerCondition, Func<ICardEffect, bool> cardEffectCondition)
    {
        List<Player> Players = GetPlayersFromHashtable(hashtable);

        if (Players != null)
        {
            if (Players.Count(player => playerCondition == null || playerCondition(player)) >= 1)
            {
                ICardEffect CardEffect = GetCardEffectFromHashtable(hashtable);

                if (cardEffectCondition == null || cardEffectCondition(CardEffect))
                {
                    return true;
                }
            }
        }

        return false;
    }
    #endregion
}