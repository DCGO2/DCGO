using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "When digivolution card is added due to effect" effect
    public static bool CanTriggerOnAddDigivolutionCard(Hashtable hashtable, Func<Permanent, bool> permanentCondition, Func<ICardEffect, bool> cardEffectCondition, Func<CardSource, bool> cardCondition)
    {
        if (hashtable != null)
        {
            Permanent Permanent = GetPermanentFromHashtable(hashtable);

            if (Permanent != null)
            {
                if (Permanent.TopCard != null)
                {
                    if (permanentCondition == null || permanentCondition(Permanent))
                    {
                        ICardEffect CardEffect = GetCardEffectFromHashtable(hashtable);

                        if (CardEffect != null)
                        {
                            if (cardEffectCondition == null || cardEffectCondition(CardEffect))
                            {
                                List<CardSource> CardSources = GetCardSourcesFromHashtable(hashtable);

                                if (CardSources != null)
                                {
                                    if (CardSources.Count(cardSource => cardCondition == null || cardCondition(cardSource)) >= 1)
                                    {
                                        return true;
                                    }
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