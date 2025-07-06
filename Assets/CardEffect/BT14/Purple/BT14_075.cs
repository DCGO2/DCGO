using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT14
{
    public class BT14_075 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 3 cards from deck top", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Trash the top 3 cards of your deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(3, card.Owner, activateClass).AddTrashCardsFromLibraryTop());
                }
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 3 cards from deck top", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] Trash the top 3 cards of your deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(3, card.Owner, activateClass).AddTrashCardsFromLibraryTop());
                }
            }

            if (timing == EffectTiming.None)
            {
                int count()
                {
                    return card.Owner.TrashCards.Count / 3;
                }

                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (count() >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect<Func<int>>(changeValue: () => 1000 * count(), isInheritedEffect: false, card: card, condition: Condition));
            }

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 card from opponent's hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] Trash 1 card in your opponent's hand without looking.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanActivateOnDeletion(card))
                    {
                        if (card.Owner.Enemy.HandCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.Enemy.HandCards.Count >= 1)
                    {
                        int maxCount = 1;

                        if (card.Owner.isYou)
                        {
                            foreach (CardSource cardSource in card.Owner.Enemy.HandCards)
                            {
                                cardSource.SetReverse();
                            }
                        }

                        card.Owner.Enemy.HandCards = RandomUtility.ShuffledDeckCards(card.Owner.Enemy.HandCards);

                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: (cardSource) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            message: "Select 1 card to trash from opponent's hand.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: false,
                            mode: SelectCardEffect.Mode.Discard,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: card.Owner.Enemy.HandCards,
                            canLookReverseCard: false,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetNotShowCard();
                        selectCardEffect.SetUseFaceDown();
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            foreach (CardSource cardSource in cardSources)
                            {
                                selectedCards.Add(cardSource);
                            }

                            yield return null;
                        }

                        if (card.Owner.isYou)
                        {
                            foreach (CardSource cardSource in card.Owner.Enemy.HandCards)
                            {
                                cardSource.SetFace();
                            }
                        }
                        else
                        {
                            if (selectedCards.Count >= 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(selectedCards, "Discarded Cards", true, true));
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}