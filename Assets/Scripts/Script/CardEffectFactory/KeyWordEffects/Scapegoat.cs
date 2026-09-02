using System.Collections;
using System;

public partial class CardEffectFactory
{
    #region Trigger effect of [Scapegoat] on oneself
    public static ActivateClass ScapegoatSelfEffect(bool isInheritedEffect, CardSource card, Func<bool> condition, string effectName, ICardEffect rootCardEffect = null, bool isLinkedEffect = false)
    {
        Permanent targetPermanent = card.PermanentOfThisCard();

        bool CanUseCondition()
        {
            return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                && (condition == null || condition());
        }

        return ScapegoatEffect(targetPermanent: targetPermanent, isInheritedEffect: isInheritedEffect, condition: CanUseCondition, rootCardEffect: rootCardEffect, effectName: effectName, card, isLinkedEffect: isLinkedEffect);
    }
    #endregion

    #region Trigger effect of [Scapegoat]
    public static ActivateClass ScapegoatEffect(Permanent targetPermanent, bool isInheritedEffect, Func<bool> condition, ICardEffect rootCardEffect, string effectName, CardSource card, bool isLinkedEffect = false)
    {
        if (targetPermanent == null) return null;
        if (targetPermanent.TopCard == null) return null;
        if (card == null) return null;

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect(effectName, CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, DataBase.ScapegoatEffectDescription());
        activateClass.SetHashString($"Scapegoat_{card.CardID}" + (isInheritedEffect ? "_inherited" : ""));
        activateClass.SetIsInheritedEffect(isInheritedEffect);
        activateClass.SetIsLinkedEffect(isLinkedEffect);

        if (rootCardEffect != null)
        {
            activateClass.SetIsInheritedEffect(false);
            activateClass.SetEffectSourcePermanent(targetPermanent);
            activateClass.SetRootCardEffect(rootCardEffect);
        }

        bool CanSelectPermanentCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) && permanent != card.PermanentOfThisCard();

        bool PermanentCondition(Permanent permanent)
        {
            return permanent == targetPermanent;
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            bool CardEffectCondition(ICardEffect cardEffect) => CardEffectCommons.IsOwnerEffect(cardEffect,card);

            return CardEffectCommons.IsPermanentExistsOnBattleArea(targetPermanent)
                && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, PermanentCondition)
                && !CardEffectCommons.IsByEffect(hashtable, CardEffectCondition)
                && (condition == null || condition());
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanActivateScapegoat(targetPermanent, CanSelectPermanentCondition)
                && (condition == null || condition());
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            return CardEffectCommons.ScapegoatProcess(activateClass, targetPermanent, CanSelectPermanentCondition);
        }

        return activateClass;
    }
    #endregion

    #region Static effect of [Scapegoat]
    public static ScapegoatClass ScapegoatStaticEffect(Func<Permanent, bool> permanentCondition, bool isInheritedEffect, CardSource card, Func<bool> condition)
    {
        string effectName = "Scapegoat";

        ScapegoatClass scapegoateClass = new ScapegoatClass();
        scapegoateClass.SetUpICardEffect(effectName, CanUseCondition, card);
        scapegoateClass.SetUpScapegoatClass(PermanentCondition: PermanentCondition);

        if (isInheritedEffect)
        {
            scapegoateClass.SetIsInheritedEffect(true);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return condition == null || condition();
        }

        bool PermanentCondition(Permanent permanent)
        {
            return CardEffectCommons.IsPermanentExistsOnBattleArea(permanent)
                && (permanentCondition == null || permanentCondition(permanent));
        }

        return scapegoateClass;
    }
    #endregion
}