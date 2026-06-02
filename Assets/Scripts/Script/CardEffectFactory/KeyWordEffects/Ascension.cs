using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Trigger effect of [Ascension] on oneself
    public static ICardEffect AscensionSelfEffect(bool isInheritedEffect, CardSource card, Func<bool> condition, bool isLinkedEffect = false)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Ascension", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, DataBase.AscensionEffectDescription());
        activateClass.SetIsInheritedEffect(isInheritedEffect);
        activateClass.SetIsLinkedEffect(isLinkedEffect);

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanTriggerAscension(hashtable, card, activateClass)
                && (condition == null || condition());
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanActivateAscension(card, activateClass);
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            return CardEffectCommons.AscensionProcess(_hashtable, activateClass, card);
        }

        return activateClass;
    }
    #endregion
}
