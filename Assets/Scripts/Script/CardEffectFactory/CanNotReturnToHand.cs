using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect that can't return to hand by effect
    public static CannotReturnToHandClass CannotReturnToHandStaticEffect(Func<Permanent, bool> permanentCondition, Func<ICardEffect, bool> cardEffectCondition, bool isInheritedEffect, CardSource card, Func<bool> condition, string effectName)
    {
        CannotReturnToHandClass cannotReturnToHandClass = new CannotReturnToHandClass();
        cannotReturnToHandClass.SetUpICardEffect(effectName, CanUseCondition, card);
        cannotReturnToHandClass.SetUpCannotReturnToHandClass(permanentCondition: PermanentCondition, cardEffectCondition: CardEffectCondition);

        if (isInheritedEffect)
        {
            cannotReturnToHandClass.SetIsInheritedEffect(true);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return condition == null || condition();
        }

        bool PermanentCondition(Permanent permanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
            {
                if (!permanent.TopCard.CanNotBeAffected(cannotReturnToHandClass))
                {
                    if (permanentCondition == null || permanentCondition(permanent))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool CardEffectCondition(ICardEffect cardEffect)
        {
            if (cardEffectCondition == null || cardEffectCondition(cardEffect))
            {
                return true;
            }

            return false;
        }

        return cannotReturnToHandClass;
    }
    #endregion
}