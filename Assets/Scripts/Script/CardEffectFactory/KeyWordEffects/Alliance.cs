using System.Collections;
using System;

public partial class CardEffectFactory
{
    #region Trigger effect of [Alliance] on oneself
    public static ICardEffect AllianceSelfEffect(bool isInheritedEffect, CardSource card, Func<bool> condition, bool isLinkedEffect = false)
    {
        Permanent targetPermanent = card.PermanentOfThisCard();

        bool CanUseCondition()
        {
            return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                && (condition == null || condition());
        }

        return AllianceEffect(
            targetPermanent: targetPermanent,
            isInheritedEffect: isInheritedEffect,
            condition: CanUseCondition,
            rootCardEffect: null, card,
            isLinkedEffect: isLinkedEffect);
    }
    #endregion

    #region Trigger effect of [Alliance]
    public static ActivateClass AllianceEffect(Permanent targetPermanent, bool isInheritedEffect, Func<bool> condition, ICardEffect rootCardEffect, CardSource card, bool isLinkedEffect = false)
    {
        if (targetPermanent == null) return null;
        if (targetPermanent.TopCard == null) return null;
        if (card == null) return null;

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Alliance", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, DataBase.AllianceEffectDiscription());
        activateClass.SetIsInheritedEffect(isInheritedEffect);
        activateClass.SetIsLinkedEffect(isLinkedEffect);

        if (rootCardEffect != null)
        {
            activateClass.SetIsInheritedEffect(false);
            activateClass.SetIsLinkedEffect(false);
            activateClass.SetEffectSourcePermanent(targetPermanent);
            activateClass.SetRootCardEffect(rootCardEffect);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, (permanent) => permanent.cardSources.Contains(targetPermanent.TopCard))
                && (condition == null || condition());
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanActivateAlliance(hashtable, card);
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            return CardEffectCommons.AllianceProcess(_hashtable, activateClass, targetPermanent, card);
        }

        return activateClass;
    }
    #endregion

    #region Static effect of [Alliance] to all PermanentCondition Digimon
    public static ActivateClass AllianceStaticEffect(Func<Permanent, bool> permanentCondition, bool isInheritedEffect, CardSource card, Func<bool> condition)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Alliance", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, DataBase.AllianceEffectDiscription());
        activateClass.SetIsInheritedEffect(isInheritedEffect);

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, permanentCondition)
                && (condition == null || condition());
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            Permanent attackingPermanent = CardEffectCommons.GetAttackerFromHashtable(hashtable);

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent != attackingPermanent
                    && CardEffectCommons.CanActivateSuspendCostEffect(permanent.TopCard);
            }

            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(attackingPermanent, card)
                && CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanentCondition)
                && (condition == null || condition());
        }

        IEnumerator ActivateCoroutine(Hashtable hashtable)
        {
            Permanent attackingPermanent = CardEffectCommons.GetAttackerFromHashtable(hashtable);
            return CardEffectCommons.AllianceProcess(hashtable, activateClass, attackingPermanent, card);
        }

        return activateClass;
    }
    #endregion
}
