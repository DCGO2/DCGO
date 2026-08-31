using System;
using System.Collections;

public partial class CardEffectCommons
{
    #region Target 1 Digimon can't digivolve

    public static IEnumerator GainCanNotDigivolve(Permanent targetPermanent, EffectDuration effectDuration, ICardEffect activateClass, Func<bool> permanentCondition, Func<CardSource, bool> cardCondition, bool isInheritedEffect, string effectName)
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
                if (permanentCondition == null || permanentCondition())
                {
                    if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        CanNotDigivolveClass canNotDigivolveClass = CardEffectFactory.CanNotDigivolveStaticEffect(
            permanentCondition: PermanentCondition,
            cardCondition: cardCondition,
            isInheritedEffect: isInheritedEffect,
            card: card,
            condition: CanUseCondition,
            effectName: effectName);

        AddEffectToPermanent(targetPermanent: targetPermanent, effectDuration: effectDuration, card: card, cardEffect: canNotDigivolveClass, timing: EffectTiming.None);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(targetPermanent));
        }
    }

    #endregion
}