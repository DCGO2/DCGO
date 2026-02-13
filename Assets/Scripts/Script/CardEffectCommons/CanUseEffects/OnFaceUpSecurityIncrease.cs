using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{

    #region Can trigger "When face-up security carsd increases" effect
    public static bool CanTriggerOnFaceUpSecurityIncreases(Hashtable hashtable, Player player = null, Func<CardSource, bool> cardCondition = null)
    {
        Player _player = GetPlayerFromHashtable(hashtable);

        if (player == null || player.Equals(_player))
        {
            List<CardSource> FaceUpCards = GetCardSourcesFromHashtable(hashtable);

            if (FaceUpCards != null)
            {
                if (FaceUpCards.Count(cardSource => cardCondition == null || cardCondition(cardSource)) >= 1)
                {
                    return true;
                }
            }
        }

        return false;
    }
    #endregion
}