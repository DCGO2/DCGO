using System.Collections;
using System.Collections.Generic;
using System;

public partial class CardEffectFactory
{
    #region Trigger effect of [Guard]
    public static ActivateClass GuardEffect(Permanent targetPermanent, bool isInheritedEffect, Func<bool> condition, ICardEffect rootCardEffect, CardSource card)
    {
        if (targetPermanent == null) return null;
        if (targetPermanent.TopCard == null) return null;
        if (card == null) return null;

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Guard", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, DataBase.GuardEffectDescription());
        activateClass.SetIsInheritedEffect(isInheritedEffect);

        if (rootCardEffect != null)
        {
            activateClass.SetIsInheritedEffect(false);
            activateClass.SetEffectSourcePermanent(targetPermanent);
            activateClass.SetRootCardEffect(rootCardEffect);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(targetPermanent)
                && !targetPermanent.TopCard.CanNotBeAffected(activateClass);
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            return CanActivateGuard(targetPermanent, activateClass);
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            return CardEffectFactory.GuardProcess(_hashtable, activateClass, targetPermanent);
        }

        return activateClass;
    }
    #endregion

    #region Can activate [Guard]
    public static bool CanActivateGuard(Permanent permanent, ICardEffect activateClass)
    {
        return CardEffectCommons.IsPermanentExistsOnBattleArea(permanent)
            && permanent.CanBeDestroyedBySkill(activateClass);
    }
    #endregion

    #region Effect process of [Guard]
    public static IEnumerator GuardProcess(Hashtable hashtable, ICardEffect activateClass, Permanent permanent)
    {
        if (permanent == null) yield break;
        if (permanent.TopCard == null) yield break;

        Player owner = permanent.TopCard.Owner;

        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { permanent }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

        IEnumerator SuccessProcess()
        {
            permanent.willBeRemoveField = false;
            permanent.HideDeleteEffect();
            permanent.HideHandBounceEffect();
            permanent.HideDeckBounceEffect();
            permanent.HideWillRemoveFieldEffect();

            yield return null;
        }
    }
    #endregion

    #region Target 1 Digimon gains [Guard]
    public static IEnumerator GainGuard(Permanent targetPermanent, EffectDuration effectDuration, ICardEffect activateClass)
    {
        if (targetPermanent == null) yield break;
        if (!CardEffectCommons.IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        bool CanUseCondition()
        {
            return CardEffectCommons.IsPermanentExistsOnBattleArea(targetPermanent)
                && !targetPermanent.TopCard.CanNotBeAffected(activateClass);
        }

        ActivateClass guard = CardEffectFactory.GuardEffect(
            targetPermanent: targetPermanent,
            isInheritedEffect: false,
            condition: CanUseCondition,
            rootCardEffect: activateClass,
            card: targetPermanent.TopCard);

        CardEffectCommons.AddEffectToPermanent(
            targetPermanent: targetPermanent,
            effectDuration: effectDuration,
            card: card,
            cardEffect: guard,
            timing: EffectTiming.WhenRemoveField);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
        }
    }
    #endregion

    #region Player gains effect to have Digimon gains [Guard]
    public static IEnumerator GainGuardPlayerEffect(Func<Permanent, bool> permanentCondition, EffectDuration effectDuration, ICardEffect activateClass)
    {
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        bool PermanentCondition(Permanent permanent)
        {
            return CardEffectCommons.IsPermanentExistsOnBattleArea(permanent)
                && !permanent.TopCard.CanNotBeAffected(activateClass)
                && (permanentCondition == null
                    || permanentCondition(permanent));
        }

        bool CanUseCondition()
        {
            return true;
        }

        ICardEffect guard = CardEffectFactory.GuardStaticEffect(permanentCondition: PermanentCondition, isInheritedEffect: false, card: card, condition: CanUseCondition);

        CardEffectCommons.AddEffectToPlayer(effectDuration: effectDuration, card: card, cardEffect: guard, timing: EffectTiming.WhenRemoveField);

        foreach (Permanent permanent in GManager.instance.turnStateMachine.gameContext.PermanentsForTurnPlayer)
        {
            if (PermanentCondition(permanent))
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(permanent));
            }
        }
    }
    #endregion

    #region Static effect of [Guard] to all PermanentCondition Digimon
    public static ActivateClass GuardStaticEffect(Func<Permanent, bool> permanentCondition, bool isInheritedEffect, CardSource card, Func<bool> condition)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Guard", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, DataBase.GuardEffectDescription());
        activateClass.SetIsInheritedEffect(isInheritedEffect);

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                && (condition == null
                    || condition());
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            return CardEffectFactory.CanActivateGuard(card.PermanentOfThisCard(), activateClass)
                && (condition == null
                    || condition());
        }

        IEnumerator ActivateCoroutine(Hashtable hashtable)
        {
            return CardEffectFactory.GuardProcess(hashtable, activateClass, card.PermanentOfThisCard());
        }

        return activateClass;
    }
    #endregion

}