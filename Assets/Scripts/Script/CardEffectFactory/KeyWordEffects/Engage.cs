using System;
using System.Collections;
using System.Collections.Generic;

public partial class CardEffectFactory
{
    #region Trigger effect of [Engage]
    public static ActivateClass EngageEffect(
        Permanent targetPermanent,
        bool isInheritedEffect,
        Func<bool> condition,
        ICardEffect rootCardEffect,
        CardSource card,
        Func<IEnumerator> beforeOnAttackCoroutine = null)
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