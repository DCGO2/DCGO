using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT6_033 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Trash your security and gaim Memory", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Play] You may trash any number of cards from the top of your Security Stack until you have 3 or more remaining to gain 1 memory for each security card you trashed.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.SecurityCards.Count > 3)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (isExistOnField(card))
                {
                    if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                    {
                        if (card.Owner.SecurityCards.Count > 3)
                        {
                            yield return GManager.instance.photonWaitController.StartWait("Pulsemon_Select_ETB");

                            List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                            for(int i = 1; i <= card.Owner.SecurityCards.Count - 3; i++)
                                selectionElements.Add(new SelectionElement<int>(message: $"{i}", value: i, spriteIndex: 0));

                            string selectPlayerMessage = "Choose how many cards to trash?";
                            string notSelectPlayerMessage = "The opponent is choosing how many cards to trash.";

                            if (selectionElements.Count > 1)
                                GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                            else
                                GManager.instance.userSelectionManager.SetInt(selectionElements[0].Value);

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            int count = GManager.instance.userSelectionManager.SelectedIntValue;

                            for (int i = count; i > 0; i--)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                    player: card.Owner,
                                    destroySecurityCount: 1,
                                    cardEffect: activateClass,
                                    fromTop: true).DestroySecurity());
                            }

                            if (count >= 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(count, activateClass));
                            }
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.None)
        {
            bool Condition()
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (card.Owner.SecurityCards.Count == 3)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.JammingSelfStaticEffect(isInheritedEffect: true, card: card, condition: Condition));
        }

        return cardEffects;
    }
}

