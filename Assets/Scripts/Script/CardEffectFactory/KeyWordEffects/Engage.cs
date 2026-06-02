using System;
using System.Collections;
using System.Collections.Generic;

public partial class CardEffectFactory
{
    #region [Engage] self effect
    public static ActivateClass EngageSelfStaticEffect(bool isInheritedEffect, CardSource card, Func<bool> condition, bool isLinkedEffect = false)
    {
        return EngageEffect(
            targetPermanent: card.PermanentOfThisCard(),
            isInheritedEffect,
            condition,
            rootCardEffect: null,
            card,
            beforeOnAttackCoroutine: null,
            isLinkedEffect
        );
    }
    #endregion

    #region [Engage] effect
    public static ActivateClass EngageEffect(
        Permanent targetPermanent,
        bool isInheritedEffect,
        Func<bool> condition,
        ICardEffect rootCardEffect,
        CardSource card,
        Func<IEnumerator> beforeOnAttackCoroutine = null,
        bool isLinkedEffect = false)
    {
        if (targetPermanent == null) return null;
        if (targetPermanent.TopCard == null) return null;
        if (card == null) return null;

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Engage", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
        activateClass.SetIsInheritedEffect(isInheritedEffect);

        string EffectDescription()
        {
            return $"{DataBase.EngageEffectDescription()}";
        }

        if (rootCardEffect != null)
        {
            activateClass.SetIsInheritedEffect(false);
            activateClass.SetEffectSourcePermanent(targetPermanent);
            activateClass.SetRootCardEffect(rootCardEffect);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.IsOwnerTurn(card)
                && CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(targetPermanent);
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanActivateEngage(targetPermanent.TopCard, activateClass)
                && (condition == null
                    || condition());
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            return CardEffectCommons.EngageProcess(targetPermanent.TopCard, activateClass, beforeOnAttackCoroutine);
        }

        return activateClass;
    }
    #endregion
}