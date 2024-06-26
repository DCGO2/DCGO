using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Player gains effect to change Digimon's SAttack
    public static IEnumerator ChangeDigimonSAttackPlayerEffect(
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

        ChangeSAttackClass changeDPClass = CardEffectFactory.ChangeSAttackStaticEffect(
            permanentCondition: PermanentCondition,
            changeValue: changeValue,
            isInheritedEffect: false,
            card: card,
            condition: CanUseCondition);

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


    #region Player gains effect to invert Digimon's SAttack
    public static IEnumerator InvertDigimonSAttackPlayerEffect(
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

        InvertSAttackClass invertSAttackClass = CardEffectFactory.InvertSAttackStaticEffect(
            permanentCondition: PermanentCondition,
            changeValue: changeValue,
            isInheritedEffect: false,
            card: card,
            condition: CanUseCondition);

        AddEffectToPlayer(effectDuration: effectDuration, card: card, cardEffect: invertSAttackClass, timing: EffectTiming.None);

        foreach (Permanent permanent in GManager.instance.turnStateMachine.gameContext.PermanentsForTurnPlayer)
        {
            if (PermanentCondition(permanent))
            {
                if (!isUpValue)
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