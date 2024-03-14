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
                        return true;
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
        }
    }
    #endregion
}