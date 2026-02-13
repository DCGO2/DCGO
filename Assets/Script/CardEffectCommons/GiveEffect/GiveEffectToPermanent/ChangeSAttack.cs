using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Change target 1 Digimon's SAttack
    public static IEnumerator ChangeDigimonSAttack(Permanent targetPermanent, int changeValue, EffectDuration effectDuration, ICardEffect activateClass)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (changeValue == 0) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;
        bool isUpValue = changeValue > 0;

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

        ChangeSAttackClass changeSAttackClass = CardEffectFactory.ChangeTargetSAttackStaticEffect(
                    targetPermanent: targetPermanent,
                    changeValue: changeValue,
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseCondition);

        AddEffectToPermanent(
            targetPermanent: targetPermanent,
            effectDuration: effectDuration,
            card: card,
            cardEffect: changeSAttackClass,
            timing: EffectTiming.None);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            if (isUpValue)
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
            }

            else
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(targetPermanent));
            }
        }
    }

    public static IEnumerator ChangeDigimonSAttack(Permanent targetPermanent, int changeValue, EffectDuration effectDuration, ICardEffect activateClass, bool activateAnimation, string hashstring = null)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (changeValue == 0) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;
        bool isUpValue = changeValue > 0;

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

        ChangeSAttackClass changeSAttackClass = CardEffectFactory.ChangeTargetSAttackStaticEffect(
                    targetPermanent: targetPermanent,
                    changeValue: changeValue,
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseCondition,
                    hashstring: hashstring);

        AddEffectToPermanent(
            targetPermanent: targetPermanent,
            effectDuration: effectDuration,
            card: card,
            cardEffect: changeSAttackClass,
            timing: EffectTiming.None);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            if (isUpValue)
            {
                if (activateAnimation)
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
                }
            }

            else
            {
                if (activateAnimation)
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(targetPermanent));
                }
            }
        }
    }
    #endregion


    #region Invert target 1 Digimon's SAttack
    public static IEnumerator InverteDigimonSAttack(Permanent targetPermanent, int changeValue, EffectDuration effectDuration, ICardEffect activateClass)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (changeValue == 0) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;
        bool isUpValue = changeValue > 0;

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

        InvertSAttackClass invertSAttackClass = CardEffectFactory.InvertTargetSAttackStaticEffect(
                    targetPermanent: targetPermanent,
                    changeValue: changeValue,
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseCondition);

        AddEffectToPermanent(
            targetPermanent: targetPermanent,
            effectDuration: effectDuration,
            card: card,
            cardEffect: invertSAttackClass,
            timing: EffectTiming.None);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            if (!isUpValue)
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
            }

            else
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(targetPermanent));
            }
        }
    }

    public static IEnumerator InvertDigimonSAttack(Permanent targetPermanent, int changeValue, EffectDuration effectDuration, ICardEffect activateClass, bool activateAnimation)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (changeValue == 0) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;
        bool isUpValue = changeValue > 0;

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

        InvertSAttackClass invertSAttackClass = CardEffectFactory.InvertTargetSAttackStaticEffect(
                    targetPermanent: targetPermanent,
                    changeValue: changeValue,
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseCondition);

        AddEffectToPermanent(
            targetPermanent: targetPermanent,
            effectDuration: effectDuration,
            card: card,
            cardEffect: invertSAttackClass,
            timing: EffectTiming.None);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            if (activateAnimation)
            {
                if (!isUpValue)
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
                }

                else
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(targetPermanent));
                }
            }
        }
    }
    #endregion
}