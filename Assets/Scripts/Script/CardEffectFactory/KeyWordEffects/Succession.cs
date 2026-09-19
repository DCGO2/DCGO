using System;
using System.Collections;
using System.Collections.Generic;

public partial class CardEffectFactory
{    
    public static void SuccessionSelfEffect(ref List<ICardEffect> cardEffects, EffectTiming timing, CardSource card, Func<bool> condition, Func<CardSource, bool> cardCondition, bool isInheritedEffect = false, bool isLinkedEffect = false)
    {
        bool CanUseCondition(Hashtable hashtable)
        {
            return condition == null || condition();
        }

        CopyDigivolutionCardEffects(
                ref cardEffects, 
                timing, 
                card,
                isInheritedEffect,
                isLinkedEffect,
                canUseCondition: CanUseCondition,
                cardCondition: cardCondition,
                isSuccession: true);
    }
}