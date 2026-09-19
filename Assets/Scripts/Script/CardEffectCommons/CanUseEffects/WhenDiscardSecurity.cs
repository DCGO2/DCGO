using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "When this security card is trashed due to effect" effect
    public static bool CanTriggerOnTrashSelfSecurity(Hashtable hashtable, Func<ICardEffect, bool> cardEffectCondition, CardSource card)
    {
        return CanTriggerOnTrashSecurity(hashtable, cardEffectCondition, cardSource => cardSource == card);
    }
    #endregion

    #region Can trigger "When security card is trashed due to effect" effect
    public static bool CanTriggerOnTrashSecurity(Hashtable hashtable, Func<ICardEffect, bool> cardEffectCondition, Func<CardSource, bool> cardCondition)
    {
        return CanTriggerOnTrashHand(hashtable, cardEffectCondition, cardCondition);
    }
    #endregion
}