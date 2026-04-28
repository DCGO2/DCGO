using System;
using System.Collections;


public partial class CardEffectFactory
{
    #region Static effect of [Collision] on oneself
    public static CollisionClass CollisionSelfStaticEffect(bool isInheritedEffect, CardSource card, Func<bool> condition, bool isLinkedEffect = false)
    {
        bool PermanentCondition(Permanent permanent) => permanent == card.PermanentOfThisCard();

        bool CanUseCondition()
        {
            if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
            {
                if (condition == null || condition())
                {
                    return true;
                }
            }

            return false;
        }

        return CollisionStaticEffect(permanentCondition: PermanentCondition, isInheritedEffect: isInheritedEffect, card: card, condition: CanUseCondition, isLinkedEffect: isLinkedEffect);
    }
    #endregion

    #region Static effect of [Collision]
    public static CollisionClass CollisionStaticEffect(
        Func<Permanent, bool> permanentCondition,
        bool isInheritedEffect,
        CardSource card,
        Func<bool> condition,
        bool isLinkedEffect)
    {
        CollisionClass collisionClass = new();
        collisionClass.SetUpICardEffect("Collision", CanUseCondition, card);
        collisionClass.SetUpCollisionClass(PermanentCondition);
        collisionClass.SetIsInheritedEffect(isInheritedEffect);
        collisionClass.SetIsLinkedEffect(isLinkedEffect);

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
            return condition == null || condition();
        }

        return collisionClass;
    }
    #endregion
}