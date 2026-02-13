using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Target 1 Digimon can't be attacked
    public static IEnumerator GainCanNotBeAttacked(
        Permanent targetPermanent,
        Func<Permanent, bool> attackerCondition,
        EffectDuration effectDuration,
        ICardEffect activateClass,
        string effectName)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        bool AttackerCondition(Permanent defender)
        {
            if (attackerCondition == null || attackerCondition(defender))
            {
                return true;
            }

            return false;
        }

        bool DefenderCondition(Permanent attacker) => attacker == targetPermanent;

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

        CanNotAttackTargetDefendingPermanentClass canNotAttackClass = CardEffectFactory.CanNotAttackStaticEffect(
            attackerCondition: AttackerCondition,
            defenderCondition: DefenderCondition,
            isInheritedEffect: false,
            card: card,
            condition: CanUseCondition,
            effectName: effectName);

        AddEffectToPermanent(
            targetPermanent: targetPermanent,
            effectDuration: effectDuration,
            card: card,
            cardEffect: canNotAttackClass,
            timing: EffectTiming.None);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
        }
    }
    #endregion
}