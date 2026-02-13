using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "when permanent moves" effect
    public static bool CanTriggerOnMove(Hashtable hashtable, Func<Permanent, bool> permanentCondition)
    {
        Permanent permanent = GetMovedPermanentFromHashtable(hashtable);

        if (permanent != null)
        {
            if (IsPermanentExistsOnBattleArea(permanent))
            {
                if (permanentCondition == null || permanentCondition(permanent))
                {
                    return true;
                }
            }
        }

        return false;
    }
    #endregion

    #region Get moved permanent
    public static Permanent GetMovedPermanentFromHashtable(Hashtable hashtable)
    {
        return GetPermanentFromHashtable(hashtable);
    }
    #endregion
}