using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger [On End Attack] effect
    public static bool CanTriggerOnEndAttack(Hashtable hashtable, CardSource card)
    {
        return CanTriggerOnAttack(hashtable, card);
    }
    #endregion
}