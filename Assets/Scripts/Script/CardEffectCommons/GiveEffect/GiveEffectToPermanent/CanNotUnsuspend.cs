using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Target 1 Digimon can't unsuspend at the next active phase
    public static IEnumerator GainCantUnsuspendNextActivePhase(Permanent targetPermanent, ICardEffect activateClass)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        bool CanUseCondition()
        {
            if (IsOpponentTurn(card))
            {
                if (GManager.instance.turnStateMachine.gameContext.TurnPhase == GameContext.phase.Active)
                {
                    return true;
                }
            }

            return false;
        }

        string effectName = "Can't unsuspend during next unsuspend phase";

        yield return ContinuousController.instance.StartCoroutine(GainCanNotUnsuspend(
            targetPermanent: targetPermanent,
            effectDuration: EffectDuration.UntilNextUntap,
            activateClass: activateClass,
            condition: CanUseCondition,
            effectName: effectName
        ));
    }
    #endregion

    #region Target 1 Digimon can't unsuspend until opponent's turn end
    public static IEnumerator GainCantUnsuspendUntilOpponentTurnEnd(Permanent targetPermanent, ICardEffect activateClass)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        static bool CanUseCondition() => true;

        string effectName = "Can't unsuspend until the end of this card's owner's turn";

        yield return ContinuousController.instance.StartCoroutine(GainCanNotUnsuspend(
            targetPermanent: targetPermanent,
            effectDuration: EffectDuration.UntilOpponentTurnEnd,
            activateClass: activateClass,
            condition: CanUseCondition,
            effectName: effectName
        ));
    }
    #endregion

    #region Target 1 Digimon can't unsuspend
    public static IEnumerator GainCanNotUnsuspend(Permanent targetPermanent, EffectDuration effectDuration, ICardEffect activateClass, Func<bool> condition, string effectName)
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
                if (condition == null || condition())
                {
                    if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        CanNotUnsuspendClass canNotUnsuspendClass = CardEffectFactory.CantUnsuspendStaticEffect(permanentCondition: PermanentCondition, isInheritedEffect: false, card: card, condition: CanUseCondition, effectName: effectName);

        AddEffectToPermanent(targetPermanent: targetPermanent, effectDuration: effectDuration, card: card, cardEffect: canNotUnsuspendClass, timing: EffectTiming.None);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(targetPermanent));
        }
    }
    #endregion
}
