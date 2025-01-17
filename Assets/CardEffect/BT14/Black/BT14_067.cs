using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT14
{
    public class BT14_067 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 3 cards of opponent's deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Your opponent reveals the top 3 cards of their deck. Choose 1 Digimon card among them, and delete up to its play cost's total worth of your opponent's Digimon. Return the revealed cards to the top or bottom of the deck.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.Enemy.LibraryCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    CardSource selectedCard = null;

                    int maxCost = -1;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 Digimon card.",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: 1,
                            selectCardCoroutine: selectCardCoroutine),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckTopOrBottom,
                        activateClass: activateClass,
                        isSendAllCardsToSamePlace: true,
                        isOpponentDeck: true
                    ));

                    IEnumerator selectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;

                        yield return null;
                    }

                    if (selectedCard != null)
                    {
                        if (selectedCard.HasPlayCost)
                        {
                            maxCost = selectedCard.GetCostItself;
                        }
                    }

                    if (maxCost >= 0)
                    {
                        bool CanSelectPermanentCondition(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                            {
                                if (permanent.TopCard.GetCostItself <= maxCost)
                                {
                                    if (permanent.TopCard.HasPlayCost)
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }

                        if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                        {
                            int maxCount = card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition);

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                canEndSelectCondition: CanEndSelectCondition,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: true,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            bool CanEndSelectCondition(List<Permanent> permanents)
                            {
                                if (permanents.Count <= 0)
                                {
                                    return false;
                                }

                                int sumCost = 0;

                                foreach (Permanent permanent1 in permanents)
                                {
                                    sumCost += permanent1.TopCard.GetCostItself;
                                }

                                if (sumCost > maxCost)
                                {
                                    return false;
                                }

                                return true;
                            }

                            bool CanTargetCondition_ByPreSelecetedList(List<Permanent> permanents, Permanent permanent)
                            {
                                int sumCost = 0;

                                foreach (Permanent permanent1 in permanents)
                                {
                                    sumCost += permanent1.TopCard.GetCostItself;
                                }

                                sumCost += permanent.TopCard.GetCostItself;

                                if (sumCost > maxCost)
                                {
                                    return false;
                                }

                                return true;
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 3 cards of opponent's deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Your opponent reveals the top 3 cards of their deck. Choose 1 Digimon card among them, and delete up to its play cost's total worth of your opponent's Digimon. Return the revealed cards to the top or bottom of the deck.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.Enemy.LibraryCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    CardSource selectedCard = null;

                    int maxCost = -1;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 Digimon card.",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: 1,
                            selectCardCoroutine: selectCardCoroutine),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckTopOrBottom,
                        activateClass: activateClass,
                        isSendAllCardsToSamePlace: true,
                        isOpponentDeck: true
                    ));

                    IEnumerator selectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;

                        yield return null;
                    }

                    if (selectedCard != null)
                    {
                        if (selectedCard.HasPlayCost)
                        {
                            maxCost = selectedCard.GetCostItself;
                        }
                    }

                    if (maxCost >= 0)
                    {
                        bool CanSelectPermanentCondition(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                            {
                                if (permanent.TopCard.GetCostItself <= maxCost)
                                {
                                    if (permanent.TopCard.HasPlayCost)
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }

                        if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                        {
                            int maxCount = card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition);

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                canEndSelectCondition: CanEndSelectCondition,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: true,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            bool CanEndSelectCondition(List<Permanent> permanents)
                            {
                                if (permanents.Count <= 0)
                                {
                                    return false;
                                }

                                int sumCost = 0;

                                foreach (Permanent permanent1 in permanents)
                                {
                                    sumCost += permanent1.TopCard.GetCostItself;
                                }

                                if (sumCost > maxCost)
                                {
                                    return false;
                                }

                                return true;
                            }

                            bool CanTargetCondition_ByPreSelecetedList(List<Permanent> permanents, Permanent permanent)
                            {
                                int sumCost = 0;

                                foreach (Permanent permanent1 in permanents)
                                {
                                    sumCost += permanent1.TopCard.GetCostItself;
                                }

                                sumCost += permanent.TopCard.GetCostItself;

                                if (sumCost > maxCost)
                                {
                                    return false;
                                }

                                return true;
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}