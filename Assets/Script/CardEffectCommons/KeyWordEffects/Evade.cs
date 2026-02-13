using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
public partial class CardEffectCommons
{
    #region Can trigger [Evade]
    public static bool CanTriggerEvade(Hashtable hashtable, Permanent targetPermanent)
    {
        if (IsPermanentExistsOnBattleArea(targetPermanent))
        {
            if (CanTriggerWhenPermanentRemoveField(hashtable, permanent => permanent == targetPermanent))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Can activate [Evade]
    public static bool CanActivateEvade(Permanent targetPermanent)
    {
        if (IsPermanentExistsOnBattleArea(targetPermanent))
        {
            if (CanActivatePermanentSuspendCostEffect(targetPermanent))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Effect process of [Evade]
    public static IEnumerator EvadeProcess(Permanent targetPermanent, ICardEffect activateClass)
    {
        if (IsPermanentExistsOnBattleArea(targetPermanent))
        {
            yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { targetPermanent }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

            targetPermanent.willBeRemoveField = false;

            targetPermanent.HideDeleteEffect();
        }
    }
    #endregion

    #region Target 1 Digimon gains [Evade]
    public static IEnumerator GainEvade(Permanent targetPermanent, EffectDuration effectDuration, ICardEffect activateClass)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        bool CanUseCondition()
        {
            if (IsPermanentExistsOnBattleArea(targetPermanent))
            {
                if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
                {
                    return true;
                }
            }

            return false;
        }

        ActivateClass evade = CardEffectFactory.EvadeEffect(targetPermanent: targetPermanent, isInheritedEffect: false, condition: CanUseCondition, rootCardEffect: activateClass, targetPermanent.TopCard);

        AddEffectToPermanent(targetPermanent: targetPermanent, effectDuration: effectDuration, card: card, cardEffect: evade, timing: EffectTiming.WhenPermanentWouldBeDeleted);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
        }
    }
    #endregion
}