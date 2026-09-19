using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using System.Runtime.CompilerServices;

public partial class CardEffectCommons
{
    #region Player gains effect to change play cost
    public static IEnumerator ChangePlayCostPlayerEffect(
        Func<Permanent, bool> permanentCondition,
        int changeValue,
        bool setFixedCost,
        EffectDuration effectDuration,
        ICardEffect activateClass)
    {
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;
        if (changeValue == 0) yield break;

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

        ChangeCostClass changeCostClass = CardEffectFactory.ChangePlayCostStaticEffect(
            changeValue: changeValue,
            permanentCondition: PermanentCondition,
            isInheritedEffect: false,
            card: activateClass.EffectSourceCard,
            condition: CanUseCondition,
            setFixedCost: setFixedCost);

        AddEffectToPlayer(effectDuration: effectDuration, card: activateClass.EffectSourceCard, cardEffect: changeCostClass, timing: EffectTiming.None);

        foreach (Permanent permanent in GManager.instance.turnStateMachine.gameContext.PermanentsForTurnPlayer)
        {
            if (PermanentCondition(permanent))
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(permanent));
            }
        }
    }
    #endregion
}