using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can activate [Blitz]
    public static bool CanActivateBlitz(CardSource cardSource, ICardEffect activateClass)
    {
        if (IsExistOnBattleArea(cardSource))
        {
            if (cardSource.PermanentOfThisCard().CanAttack(activateClass))
            {
                if (cardSource.Owner.Enemy.MemoryForPlayer >= 1)
                {
                    if (!GManager.instance.attackProcess.IsAttacking)
                    {
                        if (!GManager.instance.attackProcess.UsedBlitz)
                        {
                            return true;
                        }                                                
                    }
                }
            }
        }

        return false;
    }
    #endregion

    #region Effect process of [Blitz]
    public static IEnumerator BlitzProcess(CardSource cardSource, ICardEffect activateClass, Func<IEnumerator> beforeOnAttackCoroutine = null)
    {
        if (CanActivateBlitz(cardSource, activateClass))
        {
            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

            selectAttackEffect.SetUp(
                attacker: cardSource.PermanentOfThisCard(),
                canAttackPlayerCondition: () => true,
                defenderCondition: (permanent) => true,
                cardEffect: activateClass);

            selectAttackEffect.SetBeforeOnAttackCoroutine(beforeOnAttackCoroutine);

            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());

            GManager.instance.attackProcess.UsedBlitz = true;
        }
    }
    #endregion

    #region Target 1 Digimon gains [Blitz]
    public static IEnumerator GainBlitz(Permanent targetPermanent, EffectDuration effectDuration, ICardEffect activateClass, bool isWhenDigivolving)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

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

        ActivateClass blitz = CardEffectFactory.BlitzEffect(
            targetPermanent: targetPermanent,
            isInheritedEffect: false,
            condition: CanUseCondition,
            isWhenDigivolving: isWhenDigivolving,
            rootCardEffect: activateClass,
            card: targetPermanent.TopCard);

        AddEffectToPermanent(
            targetPermanent: targetPermanent,
            effectDuration: effectDuration,
            card: card,
            cardEffect: blitz,
            timing: EffectTiming.OnEnterFieldAnyone);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
        }
    }
    #endregion
}