using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Trigger effect of [Detach] on oneself
    public static ActivateClass DetachSelfEffect(bool isInheritedEffect, CardSource card, Func<bool> condition, string conditionString, Func<CardSource, bool> cardCondition, bool isLinkedEffect = false)
    {
        Permanent targetPermanent = card.PermanentOfThisCard();

        bool CanUseCondition()
        {
            if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
            {
                if (condition == null || condition())
                {
                    return true;
                }
            }

            return false;
        }

        return DetachEffect(
            targetPermanent, 
            isInheritedEffect, 
            condition: CanUseCondition, 
            conditionString, 
            cardCondition, 
            rootCardEffect: null, 
            card,
            isLinkedEffect);
    }
    #endregion

    #region Trigger effect of [Detach]
    public static ActivateClass DetachEffect(Permanent targetPermanent, bool isInheritedEffect, Func<bool> condition, string conditionString, Func<CardSource, bool> cardCondition, ICardEffect rootCardEffect, CardSource card, bool isLinkedEffect = false)
    {
        if (targetPermanent == null) return null;
        if (targetPermanent.TopCard == null) return null;
        if (card == null) return null;

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Detach", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, DataBase.DetachEffectDescription(conditionString));
        activateClass.SetHashString($"Detach_{card.CardID}" + (isInheritedEffect ? "_inherited" : ""));
        activateClass.SetIsInheritedEffect(isInheritedEffect);
        activateClass.SetIsLinkedEffect(isLinkedEffect);

        if (rootCardEffect != null)
        {
            activateClass.SetIsInheritedEffect(false);
            activateClass.SetIsLinkedEffect(false);
            activateClass.SetEffectSourcePermanent(targetPermanent);
            activateClass.SetRootCardEffect(rootCardEffect);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            if (CardEffectCommons.CanTriggerDetach(hashtable, targetPermanent))
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
            if (CardEffectCommons.CanActivateDetach(targetPermanent, cardCondition))
            {
                if (condition == null || condition())
                {
                    return true;
                }
            }

            return false;
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DetachProcess(targetPermanent, cardCondition, conditionString, activateClass));
        }

        return activateClass;
    }
    #endregion
}