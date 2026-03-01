using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect of [Collision] on oneself
    public static CollisionClass CollisionSelfStaticEffect(bool isInheritedEffect, CardSource card, Func<bool> condition)
    {
        bool PermanentCondition(Permanent permanent) => permanent == card.PermanentOfThisCard();

        bool CanUseCondition()
        {
            if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
            {
                if (card.Owner.Enemy.GetBattleAreaDigimons().Count() > 0)
                {
                    if (GManager.instance.attackProcess.IsAttacking)
                    {
                        if (GManager.instance.attackProcess.AttackingPermanent == card.PermanentOfThisCard())
                        {
                            if (condition == null || condition())
                            {
                                return true;
                            }
                        }
                    }     
                }
            }

            return false;
        }

        return CollisionStaticEffect(permanentCondition: PermanentCondition, isInheritedEffect: isInheritedEffect, card: card, condition: CanUseCondition);
    }
    #endregion

    #region Static effect of [Collision]
    public static CollisionClass CollisionStaticEffect(
        Func<Permanent, bool> permanentCondition,
        bool isInheritedEffect,
        CardSource card,
        Func<bool> condition)
    {
        CollisionClass collisionClass = new ();
        collisionClass.SetUpICardEffect("Collision", CanUseCondition, card);
        collisionClass.SetUpCollisionClass(PermanentCondition);
        collisionClass.SetIsInheritedEffect(isInheritedEffect);

        bool PermanentCondition(Permanent permanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
            {
                if (permanentCondition == null || permanentCondition(permanent))
                {
                    return true;
                }
            }

            return false;
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            if (condition == null || condition())
            {
                if(PermanentCondition(card.PermanentOfThisCard()))
                    return true;
            }

            return false;
        }

        return collisionClass;
    }
    #endregion
}