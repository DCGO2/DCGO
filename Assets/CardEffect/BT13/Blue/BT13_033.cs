using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT13
{
    public class BT13_033 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                AddBurstDigivolutionConditionClass addBurstDigivolutionConditionClass = new AddBurstDigivolutionConditionClass();
                addBurstDigivolutionConditionClass.SetUpICardEffect($"Burst Digivolution", CanUseCondition, card);
                addBurstDigivolutionConditionClass.SetUpAddBurstDigivolutionConditionClass(getBurstDigivolutionCondition: GetBurstDigivolution);
                addBurstDigivolutionConditionClass.SetNotShowUI(true);
                cardEffects.Add(addBurstDigivolutionConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                BurstDigivolutionCondition GetBurstDigivolution(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        bool TamerCondition(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                            {
                                if (!permanent.CannotReturnToHand(null))
                                {
                                    if (permanent.TopCard.CardNames.Contains("Thomas H. Norstein"))
                                    {
                                        return true;
                                    }

                                    if (permanent.TopCard.CardNames.Contains("ThomasH.Norstein"))
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }

                        bool DigimonCondition(Permanent permanent)
                        {
                            if(permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.TopCard.Owner.GetFieldPermanents().Contains(permanent))
                                        {
                                            if (!card.CanNotEvolve(permanent))
                                            {
                                                if (permanent.TopCard.CardNames.Contains("MirageGaogamon"))
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        BurstDigivolutionCondition burstDigivolutionCondition = new BurstDigivolutionCondition(
                            tamerCondition: TamerCondition,
                            selectTamerMessage: "1 [Thomas H. Norstein]",
                            digimonCondition: DigimonCondition,
                            selectDigimonMessage: "1 [MirageGaogamon]",
                            cost: 0);

                        return burstDigivolutionCondition;
                    }

                    return null;
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 Digimon to hand and gain Memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Return 1 of your opponent's Digimon to the hand. Then, gain 1 memory for every 4 cards in your opponent's hand.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                int count()
                {
                    return card.Owner.Enemy.HandCards.Count / 4;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                        {
                            return true;
                        }

                        if (card.Owner.CanAddMemory(activateClass))
                        {
                            if (count() >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                    {
                        int maxCount = Math.Min(1, card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Bounce,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(count(), activateClass));
                }
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return opponent's hand cards to the bottom of deck to unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] If your opponent has 9 or more cards in their hand, by choosing cards in your opponent's hand without looking and returning them to the bottom of the deck so that 8 remain, unsuspend this Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.Enemy.HandCards.Count >= 9)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool returned = false;

                    if (card.Owner.Enemy.HandCards.Count >= 9)
                    {
                        int maxCount = card.Owner.Enemy.HandCards.Count - 8;

                        if (maxCount >= 1)
                        {
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
                                message: "Select cards to return to the bottom of opponent's deck.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: false,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: card.Owner.Enemy.HandCards,
                                canLookReverseCard: false,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetNotShowCard();
                            selectCardEffect.SetUseFaceDown();

                            if (card.Owner.isYou)
                            {
                                selectCardEffect.SetNotAddLog();
                            }

                            selectCardEffect.SetUpCustomMessage(
                            "Select cards to put on bottom of the deck.",
                            "The opponent is selecting cards to put on bottom of the deck.");

                            yield return StartCoroutine(selectCardEffect.Activate());

                            IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                            {
                                if (cardSources.Count >= 1)
                                {
                                    returned = true;

                                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(
                                        cardSources,
                                        notAddLog: card.Owner.isYou));

                                    foreach (CardSource cardSource in cardSources)
                                    {
                                        selectedCards.Add(cardSource);
                                    }
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
                                if (returned)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(selectedCards, "Deck Bottom Cards", true, true));
                                }
                            }
                        }
                    }

                    if (returned)
                    {
                        if (CardEffectCommons.IsExistOnBattleArea(card))
                        {
                            Permanent selectedPermanent = card.PermanentOfThisCard();

                            yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass).Unsuspend());
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}