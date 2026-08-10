using System;
using System.Collections;

public partial class CardEffectFactory
{    
    #region Trigger effect of [Succession] on oneself
    public static ICardEffect SuccessionSelfEffect(bool isInheritedEffect, CardSource card, Func<bool> condition, Func<CardSource, bool> cardCondition, bool isLinkedEffect = false)
    {
        bool CanUseCondition(Hashtable hashtable)
        {
            return condition == null || condition();
        }

        return CopyDigivolutionCardEffects(card,
                isInheritedEffect,
                isLinkedEffect,
                canUseCondition: CanUseCondition,
                cardSourceCondition: cardCondition,
                isSuccession: true);
    }
    #endregion
}