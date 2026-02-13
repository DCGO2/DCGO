using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger one of [On Play] effect or [When Digivolving] effect
    static bool CanTriggerOnEnterField(Hashtable hashtable, CardSource card, bool isEvolution, Func<SelectCardEffect.Root, bool> rootCondition = null)
    {
        return CanTriggerOnPermanentEnterField(hashtable, (permanent) => permanent.cardSources.Contains(card), isEvolution, rootCondition);
    }
    #endregion

    #region Can trigger "When permanent is played or digivolves" effect
    static bool CanTriggerOnPermanentEnterField(
        Hashtable hashtable,
        Func<Permanent, bool> permanentCondition,
        bool isEvolution,
        Func<SelectCardEffect.Root, bool> rootCondition = null)
    {
        bool IsEvolution = CardEffectCommons.IsEvolution(hashtable);

        if (IsEvolution == isEvolution)
        {
            List<Hashtable> hashtables = GetHashtablesFromHashtable(hashtable);

            if (hashtables != null)
            {
                foreach (Hashtable hashtable1 in hashtables)
                {
                    Permanent permanent = GetPermanentFromHashtable(hashtable1);

                    if (permanent != null)
                    {
                        if (permanentCondition != null)
                        {
                            if (permanentCondition(permanent))
                            {
                                SelectCardEffect.Root root = GetRootFromHashtable(hashtable1);

                                if (rootCondition == null || rootCondition(root) || root == SelectCardEffect.Root.None)
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