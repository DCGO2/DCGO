using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "When permanent would be played" effects of 1 permanent

    public static bool CanTriggerWhenWouldLink(Hashtable hashtable, Func<CardSource, bool> cardCondition, Func<Permanent, bool> permanentCondition, Func<SelectCardEffect.Root, bool> rootCondition = null, Func<ICardEffect, bool> cardEffectCondition = null)
    {
        CardSource cardSource = GetCardFromHashtable(hashtable);

        if (cardSource != null)
        {
            if (cardCondition == null || cardCondition(cardSource))
            {
                Permanent permanent = GetPermanentFromHashtable(hashtable);

                if (permanentCondition == null || permanentCondition(permanent))
                {
                    SelectCardEffect.Root root = GetRootFromHashtable(hashtable);

                    if (rootCondition == null || rootCondition(root))
                    {
                        ICardEffect cardEffect = GetCardEffectFromHashtable(hashtable);

                        if (cardEffectCondition == null || cardEffectCondition(cardEffect))
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