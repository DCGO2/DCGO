using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public partial class CardEffectCommons
{
    #region Can trigger "When this digivolution card is trashed" effect
    public static bool CanTriggerOnTrashSelfDigivolutionCard(Hashtable hashtable, Func<ICardEffect, bool> cardEffectCondition, CardSource card, ICardEffect cardEffect)
    {
        bool PermanentCondition(Permanent permanent)
        {
            return IsPermanentExistsOnBattleArea(permanent)
                && permanent.DigivolutionCards.Contains(card);
        }

        bool CardCondition(CardSource cardSource)
        {
            return cardSource == card;
        }

        return CanTriggerOnTrashDigivolutionCard(hashtable, PermanentCondition, cardEffectCondition, CardCondition, cardEffect);
    }
    #endregion

    #region Can trigger "When this digivolution card is trashed due to effect" effect
    public static bool CanTriggerOnTrashDigivolutionCard(Hashtable hashtable, Func<Permanent, bool> permanentCondition, Func<ICardEffect, bool> cardEffectCondition, Func<CardSource, bool> cardCondition, ICardEffect cardEffect)
    {
        Permanent permanent = GetPermanentFromHashtable(hashtable);

        if (permanent != null
        && permanent.TopCard != null
        && (permanentCondition == null
            || permanentCondition(permanent)))
        {
            ICardEffect CardEffect = GetCardEffectFromHashtable(hashtable);

            if (CardEffect != null
            && (cardEffectCondition == null
                || cardEffectCondition(CardEffect)))
            {
                List<CardSource> DiscardedCards = GetDiscardedCardsFromHashtable(hashtable);

                if (DiscardedCards != null
                && (DiscardedCards.Count(cardSource => cardCondition == null
                    || cardCondition(cardSource)) >= 1))
                {
                    CardLocationMap[cardEffect] = SelectCardEffect.Root.Trash;

                    return true;
                }
            }
        }
        
        return false;
    }
    #endregion
}