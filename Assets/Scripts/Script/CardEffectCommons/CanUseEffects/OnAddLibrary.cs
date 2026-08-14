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
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("CardSources"))
            {
                if (hashtable["CardSources"] is List<CardSource> CardSources)
                {
                    if (CardSources != null)
                    {
                        if (CardSources.Some(cardSource => cardCondition == null || cardCondition(cardSource)))
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

    #region The CardSources added to the library from an OnAddLibraryAnyone hashtable
    public static List<CardSource> GetCardSourcesFromAddLibraryHashtable(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("CardSources"))
            {
                if (hashtable["CardSources"] is List<CardSource> CardSources)
                {
                    return CardSources;
                }
            }
        }

        return null;
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
