using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX3
{
    public class EX3_013 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("Machinedramon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 1, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place cards in digivolution cards to De-Digivolve", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By placing up to 3 red and black level 5 cards with [Cyborg] in their traits and different card numbers from your hand and trash under this Digimon as its bottom digivolution cards, <De-Digivolve 1> 1 of your opponent's Digimon for each card placed by this effect.";
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

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
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
                        int digivolutionCardsCount = 0;

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

                            int maxCount = Math.Min(3, card.Owner.HandCards.Count((cardSource) => CanSelectCardCondition1(cardSource)));

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

                            int maxCount = Math.Min(3 - digivolutionCards.Count, card.Owner.TrashCards.Count((cardSource) => CanSelectCardCondition1(cardSource)));

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
                                yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(digivolutionCards_fixed, activateClass));

                                if (digivolutionCards_fixed.Count >= 1)
                                {
                                    digivolutionCardsCount = digivolutionCards_fixed.Count;
                                }
                            }
                        }

                        if (digivolutionCardsCount >= 1)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                                List<Permanent> selectedPermanents = new List<Permanent>();

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
                                    selectedPermanents.Add(permanent);

                                    yield return null;
                                }

                                foreach (Permanent selectedPermanent in selectedPermanents)
                                {
                                    for (int i = 0; i < digivolutionCardsCount; i++)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(new IDegeneration(
                                            selectedPermanent,
                                            1,
                                            activateClass).Degeneration());
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place cards in digivolution cards to De-Digivolve", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By placing up to 3 red and black level 5 cards with [Cyborg] in their traits and different card numbers from your hand and trash under this Digimon as its bottom digivolution cards, <De-Digivolve 1> 1 of your opponent's Digimon for each card placed by this effect.";
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
                        int digivolutionCardsCount = 0;

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

                            int maxCount = Math.Min(3, card.Owner.HandCards.Count((cardSource) => CanSelectCardCondition1(cardSource)));

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

                            int maxCount = Math.Min(3 - digivolutionCards.Count, card.Owner.TrashCards.Count((cardSource) => CanSelectCardCondition1(cardSource)));

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
                                yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(digivolutionCards_fixed, activateClass));

                                if (digivolutionCards_fixed.Count >= 1)
                                {
                                    digivolutionCardsCount = digivolutionCards_fixed.Count;
                                }
                            }
                        }

                        if (digivolutionCardsCount >= 1)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                                List<Permanent> selectedPermanents = new List<Permanent>();

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
                                    selectedPermanents.Add(permanent);

                                    yield return null;
                                }

                                foreach (Permanent selectedPermanent in selectedPermanents)
                                {
                                    for (int i = 0; i < digivolutionCardsCount; i++)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(new IDegeneration(
                                            selectedPermanent,
                                            1,
                                            activateClass).Degeneration());
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted || timing == EffectTiming.WhenReturntoHandAnyone || timing == EffectTiming.WhenReturntoLibraryAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Prevent this Digimon from being deleted or returned to hand or deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("Substitute_EX3_013");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When this Digimon would be deleted or returned to your hand or deck, you may trash 2 level 5 cards in this Digimon's digivolution cards to prevent it from leaving play.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
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

                            if (selectedCards.Count >= 1)
                            {
                                if (selectedCards.Count == 2)
                                {
                                    selectedPermanent.willBeRemoveField = false;

                                    selectedPermanent.HideDeleteEffect();
                                    selectedPermanent.HideHandBounceEffect();
                                    selectedPermanent.HideDeckBounceEffect();
                                }

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