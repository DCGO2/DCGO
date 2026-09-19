using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger [On Attack] effect
    public static bool CanTriggerOnAttack(Hashtable hashtable, CardSource card)
    {
        return CanTriggerOnPermanentAttack(hashtable, (permanent) => permanent.cardSources.Contains(card));
    }
    #endregion

    #region Can trigger "when permanent attacks" effect
    public static bool CanTriggerOnPermanentAttack(Hashtable hashtable, Func<Permanent, bool> permanentCondition)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("AttackingPermanent"))
            {
                if (hashtable["AttackingPermanent"] is Permanent)
                {
                    Permanent AttackingPermanent = (Permanent)hashtable["AttackingPermanent"];

                    if (AttackingPermanent != null)
                    {
                        if (AttackingPermanent.TopCard != null)
                        {
                            if (permanentCondition != null)
                            {
                                if (permanentCondition(AttackingPermanent))
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