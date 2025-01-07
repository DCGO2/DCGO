using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Trigger effect of [Fragment] on oneself
    public static ActivateClass FragmentSelfEffect(bool isInheritedEffect, CardSource card, Func<bool> condition, int trashValue, string effectName, string effectDiscription, ICardEffect rootCardEffect = null)
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

        return FragmentEffect(targetPermanent: targetPermanent, isInheritedEffect: isInheritedEffect, condition: CanUseCondition, rootCardEffect: rootCardEffect, trashValue: trashValue, effectName: effectName, effectDiscription: effectDiscription, card);
    }
    #endregion

    #region Trigger effect of [Fragment]
    public static ActivateClass FragmentEffect(Permanent targetPermanent, bool isInheritedEffect, Func<bool> condition, ICardEffect rootCardEffect, int trashValue, string effectName, string effectDiscription, CardSource card)
    {
        if (targetPermanent == null) return null;
        if (targetPermanent.TopCard == null) return null;
        if (card == null) return null;

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect(effectName, CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, effectDiscription);
        activateClass.SetHashString($"Fragment_{card.CardID}" + (isInheritedEffect ? "_inherited" : ""));
        activateClass.SetIsInheritedEffect(isInheritedEffect);

        if (rootCardEffect != null)
        {
            activateClass.SetIsInheritedEffect(false);
            activateClass.SetEffectSourcePermanent(targetPermanent);
            activateClass.SetRootCardEffect(rootCardEffect);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            Debug.Log($"FRAGMENT CAN USE: {CardEffectCommons.IsPermanentExistsOnBattleArea(targetPermanent)}");
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(targetPermanent))
            {
                Debug.Log($"FRAGMENT CAN USE: {CardEffectCommons.CanTriggerWhenRemoveField(hashtable, targetPermanent.TopCard)}");
                if (CardEffectCommons.CanTriggerWhenRemoveField(hashtable, targetPermanent.TopCard))
                {
                    if (condition == null || condition())
                    {
                        Debug.Log($"FRAGMENT CAN USE: TRUE");
                        return true;
                    }
                }
            }

            return false;
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            if (CardEffectCommons.CanActivateFragment(targetPermanent, trashValue, activateClass))
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
            return CardEffectCommons.FragmentProcess(activateClass, targetPermanent, trashValue);
        }

        return activateClass;
    }
    #endregion

}