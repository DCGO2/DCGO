using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Trigger effect of [Fortitude] on oneself
    public static ActivateClass FortitudeSelfEffect(bool isInheritedEffect, CardSource card, Func<bool> condition)
    {
        Permanent targetPermanent = card.PermanentOfThisCard() ?? new Permanent(new List<CardSource>() { card });

        bool CanUseCondition()
        {
            if (condition == null || condition())
            {
                return true;
            }

            return false;
        }

        return FortitudeEffect(
            targetPermanent: targetPermanent,
            isInheritedEffect: isInheritedEffect,
            condition: CanUseCondition,
            rootCardEffect: null, card);
    }
    #endregion

    #region Trigger effect of [Fortitude]
    public static ActivateClass FortitudeEffect(Permanent targetPermanent, bool isInheritedEffect, Func<bool> condition, ICardEffect rootCardEffect, CardSource card)
    {
        if (targetPermanent == null) return null;
        if (targetPermanent.TopCard == null) return null;
        if (card == null) return null;

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Fortitude", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, DataBase.FortitudeEffectDiscription());
        activateClass.SetHashString($"Fortitude_{card.CardID}" + (isInheritedEffect ? "_inherited" : ""));
        activateClass.SetIsInheritedEffect(isInheritedEffect);

        if (rootCardEffect != null)
        {
            activateClass.SetIsInheritedEffect(false);
            activateClass.SetEffectSourcePermanent(targetPermanent);
            activateClass.SetRootCardEffect(rootCardEffect);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            if (CardEffectCommons.CanTriggerFortitude(hashtable, card))
            {
                if (condition == null || condition())
                {
                    return true;
                }
            }

            return false;
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanActivateFortitude(hashtable, card, isInheritedEffect, activateClass);
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.FortitudeProcess(_hashtable, card, activateClass));
        }

        return activateClass;
    }
    #endregion
}