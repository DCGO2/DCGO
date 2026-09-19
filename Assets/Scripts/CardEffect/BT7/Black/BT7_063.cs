using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT7_063 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Place cards in digivolution cards", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Play] You may place 1 [SkullKnightmon] and 1 [DeadlyAxemon] from your hand and/or trash in this Digimon's digivolution cards in any order.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.CardNames.Contains("SkullKnightmon");
            }

            bool CanSelectCardCondition1(CardSource cardSource)
            {
                return cardSource.CardNames.Contains("DeadlyAxemon");
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition(cardSource) || CanSelectCardCondition1(cardSource)))
                    {
                        return true;
                    }

                    if (card.Owner.HandCards.Count((cardSource) => CanSelectCardCondition(cardSource) || CanSelectCardCondition1(cardSource)) >= 1)
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
                    List<CardSource> selectedCards = new List<CardSource>();

                    {
                        bool canSelectHand = card.Owner.HandCards.Count(CanSelectCardCondition) >= 1;
                        bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                        if (canSelectHand || canSelectTrash)
                        {
                            if (canSelectHand && canSelectTrash)
                            {
                                List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                        };

                                string selectPlayerMessage = "From which area do you select 1 [SkullKnightmon]?";
                                string notSelectPlayerMessage = "The opponent is choosing from which area to select 1 [SkullKnightmon].";

                                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                            }

                            else
                            {
                                GManager.instance.userSelectionManager.SetBool(canSelectHand);
                            }

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            if (fromHand)
                            {
                                int maxCount = 1;

                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectHandEffect.SetUpCustomMessage("Select 1 [SkullKnightmon].", "The opponent is selecting 1 [SkullKnightmon].");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                                yield return StartCoroutine(selectHandEffect.Activate());
                            }

                            else
                            {
                                int maxCount = 1;

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 [SkullKnightmon].",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage("Select 1 [SkullKnightmon].", "The opponent is selecting 1 [SkullKnightmon].");
                                selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                            }
                        }
                    }

                    {
                        bool canSelectHand = card.Owner.HandCards.Count(CanSelectCardCondition1) >= 1;
                        bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition1);

                        if (canSelectHand || canSelectTrash)
                        {
                            if (canSelectHand && canSelectTrash)
                            {
                                List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                        };

                                string selectPlayerMessage = "From which area do you select 1 [DeadlyAxemon]?";
                                string notSelectPlayerMessage = "The opponent is choosing from which area to select 1 [DeadlyAxemon].";

                                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                            }

                            else
                            {
                                GManager.instance.userSelectionManager.SetBool(canSelectHand);
                            }

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            if (fromHand)
                            {
                                int maxCount = 1;

                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectCardCondition1,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectHandEffect.SetUpCustomMessage("Select 1 [DeadlyAxemon].", "The opponent is selecting 1 [DeadlyAxemon].");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");

                                yield return StartCoroutine(selectHandEffect.Activate());
                            }

                            else
                            {
                                int maxCount = 1;

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition1,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 [DeadlyAxemon].",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage("Select 1 [DeadlyAxemon].", "The opponent is selecting 1 [DeadlyAxemon].");
                                selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                            }
                        }
                    }

                    if (selectedCards.Count >= 1)
                    {
                        foreach (CardSource selectedCard in selectedCards)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(selectedCard));
                        }

                        if (selectedCards.Count >= 1)
                        {
                            List<CardSource> digivolutionCards_fixed = new List<CardSource>();

                            if (selectedCards.Count == 1)
                            {
                                foreach (CardSource cardSource in selectedCards)
                                {
                                    digivolutionCards_fixed = selectedCards.Clone();
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
                                    maxCount: selectedCards.Count,
                                    canEndNotMax: false,
                                    isShowOpponent: false,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: selectedCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Cards");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
                                {
                                    digivolutionCards_fixed = cardSources.Clone();

                                    yield return null;
                                }
                            }

                            if (digivolutionCards_fixed.Count >= 1)
                            {
                                if (CardEffectCommons.IsExistOnBattleArea(card))
                                {
                                    digivolutionCards_fixed.Reverse();

                                    yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsTop(digivolutionCards_fixed, activateClass));
                                }
                            }
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Play [SkullKnightmon] and [DeadlyAxemon] from digivolution cards", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            activateClass.SetHashString("PlayDigivolutionCards_BT7_063");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[All Turns] When this Digimon would be deleted, you may play 1 [SkullKnightmon] and 1 [DeadlyAxemon] from this Digimon's digivolution cards suspended without paying their memory costs.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.IsDigimon)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                    {
                        if (cardSource.CardNames.Contains("SkullKnightmon"))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanSelectCardCondition1(CardSource cardSource)
            {
                if (cardSource.IsDigimon)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                    {
                        if (cardSource.CardNames.Contains("DeadlyAxemon"))
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
                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => CanSelectCardCondition(cardSource) || CanSelectCardCondition1(cardSource)) >= 1)
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

                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => CanSelectCardCondition(cardSource) || CanSelectCardCondition1(cardSource)) >= 1)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = 2;

                        #region max count
                        List<CardSource[]> cardsList = ParameterComparer.Enumerate(card.PermanentOfThisCard().DigivolutionCards, maxCount).ToList();

                        List<int> maxCounts = new List<int>() { 0 };

                        foreach (CardSource[] cardSources in cardsList)
                        {
                            List<int> _maxCounts = new List<int>();

                            if (cardSources.Length >= 2)
                            {
                                if (CanSelectCardCondition(cardSources[0]))
                                {
                                    if (CanSelectCardCondition1(cardSources[1]))
                                    {
                                        _maxCounts.Add(2);
                                    }

                                    else
                                    {
                                        _maxCounts.Add(1);
                                    }
                                }

                                if (CanSelectCardCondition1(cardSources[0]))
                                {
                                    if (CanSelectCardCondition(cardSources[1]))
                                    {
                                        _maxCounts.Add(2);
                                    }

                                    else
                                    {
                                        _maxCounts.Add(1);
                                    }
                                }

                                if (!CanSelectCardCondition(cardSources[0]) && !CanSelectCardCondition1(cardSources[0]))
                                {
                                    if (CanSelectCardCondition(cardSources[1]))
                                    {
                                        _maxCounts.Add(1);
                                    }

                                    if (CanSelectCardCondition1(cardSources[1]))
                                    {
                                        _maxCounts.Add(1);
                                    }
                                }
                            }

                            else if (cardSources.Length == 1)
                            {
                                if (CanSelectCardCondition(cardSources[0]))
                                {
                                    _maxCounts.Add(1);
                                }

                                else if (CanSelectCardCondition1(cardSources[0]))
                                {
                                    _maxCounts.Add(1);
                                }
                            }

                            if (_maxCounts.Count >= 1)
                            {
                                maxCounts.Add(_maxCounts.Max());
                            }
                        }

                        if (maxCounts.Count >= 1)
                        {
                            maxCount = maxCounts.Max();
                        }
                        #endregion

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: (cardSource) => CanSelectCardCondition(cardSource) || CanSelectCardCondition1(cardSource),
                                    canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                    canEndSelectCondition: CanEndSelectCondition,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select cards to play.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: selectedPermanent.DigivolutionCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select digivolution cards to play.", "The opponent is selecting digivolution cards to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Cards");

                        yield return StartCoroutine(selectCardEffect.Activate());

                        #region CanTargetCondition_ByPreSelecetedList
                        bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                        {
                            List<CardSource> _cardSources = new List<CardSource>();

                            foreach (CardSource cardSource1 in cardSources)
                            {
                                _cardSources.Add(cardSource1);
                            }

                            _cardSources.Add(cardSource);

                            List<CardSource[]> cardsList = ParameterComparer.Enumerate(_cardSources, _cardSources.Count).ToList();

                            bool match = false;

                            if (cardsList.Count >= 2)
                            {
                                foreach (CardSource[] cardSources1 in cardsList)
                                {
                                    if (CanSelectCardCondition(cardSources1[0]))
                                    {
                                        if (CanSelectCardCondition1(cardSources1[1]))
                                        {
                                            match = true;
                                        }
                                    }

                                    if (CanSelectCardCondition1(cardSources1[0]))
                                    {
                                        if (CanSelectCardCondition(cardSources1[1]))
                                        {
                                            match = true;
                                        }
                                    }
                                }
                            }

                            else
                            {
                                match = true;
                            }

                            if (!match)
                            {
                                return false;
                            }

                            return true;
                        }
                        #endregion

                        #region CanEndSelectCondition
                        bool CanEndSelectCondition(List<CardSource> cardSources)
                        {
                            List<CardSource[]> cardsList = ParameterComparer.Enumerate(cardSources, cardSources.Count).ToList();

                            bool match = false;

                            if (cardsList.Count >= 2)
                            {
                                foreach (CardSource[] cardSources1 in cardsList)
                                {
                                    if (CanSelectCardCondition(cardSources1[0]))
                                    {
                                        if (CanSelectCardCondition1(cardSources1[1]))
                                        {
                                            match = true;
                                        }
                                    }

                                    if (CanSelectCardCondition1(cardSources1[0]))
                                    {
                                        if (CanSelectCardCondition(cardSources1[1]))
                                        {
                                            match = true;
                                        }
                                    }
                                }
                            }

                            else
                            {
                                match = true;
                            }

                            if (!match)
                            {
                                return false;
                            }

                            return true;
                        }
                        #endregion

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: true,
                            root: SelectCardEffect.Root.DigivolutionCards,
                            activateETB: true));
                    }
                }
            }
        }

        return cardEffects;
    }
}
