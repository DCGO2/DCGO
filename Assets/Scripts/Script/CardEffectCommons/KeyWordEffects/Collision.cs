using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Target 1 Digimon gains [Collision]
    public static IEnumerator GainCollision(Permanent targetPermanent, EffectDuration effectDuration, ICardEffect activateClass)
    {
        if (targetPermanent == null) yield break;

        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;

        if (activateClass == null) yield break;

        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = targetPermanent.TopCard;

        bool PermanentCondition(Permanent permanent) => permanent == targetPermanent;

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

        CollisionClass collision = CardEffectFactory.CollisionSelfStaticEffect(false, targetPermanent.TopCard, CanUseCondition);

        AddEffectToPermanent(targetPermanent: targetPermanent, effectDuration: effectDuration, card: card, cardEffect: collision, timing: EffectTiming.OnCounterTiming);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
        }
    }
    #endregion
}