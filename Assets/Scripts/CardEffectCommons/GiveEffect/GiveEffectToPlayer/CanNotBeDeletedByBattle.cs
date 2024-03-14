using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Player gains effect to have Digimon can't be deleted by battle
    public static IEnumerator GainCanNotBeDeletedPlayerEffect(Func<Permanent, bool> permanentCondition, Func<Permanent, Permanent, Permanent, CardSource, bool> canNotBeDestroyedByBattleCondition, EffectDuration effectDuration, ICardEffect activateClass, string effectName)
    {
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        bool PermanentCondition(Permanent attacker)
        {
            if (IsPermanentExistsOnBattleArea(attacker))
            {
                if (!attacker.TopCard.CanNotBeAffected(activateClass))
                {
                    if (permanentCondition == null || permanentCondition(attacker))
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

        CanNotBeDestroyedByBattleClass canNotBeDestroyedByBattleClass = CardEffectFactory.CanNotBeDestroyedByBattleStaticEffect(
            permanentCondition: PermanentCondition,
            canNotBeDestroyedByBattleCondition: canNotBeDestroyedByBattleCondition,
            isInheritedEffect: false,
            card: card,
            condition: CanUseCondition,
            effectName: effectName);

        AddEffectToPlayer(effectDuration: effectDuration, card: card, cardEffect: canNotBeDestroyedByBattleClass, timing: EffectTiming.None);

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