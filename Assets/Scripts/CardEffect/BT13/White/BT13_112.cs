using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT13
{
    public class BT13_112 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 Digimon or play digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] You may delete 1 of your opponent's Digimon, or play 1 of each Digimon with the [Royal Knight] trait and different names from the digivolution cards of your Digimon in the breeding area without paying the costs. When a Digimon is played by this effect, trash your Digimon in the breeding area, and all your Digimon gain <Rush> for the turn.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBreedingArea(permanent, card))
                    {
                        if (permanent.IsDigimon)
                        {
                            if (permanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.HasRoyalKnightTraits)
                        {
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                            {
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
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }

                        if (card.Owner.GetBreedingAreaPermanents().Some(CanSelectPermanentCondition1))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canSelectDelete = card.Owner.Enemy.GetBattleAreaDigimons().Some(CanSelectPermanentCondition);

                    bool canSelectPlay = card.Owner.GetBreedingAreaPermanents().Some(CanSelectPermanentCondition1);

                    if (canSelectDelete || canSelectPlay)
                    {
                        if (canSelectDelete && canSelectPlay)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Delete Digimon", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Play digivolution cards", value : false, spriteIndex: 1),
                        };

                            string selectPlayerMessage = "Which effect do you activate?";
                            string notSelectPlayerMessage = "The opponent is choosing from which effect to activate.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectDelete);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool isDelete = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (isDelete)
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
                        else
                        {
                            if (card.Owner.GetBreedingAreaPermanents().Count(CanSelectPermanentCondition1) >= 1)
                            {
                                Permanent selectedPermanent = card.Owner.GetBreedingAreaPermanents()[0];

                                if (CanSelectPermanentCondition1(selectedPermanent))
                                {
                                    bool played = false;

                                    List<CardSource> selectedCards = new List<CardSource>();

                                    int maxCount = selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition);

                                    List<string> royalKnightCardNames = selectedPermanent.DigivolutionCards
                                        .Filter(CanSelectCardCondition)
                                        .Map(digivolutionCard => digivolutionCard.CardNames)
                                        .Flat()
                                        .Distinct()
                                        .ToList();

                                    maxCount = Math.Min(maxCount, royalKnightCardNames.Count);

                                    maxCount = Math.Min(maxCount, card.Owner.fieldCardFrames.Count((frame) => frame.IsEmptyFrame() && frame.IsBattleAreaFrame()));

                                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                    selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectCardCondition,
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

                                    selectCardEffect.SetUpCustomMessage("Select cards to play.", "The opponent is selecting cards to play.");
                                    selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                    bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                                    {
                                        List<string> cardNames = cardSources
                                            .Map(cardSource1 => cardSource1.CardNames)
                                            .Flat()
                                            .Distinct()
                                            .ToList();

                                        if (cardSource.CardNames.Some((cardName) => cardNames.Contains(cardName)))
                                        {
                                            return false;
                                        }

                                        return true;
                                    }

                                    bool CanEndSelectCondition(List<CardSource> cardSources)
                                    {
                                        List<string> cardNames = cardSources
                                            .Map(cardSource1 => cardSource1.CardNames)
                                            .Flat()
                                            .Distinct()
                                            .ToList();

                                        if (cardNames.Some(cardName => cardSources.Count(cardSource => cardSource.CardNames.Contains(cardName)) >= 2))
                                        {
                                            return false;
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
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.DigivolutionCards, activateETB: true));

                                        if (selectedCards.Some(CardEffectCommons.IsExistOnBattleArea))
                                        {
                                            played = true;
                                        }
                                    }

                                    if (played)
                                    {
                                        List<Permanent> DigitamaPermanents = card.Owner.GetBreedingAreaPermanents()
                                            .Filter(permanent => permanent.IsDigimon)
                                            .Filter(permanent => !permanent.TopCard.CanNotBeAffected(activateClass))
                                            .Clone();

                                        if (DigitamaPermanents.Count >= 1)
                                        {
                                            foreach (Permanent permanent in DigitamaPermanents)
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(permanent.DiscardEvoRoots());

                                                CardSource cardSource = permanent.TopCard;

                                                ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { cardSource }, "Cards put to trash", true, true));

                                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveField(permanent));
                                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(cardSource));
                                            }
                                        }

                                        bool PermanentCondition(Permanent permanent)
                                        {
                                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                                        }

                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRushPlayerEffect(
                                                    permanentCondition: PermanentCondition,
                                                    effectDuration: EffectDuration.UntilEachTurnEnd,
                                                    activateClass: activateClass));
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
                activateClass.SetUpICardEffect("Delete 1 Digimon or play digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may delete 1 of your opponent's Digimon, or play 1 of each Digimon with the [Royal Knight] trait and different names from the digivolution cards of your Digimon in the breeding area without paying the costs. When a Digimon is played by this effect, trash your Digimon in the breeding area, and all your Digimon gain <Rush> for the turn.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBreedingArea(permanent, card))
                    {
                        if (permanent.IsDigimon)
                        {
                            if (permanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.HasRoyalKnightTraits)
                        {
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                            {
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
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }

                        if (card.Owner.GetBreedingAreaPermanents().Some(CanSelectPermanentCondition1))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canSelectDelete = card.Owner.Enemy.GetBattleAreaDigimons().Some(CanSelectPermanentCondition);

                    bool canSelectPlay = card.Owner.GetBreedingAreaPermanents().Some(CanSelectPermanentCondition1);

                    if (canSelectDelete || canSelectPlay)
                    {
                        if (canSelectDelete && canSelectPlay)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Delete Digimon", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Play digivolution cards", value : false, spriteIndex: 1),
                        };

                            string selectPlayerMessage = "Which effect do you activate?";
                            string notSelectPlayerMessage = "The opponent is choosing from which effect to activate.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectDelete);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool isDelete = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (isDelete)
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
                        else
                        {
                            if (card.Owner.GetBreedingAreaPermanents().Count(CanSelectPermanentCondition1) >= 1)
                            {
                                Permanent selectedPermanent = card.Owner.GetBreedingAreaPermanents()[0];

                                if (CanSelectPermanentCondition1(selectedPermanent))
                                {
                                    bool played = false;

                                    List<CardSource> selectedCards = new List<CardSource>();

                                    int maxCount = selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition);

                                    List<string> royalKnightCardNames = selectedPermanent.DigivolutionCards
                                        .Filter(CanSelectCardCondition)
                                        .Map(digivolutionCard => digivolutionCard.CardNames)
                                        .Flat()
                                        .Distinct()
                                        .ToList();

                                    maxCount = Math.Min(maxCount, royalKnightCardNames.Count);

                                    maxCount = Math.Min(maxCount, card.Owner.fieldCardFrames.Count((frame) => frame.IsEmptyFrame() && frame.IsBattleAreaFrame()));

                                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                    selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectCardCondition,
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

                                    selectCardEffect.SetUpCustomMessage("Select cards to play.", "The opponent is selecting cards to play.");
                                    selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                    bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                                    {
                                        List<string> cardNames = cardSources
                                            .Map(cardSource1 => cardSource1.CardNames)
                                            .Flat()
                                            .Distinct()
                                            .ToList();

                                        if (cardSource.CardNames.Some((cardName) => cardNames.Contains(cardName)))
                                        {
                                            return false;
                                        }

                                        return true;
                                    }

                                    bool CanEndSelectCondition(List<CardSource> cardSources)
                                    {
                                        List<string> cardNames = cardSources
                                            .Map(cardSource1 => cardSource1.CardNames)
                                            .Flat()
                                            .Distinct()
                                            .ToList();

                                        if (cardNames.Some(cardName => cardSources.Count(cardSource => cardSource.CardNames.Contains(cardName)) >= 2))
                                        {
                                            return false;
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
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.DigivolutionCards, activateETB: true));

                                        if (selectedCards.Some(CardEffectCommons.IsExistOnBattleArea))
                                        {
                                            played = true;
                                        }
                                    }

                                    if (played)
                                    {
                                        List<Permanent> DigitamaPermanents = card.Owner.GetBreedingAreaPermanents()
                                            .Filter(permanent => permanent.IsDigimon)
                                            .Filter(permanent => !permanent.TopCard.CanNotBeAffected(activateClass))
                                            .Clone();

                                        if (DigitamaPermanents.Count >= 1)
                                        {
                                            foreach (Permanent permanent in DigitamaPermanents)
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(permanent.DiscardEvoRoots());

                                                CardSource cardSource = permanent.TopCard;

                                                ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { cardSource }, "Cards put to trash", true, true));

                                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveField(permanent));
                                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(cardSource));
                                            }
                                        }

                                        bool PermanentCondition(Permanent permanent)
                                        {
                                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                                        }

                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRushPlayerEffect(
                                                    permanentCondition: PermanentCondition,
                                                    effectDuration: EffectDuration.UntilEachTurnEnd,
                                                    activateClass: activateClass));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}