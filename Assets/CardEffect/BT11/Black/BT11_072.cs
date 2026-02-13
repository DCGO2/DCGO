using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT11
{
    public class BT11_072 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 5 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Reveal the top 5 cards of your deck. Add 1 [Analogman] among them to your hand, and add 1 card with [Cyborg] or [Machine] in its traits to your hand or place it under this Digimon as its bottom digivolution card. Trash the rest.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.CardNames.Contains("Analogman");
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    return cardSource.CardTraits.Contains("Cyborg") || cardSource.CardTraits.Contains("Machine");
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
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                                        revealCount: 5,
                                        simplifiedSelectCardConditions:
                                        new SimplifiedSelectCardConditionClass[]
                                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 [Analogman].",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition1,
                            message: "Select 1 card with [Cyborg] or [Machine] in its traits.",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: 1,
                            selectCardCoroutine: SelectCardCoroutine),
                                        },
                                        remainingCardsPlace: RemainingCardsPlace.Trash,
                                        activateClass: activateClass
                                    ));

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
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
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { cardSource }, "Added Hand Card", true, true));

                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { cardSource }, false, activateClass));
                        }
                        else
                        {
                            if (CardEffectCommons.IsExistOnBattleArea(card))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { cardSource }, "Digivolution Card", true, true));

                                yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { cardSource }, activateClass));

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(card.PermanentOfThisCard()));
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 5 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Reveal the top 5 cards of your deck. Add 1 [Analogman] among them to your hand, and add 1 card with [Cyborg] or [Machine] in its traits to your hand or place it under this Digimon as its bottom digivolution card. Trash the rest.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.CardNames.Contains("Analogman");
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    return cardSource.CardTraits.Contains("Cyborg") || cardSource.CardTraits.Contains("Machine");
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
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                                        revealCount: 5,
                                        simplifiedSelectCardConditions:
                                        new SimplifiedSelectCardConditionClass[]
                                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 [Analogman].",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition1,
                            message: "Select 1 card with [Cyborg] or [Machine] in its traits.",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: 1,
                            selectCardCoroutine: SelectCardCoroutine),
                                        },
                                        remainingCardsPlace: RemainingCardsPlace.Trash,
                                        activateClass: activateClass
                                    ));

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
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
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { cardSource }, "Added Hand Card", true, true));

                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { cardSource }, false, activateClass));
                        }
                        else
                        {
                            if (CardEffectCommons.IsExistOnBattleArea(card))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { cardSource }, "Digivolution Card", true, true));

                                yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { cardSource }, activateClass));

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(card.PermanentOfThisCard()));
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 [Analogman] to the bottom of deck to play 1 [Machinedramon] from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] By placing 1 of your [Analogman]s in play at the bottom of its owner's deck, you may play 1 [Machinedramon] from your hand without paying the cost.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.CardNames.Contains("Analogman"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardNames.Contains("Machinedramon"))
                    {
                        if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanActivateOnDeletion(card))
                    {
                        if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                    {
                        Permanent bouncePermanent = null;

                        int maxCount = 1;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 [Analogman] to return to the bottom of deck.", "The opponent is selecting 1 [Analogman] to return to the bottom of deck.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            bouncePermanent = permanent;

                            yield return null;
                        }

                        if (bouncePermanent != null)
                        {
                            if (bouncePermanent.TopCard != null)
                            {
                                if (!bouncePermanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    if (!bouncePermanent.CannotReturnToLibrary(activateClass))
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(new List<Permanent>() { bouncePermanent }, CardEffectCommons.CardEffectHashtable(activateClass)).DeckBounce());

                                        if (bouncePermanent.TopCard == null && bouncePermanent.LibraryBounceEffect == activateClass)
                                        {
                                            if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                                            {
                                                List<CardSource> selectedCards = new List<CardSource>();

                                                maxCount = 1;

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

                                                selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                                                selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                                                yield return StartCoroutine(selectHandEffect.Activate());

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
                                                    root: SelectCardEffect.Root.Hand,
                                                    activateETB: true));
                                            }
                                        }
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