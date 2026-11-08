using System;
using System.Collections;
using System.Collections.Generic;

public partial class CardEffectCommons
{
    #region Can trigger "When cards are placed in your security" effects
    public static bool CanTriggerWhenSendToSecurity(
    Hashtable hashtable,
    Func<Player, bool> playerCondition)
    {
        Player player = GetPlayerFromHashtable(hashtable);

        if (player != null)
        {
            if (playerCondition == null || playerCondition(player))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Can trigger "When face-up cards are placed in your security" effects
    public static bool PlacedFaceUpToSecurity(Hashtable hashtable, Func<CardSource, bool> condition = null)
    {
        bool faceUp = hashtable.ContainsKey("FaceUp")
                        && hashtable["FaceUp"] is bool isFaceUp
                        && isFaceUp;

        bool SecurityCondition(CardSource cardSource, bool faceUp)
        {
            return faceUp
                && (condition == null
                    || condition(cardSource));
        }

        List<CardSource> sentCards =
            CardEffectCommons.GetCardSourcesFromHashtable(hashtable)
                .Filter(cardSource => SecurityCondition(cardSource, faceUp));

        if (sentCards.Count >= 1)
        {
            return true;
        }

        return false;
    }
    #endregion
}