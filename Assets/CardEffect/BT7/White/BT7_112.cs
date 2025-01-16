using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT7_112 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.CardTraits.Contains("Hybrid"))
                {
                    return true;
                }

                if (cardSource.IsTamer)
                {
                    return true;
                }

                return false;
            }

            bool Condition()
            {
                return card.Owner.HandCards.Contains(card) && (card.Owner.HandCards.Count(CanSelectCardCondition) + card.Owner.TrashCards.Count(CanSelectCardCondition) >= 10);
            }

            bool PermanentCondition(Permanent targetPermanent)
            {
                return targetPermanent.IsTamer;
            }

            cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                permanentCondition: PermanentCondition,
                digivolutionCost: 7,
                ignoreDigivolutionRequirement: false,
                card: card,
                condition: Condition));
        }

        if (timing == EffectTiming.BeforePayCost)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Return cards to deck", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetHashString("ReturnCardsToDeck_BT7_112");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "You may digivolve this card from your hand onto one of your Tamers as if the Tamer is a level 6 Digimon by placing 10 Tamer cards and/or cards with [Hybrid] in their traits from your hand and/or trash at the bottom of your deck in any order.";
            }

            bool PermanentCondition(Permanent targetPermanent)
            {
                return targetPermanent.IsTamer;
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.CardTraits.Contains("Hybrid"))
                {
                    return true;
                }

                if (cardSource.IsTamer)
                {
                    return true;
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(hashtable, PermanentCondition, cardSource => cardSource == card))
                {
                    return true;
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (card.Owner.HandCards.Count(CanSelectCardCondition) + card.Owner.TrashCards.Count(CanSelectCardCondition) >= 10)
                {
                    return true;
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (card.Owner.HandCards.Count(CanSelectCardCondition) + card.Owner.TrashCards.Count(CanSelectCardCondition) >= 10)
                {
                    List<CardSource> libraryCards = new List<CardSource>();

                    if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = Math.Min(10, card.Owner.HandCards.Count(CanSelectCardCondition));

                        int minCount = 10 - card.Owner.TrashCards.Count(CanSelectCardCondition);

                        bool CanNoSelect = minCount <= 0;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: CanEndSelectCondition,
                            maxCount: maxCount,
                            canNoSelect: CanNoSelect,
                            canEndNotMax: true,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select cards to place to the bottom of the deck.", "The opponent is selecting cards.");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        bool CanEndSelectCondition(List<CardSource> cardSources)
                        {
                            if (cardSources.Count < minCount)
                            {
                                return false;
                            }

                            return true;
                        }

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            libraryCards.Add(cardSource);
                            yield return null;
                        }

                        foreach (CardSource selectedCard in selectedCards)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(selectedCard));
                        }
                    }

                    if (card.Owner.TrashCards.Count(CanSelectCardCondition) >= 1)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = Math.Min(10 - libraryCards.Count, card.Owner.TrashCards.Count(CanSelectCardCondition));

                        if (maxCount >= 1)
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => false,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select cards to place to the bottom of the deck.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);
                                libraryCards.Add(cardSource);
                                yield return null;
                            }

                            foreach (CardSource selectedCard in selectedCards)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(selectedCard));
                            }
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ReturnRevealedCardsToLibraryBottom(libraryCards, activateClass));

                    if (libraryCards.Count == 10)
                    {
                        ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                        AddDigivolutionRequirementClass addEvolutionConditionClass = new AddDigivolutionRequirementClass();
                        addEvolutionConditionClass.SetUpICardEffect($"Can digivolve to this card", CanUseCondition1, card);
                        addEvolutionConditionClass.SetUpAddDigivolutionRequirementClass(getEvoCost: GetEvoCost);
                        card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => addEvolutionConditionClass);

                        #region show cost
                        if (_hashtable != null)
                        {
                            if (_hashtable.ContainsKey("Permanents"))
                            {
                                if (_hashtable["Permanents"] is List<Permanent>)
                                {
                                    List<Permanent> Permanents = (List<Permanent>)_hashtable["Permanents"];

                                    if (Permanents != null)
                                    {
                                        if (Permanents.Count >= 1)
                                        {
                                            if (_hashtable.ContainsKey("IPlayCard"))
                                            {
                                                if (_hashtable["IPlayCard"] is PlayCardClass)
                                                {
                                                    PlayCardClass playCard = (PlayCardClass)_hashtable["IPlayCard"];

                                                    if (playCard != null)
                                                    {
                                                        if (!playCard.isJogress && playCard.PayCost)
                                                        {
                                                            Permanent Permanent = Permanents[0];

                                                            if (Permanent != null)
                                                            {
                                                                if (Permanent.TopCard != null)
                                                                {
                                                                    if (_hashtable.ContainsKey("Card"))
                                                                    {
                                                                        CardSource Card = (CardSource)_hashtable["Card"];

                                                                        if (Card != null)
                                                                        {
                                                                            GManager.instance.memoryObject.ShowMemoryPredictionLine(Card.Owner.ExpectedMemory(Card.PayingCost(playCard.Root, Permanents, checkAvailability: false)));
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            return true;
                        }

                        int GetEvoCost(Permanent permanent, CardSource cardSource, bool ignoreDigivolutionCondition)
                        {
                            if ((CardSourceCondition(cardSource) && PermanentCondition(permanent)))
                            {
                                return 7;
                            }

                            return -1;
                        }

                        bool PermanentCondition(Permanent targetPermanent)
                        {
                            if (targetPermanent != null)
                            {
                                if (targetPermanent.IsTamer)
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        bool CardSourceCondition(CardSource cardSource)
                        {
                            return cardSource == card;
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.None)
        {
            cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 2, isInheritedEffect: false, card: card, condition: null));
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Delete 1 Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Delete 1 of your opponent's Digimon.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

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
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
        }

        return cardEffects;
    }
}
