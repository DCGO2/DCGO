using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "When this hand card is trashed due to effect" effect
    public static bool CanTriggerOnTrashSelfHand(Hashtable hashtable, Func<ICardEffect, bool> cardEffectCondition, CardSource card)
    {
        return CanTriggerOnTrashHand(hashtable, cardEffectCondition, cardSource => cardSource == card);
    }
    #endregion

    #region Can trigger "When hand card is trashed due to effect" effect
    public static bool CanTriggerOnTrashHand(Hashtable hashtable, Func<ICardEffect, bool> cardEffectCondition, Func<CardSource, bool> cardCondition)
    {
        ICardEffect CardEffect = GetCardEffectFromHashtable(hashtable);

        if (CardEffect != null)
        {
            if (cardEffectCondition == null || cardEffectCondition(CardEffect))
            {
                List<CardSource> DiscardedCards = GetDiscardedCardsFromHashtable(hashtable);

                if (DiscardedCards != null)
                {
                    if (DiscardedCards.Count(cardSource => cardCondition == null || cardCondition(cardSource)) >= 1)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    #endregion
}