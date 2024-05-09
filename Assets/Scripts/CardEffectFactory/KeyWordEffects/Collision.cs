using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect of [Collision] on oneself
    public static ActivateClass CollisionSelfStaticEffect(bool isInheritedEffect, CardSource card, Func<IEnumerator> beforeOnAttackCoroutine = null)
    {
        bool PermanentCondition(Permanent permanent) => permanent == card.PermanentOfThisCard();

        bool CanUseCondition()
        {
            if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
            {
                if(card.Owner.Enemy.GetBattleAreaDigimons().Count() > 0)
                {
                    return true;
                }
            }

            return false;
        }

        return CollisionStaticEffect(isInheritedEffect: isInheritedEffect, card: card, beforeOnAttackCoroutine: beforeOnAttackCoroutine);
    }
    #endregion

    #region Static effect of [Collision]
    public static ActivateClass CollisionStaticEffect(
        bool isInheritedEffect,
        CardSource card,
        Func<IEnumerator> beforeOnAttackCoroutine = null)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Collision", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
        activateClass.SetIsInheritedEffect(isInheritedEffect);

        string EffectDiscription()
        {
            return DataBase.CollisionEffectDiscription();
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            return true;
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            return CardEffectCommons.CollisionProcess(card, activateClass, beforeOnAttackCoroutine);
        }

        return activateClass;
    }
    #endregion
}