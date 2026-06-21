using System.Collections;
using System.Collections.Generic;
using System;

public partial class CardEffectFactory
{
    #region [Guard] self effect
    public static ActivateClass GuardSelfEffect(bool isInheritedEffect, CardSource card, Func<bool> condition, bool isLinkedEffect = false)
    {
        return GuardEffect(
            targetPermanent: card.PermanentOfThisCard(), 
            isInheritedEffect,
            condition,
            rootCardEffect: null,
            card,
            isLinkedEffect);
    }
    #endregion

    #region [Guard] Effect
    public static ActivateClass GuardEffect(Permanent targetPermanent, bool isInheritedEffect, Func<bool> condition, ICardEffect rootCardEffect, CardSource card, bool isLinkedEffect = false)
    {
        if (targetPermanent == null) return null;
        if (targetPermanent.TopCard == null) return null;
        if (card == null) return null;

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Guard", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, DataBase.GuardEffectDescription());
        activateClass.SetIsInheritedEffect(isInheritedEffect);
        activateClass.SetIsLinkedEffect(isLinkedEffect);

        if (rootCardEffect != null)
        {
            activateClass.SetIsInheritedEffect(false);
            activateClass.SetEffectSourcePermanent(targetPermanent);
            activateClass.SetRootCardEffect(rootCardEffect);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(targetPermanent)
                && !targetPermanent.TopCard.CanNotBeAffected(activateClass)
                && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, PermanentCondition)
                && CardEffectCommons.IsByEffect(hashtable, effect => CardEffectCommons.IsOpponentEffect(effect, card));
        }

        bool PermanentCondition(Permanent permanent) => permanent != targetPermanent && CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

        bool CanActivateCondition(Hashtable hashtable)
        {
            return CanActivateGuard(targetPermanent, activateClass);
        }

        IEnumerator ActivateCoroutine(Hashtable hashtable)
        {
            return CardEffectFactory.GuardProcess(hashtable, activateClass, targetPermanent);
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
    public static IEnumerator GuardProcess(Hashtable hashtable, ICardEffect activateClass, Permanent guardPermanent)
    {
        if (guardPermanent == null) yield break;
        if (guardPermanent.TopCard == null) yield break;

        bool PermanentCondition(Permanent otherPermanent)
        {
            return otherPermanent != guardPermanent
                && CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(otherPermanent, guardPermanent.TopCard);
        }

        Player owner = guardPermanent.TopCard.Owner;

        string selectPlayerMessage = "Will you delete this digimon to prevent the removal?";
        string notSelectPlayerMessage = "The opponent is choosing if they will use Guard.";

        List<SelectionElement<bool>> command_SelectCommands = new List<SelectionElement<bool>>()
        {
            new SelectionElement<bool>(message: $"Yes", value: true, spriteIndex: 0),
            new SelectionElement<bool>(message: $"No", value: false, spriteIndex: 1),
        };

        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: command_SelectCommands, selectPlayer: owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

        if (GManager.instance.userSelectionManager.SelectedBoolValue)
        {
            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { guardPermanent }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

            IEnumerator SuccessProcess()
            {
                List<Permanent> removedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable).Filter(PermanentCondition);

                foreach (Permanent removed in removedPermanents)
                {
                    removed.willBeRemoveField = false;
                    removed.HideDeleteEffect();
                    removed.HideHandBounceEffect();
                    removed.HideDeckBounceEffect();
                    removed.HideWillRemoveFieldEffect();
                }

                yield return null;
            }
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

}