using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can activate [Barrier]
    public static bool CanActivateBarrier(Permanent permanent)
    {
        if (IsPermanentExistsOnBattleArea(permanent))
        {
            if (permanent.TopCard.Owner.SecurityCards.Count >= 1)
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Effect process of [Barrier]
    public static IEnumerator BarrierProcess(Permanent permanent, ICardEffect activateClass)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                CardSource topCard = permanent.TopCard;

                if (topCard.Owner.SecurityCards.Count >= 1)
                {
                    permanent.ShowDeleteEffect();

                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                            player: topCard.Owner,
                                            destroySecurityCount: 1,
                                            cardEffect: activateClass,
                                            fromTop: true).DestroySecurity());

                    permanent.willBeRemoveField = false;

                    permanent.HideDeleteEffect();

                    #region log
                    string log = "";

                    log += $"\nBarrier :";

                    log += $"\n{topCard.BaseENGCardNameFromEntity}({topCard.CardID})";

                    log += "\n";

                    PlayLog.OnAddLog?.Invoke(log);
                    #endregion
                }
            }
        }
    }
    #endregion

    #region Target 1 Digimon gains [Barrier]
    public static IEnumerator GainBarrier(Permanent targetPermanent, EffectDuration effectDuration, ICardEffect activateClass)
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

        ActivateClass retaliation = CardEffectFactory.BarrierEffect(
            targetPermanent: targetPermanent,
            isInheritedEffect: false,
            condition: CanUseCondition,
            rootCardEffect: activateClass,
            card: targetPermanent.TopCard);

        AddEffectToPermanent(
            targetPermanent: targetPermanent,
            effectDuration: effectDuration,
            card: card,
            cardEffect: retaliation,
            timing: EffectTiming.WhenPermanentWouldBeDeleted);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
        }
    }
    #endregion
}