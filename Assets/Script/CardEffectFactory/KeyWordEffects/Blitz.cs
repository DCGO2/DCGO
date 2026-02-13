using System;
using System.Collections;
using System.Collections.Generic;

public partial class CardEffectFactory
{
    #region Trigger effect of [Blitz] on oneself
    public static ActivateClass BlitzSelfEffect(
        bool isInheritedEffect,
        CardSource card,
        Func<bool> condition,
        bool isWhenDigivolving,
        ICardEffect rootCardEffect = null)
    {
        Permanent targetPermanent = card.PermanentOfThisCard() ?? new Permanent(new List<CardSource>() { card });

        bool CanUseCondition()
        {
            if (condition == null || condition())
            {
                return true;
            }

            return false;
        }

        return BlitzEffect(
            targetPermanent: targetPermanent,
            isInheritedEffect: isInheritedEffect,
            condition: CanUseCondition,
            isWhenDigivolving: isWhenDigivolving,
            rootCardEffect: rootCardEffect,
            card: card);
    }
    #endregion

    #region Trigger effect of [Blitz]
    public static ActivateClass BlitzEffect(
        Permanent targetPermanent,
        bool isInheritedEffect,
        Func<bool> condition,
        bool isWhenDigivolving,
        ICardEffect rootCardEffect,
        CardSource card,
        Func<IEnumerator> beforeOnAttackCoroutine = null)
    {
        if (targetPermanent == null) return null;
        if (targetPermanent.TopCard == null) return null;
        if (card == null) return null;

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Blitz", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
        activateClass.SetIsInheritedEffect(isInheritedEffect);

        string EffectDiscription()
        {
            if (isWhenDigivolving)
            {
                return $"[When Digivolving] {DataBase.BilitzEffectDiscription()}.";
            }

            else
            {
                return $"[On Play] {DataBase.BilitzEffectDiscription()}.";
            }
        }

        if (rootCardEffect != null)
        {
            activateClass.SetIsInheritedEffect(false);
            activateClass.SetEffectSourcePermanent(targetPermanent);
            activateClass.SetRootCardEffect(rootCardEffect);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            if (isWhenDigivolving)
            {
                if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                {
                    return true;
                }
            }

            else
            {
                if (CardEffectCommons.CanTriggerOnPlay(hashtable, card))
                {
                    return true;
                }
            }

            return false;
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            if (CardEffectCommons.CanActivateBlitz(targetPermanent.TopCard, activateClass))
            {
                if (condition == null || condition())
                {
                    return true;
                }
            }

            return false;
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            return CardEffectCommons.BlitzProcess(targetPermanent.TopCard, activateClass, beforeOnAttackCoroutine);
        }

        return activateClass;
    }
    #endregion
}