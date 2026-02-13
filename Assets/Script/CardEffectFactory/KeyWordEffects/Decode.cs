using System.Collections;
using System;

public partial class CardEffectFactory
{
    #region Trigger effect of [Decode] on oneself

    public static ActivateClass DecodeSelfEffect(CardSource card, bool isInheritedEffect, string[] decodeStrings, Func<CardSource, bool> sourceCondition, Func<bool> condition,
        ICardEffect rootCardEffect = null)
    {
        Permanent targetPermanent = card.PermanentOfThisCard();

        bool CanUseCondition()
        {
            return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                   (condition == null || condition());
        }
        if (sourceCondition == null) sourceCondition = _ => true;

        return DecodeEffect(targetPermanent: targetPermanent, isInheritedEffect: isInheritedEffect, decodeStrings,
            condition: CanUseCondition, sourceCondition: sourceCondition, rootCardEffect: rootCardEffect, card);
    }

    #endregion

    #region Trigger effect of [Decode]

    public static ActivateClass DecodeEffect(Permanent targetPermanent, bool isInheritedEffect, string[] decodeStrings,
        Func<bool> condition, Func<CardSource, bool> sourceCondition, ICardEffect rootCardEffect, CardSource card)
    {
        if (targetPermanent == null) return null;
        if (targetPermanent.TopCard == null) return null;
        if (card == null) return null;
        if (sourceCondition == null) sourceCondition = _ => true;

        // EX: "Decode (Red/Black)"
        string effectname = $"Decode {decodeStrings[0]}";

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect(effectname, CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, DataBase.DecodeEffectDiscription(decodeStrings));
        activateClass.SetIsInheritedEffect(isInheritedEffect);

        if (rootCardEffect != null)
        {
            activateClass.SetIsInheritedEffect(false);
            activateClass.SetEffectSourcePermanent(targetPermanent);
            activateClass.SetRootCardEffect(rootCardEffect);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                   CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card) &&
                   !CardEffectCommons.IsByBattle(hashtable);
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanActivateDecode(targetPermanent.TopCard, sourceCondition, activateClass) &&
                   (condition == null || condition());
        }

        IEnumerator ActivateCoroutine(Hashtable hashtable)
        {
            return CardEffectCommons.DecodeProcess(targetPermanent.TopCard, sourceCondition, decodeStrings, activateClass);
        }

        return activateClass;
    }

    #endregion
}