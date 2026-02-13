using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.P
{
    public class P_167 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            int totalSourceCount = 0;

            #region Shared When Digivolving/Start of Main

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.EqualsTraits("Mineral") || cardSource.EqualsTraits("Rock");
            }
            
            #endregion
            
            #region When Digivolving
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 source and reveal 3.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] By trashing 1 [Mineral] or [Rock] trait card from any of your Digimon's digivolution cards, reveal the top 3 cards of your deck. Among them, add 1 [Mineral] or [Rock] trait card to the hand or place 1 such card as this Digimon's bottom digivolution card. Return the rest to the top or bottom of the deck.";
                }

                bool HasProperTrait(CardSource source)
                {
                    return !source.CanNotTrashFromDigivolutionCards(activateClass) &&
                        source.EqualsTraits("Mineral") || source.EqualsTraits("Rock");
                }

                bool CanSelectTrashTargetCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        totalSourceCount = permanent.DigivolutionCards.Count(HasProperTrait);
                        if (totalSourceCount >= 1)
                        {
                            return true;
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
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTrashTargetCondition))
                        {
                            if (totalSourceCount >= 1)
                                return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool cardsTrashed = false;
                    Permanent selectedPermanent = null;
                    CardSource selectedCard = null;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                        permanentCondition: CanSelectTrashTargetCondition,
                        cardCondition: HasProperTrait,
                        maxCount: 1,
                        canNoTrash: false,
                        isFromOnly1Permanent: false,
                        activateClass: activateClass,
                        afterSelectionCoroutine: AfterTrashedCards
                    ));

                    IEnumerator AfterTrashedCards(Permanent permanent, List<CardSource> cards)
                    {
                        if (cards.Count == 1)
                            cardsTrashed = true;

                        selectedPermanent = permanent;

                        yield return null;
                    }

                    if (cardsTrashed)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                            revealCount: 3,
                            simplifiedSelectCardConditions:
                            new SimplifiedSelectCardConditionClass[]
                            {
                                new SimplifiedSelectCardConditionClass(
                                    canTargetCondition:CanSelectCardCondition,
                                    message: "Select 1 card with [Mineral] or [Rock] trait.",
                                    mode: SelectCardEffect.Mode.Custom,
                                    maxCount: 1,
                                    selectCardCoroutine: SelectCardCoroutine),
                            },
                            remainingCardsPlace: RemainingCardsPlace.DeckTopOrBottom,
                            activateClass: activateClass
                        )); 
                        
                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;

                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                            {
                                new SelectionElement<bool>(message: $"Add to hand", value : true, spriteIndex: 0),
                                new SelectionElement<bool>(message: $"Add to digivolution cards", value : false, spriteIndex: 1),
                            };

                            string selectPlayerMessage = "To which area do you place the card?";
                            string notSelectPlayerMessage = "The opponent is choosing to which area to place the card.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            bool toHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                            if (toHand)
                            {
                                yield return ContinuousController.instance.StartCoroutine(
                                    GManager.instance.GetComponent<Effects>().ShowCardEffect(
                                        new List<CardSource>() { cardSource },
                                        "Added Hand Card",
                                        true,
                                        true));

                                yield return ContinuousController.instance.StartCoroutine(
                                    CardObjectController.AddHandCards(
                                        new List<CardSource>() { cardSource },
                                        false,
                                        activateClass));
                            }
                            else
                            {
                                if (CardEffectCommons.IsExistOnBattleArea(card))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { cardSource }, "Digivolution Card", true, true));
                                    Permanent selectedPermanent = card.PermanentOfThisCard();

                                    if (selectedPermanent != null)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass));
                                    }
                                }
                            }
                        }

                    }
                }
            }
            
            #endregion
            
            #region Start of Your Main Phase
            
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 source and reveal 3.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Start of Your Main Phase] By trashing 1 [Mineral] or [Rock] trait card from any of your Digimon's digivolution cards, reveal the top 3 cards of your deck. Among them, add 1 [Mineral] or [Rock] trait card to the hand or place 1 such card as this Digimon's bottom digivolution card. Return the rest to the top or bottom of the deck.";
                }

                bool HasProperTrait(CardSource source)
                {
                    return !source.CanNotTrashFromDigivolutionCards(activateClass) &&
                        source.EqualsTraits("Mineral") || source.EqualsTraits("Rock");
                }

                bool CanSelectTrashTargetCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        totalSourceCount = permanent.DigivolutionCards.Count(HasProperTrait);
                        if (totalSourceCount >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
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
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTrashTargetCondition))
                        {
                            if (totalSourceCount >= 1)
                                return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool cardsTrashed = false;
                    Permanent selectedPermanent = null;
                    CardSource selectedCard = null;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                        permanentCondition: CanSelectTrashTargetCondition,
                        cardCondition: HasProperTrait,
                        maxCount: 1,
                        canNoTrash: false,
                        isFromOnly1Permanent: false,
                        activateClass: activateClass,
                        afterSelectionCoroutine: AfterTrashedCards
                    ));

                    IEnumerator AfterTrashedCards(Permanent permanent, List<CardSource> cards)
                    {
                        if (cards.Count == 1)
                            cardsTrashed = true;

                        selectedPermanent = permanent;

                        yield return null;
                    }

                    if (cardsTrashed)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                            revealCount: 3,
                            simplifiedSelectCardConditions:
                            new SimplifiedSelectCardConditionClass[]
                            {
                                new SimplifiedSelectCardConditionClass(
                                    canTargetCondition:CanSelectCardCondition,
                                    message: "Select 1 card with [Mineral] or [Rock] trait.",
                                    mode: SelectCardEffect.Mode.Custom,
                                    maxCount: 1,
                                    selectCardCoroutine: SelectCardCoroutine),
                            },
                            remainingCardsPlace: RemainingCardsPlace.DeckTopOrBottom,
                            activateClass: activateClass
                        )); 
                        
                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;

                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                            {
                                new SelectionElement<bool>(message: $"Add to hand", value : true, spriteIndex: 0),
                                new SelectionElement<bool>(message: $"Add to digivolution cards", value : false, spriteIndex: 1),
                            };

                            string selectPlayerMessage = "To which area do you place the card?";
                            string notSelectPlayerMessage = "The opponent is choosing to which area to place the card.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            bool toHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                            if (toHand)
                            {
                                yield return ContinuousController.instance.StartCoroutine(
                                    GManager.instance.GetComponent<Effects>().ShowCardEffect(
                                        new List<CardSource>() { cardSource },
                                        "Added Hand Card",
                                        true,
                                        true));

                                yield return ContinuousController.instance.StartCoroutine(
                                    CardObjectController.AddHandCards(
                                        new List<CardSource>() { cardSource },
                                        false,
                                        activateClass));
                            }
                            else
                            {
                                if (CardEffectCommons.IsExistOnBattleArea(card))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { cardSource }, "Digivolution Card", true, true));
                                    Permanent selectedPermanent = card.PermanentOfThisCard();

                                    if (selectedPermanent != null)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass));
                                    }
                                }
                            }
                        }

                    }
                }
            }
            
            #endregion

            #region Trash Source - ESS
            
            if (timing == EffectTiming.OnDigivolutionCardDiscarded)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("De-Digivolve 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "When effects trash this card from a [Mineral] or [Rock] trait Digimon's digivolution cards, <De-Digivolve 1> 1 of your opponent's Digimon.";
                }

                bool CanSelectOpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    Permanent trashedPermanent = CardEffectCommons.GetPermanentFromHashtable(hashtable);
                    return (trashedPermanent.TopCard.EqualsTraits("Mineral") || trashedPermanent.TopCard.EqualsTraits("Rock")) && 
                           CardEffectCommons.CanTriggerOnTrashSelfDigivolutionCard(hashtable, cardEffect => cardEffect != null, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnTrash(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectOpponentsDigimon))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOpponentsDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
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
                        yield return ContinuousController.instance.StartCoroutine(new IDegeneration(permanent, 1, activateClass).Degeneration());
                    }
                }
            }
            
            #endregion

            return cardEffects;
        }
    }
}