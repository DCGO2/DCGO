using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Target 1 Tamer becomes a Digimon that can't digivolve
    public static IEnumerator BecomeDigimonThatCantDigivolve(Permanent targetPermanent, int DP, EffectDuration effectDuration, ICardEffect activateClass)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;
        if (DP < 0) yield break;

        CardSource card = activateClass.EffectSourceCard;

        bool PermanentCondition(Permanent permanent) => permanent == targetPermanent;

        bool CanUseCondition()
        {
            return IsPermanentExistsOnBattleArea(targetPermanent);
        }

        //treat as Digimon
        TreatAsDigimonClass treatAsDigimonClass = CardEffectFactory.TreatAsDigimonStaticEffect(
            permanentCondition: PermanentCondition, 
            isInheritedEffect: false, 
            card: card, 
            condition: CanUseCondition);

        AddEffectToPermanent(
            targetPermanent: targetPermanent, 
            effectDuration: effectDuration, 
            card: card, 
            cardEffect: treatAsDigimonClass, 
            timing: EffectTiming.None);

        //change origin DP
        ChangeBaseDPClass changeBaseDPClass = CardEffectFactory.ChangeBaseDPStaticEffect(
            targetPermanent: targetPermanent, 
            changeValue: DP, 
            isInheritedEffect: false, 
            card: card, 
            condition: CanUseCondition);

        AddEffectToPermanent(
            targetPermanent: targetPermanent, 
            effectDuration: effectDuration, 
            card: card, 
            cardEffect: changeBaseDPClass, 
            timing: EffectTiming.None);

        //can't Digivolve
        CanNotDigivolveClass canNotEvolveClass = CardEffectFactory.CanNotDigivolveStaticEffect(
            permanentCondition: PermanentCondition, 
            cardCondition: (cardSource) => true, 
            isInheritedEffect: false, 
            card: card, 
            condition: CanUseCondition, 
            effectName: "Can't digivolve");

        AddEffectToPermanent(
            targetPermanent: targetPermanent, 
            effectDuration: effectDuration, 
            card: card, 
            cardEffect: canNotEvolveClass, 
            timing: EffectTiming.None);

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
    }
    #endregion
}