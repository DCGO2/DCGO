using System.Collections;
using System;

public partial class CardEffectFactory
{
    #region Trigger effect of [Overclock] on oneself

    public static ActivateClass OverclockSelfEffect(string trait, bool isInheritedEffect, CardSource card, Func<bool> condition,
        ICardEffect rootCardEffect = null)
    {
        bool TraitCondition(Permanent permanent)
        {
            return permanent.TopCard.ContainsTraits(trait);
        }

        return OverclockSelfEffect(
            permanentCondition: TraitCondition, 
            isInheritedEffect: isInheritedEffect,
            card: card,
            condition: condition,
            displayString: $"[{trait}] trait",
            rootCardEffect: rootCardEffect);
    }

    public static ActivateClass OverclockSelfEffect(Func<Permanent, bool> permanentCondition, bool isInheritedEffect, CardSource card, Func<bool> condition, string displayString,
        ICardEffect rootCardEffect = null)
    {
        Permanent targetPermanent = card.PermanentOfThisCard();

        bool CanUseCondition()
        {
            return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                   (condition == null || condition());
        }

        bool CanSelectPermanentCondition(Permanent permanent)
        {
            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                permanent != card.PermanentOfThisCard() &&
                (permanent.IsToken || permanentCondition(permanent));
        }

        return OverclockEffect(permanentCondition: CanSelectPermanentCondition, targetPermanent: targetPermanent, isInheritedEffect: isInheritedEffect,
            condition: CanUseCondition,
            rootCardEffect: rootCardEffect, card, displayString);
    }

    #endregion

    #region Trigger effect of [Overclock]

    public static ActivateClass OverclockEffect(Func<Permanent, bool> permanentCondition, Permanent targetPermanent, bool isInheritedEffect, Func<bool> condition,
        ICardEffect rootCardEffect, CardSource card, string displayString)
    {
        if (targetPermanent == null) return null;
        if (targetPermanent.TopCard == null) return null;
        if (card == null) return null;

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Overclock", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, DataBase.OverclockEffectDiscription(displayString));
        activateClass.SetIsInheritedEffect(isInheritedEffect);

        if (rootCardEffect != null)
        {
            activateClass.SetIsInheritedEffect(false);
            activateClass.SetEffectSourcePermanent(targetPermanent);
            activateClass.SetRootCardEffect(rootCardEffect);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass) &&
                   CardEffectCommons.IsOwnerTurn(card);
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanActivateOverclock(permanentCondition, targetPermanent.TopCard, activateClass) &&
                   (condition == null || condition());
        }

        IEnumerator ActivateCoroutine(Hashtable hashtable)
        {
            return CardEffectCommons.OverclockProcess(permanentCondition, targetPermanent.TopCard, activateClass);
        }

        return activateClass;
    }

    #endregion
}