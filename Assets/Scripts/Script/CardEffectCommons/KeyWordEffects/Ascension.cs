using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can Trigger [Ascension]
    public static bool CanTriggerAscension(Hashtable hashtable, CardSource card, ICardEffect activateClass)
    {
        return CanTriggerOnDeletion(hashtable, card, activateClass);
    }

    public static bool CanTriggerPermanentAscension(Hashtable hashtable, Func<Permanent, bool> permanentCondition, ICardEffect activateClass)
    {
        return CanTriggerOnPermanentDeleted(hashtable, permanentCondition, activateClass);
    }
    #endregion

    #region Can activate [Ascension]
    public static bool CanActivateAscension(CardSource card, ICardEffect activateClass)
    {
        return CanActivateOnDeletion(card, activateClass);
    }
    #endregion

    #region Effect process of [Ascension]
    public static IEnumerator AscensionProcess(Hashtable hashtable, ICardEffect activateClass, CardSource card)
    {
        if (card.Owner.CanAddSecurity(activateClass))
        {
            string selectPlayerMessage = "Will you place this card in security?";
            string notSelectPlayerMessage = "The opponent is choosing if they will use Ascension.";

            List<SelectionElement<bool>> command_SelectCommands = new List<SelectionElement<bool>>()
            {
                new SelectionElement<bool>(message: $"Yes", value: true, spriteIndex: 0),
                new SelectionElement<bool>(message: $"No", value: false, spriteIndex: 1),
            };

            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: command_SelectCommands, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

            if(GManager.instance.userSelectionManager.SelectedBoolValue)
            {
                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(card, true));
            }
        }
    }
    #endregion

    #region Target 1 Digimon gains [Ascension]
    public static IEnumerator GainAscension(Permanent targetPermanent, EffectDuration effectDuration, ICardEffect activateClass)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        bool CanUseCondition()
        {
            return IsPermanentExistsOnBattleArea(targetPermanent) &&
                   !targetPermanent.TopCard.CanNotBeAffected(activateClass);
        }

        ActivateClass ascension = CardEffectFactory.AscensionEffect(
            targetPermanent: targetPermanent,
            isInheritedEffect: false,
            condition: CanUseCondition,
            rootCardEffect: activateClass,
            targetPermanent.TopCard);

        AddEffectToPermanent(
            targetPermanent: targetPermanent,
            effectDuration: effectDuration,
            card: card,
            cardEffect: ascension,
            timing: EffectTiming.OnDestroyedAnyone);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                .CreateBuffEffect(targetPermanent));
        }
    }
    #endregion
}