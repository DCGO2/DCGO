using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can activate [Collision]
    public static bool CanActivateCollision(CardSource cardSource)
    {
        if (IsExistOnBattleArea(cardSource))
        {
            if (cardSource.Owner.Enemy.GetBattleAreaDigimons().Count >= 1)
            {
                if (GManager.instance.attackProcess.IsAttacking)
                {
                    return true;
                }
            }
        }

        return false;
    }
    #endregion

    #region Effect process of [Collision]
    public static IEnumerator CollisionProcess(CardSource cardSource, ICardEffect activateClass, Func<IEnumerator> beforeOnAttackCoroutine = null)
    {
        List<Permanent> enemyDigimons = cardSource.Owner.Enemy.GetBattleAreaDigimons();

        if (CanActivateCollision(cardSource))
        {
            foreach (Permanent enemyDigimon in enemyDigimons)
            {
                if (enemyDigimon.TopCard.CanNotBeAffected(activateClass))
                    continue;

                yield return ContinuousController.instance.StartCoroutine(GainBlocker(
                        targetPermanent: enemyDigimon,
                        effectDuration: EffectDuration.UntilEndAttack,
                        activateClass: activateClass));  
            }   
            
            if(HasMatchConditionOpponentsPermanent(cardSource,permanent => permanent.HasBlocker))
                GManager.instance.attackProcess.IsBlocking = true;
        }
    }
    #endregion
}