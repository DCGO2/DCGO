using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.EX6
{
    public class EX6_069 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 Digimon with [Seven Great Demon Lords] trait as bottom digivolution source in breeding area.", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] You may place 1 Digimon card with the [Seven Great Demon Lords] trait from your hand or trash as the bottom digivolution card of the [Gate of Deadly Sins] in your breeding area. Then, place this card in the battle area.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBreedingArea(permanent, card))
                    {
                        if (!permanent.IsToken)
                        {
                            if (permanent.TopCard.CardNames.Contains("Gate of Deadly Sins"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool HasSevenGreatDemonLordTrait(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                        return cardSource.ContainsTraits("Seven Great Demon Lords");

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.GetBreedingAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                    {
                        bool validCardInTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, HasSevenGreatDemonLordTrait);
                        bool validCardInHand = card.Owner.HandCards.Count(HasSevenGreatDemonLordTrait) > 0;
                        bool useTrash = false;

                        if (validCardInTrash || validCardInHand)
                        {
                            #region Selecting Hand or Trash
                            if (validCardInTrash && validCardInHand)
                            {
                                List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                                {
                                    new SelectionElement<int>(message: $"Trash", value : 0, spriteIndex: 0),
                                    new SelectionElement<int>(message: $"Hand", value : 1, spriteIndex: 0),
                                    new SelectionElement<int>(message: $"No Selection", value : 2, spriteIndex: 1),
                                };

                                string selectPlayerMessage = "Will you use a card from hand or trash?";
                                string notSelectPlayerMessage = "The opponent is choosing whether use a card from hand or trash.";

                                GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                            }
                            else if(validCardInTrash && !validCardInHand)
                            {
                                GManager.instance.userSelectionManager.SetInt(0);
                            }
                            else if (!validCardInTrash && validCardInHand)
                            {
                                GManager.instance.userSelectionManager.SetInt(1);
                            }
                            else
                            {
                                GManager.instance.userSelectionManager.SetInt(2);
                            }

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            useTrash = (GManager.instance.userSelectionManager.SelectedIntValue == 0);
                            #endregion

                            if(GManager.instance.userSelectionManager.SelectedIntValue != 2)
                            {
                                #region Card Selection
                                CardSource selectedCard = null;

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCard = cardSource;

                                    yield return null;
                                }


                                if (!useTrash)
                                {
                                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                    selectHandEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: HasSevenGreatDemonLordTrait,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: 1,
                                        canNoSelect: true,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        mode: SelectHandEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectHandEffect.SetUpCustomMessage(
                            "Select 1 card to place at the bottom of digivolution cards.",
                            "The opponent is selecting 1 card to place at the bottom of digivolution cards.");
                                    selectHandEffect.SetUpCustomMessage_ShowCard("Place bottom digivolution card");

                                    yield return StartCoroutine(selectHandEffect.Activate());
                                }

                                else
                                {
                                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                    selectCardEffect.SetUp(
                                        canTargetCondition: HasSevenGreatDemonLordTrait,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => true,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select 1 card to play.",
                                        maxCount: 1,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Trash,
                                        customRootCardList: null,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                                    selectCardEffect.SetUpCustomMessage(
                            "Select 1 card to place at the bottom of digivolution cards.",
                            "The opponent is selecting 1 card to place at the bottom of digivolution cards.");
                                    selectCardEffect.SetUpCustomMessage_ShowCard("Place bottom digivolution card");

                                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                                }
                                #endregion

                                #region Place As Source
                                if (selectedCard != null)
                                {
                                    if (card.Owner.GetBreedingAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                                    {
                                        Permanent selectedPermanent = card.Owner.GetBreedingAreaPermanents()[0];

                                        if (CanSelectPermanentCondition(selectedPermanent))
                                        {
                                            if (selectedPermanent != null)
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(
                                                    new List<CardSource>() { selectedCard },
                                                    activateClass));
                                            }
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                        
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }
            #endregion

            #region All Turns - Delay
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Digimon from breeding area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When one of your Digimon with the [Seven Great Demon Lords] trait is deleted, <Delay>.\r\n• You may play 1 Digimon card with the [Seven Great Demon Lords] trait from the digivolution cards of your [Gate of Deadly Sins] in the breeding area without paying the cost.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBreedingArea(permanent, card))
                    {
                        if (permanent.IsDigimon)
                        {
                            if(permanent.TopCard.ContainsCardName("Gate of Deadly Sins"))
                            {
                                if (permanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if(CardEffectCommons.IsOwnerPermanent(permanent, card))
                    {
                        return HasSevenGreatDemonLordTrait(permanent.TopCard);
                    }

                    return false;
                }

                bool HasSevenGreatDemonLordTrait(CardSource cardSource)
                {
                    return cardSource.ContainsTraits("Seven Great Demon Lords");
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                    {
                        return HasSevenGreatDemonLordTrait(cardSource);
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermanentCondition))
                        {
                            if (CardEffectCommons.CanDeclareOptionDelayEffect(card))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().CanBeDestroyedBySkill(activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool deleted = false;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass: activateClass,
                        successProcess: permanents => SuccessProcess(),
                        failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        deleted = true;

                        yield return null;
                    }

                    if (deleted)
                    {
                        if (card.Owner.GetBreedingAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                        {
                            Permanent selectedPermanent = card.Owner.GetBreedingAreaPermanents()[0];

                            if (CanSelectPermanentCondition(selectedPermanent))
                            {
                                if (selectedPermanent != null)
                                {
                                    if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                                    {
                                        int maxCount = Math.Min(1, selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition));

                                        List<CardSource> selectedCards = new List<CardSource>();

                                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                        selectCardEffect.SetUp(
                                                    canTargetCondition: CanSelectCardCondition,
                                                    canTargetCondition_ByPreSelecetedList: null,
                                                    canEndSelectCondition: null,
                                                    canNoSelect: () => false,
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

                                        selectCardEffect.SetUpCustomMessage(
                                            "Select 1 digivolution card to play.",
                                            "The opponent is selecting 1 digivolution card to play.");
                                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                                        yield return StartCoroutine(selectCardEffect.Activate());

                                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                                        {
                                            selectedCards.Add(cardSource);

                                            yield return null;
                                        }

                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                            cardSources: selectedCards,
                                            activateClass: activateClass,
                                            payCost: false,
                                            isTapped: false,
                                            root: SelectCardEffect.Root.DigivolutionCards,
                                            activateETB: true));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaceSelfDelayOptionSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}