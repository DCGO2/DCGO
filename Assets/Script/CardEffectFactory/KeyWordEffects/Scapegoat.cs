using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using System.Runtime.InteropServices.WindowsRuntime;

public partial class CardEffectFactory
{
    #region Trigger effect of [Scapegoat] on oneself
    public static ActivateClass ScapegoatSelfEffect(bool isInheritedEffect, CardSource card, Func<bool> condition, string effectName, string effectDiscription, ICardEffect rootCardEffect = null, bool isLinkedEffect = false)
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

        return ScapegoatEffect(targetPermanent: targetPermanent, isInheritedEffect: isInheritedEffect, condition: CanUseCondition, rootCardEffect: rootCardEffect, effectName: effectName, effectDiscription: effectDiscription, card, isLinkedEffect: isLinkedEffect);
    }
    #endregion

    #region Trigger effect of [Scapegoat]
    public static ActivateClass ScapegoatEffect(Permanent targetPermanent, bool isInheritedEffect, Func<bool> condition, ICardEffect rootCardEffect, string effectName, string effectDiscription, CardSource card, bool isLinkedEffect = false)
    {
        if (targetPermanent == null) return null;
        if (targetPermanent.TopCard == null) return null;
        if (card == null) return null;

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect(effectName, CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, effectDiscription);
        activateClass.SetHashString($"Scapegoat_{card.CardID}" + (isInheritedEffect ? "_inherited" : ""));
        activateClass.SetIsInheritedEffect(isInheritedEffect);
        activateClass.SetIsLinkedEffect(isLinkedEffect);

        if (rootCardEffect != null)
        {
            activateClass.SetIsInheritedEffect(false);
            activateClass.SetEffectSourcePermanent(targetPermanent);
            activateClass.SetRootCardEffect(rootCardEffect);
        }

        bool CanSelectPermanentCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) && permanent != card.PermanentOfThisCard();

        bool PermanentCondition(Permanent permanent)
        {
            if(permanent == targetPermanent)
            {
                return true;
            }
            return false;
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            bool CardEffectCondition(ICardEffect cardEffect) => CardEffectCommons.IsOwnerEffect(cardEffect,card);

            if (CardEffectCommons.IsPermanentExistsOnBattleArea(targetPermanent))
            {
                if (CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, PermanentCondition))
                {
                    if (CardEffectCommons.IsByEffect(hashtable, CardEffectCondition))
                        return false;

                    if (condition == null || condition())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            if (CardEffectCommons.CanActivateScapegoat(targetPermanent, CanSelectPermanentCondition))
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
            return CardEffectCommons.ScapegoatProcess(activateClass, targetPermanent, CanSelectPermanentCondition);
        }

        return activateClass;
    }
    #endregion

    #region Static effect of [Scapegoat]
    public static ScapegoatClass ScapegoatStaticEffect(Func<Permanent, bool> permanentCondition, bool isInheritedEffect, CardSource card, Func<bool> condition)
    {
        string effectName = "Scapegoat";

        ScapegoatClass scapegoateClass = new ScapegoatClass();
        scapegoateClass.SetUpICardEffect(effectName, CanUseCondition, card);
        scapegoateClass.SetUpScapegoatClass(PermanentCondition: PermanentCondition);

        if (isInheritedEffect)
        {
            scapegoateClass.SetIsInheritedEffect(true);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return condition == null || condition();
        }

        bool PermanentCondition(Permanent permanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
            {
                if (permanentCondition == null || permanentCondition(permanent))
                {
                    return true;
                }
            }

            return false;
        }

        return scapegoateClass;
    }
    #endregion
}