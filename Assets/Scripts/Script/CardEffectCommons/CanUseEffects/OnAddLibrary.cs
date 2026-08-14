using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "When cards are added to the library" effect, regardless of origin
    public static bool CanTriggerOnAddLibrary(Hashtable hashtable, Func<CardSource, bool> cardCondition)
    {
        List<CardSource> CardSources = GetCardSourcesFromHashtable(hashtable);

        if (CardSources != null)
        {
            if (CardSources.Some(cardSource =>
                cardSource != null
                && !cardSource.IsBeingRevealed
                && (cardCondition == null || cardCondition(cardSource))))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Whether the OnAddLibraryAnyone hashtable was for the top or bottom of the library
    public static bool IsAddLibraryTop(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("IsTop"))
            {
                if (hashtable["IsTop"] is bool IsTop)
                {
                    return IsTop;
                }
            }
        }

        return false;
    }
    #endregion
}
