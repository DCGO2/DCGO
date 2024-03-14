using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "when this permanent 's attack target switched" effect
    public static bool CanTriggerOnAttackTargetSwitch(Hashtable hashtable, CardSource card)
    {
        return CanTriggerOnPermanentAttackTargetSwitch(hashtable, permanent => permanent.cardSources.Contains(card));
    }
    #endregion

    #region Can trigger "when attack target switched" effect
    public static bool CanTriggerOnPermanentAttackTargetSwitch(Hashtable hashtable, Func<Permanent, bool> permanentCondition)
    {
        return CanTriggerOnPermanentAttack(hashtable, permanentCondition);
    }
    #endregion
}