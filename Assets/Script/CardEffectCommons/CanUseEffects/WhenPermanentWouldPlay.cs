using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "When permanent would be played" effects of 1 permanent

    public static bool CanTriggerWhenPermanentWouldPlay(Hashtable hashtable, Func<CardSource, bool> cardCondition)
    {
        bool IsEvolution = CardEffectCommons.IsEvolution(hashtable);

        if (!IsEvolution)
        {
            CardSource Card = GetCardFromHashtable(hashtable);

            if (Card != null)
            {
                if (cardCondition == null || cardCondition(Card))
                {
                    return true;
                }
            }
        }

        return false;
    }
    #endregion
}