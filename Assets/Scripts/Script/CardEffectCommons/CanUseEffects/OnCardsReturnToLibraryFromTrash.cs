using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "When your cards return to library from tash" effects

    public static bool CanTriggerWhenOwnerCardsReturnToLibraryFromTrash(Hashtable hashtable, Func<CardSource, bool> cardCondition, CardSource card)
    {
        bool CardCondition(CardSource cardSource) => cardSource.Owner == card.Owner && (cardCondition == null || cardCondition(cardSource));

        return CanTriggerWhenCardsReturnToLibraryFromTrash(hashtable, CardCondition, card);
    }
    #endregion

    #region Can trigger "When cards return to library from tash" effects

    public static bool CanTriggerWhenCardsReturnToLibraryFromTrash(Hashtable hashtable, Func<CardSource, bool> cardCondition, CardSource card)
    {
        List<CardSource> CardSources = GetCardSourcesFromHashtable(hashtable);

        if (CardSources != null)
        {
            CardSources = CardSources.Filter(cardSource => !cardSource.IsDigiEgg && (cardCondition == null || cardCondition(cardSource)));

            if (CardSources.Count >= 1)
            {
                return true;
            }
        }

        return false;
    }
    #endregion
}