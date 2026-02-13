using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class P_070 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Reveal the top card and add this card to hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] At the end of the battle, reveal the top card of your deck. If itÅfs a black Digimon card with a play cost of 4 or less, you may play it without paying its memory cost. Add the remaining cards to your hand. Then, add this card to its ownerÅfs hand.";
            }


            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnExecutingArea(card))
                {
                    return true;
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return null;

                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                ActivateClass activateClass1 = new ActivateClass();
                activateClass1.SetUpICardEffect("Reveal the top card and add this card to hand", CanUseCondition1, card);
                activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                card.Owner.UntilEndBattleEffects.Add(GetCardEffect1);

                string EffectDiscription1()
                {
                    return "Reveal the top card of your deck. If itÅfs a black Digimon card with a play cost of 4 or less, you may play it without paying its memory cost. Add the remaining cards to your hand. Then, add this card to its ownerÅfs hand.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                        {
                            if (cardSource.GetCostItself <= 4)
                            {
                                if (cardSource.CardColors.Contains(CardColor.Black))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition1(Hashtable hashtable)
                {
                    return true;
                }

                bool CanActivateCondition1(Hashtable hashtable)
                {
                    return true;
                }

                IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                {
                    RevealLibraryClass revealLibrary = new RevealLibraryClass(card.Owner, 1);

                    yield return ContinuousController.instance.StartCoroutine(revealLibrary.RevealLibrary());

                    List<CardSource> handCards = new List<CardSource>();

                    foreach (CardSource cardSource in revealLibrary.RevealedCards)
                    {
                        CardSource selectedCard = cardSource;

                        bool played = false;

                        if (CanSelectCardCondition(selectedCard))
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Play", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Not Play", value : false, spriteIndex: 1),
                        };

                            string selectPlayerMessage = "Will you play the revealed card?";
                            string notSelectPlayerMessage = "The opponent is choosing wheter to play the revealed card.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            bool willPlay = GManager.instance.userSelectionManager.SelectedBoolValue;

                            if (willPlay)
                            {
                                if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: selectedCard, payCost: false, cardEffect: activateClass))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                        cardSources: new List<CardSource>() { selectedCard },
                                        activateClass: activateClass,
                                        payCost: false,
                                        isTapped: false,
                                        root: SelectCardEffect.Root.Library,
                                        activateETB: true
                                    ));

                                    if (CardEffectCommons.IsExistOnBattleArea(selectedCard))
                                    {
                                        played = true;
                                    }
                                }
                            }
                        }

                        if (!played)
                        {
                            handCards.Add(cardSource);
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(handCards, false, activateClass));

                    if (card.Owner.ExecutingCards.Contains(card))
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.brainStormObject.CloseBrainstrorm(card));

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { card }, false, activateClass));
                    }
                }

                ICardEffect GetCardEffect1(EffectTiming _timing)
                {
                    if (_timing == EffectTiming.OnEndBattle)
                    {
                        return activateClass1;
                    }

                    return null;
                }
            }
        }

        return cardEffects;
    }
}
