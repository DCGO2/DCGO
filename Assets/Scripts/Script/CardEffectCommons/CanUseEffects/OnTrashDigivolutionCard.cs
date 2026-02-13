using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "When this digivolution card is trashed" effect
    public static bool CanTriggerOnTrashSelfDigivolutionCard(Hashtable hashtable, Func<ICardEffect, bool> cardEffectCondition, CardSource card)
    {
        bool PermanentCondition(Permanent permanent)
        {
            if (IsPermanentExistsOnBattleArea(permanent))
            {
                if (permanent.DigivolutionCards.Contains(card))
                {
                    return true;
                }
            }

            return false;
        }

        bool CardCondition(CardSource cardSource)
        {
            return cardSource == card;
        }

        return CanTriggerOnTrashDigivolutionCard(hashtable, PermanentCondition, cardEffectCondition, CardCondition);
    }
    #endregion

    #region Can trigger "When this digivolution card is trashed due to effect" effect
    public static bool CanTriggerOnTrashDigivolutionCard(Hashtable hashtable, Func<Permanent, bool> permanentCondition, Func<ICardEffect, bool> cardEffectCondition, Func<CardSource, bool> cardCondition)
    {
        Permanent permanent = GetPermanentFromHashtable(hashtable);

        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (permanentCondition == null || permanentCondition(permanent))
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
                }
            }
        }

        return false;
    }
    #endregion
}