using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT9_067 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Place cards in digivolution cards from trash to gain Memory", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Play] Place 1 [Raijinmon], 1 [Fujinmon], and 1 [Suijinmon] from your trash under this Digimon in any order as its bottom digivolution cards. Gain 1 memory for each card placed.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.CardNames.Contains("Raijinmon");
            }

            bool CanSelectCardCondition1(CardSource cardSource)
            {
                return cardSource.CardNames.Contains("Fujinmon");
            }

            bool CanSelectCardCondition2(CardSource cardSource)
            {
                return cardSource.CardNames.Contains("Suijinmon");
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition(cardSource) || CanSelectCardCondition1(cardSource) || CanSelectCardCondition2(cardSource)))
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                List<CardSource> selectedCards = new List<CardSource>();

                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition(cardSource) || CanSelectCardCondition1(cardSource) || CanSelectCardCondition2(cardSource)))
                {
                    int maxCount = 0;

                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition(cardSource)))
                    {
                        maxCount++;
                    }

                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition1(cardSource)))
                    {
                        maxCount++;
                    }

                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition2(cardSource)))
                    {
                        maxCount++;
                    }

                    if (maxCount >= 1)
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: (cardSource) => CanSelectCardCondition(cardSource) || CanSelectCardCondition1(cardSource) || CanSelectCardCondition2(cardSource),
                            canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                            canEndSelectCondition: CanEndSelectCondition,
                            canNoSelect: () => false,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select cards to place in Digivolution cards\n(cards will be placed so that cards with lower numbers are on top).",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Cards");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                        {
                            if (cardSources.Count(CanSelectCardCondition) >= 1)
                            {
                                if (CanSelectCardCondition(cardSource))
                                {
                                    return false;
                                }
                            }

                            if (cardSources.Count(CanSelectCardCondition1) >= 1)
                            {
                                if (CanSelectCardCondition1(cardSource))
                                {
                                    return false;
                                }
                            }

                            if (cardSources.Count(CanSelectCardCondition2) >= 1)
                            {
                                if (CanSelectCardCondition2(cardSource))
                                {
                                    return false;
                                }
                            }

                            return true;
                        }

                        bool CanEndSelectCondition(List<CardSource> cardSources)
                        {
                            if (cardSources.Count(CanSelectCardCondition) >= 2)
                            {
                                return false;
                            }

                            if (cardSources.Count(CanSelectCardCondition) == 0)
                            {
                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                                {
                                    return false;
                                }
                            }

                            if (cardSources.Count(CanSelectCardCondition1) >= 2)
                            {
                                return false;
                            }

                            if (cardSources.Count(CanSelectCardCondition1) == 0)
                            {
                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition1))
                                {
                                    return false;
                                }
                            }

                            if (cardSources.Count(CanSelectCardCondition2) >= 2)
                            {
                                return false;
                            }

                            if (cardSources.Count(CanSelectCardCondition2) == 0)
                            {
                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition2))
                                {
                                    return false;
                                }
                            }

                            return true;
                        }

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        if (selectedCards.Count >= 1)
                        {
                            foreach (CardSource selectedCard in selectedCards)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(selectedCard));
                            }
                        }
                    }
                }

                if (selectedCards.Count >= 1)
                {
                    List<CardSource> digivolutionCards_fixed = new List<CardSource>();

                    foreach (CardSource cardSource in selectedCards)
                    {
                        digivolutionCards_fixed.Add(cardSource);
                    }

                    if (digivolutionCards_fixed.Count >= 1)
                    {
                        if (CardEffectCommons.IsExistOnBattleArea(card))
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(digivolutionCards_fixed, activateClass));

                            int plusMemory = digivolutionCards_fixed.Count;

                            yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(plusMemory, activateClass));
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Place cards in digivolution cards from trash to gain Memory", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Place 1 [Raijinmon], 1 [Fujinmon], and 1 [Suijinmon] from your trash under this Digimon in any order as its bottom digivolution cards. Gain 1 memory for each card placed.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.CardNames.Contains("Raijinmon");
            }

            bool CanSelectCardCondition1(CardSource cardSource)
            {
                return cardSource.CardNames.Contains("Fujinmon");
            }

            bool CanSelectCardCondition2(CardSource cardSource)
            {
                return cardSource.CardNames.Contains("Suijinmon");
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition(cardSource) || CanSelectCardCondition1(cardSource) || CanSelectCardCondition2(cardSource)))
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                List<CardSource> selectedCards = new List<CardSource>();

                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition(cardSource) || CanSelectCardCondition1(cardSource) || CanSelectCardCondition2(cardSource)))
                {
                    int maxCount = 0;

                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition(cardSource)))
                    {
                        maxCount++;
                    }

                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition1(cardSource)))
                    {
                        maxCount++;
                    }

                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition2(cardSource)))
                    {
                        maxCount++;
                    }

                    if (maxCount >= 1)
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: (cardSource) => CanSelectCardCondition(cardSource) || CanSelectCardCondition1(cardSource) || CanSelectCardCondition2(cardSource),
                            canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                            canEndSelectCondition: CanEndSelectCondition,
                            canNoSelect: () => false,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select cards to place in Digivolution cards\n(cards will be placed so that cards with lower numbers are on top).",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Cards");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                        {
                            if (cardSources.Count(CanSelectCardCondition) >= 1)
                            {
                                if (CanSelectCardCondition(cardSource))
                                {
                                    return false;
                                }
                            }

                            if (cardSources.Count(CanSelectCardCondition1) >= 1)
                            {
                                if (CanSelectCardCondition1(cardSource))
                                {
                                    return false;
                                }
                            }

                            if (cardSources.Count(CanSelectCardCondition2) >= 1)
                            {
                                if (CanSelectCardCondition2(cardSource))
                                {
                                    return false;
                                }
                            }

                            return true;
                        }

                        bool CanEndSelectCondition(List<CardSource> cardSources)
                        {
                            if (cardSources.Count(CanSelectCardCondition) >= 2)
                            {
                                return false;
                            }

                            if (cardSources.Count(CanSelectCardCondition) == 0)
                            {
                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                                {
                                    return false;
                                }
                            }

                            if (cardSources.Count(CanSelectCardCondition1) >= 2)
                            {
                                return false;
                            }

                            if (cardSources.Count(CanSelectCardCondition1) == 0)
                            {
                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition1))
                                {
                                    return false;
                                }
                            }

                            if (cardSources.Count(CanSelectCardCondition2) >= 2)
                            {
                                return false;
                            }

                            if (cardSources.Count(CanSelectCardCondition2) == 0)
                            {
                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition2))
                                {
                                    return false;
                                }
                            }

                            return true;
                        }

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        if (selectedCards.Count >= 1)
                        {
                            foreach (CardSource selectedCard in selectedCards)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(selectedCard));
                            }
                        }
                    }
                }

                if (selectedCards.Count >= 1)
                {
                    List<CardSource> digivolutionCards_fixed = new List<CardSource>();

                    foreach (CardSource cardSource in selectedCards)
                    {
                        digivolutionCards_fixed.Add(cardSource);
                    }

                    if (digivolutionCards_fixed.Count >= 1)
                    {
                        if (CardEffectCommons.IsExistOnBattleArea(card))
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(digivolutionCards_fixed, activateClass));

                            int plusMemory = digivolutionCards_fixed.Count;

                            yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(plusMemory, activateClass));
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.OnAllyAttack)
        {
            int colorCount()
            {
                if (!CardEffectCommons.IsExistOnBattleArea(card)) return 0;

                List<CardColor> cardColors = card.PermanentOfThisCard().DigivolutionCards
                    .Filter(digivolutionCard => digivolutionCard.HasLevel && digivolutionCard.Level == 6)
                    .Map(digivolutionCard => digivolutionCard.CardColors)
                    .Flat()
                    .Distinct()
                    .ToList();

                return cardColors.Count;
            }

            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("DP +3000 and De-Digivolve 1 to 1 Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Attacking] If the level 6 cards in this Digimon's digivolution cards have 3 or more colors among them, this Digimon gets +3000 DP until the end of your opponent's turn. If they have 4 or more colors, <De-Digivolve 1> 1 of your opponent's Digimon.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (colorCount() >= 3)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (colorCount() >= 3)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: card.PermanentOfThisCard(), changeValue: 3000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                }

                if (colorCount() >= 4)
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
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to De-Digivolve.", "The opponent is selecting 1 Digimon to De-Digivolve.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        Permanent selectedPermanent = permanent;

                        yield return ContinuousController.instance.StartCoroutine(new IDegeneration(selectedPermanent, 1, activateClass).Degeneration());
                    }
                }
            }
        }

        return cardEffects;
    }
}
