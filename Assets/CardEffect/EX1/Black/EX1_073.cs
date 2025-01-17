using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX1
{
    public class EX1_073 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place cards in digivolution cards to gain Memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] You may place up to 5 level 5 red and black cards with [Cyborg] in their traits and different card numbers from your hand and/or trash in this Digimon's digivolution cards to gain 1 memory for each card placed.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.Level == 5 && cardSource.HasLevel)
                        {
                            if (cardSource.CardTraits.Contains("Cyborg"))
                            {
                                if (cardSource.CardColors.Contains(CardColor.Red))
                                {
                                    return true;
                                }

                                if (cardSource.CardColors.Contains(CardColor.Black))
                                {
                                    return true;
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
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            return true;
                        }

                        if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        List<CardSource> digivolutionCards = new List<CardSource>();

                        bool CanSelectCardCondition1(CardSource cardSource)
                        {
                            if (CanSelectCardCondition(cardSource))
                            {
                                if (digivolutionCards.Count((cardSource1) => cardSource1.CardID == cardSource.CardID) == 0)
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        if (card.Owner.HandCards.Count((cardSource) => CanSelectCardCondition1(cardSource)) >= 1)
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            int maxCount = Math.Min(5, card.Owner.HandCards.Count((cardSource) => CanSelectCardCondition1(cardSource)));

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCardCondition1,
                                canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                canEndSelectCondition: CanEndSelectCondition,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: true,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectHandEffect.SetUpCustomMessage("Select cards to place in Digivolution cards.", "The opponent is selecting cards.");

                            yield return StartCoroutine(selectHandEffect.Activate());

                            bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                            {
                                List<string> cardIDs = new List<string>();

                                foreach (CardSource cardSource1 in cardSources)
                                {
                                    if (!cardIDs.Contains(cardSource1.CardID))
                                    {
                                        cardIDs.Add(cardSource1.CardID);
                                    }
                                }

                                if (cardIDs.Contains(cardSource.CardID))
                                {
                                    return false;
                                }

                                return true;
                            }

                            bool CanEndSelectCondition(List<CardSource> cardSources)
                            {
                                List<string> cardIDs = new List<string>();

                                foreach (CardSource cardSource1 in cardSources)
                                {
                                    if (!cardIDs.Contains(cardSource1.CardID))
                                    {
                                        cardIDs.Add(cardSource1.CardID);
                                    }
                                }

                                if (cardIDs.Count != cardSources.Count)
                                {
                                    return false;
                                }

                                return true;
                            }

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);
                                digivolutionCards.Add(cardSource);
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

                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition1(cardSource)))
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            int maxCount = Math.Min(5 - digivolutionCards.Count, card.Owner.TrashCards.Count((cardSource) => CanSelectCardCondition1(cardSource)));

                            if (maxCount >= 1)
                            {
                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition1,
                                    canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                    canEndSelectCondition: CanEndSelectCondition,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select cards to place in Digivolution cards.",
                                    maxCount: maxCount,
                                    canEndNotMax: true,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                                {
                                    List<string> cardIDs = new List<string>();

                                    foreach (CardSource cardSource1 in cardSources)
                                    {
                                        if (!cardIDs.Contains(cardSource1.CardID))
                                        {
                                            cardIDs.Add(cardSource1.CardID);
                                        }
                                    }

                                    foreach (CardSource cardSource1 in digivolutionCards)
                                    {
                                        if (!cardIDs.Contains(cardSource1.CardID))
                                        {
                                            cardIDs.Add(cardSource1.CardID);
                                        }
                                    }

                                    if (cardIDs.Contains(cardSource.CardID))
                                    {
                                        return false;
                                    }

                                    return true;
                                }

                                bool CanEndSelectCondition(List<CardSource> cardSources)
                                {
                                    List<string> cardIDs = new List<string>();

                                    foreach (CardSource cardSource1 in cardSources)
                                    {
                                        if (!cardIDs.Contains(cardSource1.CardID))
                                        {
                                            cardIDs.Add(cardSource1.CardID);
                                        }
                                    }

                                    foreach (CardSource cardSource1 in digivolutionCards)
                                    {
                                        if (!cardIDs.Contains(cardSource1.CardID))
                                        {
                                            cardIDs.Add(cardSource1.CardID);
                                        }
                                    }

                                    if (cardIDs.Count != cardSources.Count + digivolutionCards.Count)
                                    {
                                        return false;
                                    }

                                    return true;
                                }

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCards.Add(cardSource);
                                    digivolutionCards.Add(cardSource);
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

                        if (digivolutionCards.Count >= 1)
                        {
                            List<CardSource> digivolutionCards_fixed = new List<CardSource>();

                            if (digivolutionCards.Count == 1)
                            {
                                foreach (CardSource cardSource in digivolutionCards)
                                {
                                    digivolutionCards_fixed.Add(cardSource);
                                }
                            }

                            else
                            {
                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: (cardSource) => true,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: AfterSelectCardCoroutine1,
                                    message: "Specify the order to place the cards in the digivolution cards\n(cards will be placed so that cards with lower numbers are on top).",
                                    maxCount: digivolutionCards.Count,
                                    canEndNotMax: false,
                                    isShowOpponent: false,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: digivolutionCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Cards");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
                                {
                                    foreach (CardSource cardSource in cardSources)
                                    {
                                        digivolutionCards_fixed.Add(cardSource);
                                    }

                                    yield return null;
                                }
                            }

                            if (CardEffectCommons.IsExistOnBattleArea(card))
                            {
                                digivolutionCards_fixed.Reverse();

                                yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsTop(digivolutionCards_fixed, activateClass));

                                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(digivolutionCards_fixed.Count, activateClass));
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                bool CanUseCondition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                string effectName = "Can't have DP reduced";

                cardEffects.Add(CardEffectFactory.ImmuneFromDPMinusStaticEffect(
                    permanentCondition: PermanentCondition,
                    cardEffectCondition: null,
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseCondition,
                    effectName: effectName
                ));
            }

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Prevent this Digimon from being deleted", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("Substitute_EX1_073");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns]When this Digimon would be deleted, you may trash 2 level5 Digimon cards in this Digimon's digivolution cards to prevent this Digimon from being deleted.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.Level == 5)
                        {
                            if (!cardSource.CanNotTrashFromDigivolutionCards(activateClass))
                            {
                                if (cardSource.HasLevel)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 2)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        Permanent selectedPermanent = card.PermanentOfThisCard();

                        if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition) >= 2)
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            int maxCount = Math.Min(2, selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition));

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectCardCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => true,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select cards to discard.",
                                        maxCount: maxCount,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Custom,
                                        customRootCardList: selectedPermanent.DigivolutionCards,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                            selectCardEffect.SetNotShowCard();
                            yield return StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);
                                yield return null;
                            }

                            if (selectedCards.Count == 2)
                            {
                                selectedPermanent.willBeRemoveField = false;

                                yield return ContinuousController.instance.StartCoroutine(new ITrashDigivolutionCards(selectedPermanent, selectedCards, activateClass).TrashDigivolutionCards());
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}