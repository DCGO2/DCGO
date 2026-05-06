using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can Trigger [Ascension]
    public static bool CanTriggerAscension(Hashtable hashtable, CardSource card)
    {
        return CanTriggerOnDeletion(hashtable, card);
    }

    public static bool CanTriggerPermanentAscension(Hashtable hashtable, Func<Permanent, bool> permanentCondition)
    {
        return CanTriggerOnPermanentDeleted(hashtable, permanentCondition);
    }
    #endregion

    #region Can activate [Ascension]
    public static bool CanActivateAscension(Hashtable hashtable, CardSource card)
    {
        return CanActivateOnDeletion(card);
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
}