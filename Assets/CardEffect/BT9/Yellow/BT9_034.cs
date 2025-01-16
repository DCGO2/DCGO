using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;


public class BT9_034 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            bool PermanentCondition(Permanent targetPermanent)
            {
                return targetPermanent.TopCard.CardNames.Contains("Salamon");
            }

            cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Look at the top card of your Security", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Look at the top card of your security stack, and you may add it to your hand. If you do, <Recovery +1 (Deck)>. (Place the top card of your deck on top of your security stack.)";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (card.Owner.SecurityCards.Count >= 1)
                {
                    List<CardSource> topCards = new List<CardSource>();

                    for (int i = 0; i < 1; i++)
                    {
                        if (card.Owner.SecurityCards.Count > i)
                        {
                            topCards.Add(card.Owner.SecurityCards[i]);
                        }
                    }

                    if (topCards.Count >= 1)
                    {
                        if (card.Owner.isYou)
                        {
                            ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(topCards, "Security Top Card", true, true));
                        }

                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Add to hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Not add to hand", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Will you add the card to hand?";
                        string notSelectPlayerMessage = "The opponent is choosing whether to add the card to hand.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool addHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (addHand)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(topCards, false, activateClass));

                            yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                                player: card.Owner,
                                refSkillInfos: ref ContinuousController.instance.nullSkillInfos).ReduceSecurity());

                            yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                        }

                        else
                        {
                            GManager.instance.commandText.OpenCommandText("The card was not added to hand.");

                            yield return new WaitForSeconds(0.5f);

                            GManager.instance.commandText.CloseCommandText();
                            yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);
                        }
                    }
                }
            }
        }

        return cardEffects;
    }
}
