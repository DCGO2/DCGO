using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT13
{
    public class BT13_019 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Digimon from trash or digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Without paying the cost, you may play 1 Digimon card with [Sistermon] in its name from your trash or 1 Digimon card with the [Royal Knight] trait from the digivolution cards of your Digimon in the breeding area. This effect can't play [Omnimon] or [Gankoomon].";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.ContainsCardName("Sistermon"))
                        {
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                            {
                                if (cardSource.CardNames.Contains("Omnimon") || cardSource.CardNames.Contains("Gankoomon"))
                                {
                                    return false;
                                }

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
                        if (cardSource.HasRoyalKnightTraits)
                        {
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                            {
                                if (cardSource.CardNames.Contains("Omnimon") || cardSource.CardNames.Contains("Gankoomon"))
                                {
                                    return false;
                                }

                                return true;
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

                        if (card.Owner.GetBreedingAreaPermanents().Count >= 1)
                        {
                            Permanent selectedPermanent = card.Owner.GetBreedingAreaPermanents()[0];

                            if (selectedPermanent.IsDigimon)
                            {
                                if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition1) >= 1)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    bool canSelectDigivolutionCard = false;

                    if (card.Owner.GetBreedingAreaPermanents().Count >= 1)
                    {
                        Permanent selectedPermanent = card.Owner.GetBreedingAreaPermanents()[0];

                        if (selectedPermanent.IsDigimon)
                        {
                            if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition1) >= 1)
                            {
                                canSelectDigivolutionCard = true;
                            }
                        }
                    }

                    if (canSelectTrash || canSelectDigivolutionCard)
                    {
                        if (canSelectTrash && canSelectDigivolutionCard)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From trash", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From Digivolution cards", value : false, spriteIndex: 1),
                        };

                            string selectPlayerMessage = "From which area do you play a card?";
                            string notSelectPlayerMessage = "The opponent is choosing from which area to play a card.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectTrash);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool fromTrash = GManager.instance.userSelectionManager.SelectedBoolValue;

                        List<CardSource> selectedCards = new List<CardSource>();

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        if (fromTrash)
                        {
                            int maxCount = 1;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to play.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }
                        else
                        {
                            if (card.Owner.GetBreedingAreaPermanents().Count >= 1)
                            {
                                Permanent selectedPermanent = card.Owner.GetBreedingAreaPermanents()[0];

                                if (selectedPermanent.IsDigimon)
                                {
                                    if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition1) >= 1)
                                    {
                                        int maxCount = Math.Min(1, selectedPermanent.DigivolutionCards.Count((cardSource) => CanSelectCardCondition1(cardSource)));

                                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                        selectCardEffect.SetUp(
                                                    canTargetCondition: CanSelectCardCondition1,
                                                    canTargetCondition_ByPreSelecetedList: null,
                                                    canEndSelectCondition: null,
                                                    canNoSelect: () => true,
                                                    selectCardCoroutine: SelectCardCoroutine,
                                                    afterSelectCardCoroutine: null,
                                                    message: "Select 1 digivolution card to play.",
                                                    maxCount: maxCount,
                                                    canEndNotMax: false,
                                                    isShowOpponent: true,
                                                    mode: SelectCardEffect.Mode.Custom,
                                                    root: SelectCardEffect.Root.Custom,
                                                    customRootCardList: selectedPermanent.DigivolutionCards,
                                                    canLookReverseCard: true,
                                                    selectPlayer: card.Owner,
                                                    cardEffect: activateClass);

                                        selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to play.", "The opponent is selecting 1 digivolution card to play.");
                                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                                        yield return StartCoroutine(selectCardEffect.Activate());
                                    }
                                }
                            }
                        }

                        SelectCardEffect.Root root = SelectCardEffect.Root.Trash;

                        if (!fromTrash)
                        {
                            root = SelectCardEffect.Root.DigivolutionCards;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: root, activateETB: true));
                    }
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Digimon from trash or digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Without paying the cost, you may play 1 Digimon card with [Sistermon] in its name from your trash or 1 Digimon card with the [Royal Knight] trait from the digivolution cards of your Digimon in the breeding area. This effect can't play [Omnimon] or [Gankoomon].";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.ContainsCardName("Sistermon"))
                        {
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                            {
                                if (cardSource.CardNames.Contains("Omnimon") || cardSource.CardNames.Contains("Gankoomon"))
                                {
                                    return false;
                                }

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
                        if (cardSource.HasRoyalKnightTraits)
                        {
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                            {
                                if (cardSource.CardNames.Contains("Omnimon") || cardSource.CardNames.Contains("Gankoomon"))
                                {
                                    return false;
                                }

                                return true;
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
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            return true;
                        }

                        if (card.Owner.GetBreedingAreaPermanents().Count >= 1)
                        {
                            Permanent selectedPermanent = card.Owner.GetBreedingAreaPermanents()[0];

                            if (selectedPermanent.IsDigimon)
                            {
                                if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition1) >= 1)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    bool canSelectDigivolutionCard = false;

                    if (card.Owner.GetBreedingAreaPermanents().Count >= 1)
                    {
                        Permanent selectedPermanent = card.Owner.GetBreedingAreaPermanents()[0];

                        if (selectedPermanent.IsDigimon)
                        {
                            if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition1) >= 1)
                            {
                                canSelectDigivolutionCard = true;
                            }
                        }
                    }

                    if (canSelectTrash || canSelectDigivolutionCard)
                    {
                        if (canSelectTrash && canSelectDigivolutionCard)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From trash", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From Digivolution cards", value : false, spriteIndex: 1),
                        };

                            string selectPlayerMessage = "From which area do you play a card?";
                            string notSelectPlayerMessage = "The opponent is choosing from which area to play a card.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectTrash);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool fromTrash = GManager.instance.userSelectionManager.SelectedBoolValue;

                        List<CardSource> selectedCards = new List<CardSource>();

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        if (fromTrash)
                        {
                            int maxCount = 1;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to play.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }
                        else
                        {
                            if (card.Owner.GetBreedingAreaPermanents().Count >= 1)
                            {
                                Permanent selectedPermanent = card.Owner.GetBreedingAreaPermanents()[0];

                                if (selectedPermanent.IsDigimon)
                                {
                                    if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition1) >= 1)
                                    {
                                        int maxCount = Math.Min(1, selectedPermanent.DigivolutionCards.Count((cardSource) => CanSelectCardCondition1(cardSource)));

                                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                        selectCardEffect.SetUp(
                                                    canTargetCondition: CanSelectCardCondition1,
                                                    canTargetCondition_ByPreSelecetedList: null,
                                                    canEndSelectCondition: null,
                                                    canNoSelect: () => true,
                                                    selectCardCoroutine: SelectCardCoroutine,
                                                    afterSelectCardCoroutine: null,
                                                    message: "Select 1 digivolution card to play.",
                                                    maxCount: maxCount,
                                                    canEndNotMax: false,
                                                    isShowOpponent: true,
                                                    mode: SelectCardEffect.Mode.Custom,
                                                    root: SelectCardEffect.Root.Custom,
                                                    customRootCardList: selectedPermanent.DigivolutionCards,
                                                    canLookReverseCard: true,
                                                    selectPlayer: card.Owner,
                                                    cardEffect: activateClass);

                                        selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to play.", "The opponent is selecting 1 digivolution card to play.");
                                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                                        yield return StartCoroutine(selectCardEffect.Activate());
                                    }
                                }
                            }
                        }

                        SelectCardEffect.Root root = SelectCardEffect.Root.Trash;

                        if (!fromTrash)
                        {
                            root = SelectCardEffect.Root.DigivolutionCards;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: root, activateETB: true));
                    }
                }
            }

            return cardEffects;
        }
    }
}