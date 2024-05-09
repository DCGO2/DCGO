using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Trigger effect of [Partition] on oneself
    public static ActivateClass PartitionSelfEffect(bool isInheritedEffect, CardSource card, Func<bool> condition, Func<CardSource, bool> canSelectFirstSourceCondition, Func<CardSource, bool> canSelectSecondSourceCondition)
    {
        Permanent targetPermanent = card.PermanentOfThisCard();

        bool CanUseCondition()
        {
            if (condition == null || condition())
            {
                return true;
            }

            return false;
        }

        return PartitionEffect(targetPermanent: targetPermanent, isInheritedEffect: isInheritedEffect, condition: CanUseCondition, firstSourceCondition: canSelectFirstSourceCondition, secondSourceCondition: canSelectSecondSourceCondition, card);
    }
    #endregion

    #region Trigger effect of [Partition]
    public static ActivateClass PartitionEffect(Permanent targetPermanent, bool isInheritedEffect, Func<bool> condition, Func<CardSource, bool> firstSourceCondition, Func<CardSource, bool> secondSourceCondition, CardSource card)
    {
        if (targetPermanent == null) return null;
        if (targetPermanent.TopCard == null) return null;
        if (card == null) return null;

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Partition", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, DataBase.PartitionEffectDiscription());
        activateClass.SetHashString($"Partition_{card.CardID}" + (isInheritedEffect ? "_inherited" : ""));
        activateClass.SetIsInheritedEffect(isInheritedEffect);

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
            return CardEffectCommons.CanActivatePartition(targetPermanent);
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            return CardEffectCommons.PartitionProcess(activateClass, targetPermanent, firstSourceCondition, secondSourceCondition);
        }

        return activateClass;
    }
    #endregion
}