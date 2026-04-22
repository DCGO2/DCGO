using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger [When Digivolving] effect
    public static bool CanTriggerWhenDigivolving(Hashtable hashtable, CardSource card, Func<SelectCardEffect.Root, bool> rootCondition = null)
    {
        return CanTriggerOnEnterField(hashtable, card, true, rootCondition);
    }
    #endregion

    #region Can trigger "When permanent digivolves" effect
    public static bool CanTriggerWhenPermanentDigivolving(
        Hashtable hashtable,
        Func<Permanent, bool> permanentCondition,
        Func<SelectCardEffect.Root, bool> rootCondition = null)
    {
        return CanTriggerOnPermanentEnterField(hashtable, permanentCondition, true, rootCondition);
    }
    #endregion

    #region Can Trigger "When Digimon Digivolve"
    public static bool CanTriggerWhenDigimonDigivolving(
        Hashtable hashtable, 
        Func<Permanent, bool> permanentCondition,
        Func<SelectCardEffect.Root, bool> rootCondition = null)
    {
        if (!CanTriggerWhenPermanentDigivolving(hashtable, permanentCondition, rootCondition))
            return false;
        List<CardSource> evoRootTops = GetEvoRootTopsFromEnterFieldHashtable(hashtable, permanentCondition);
        return evoRootTops != null
            && evoRootTops.Any(cardSource => cardSource.IsDigimon);
        
    }
    #endregion

}