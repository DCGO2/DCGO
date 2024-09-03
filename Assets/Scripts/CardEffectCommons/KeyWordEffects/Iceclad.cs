using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Target 1 Digimon gains [Iceclad]
    public static IEnumerator GainIceclad(Permanent targetPermanent, EffectDuration effectDuration, ICardEffect activateClass)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

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

        IcecladClass iceclad = CardEffectFactory.IcecladStaticEffect(permanentCondition: PermanentCondition, isInheritedEffect: false, card: card, condition: CanUseCondition);

        AddEffectToPermanent(targetPermanent: targetPermanent, effectDuration: effectDuration, card: card, cardEffect: iceclad, timing: EffectTiming.None);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
        }
    }
    #endregion

    #region Player gains effect to have Digimon gains [Iceclad]
    public static IEnumerator GainIcecladPlayerEffect(Func<Permanent, bool> permanentCondition, EffectDuration effectDuration, ICardEffect activateClass)
    {
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        bool PermanentCondition(Permanent permanent)
        {
            if (IsPermanentExistsOnBattleArea(permanent))
            {
                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                {
                    if (permanentCondition == null || permanentCondition(permanent))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool CanUseCondition()
        {
            return true;
        }

        IcecladClass iceclad = CardEffectFactory.IcecladStaticEffect(permanentCondition: PermanentCondition, isInheritedEffect: false, card: card, condition: CanUseCondition);

        AddEffectToPlayer(effectDuration: effectDuration, card: card, cardEffect: iceclad, timing: EffectTiming.None);

        foreach (Permanent permanent in GManager.instance.turnStateMachine.gameContext.PermanentsForTurnPlayer)
        {
            if (PermanentCondition(permanent))
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(permanent));
            }
        }
    }
    #endregion
}