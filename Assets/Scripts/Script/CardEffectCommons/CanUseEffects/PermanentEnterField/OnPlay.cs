using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{

    #region Can trigger [On Play] effect
    public static bool CanTriggerOnPlay(Hashtable hashtable, CardSource card, Func<SelectCardEffect.Root, bool> rootCondition = null)
    {
        return CanTriggerOnEnterField(hashtable, card, false, rootCondition);
    }
    #endregion

    #region Can trigger "When permanent is played" effect
    public static bool CanTriggerOnPermanentPlay(
        Hashtable hashtable,
        Func<Permanent, bool> permanentCondition,
        Func<SelectCardEffect.Root, bool> rootCondition = null)
    {
        return CanTriggerOnPermanentEnterField(hashtable, permanentCondition, false, rootCondition);
    }
    #endregion
}