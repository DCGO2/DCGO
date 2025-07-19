using System;
using System.Collections;

public partial class CardEffectCommons
{
    #region Target 1 Digimon can't suspend until opponent's turn end

    public static IEnumerator GainCantSuspendUntilOpponentTurnEnd(Permanent targetPermanent, ICardEffect activateClass)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        static bool CanUseCondition() => true;

        string effectName = "Can't suspend until the end of this card's owner's turn";

        yield return ContinuousController.instance.StartCoroutine(GainCanNotSuspend(
            targetPermanent: targetPermanent,
            effectDuration: EffectDuration.UntilOpponentTurnEnd,
            activateClass: activateClass,
            condition: CanUseCondition,
            effectName: effectName
        ));
    }

    #endregion

    #region Target 1 Digimon can't suspend

    public static IEnumerator GainCanNotSuspend(Permanent targetPermanent, EffectDuration effectDuration, ICardEffect activateClass, Func<bool> condition, string effectName)
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

        CanNotSuspendClass canNotSuspendClass = CardEffectFactory.CantSuspendStaticEffect(permanentCondition: PermanentCondition, isInheritedEffect: false, card: card, condition: CanUseCondition, effectName: effectName);

        AddEffectToPermanent(targetPermanent: targetPermanent, effectDuration: effectDuration, card: card, cardEffect: canNotSuspendClass, timing: EffectTiming.None);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(targetPermanent));
        }
    }

    #endregion
}