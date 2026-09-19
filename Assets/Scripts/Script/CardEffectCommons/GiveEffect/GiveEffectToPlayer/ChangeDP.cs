using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Player gains effect to change Digimon's DP
    public static IEnumerator ChangeDigimonDPPlayerEffect(
        Func<Permanent, bool> permanentCondition, 
        int changeValue, 
        EffectDuration effectDuration, 
        ICardEffect activateClass)
    {
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;
        if (changeValue == 0) yield break;

        CardSource card = activateClass.EffectSourceCard;
        bool isUpValue = changeValue > 0;

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

        ChangeDPClass changeDPClass = CardEffectFactory.ChangeDPStaticEffect(
            permanentCondition: PermanentCondition,
            changeValue: changeValue,
            isInheritedEffect: false,
            card: card,
            condition: CanUseCondition,
            effectName: null);

        AddEffectToPlayer(effectDuration: effectDuration, card: card, cardEffect: changeDPClass, timing: EffectTiming.None);

        foreach (Permanent permanent in GManager.instance.turnStateMachine.gameContext.PermanentsForTurnPlayer)
        {
            if (PermanentCondition(permanent))
            {
                if (isUpValue)
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(permanent));
                }

                else
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(permanent));
                }
            }
        }
    }
    #endregion
}