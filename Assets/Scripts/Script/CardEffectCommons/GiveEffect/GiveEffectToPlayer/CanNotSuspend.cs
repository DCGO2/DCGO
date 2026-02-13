using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Player gains effect to have Digimon can't suspend
    public static IEnumerator GainCanNotSuspendPlayerEffect(Func<Permanent, bool> permanentCondition, EffectDuration effectDuration, ICardEffect activateClass, bool isOnlyActivePhase, string effectName)
    {
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        bool _PermanentCondition(Permanent permanent)
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

        bool PermanentCondition(Permanent permanent)
        {
            if (_PermanentCondition(permanent))
            {
                if (!isOnlyActivePhase || GManager.instance.turnStateMachine.gameContext.TurnPlayer == permanent.TopCard.Owner)
                {
                    return true;
                }
            }

            return false;
        }

        bool CanUseCondition()
        {
            return !isOnlyActivePhase || GManager.instance.turnStateMachine.gameContext.TurnPhase == GameContext.phase.Active;
        }

        CanNotSuspendClass canNotSuspendClass = CardEffectFactory.CantSuspendStaticEffect(
            permanentCondition: PermanentCondition,
            isInheritedEffect: false,
            card: card,
            condition: CanUseCondition,
            effectName: effectName);

        AddEffectToPlayer(effectDuration: effectDuration, card: card, cardEffect: canNotSuspendClass, timing: EffectTiming.None);

        foreach (Permanent permanent in GManager.instance.turnStateMachine.gameContext.PermanentsForTurnPlayer)
        {
            if (_PermanentCondition(permanent))
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(permanent));
            }
        }
    }
    #endregion
}