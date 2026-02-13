using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "when this permanent unsuspends" effect
    public static bool CanTriggerWhenSelfPermanentUnsuspends(Hashtable hashtable, CardSource card)
    {
        return CanTriggerWhenPermanentUnsuspends(hashtable, (permanent) => permanent.cardSources.Contains(card));
    }
    #endregion

    #region Can trigger "when permanent unsuspends" effect
    public static bool CanTriggerWhenPermanentUnsuspends(Hashtable hashtable, Func<Permanent, bool> permanentCondition)
    {
        return CanTriggerWhenPermanentSuspends(hashtable, permanentCondition);
    }
    #endregion
}