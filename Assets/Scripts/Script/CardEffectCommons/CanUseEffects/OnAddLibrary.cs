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
        return CanTriggerOnAddLibrary(hashtable, cardCondition, null);
    }

    public static bool CanTriggerOnAddLibrary(Hashtable hashtable, Func<CardSource, bool> cardCondition, Func<ICardEffect, bool> cardEffectCondition)
    {
        List<CardSource> CardSources = GetCardSourcesFromHashtable(hashtable);

        if (CardSources != null)
        {
            if (cardEffectCondition != null)
            {
                ICardEffect CardEffect = GetCardEffectFromHashtable(hashtable);

                if (CardEffect == null || !cardEffectCondition(CardEffect))
                {
                    return false;
                }
            }

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
