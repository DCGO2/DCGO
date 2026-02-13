using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT11
{
    public class BT11_044 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 4 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Reveal the top 4 cards of your deck. You may play any number of Digimon cards with [Chuumon], [Sukamon], or [Etemon] in their names whose total play costs add up to 7 or less among them without paying the costs. Trash the rest.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                    {
                        if (cardSource.GetCostItself <= 7)
                        {
                            if (cardSource.HasPlayCost)
                            {
                                if (cardSource.IsDigimon)
                                {
                                    if (cardSource.ContainsCardName("Chuumon"))
                                    {
                                        return true;
                                    }

                                    if (cardSource.ContainsCardName("Sukamon"))
                                    {
                                        return true;
                                    }

                                    if (cardSource.ContainsCardName("Etemon"))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }

                    return false;
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
                    int maxCount = card.Owner.fieldCardFrames.Count((frame) => frame.IsEmptyFrame() && frame.IsBattleAreaFrame());

                    List<CardSource> selectedCards = new List<CardSource>();

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 4,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select Digimon cards with [Chuumon], [Sukamon], or [Etemon] in their names whose total play costs add up to 7 or less.",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: maxCount,
                            selectCardCoroutine: SelectCardCoroutine),
                        },
                        remainingCardsPlace: RemainingCardsPlace.Trash,
                        activateClass: activateClass,
                        canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                        canEndSelectCondition: CanEndSelectCondition,
                        canNoSelect: true,
                        canEndNotMax: true
                    ));

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);
                        yield return null;
                    }

                    bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                    {
                        int sumCost = 0;

                        foreach (CardSource cardSource1 in cardSources)
                        {
                            sumCost += cardSource1.GetCostItself;
                        }

                        sumCost += cardSource.GetCostItself;

                        if (sumCost > 7)
                        {
                            return false;
                        }

                        return true;
                    }

                    bool CanEndSelectCondition(List<CardSource> cardSources)
                    {
                        if (cardSources.Count <= 0)
                        {
                            return false;
                        }

                        int sumCost = 0;

                        foreach (CardSource cardSource1 in cardSources)
                        {
                            sumCost += cardSource1.GetCostItself;
                        }

                        if (sumCost > 7)
                        {
                            return false;
                        }

                        return true;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: selectedCards,
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Library,
                        activateETB: true));
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 4 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Reveal the top 4 cards of your deck. You may play any number of Digimon cards with [Chuumon], [Sukamon], or [Etemon] in their names whose total play costs add up to 7 or less among them without paying the costs. Trash the rest.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                    {
                        if (cardSource.GetCostItself <= 7)
                        {
                            if (cardSource.HasPlayCost)
                            {
                                if (cardSource.IsDigimon)
                                {
                                    if (cardSource.ContainsCardName("Chuumon"))
                                    {
                                        return true;
                                    }

                                    if (cardSource.ContainsCardName("Sukamon"))
                                    {
                                        return true;
                                    }

                                    if (cardSource.ContainsCardName("Etemon"))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }

                    return false;
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
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    int maxCount = card.Owner.fieldCardFrames.Count((frame) => frame.IsEmptyFrame() && frame.IsBattleAreaFrame());

                    List<CardSource> selectedCards = new List<CardSource>();

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 4,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select Digimon cards with [Chuumon], [Sukamon], or [Etemon] in their names whose total play costs add up to 7 or less.",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: maxCount,
                            selectCardCoroutine: SelectCardCoroutine),
                        },
                        remainingCardsPlace: RemainingCardsPlace.Trash,
                        activateClass: activateClass,
                        canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                        canEndSelectCondition: CanEndSelectCondition,
                        canNoSelect: true,
                        canEndNotMax: true
                    ));

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);
                        yield return null;
                    }

                    bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                    {
                        int sumCost = 0;

                        foreach (CardSource cardSource1 in cardSources)
                        {
                            sumCost += cardSource1.GetCostItself;
                        }

                        sumCost += cardSource.GetCostItself;

                        if (sumCost > 7)
                        {
                            return false;
                        }

                        return true;
                    }

                    bool CanEndSelectCondition(List<CardSource> cardSources)
                    {
                        if (cardSources.Count <= 0)
                        {
                            return false;
                        }

                        int sumCost = 0;

                        foreach (CardSource cardSource1 in cardSources)
                        {
                            sumCost += cardSource1.GetCostItself;
                        }

                        if (sumCost > 7)
                        {
                            return false;
                        }

                        return true;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: selectedCards,
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Library,
                        activateETB: true));
                }
            }

            return cardEffects;
        }
    }
}