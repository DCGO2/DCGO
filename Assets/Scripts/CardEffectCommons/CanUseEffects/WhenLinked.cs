using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "When card is linked" effect
    public static bool CanTriggerWhenLinking(Hashtable hashtable, Func<Permanent, bool> permanentCondition, CardSource card)
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
                            CardSource cardSource = GetCardFromHashtable(hashtable);

                            if (cardSource != null)
                            {
                                if (cardSource == card)
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

    #region Can trigger "When a card gets linked" effect
    public static bool CanTriggerWhenLinked(Hashtable hashtable, Func<Permanent, bool> permanentCondition, Func<CardSource, bool> sourcecCondition)
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
                            CardSource cardSource = GetCardFromHashtable(hashtable);

                            if (cardSource != null)
                            {
                                if (sourcecCondition == null || sourcecCondition(cardSource))
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