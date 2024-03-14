using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "When this permanent would leave the battle area" effects of 1 card

    public static bool CanTriggerWhenRemoveField(Hashtable hashtable, CardSource card)
    {
        return CanTriggerWhenPermanentRemoveField(hashtable, (permanent) => permanent.cardSources.Contains(card));
    }
    #endregion

    #region Can trigger "When this permanent would leave the battle area" effects of 1 permanent

    public static bool CanTriggerWhenPermanentRemoveField(Hashtable hashtable, Func<Permanent, bool> permanentCondition)
    {
        List<Permanent> permanents = GetPermanentsFromHashtable(hashtable);

        if (permanentCondition != null)
        {
            if (permanents.Count((permanent) => permanent != null && permanent.TopCard != null && permanentCondition(permanent)) >= 1)
            {
                return true;
            }
        }

        return false;
    }
    #endregion
}