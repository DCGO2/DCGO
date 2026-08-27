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
        Permanent targetPermanent = card.PermanentOfThisCard() ?? new Permanent(new List<CardSource>() { card });

        bool CanUseCondition()
        {
            return condition == null || condition();
        }

        return AscensionEffect(targetPermanent, isInheritedEffect, CanUseCondition, null, card, isLinkedEffect);
    }
    #endregion

    #region Trigger effect of [Ascension]
    public static ActivateClass AscensionEffect(Permanent targetPermanent, bool isInheritedEffect, Func<bool> condition, ICardEffect rootCardEffect, CardSource card, bool isLinkedEffect = false)
    {
        if (targetPermanent == null) return null;
        if (targetPermanent.TopCard == null) return null;
        if (card == null) return null;

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Ascension", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, DataBase.AscensionEffectDescription());
        activateClass.SetIsInheritedEffect(isInheritedEffect);
        activateClass.SetIsLinkedEffect(isLinkedEffect);

        if (rootCardEffect != null)
        {
            activateClass.SetIsInheritedEffect(false);
            activateClass.SetEffectSourcePermanent(targetPermanent);
            activateClass.SetRootCardEffect(rootCardEffect);
        }

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
