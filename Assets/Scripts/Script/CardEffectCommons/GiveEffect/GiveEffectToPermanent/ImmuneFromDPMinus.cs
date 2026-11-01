using System.Collections;
using System;

public partial class CardEffectCommons
{
    #region Target 1 Digimon can't reduce DP
    public static IEnumerator GainImmuneFromDPMinus(Permanent targetPermanent, Func<ICardEffect, bool> cardEffectCondition, EffectDuration effectDuration, ICardEffect activateClass, string effectName)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        bool PermanentCondition(Permanent attacker) => attacker == targetPermanent;

        bool CanUseCondition()
        {
            return IsPermanentExistsOnBattleArea(targetPermanent)
                && !targetPermanent.TopCard.CanNotBeAffected(activateClass);
        }

        ImmuneFromDPMinusClass immuneFromDPMinusClass = CardEffectFactory.ImmuneFromDPMinusStaticEffect(
            permanentCondition: PermanentCondition,
            cardEffectCondition: cardEffectCondition,
            isInheritedEffect: false,
            card: card,
            condition: CanUseCondition,
            effectName: effectName);

        immuneFromDPMinusClass.SetIsOptionEffect(activateClass.IsOptionEffect);

        AddEffectToPermanent(
            targetPermanent: targetPermanent,
            effectDuration: effectDuration,
            card: card,
            cardEffect: immuneFromDPMinusClass,
            timing: EffectTiming.None);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
        }
    }
    #endregion
}