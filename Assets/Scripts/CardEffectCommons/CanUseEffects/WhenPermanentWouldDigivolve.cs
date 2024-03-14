using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "When card would digivolve" effects of 1 permanent

    public static bool CanTriggerWhenPermanentWouldDigivolveOfCard(Hashtable hashtable, Func<CardSource, bool> cardCondition, CardSource card)
    {
        bool PermanentCondition(Permanent permanent)
        {
            return permanent == card.PermanentOfThisCard();
        }

        return CanTriggerWhenPermanentWouldDigivolve(hashtable: hashtable, permanentCondition: PermanentCondition, cardCondition: cardCondition);
    }
    #endregion

    #region Can trigger when "1 Digimon would digivolve into 1 card" effect
    public static bool CanTriggerWhenPermanentWouldDigivolve(Hashtable hashtable, Func<Permanent, bool> permanentCondition,
    Func<CardSource, bool> cardCondition)
    {
        bool IsEvolution = CardEffectCommons.IsEvolution(hashtable);

        if (IsEvolution)
        {
            CardSource Card = GetCardFromHashtable(hashtable);

            if (Card != null)
            {
                if (cardCondition == null || cardCondition(Card))
                {
                    List<Permanent> permanents = GetPermanentsFromHashtable(hashtable);

                    if (permanents != null)
                    {
                        if (permanents
                        .Filter(permanent => permanent != null && permanent.TopCard != null)
                        .Some(permanent => permanentCondition == null || permanentCondition(permanent)))
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