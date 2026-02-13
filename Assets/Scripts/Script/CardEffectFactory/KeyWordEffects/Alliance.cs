using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Trigger effect of [Alliance] on oneself
    public static ICardEffect AllianceSelfEffect(bool isInheritedEffect, CardSource card, Func<bool> condition)
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

        return AllianceEffect(
            targetPermanent: targetPermanent,
            isInheritedEffect: isInheritedEffect,
            condition: CanUseCondition,
            rootCardEffect: null, card);
    }
    #endregion

    #region Trigger effect of [Alliance]
    public static ActivateClass AllianceEffect(Permanent targetPermanent, bool isInheritedEffect, Func<bool> condition, ICardEffect rootCardEffect, CardSource card)
    {
        if (targetPermanent == null) return null;
        if (targetPermanent.TopCard == null) return null;
        if (card == null) return null;

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Alliance", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, DataBase.AllianceEffectDiscription());
        activateClass.SetIsInheritedEffect(isInheritedEffect);

        if (rootCardEffect != null)
        {
            activateClass.SetIsInheritedEffect(false);
            activateClass.SetEffectSourcePermanent(targetPermanent);
            activateClass.SetRootCardEffect(rootCardEffect);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            if (CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, (permanent) => permanent.cardSources.Contains(targetPermanent.TopCard)))
            {
                if (condition == null || condition())
                {
                    return true;
                }
            }

            return false;
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanActivateAlliance(hashtable, card);
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            return CardEffectCommons.AllianceProcess(_hashtable, activateClass, card);
        }

        return activateClass;
    }
    #endregion

    #region Static effect of [Alliance]
    public static AllianceClass AllianceStaticEffect(Func<Permanent, bool> permanentCondition, bool isInheritedEffect, CardSource card, Func<bool> condition)
    {
        string effectName = "Alliance";

        AllianceClass allianceClass = new AllianceClass();
        allianceClass.SetUpICardEffect(effectName, CanUseCondition, card);
        allianceClass.SetUpAllianceClass(PermanentCondition: PermanentCondition);

        if (isInheritedEffect)
        {
            allianceClass.SetIsInheritedEffect(true);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return condition == null || condition();
        }

        bool PermanentCondition(Permanent permanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
            {
                if (permanentCondition == null || permanentCondition(permanent))
                {
                    return true;
                }
            }

            return false;
        }

        return allianceClass;
    }
    #endregion
}