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

        bool orientantion = false;

        if (sendFaceUp)
        {
            orientantion = hashtable.ContainsKey("FaceUp")
                && hashtable["FaceUp"] is bool isFaceUp
                && isFaceUp;
        }
        else
        {
            orientantion = !hashtable.ContainsKey("FaceUp")
                && hashtable["FaceUp"] is bool isFaceUp
                && !isFaceUp;
        }

        bool SecurityCondition(CardSource cardSource, bool orientantion)
        {
            return orientantion
                && (condition == null
                    || condition(cardSource));
        }

        List<CardSource> sentCards =
            CardEffectCommons.GetCardSourcesFromHashtable(hashtable)
                .Filter(cardSource => SecurityCondition(cardSource, orientantion));

        return sentCards.Count >= 1;
    }
    #endregion
}
