using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "When you use an Option card" effects

    public static bool CanTriggerWhenOwnerUseOption(Hashtable hashtable, Func<CardSource, bool> cardCondition, Func<int, bool> constCondition, CardSource card)
    {
        bool CardCondition(CardSource cardSource) => cardSource.Owner == card.Owner && (cardCondition == null || cardCondition(cardSource));

        return CanTriggerWhenUseOption(hashtable, CardCondition, constCondition, card);
    }
    #endregion

    #region Can trigger "When you or the opponent use an Option card" effects

    public static bool CanTriggerWhenUseOption(Hashtable hashtable, Func<CardSource, bool> cardCondition, Func<int, bool> constCondition, CardSource card)
    {
        CardSource Card = GetCardFromHashtable(hashtable);

        if (Card != null)
        {
            if (cardCondition == null || cardCondition(Card))
            {
                if (hashtable.ContainsKey("Cost"))
                {
                    if (hashtable["Cost"] is int)
                    {
                        int Cost = (int)hashtable["Cost"];

                        if (constCondition == null || constCondition(Cost))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
    #endregion
}