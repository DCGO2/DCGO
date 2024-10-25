using System.Collections;
using System;

public partial class CardEffectFactory
{
    #region Trigger effect of [Decode] on oneself

    public static ActivateClass DecodeSelfEffect(CardColor color, int level, bool isInheritedEffect, CardSource card, Func<bool> condition,
        ICardEffect rootCardEffect = null)
    {
        Permanent targetPermanent = card.PermanentOfThisCard();

        bool CanUseCondition()
        {
            return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                   (condition == null || condition());
        }

        return DecodeEffect(color: color, level: level, targetPermanent: targetPermanent, isInheritedEffect: isInheritedEffect,
            condition: CanUseCondition, rootCardEffect: rootCardEffect, card);
    }

    #endregion

    #region Trigger effect of [Decode]

    public static ActivateClass DecodeEffect(CardColor color, int level, Permanent targetPermanent, bool isInheritedEffect,
        Func<bool> condition, ICardEffect rootCardEffect, CardSource card)
    {
        if (targetPermanent == null) return null;
        if (targetPermanent.TopCard == null) return null;
        if (card == null) return null;

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Decode", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, DataBase.DecodeEffectDiscription(color, level));
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
            return CardEffectCommons.CanActivateDecode(color, level, targetPermanent.TopCard, activateClass) &&
                   (condition == null || condition());
        }

        IEnumerator ActivateCoroutine(Hashtable hashtable)
        {
            return CardEffectCommons.DecodeProcess(color, level, targetPermanent.TopCard, activateClass);
        }

        return activateClass;
    }

    #endregion
}