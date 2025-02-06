using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT18
{
    public class BT18_068 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Blocker

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal 5 of either player's deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Reveal the top 5 cards of either player's deck. Return the revealed cards to the top or bottom of the deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable,card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            return true;
                        }

                        if(card.Owner.Enemy.LibraryCards.Count >=1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    

                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Your deck", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Opponent's deck", value : false, spriteIndex: 1),
                        };

                    string selectPlayerMessage = "Reveal your deck or your opponent's deck?";
                    string notSelectPlayerMessage = "The opponent is choosing which deck to reveal.";

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool yourDeck = GManager.instance.userSelectionManager.SelectedBoolValue;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndProcessForAll(
                    revealCount: 5,
                    simplifiedSelectCardCondition:
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition: (cardSource) => false,
                            message: "",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: -1,
                            selectCardCoroutine: null),
                    remainingCardsPlace: RemainingCardsPlace.DeckTopOrBottom,
                    activateClass: activateClass,
                    revealedCardsCoroutine: null,
                    isOpponentDeck: !yourDeck
                    ));
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal 5 of either player's deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Reveal the top 5 cards of either player's deck. Return the revealed cards to the top or bottom of the deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            return true;
                        }

                        if (card.Owner.Enemy.LibraryCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {


                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Your deck", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Opponent's deck", value : false, spriteIndex: 1),
                        };

                    string selectPlayerMessage = "Reveal your deck or your opponent's deck?";
                    string notSelectPlayerMessage = "The opponent is choosing which deck to reveal.";

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool yourDeck = GManager.instance.userSelectionManager.SelectedBoolValue;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndProcessForAll(
                    revealCount: 5,
                    simplifiedSelectCardCondition:
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition: (cardSource) => false,
                            message: "",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: -1,
                            selectCardCoroutine: null),
                    remainingCardsPlace: RemainingCardsPlace.DeckTopOrBottom,
                    activateClass: activateClass,
                    revealedCardsCoroutine: null,
                    isOpponentDeck: !yourDeck
                    ));
                }
            }
            #endregion


            return cardEffects;
        }
    }
}