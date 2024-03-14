using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Trigger effect of [Decoy] on oneself
    public static ActivateClass DecoySelfEffect(bool isInheritedEffect, CardSource card, Func<bool> condition, Func<Permanent, bool> permanentCondition, string effectName, string effectDiscription, ICardEffect rootCardEffect = null)
    {
        Permanent targetPermanent = card.PermanentOfThisCard();

        bool CanUseCondition()
        {
            if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
            {
                if (condition == null || condition())
                {
                    return true;
                }
            }

            return false;
        }

        return DecoyEffect(targetPermanent: targetPermanent, isInheritedEffect: isInheritedEffect, condition: CanUseCondition, rootCardEffect: rootCardEffect, permanentCondition: permanentCondition, effectName: effectName, effectDiscription: effectDiscription, card);
    }
    #endregion

    #region Trigger effect of [Decoy]
    public static ActivateClass DecoyEffect(Permanent targetPermanent, bool isInheritedEffect, Func<bool> condition, ICardEffect rootCardEffect, Func<Permanent, bool> permanentCondition, string effectName, string effectDiscription, CardSource card)
    {
        if (targetPermanent == null) return null;
        if (targetPermanent.TopCard == null) return null;
        if (card == null) return null;

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect(effectName, CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, effectDiscription);
        activateClass.SetHashString($"Decoy_{card.CardID}" + (isInheritedEffect ? "_inherited" : ""));
        activateClass.SetIsInheritedEffect(isInheritedEffect);

        if (rootCardEffect != null)
        {
            activateClass.SetIsInheritedEffect(false);
            activateClass.SetEffectSourcePermanent(targetPermanent);
            activateClass.SetRootCardEffect(rootCardEffect);
        }

        bool CanSelectPermanentCondition(Permanent permanent) => permanent != targetPermanent && CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) && (permanentCondition == null || permanentCondition(permanent));

        bool CanUseCondition(Hashtable hashtable)
        {
            bool CardEffectCondition(ICardEffect cardEffect) => cardEffect.EffectSourceCard.Owner == card.Owner.Enemy;

            if (CardEffectCommons.IsPermanentExistsOnBattleArea(targetPermanent))
            {
                if (CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, CanSelectPermanentCondition))
                {
                    if (CardEffectCommons.IsByEffect(hashtable, CardEffectCondition))
                    {
                        if (condition == null || condition())
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            if (CardEffectCommons.CanActivateDecoy(targetPermanent, activateClass))
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
            return CardEffectCommons.DecoyProcess(activateClass, targetPermanent, CanSelectPermanentCondition);
        }

        return activateClass;
    }
    #endregion

}