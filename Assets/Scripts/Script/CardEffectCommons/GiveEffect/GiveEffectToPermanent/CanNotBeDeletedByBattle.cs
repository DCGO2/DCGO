using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{

    #region Target 1 Digimon can't be deleted by battle
    public static IEnumerator GainCanNotBeDeletedByBattle(Permanent targetPermanent, Func<Permanent, Permanent, Permanent, CardSource, bool> canNotBeDestroyedByBattleCondition, EffectDuration effectDuration, ICardEffect activateClass, string effectName)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        bool PermanentCondition(Permanent attacker) => attacker == targetPermanent;

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

        CanNotBeDestroyedByBattleClass canNotBeDestroyedByBattleClass = CardEffectFactory.CanNotBeDestroyedByBattleStaticEffect(
            canNotBeDestroyedByBattleCondition: canNotBeDestroyedByBattleCondition,
            permanentCondition: PermanentCondition,
            isInheritedEffect: false,
            card: card,
            condition: CanUseCondition,
            effectName: effectName);

        AddEffectToPermanent(
            targetPermanent: targetPermanent,
            effectDuration: effectDuration,
            card: card,
            cardEffect: canNotBeDestroyedByBattleClass,
            timing: EffectTiming.None);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
        }
    }
    #endregion
}