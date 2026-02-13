using System.Collections;
using System.Collections.Generic;

// DemiDevimon
namespace DCGO.CardEffects.EX10
{
    public class EX10_040 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start Of Your Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash top 2 of both players decks, then if opponent has 10 or more, gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] If your opponent has 10 or fewer cards in their trash, trash the top 2 cards of both players' decks. Then, if they have 10 or more cards in their trash, gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.Enemy.TrashCards.Count <= 10)
                    {
                        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                        {
                            if (player.LibraryCards.Count >= 1)
                            {
                                IAddTrashCardsFromLibraryTop addTrashCard = new IAddTrashCardsFromLibraryTop(2, player, activateClass);
                                addTrashCard.SetNotShowCards();

                                yield return ContinuousController.instance.StartCoroutine(addTrashCard.AddTrashCardsFromLibraryTop());

                                if (player.isYou)
                                {
                                    ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(addTrashCard.discardedCards, "Your Discarded Cards", true, true));
                                }
                                else
                                {
                                    ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(addTrashCard.discardedCards, "Opponent's Discarded Cards", true, true));
                                }
                            }
                        }
                    }

                    if (card.Owner.Enemy.TrashCards.Count >= 10) yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }

            #endregion

            #region ESS - When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash top card from both players decks", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("TrashTopCard_EX10_040");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] Trash the top card of both players' decks.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                    {
                        if (player.LibraryCards.Count >= 1)
                        {
                            IAddTrashCardsFromLibraryTop addTrashCard = new IAddTrashCardsFromLibraryTop(1, player, activateClass);
                            addTrashCard.SetNotShowCards();

                            yield return ContinuousController.instance.StartCoroutine(addTrashCard.AddTrashCardsFromLibraryTop());

                            if (player.isYou)
                            {
                                ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(addTrashCard.discardedCards, "Your Discarded Card", true, true));
                            }
                            else
                            {
                                ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(addTrashCard.discardedCards, "Opponent's Discarded Card", true, true));
                            }
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}