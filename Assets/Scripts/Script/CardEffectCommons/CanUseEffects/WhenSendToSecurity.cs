using System;
using System.Collections;
using System.Collections.Generic;

public partial class CardEffectCommons
{
    #region Can trigger "When cards are placed in your security" effects
    public static bool CanTriggerWhenSendToSecurity(
    CardSource card,
    Hashtable hashtable,
    Func<CardSource, bool> condition = null,
    bool isOpponent = false,
    bool sendFaceUp = false)
    {
        Player player = GetPlayerFromHashtable(hashtable);

        if (player == null) return false;
        if (isOpponent && player == card.Owner) return false;
        if (!isOpponent && player == card.Owner.Enemy) return false;

        bool orientation = hashtable.ContainsKey("FaceUp")
            && hashtable["FaceUp"] is bool isFaceUp
            && isFaceUp == sendFaceUp;

        if (!orientation) return false;

        List<CardSource> sentCards =
            GetCardSourcesFromHashtable(hashtable)
                .Filter(cardSource =>
                    condition == null || condition(cardSource));

        return sentCards.Count >= 1;
    }
    #endregion
}
