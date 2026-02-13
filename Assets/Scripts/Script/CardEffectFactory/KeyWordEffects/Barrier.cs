using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Trigger effect of [Barrier] on oneself
    public static ICardEffect BarrierSelfEffect(bool isInheritedEffect, CardSource card, Func<bool> condition)
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

        return BarrierEffect(targetPermanent: targetPermanent, isInheritedEffect: isInheritedEffect, condition: CanUseCondition, rootCardEffect: null, card);
    }
    #endregion

    #region Trigger effect of [Barrier]
    public static ActivateClass BarrierEffect(Permanent targetPermanent, bool isInheritedEffect, Func<bool> condition, ICardEffect rootCardEffect, CardSource card)
    {
        if (targetPermanent == null) return null;
        if (targetPermanent.TopCard == null) return null;
        if (card == null) return null;

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Barrier", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, DataBase.BarrierEffectDiscription());
        activateClass.SetHashString($"Barrier_{card.CardID}" + (isInheritedEffect ? "_inherited" : ""));
        activateClass.SetIsInheritedEffect(isInheritedEffect);

        if (rootCardEffect != null)
        {
            activateClass.SetIsInheritedEffect(false);
            activateClass.SetEffectSourcePermanent(targetPermanent);
            activateClass.SetRootCardEffect(rootCardEffect);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            if (CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, (permanent) => permanent.cardSources.Contains(card)))
            {
                if (CardEffectCommons.IsByBattle(hashtable))
                {
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
            if (CardEffectCommons.CanActivateBarrier(targetPermanent))
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
            return CardEffectCommons.BarrierProcess(targetPermanent, activateClass);
        }

        return activateClass;
    }
    #endregion
}