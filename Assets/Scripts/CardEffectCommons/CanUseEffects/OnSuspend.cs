using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "when this permanent suspends" effect
    public static bool CanTriggerWhenSelfPermanentSuspends(Hashtable hashtable, CardSource card)
    {
        return CanTriggerWhenPermanentSuspends(hashtable, (permanent) => permanent.cardSources.Contains(card));
    }
    #endregion

    #region Can trigger "when permanent suspends" effect
    public static bool CanTriggerWhenPermanentSuspends(Hashtable hashtable, Func<Permanent, bool> permanentCondition)
    {
        List<Permanent> permanents = GetPermanentsFromHashtable(hashtable);

        if (permanents != null)
        {
            foreach (Permanent Permanent in permanents)
            {
                if (IsPermanentExistsOnBattleArea(Permanent))
                {
                    if (permanentCondition != null)
                    {
                        if (permanentCondition(Permanent))
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