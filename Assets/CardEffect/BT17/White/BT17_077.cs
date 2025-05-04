using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DCGO.CardEffects.BT17
{
    public class BT17_077 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if(targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 6)
                        return targetPermanent.TopCard.ContainsCardName("Imperialdramon");

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 5,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region Blast Digivolve
            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 to bottom of deck, unsusped this digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] By returning 1 of your opponent's Digimon with no digivolution cards to the bottom of the deck, unsuspend this Digimon.";
                }

                bool IsOpponentsDigimon(Permanent permanent)
                {
                    if(CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                        return permanent.DigivolutionCards.Count == 0;

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(IsOpponentsDigimon);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentsDigimon))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsOpponentsDigimon));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOpponentsDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectedBottomDeck,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to bottom deck.", "The opponent is selecting 1 Digimon to bottom deck.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectedBottomDeck(Permanent bottomDeckedPermanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeckBouncePeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent>() { bottomDeckedPermanent },
                            activateClass: activateClass,
                            successProcess: SuccessProcess(),
                            failureProcess: null));

                            IEnumerator SuccessProcess()
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                                    new List<Permanent> { card.PermanentOfThisCard() },
                                    activateClass).Unsuspend());
                            }                                
                        }
                    }
                }
            }
            #endregion

            #region On Play/When Digivolving Shared
            bool HasWhiteLevelSeven(CardSource source)
            {
                if (source.CardColors.Contains(CardColor.White))
                    return source.HasLevel && source.Level == 7;

                return false;
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash digivolution sources, Return all cards in trash, Memory +3", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Trash all digivolution cards of all of your opponent's Digimon. Then, return all cards from your or your opponent's trash to the bottom of the deck. If this effect returned a white level 7 card, gain 3 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    foreach (Permanent selectedPermanent in card.Owner.Enemy.GetBattleAreaDigimons())
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                            targetPermanent: selectedPermanent, 
                            trashCount: selectedPermanent.DigivolutionCards.Count, 
                            isFromTop: true, 
                            activateClass: activateClass));
                    }

                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Your Trash", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Opponents Trash", value : false, spriteIndex: 1),
                        };

                    string selectPlayerMessage = "Will you return cards from your or your opponent's trash?";
                    string notSelectPlayerMessage = "The opponent is choosing to return cards from your or their trash.";

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool willReturnYourTrash = GManager.instance.userSelectionManager.SelectedBoolValue;

                    List<CardSource> returnedSources = willReturnYourTrash ? card.Owner.TrashCards.Clone() : card.Owner.Enemy.TrashCards.Clone();

                    if (returnedSources.Count >= 1)
                    {
                        if (returnedSources.Count == 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(returnedSources));

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(returnedSources, "Deck Bottom Cards", true, true));
                        }
                        else
                        {
                            int maxCount = returnedSources.Count;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                            canTargetCondition: (cardSource) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine1,
                            message: "Select cards to place at the bottom of the deck\n(cards will be placed back to the bottom of the deck so that cards with lower numbers are on top).",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: false,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: returnedSources,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                            selectCardEffect.SetNotShowCard();
                            selectCardEffect.SetNotAddLog();

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(cardSources));

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(cardSources, "Deck Bottom Cards", true, true));
                            }
                        }
                    }

                    if (returnedSources.Count(HasWhiteLevelSeven) > 0)
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(3, activateClass));
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash digivolution sources, Return all cards in trash, Memory +3", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Trash all digivolution cards of all of your opponent's Digimon. Then, return all cards from your or your opponent's trash to the bottom of the deck. If this effect returned a white level 7 card, gain 3 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    foreach (Permanent selectedPermanent in card.Owner.Enemy.GetBattleAreaDigimons())
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                            targetPermanent: selectedPermanent,
                            trashCount: selectedPermanent.DigivolutionCards.Count,
                            isFromTop: true,
                            activateClass: activateClass));
                    }

                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Your Trash", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Opponents Trash", value : false, spriteIndex: 1),
                        };

                    string selectPlayerMessage = "Will you return cards from your or your opponent's trash?";
                    string notSelectPlayerMessage = "The opponent is choosing to return cards from your or their trash.";

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool willReturnYourTrash = GManager.instance.userSelectionManager.SelectedBoolValue;

                    List<CardSource> returnedSources = willReturnYourTrash ? card.Owner.TrashCards.Clone() : card.Owner.Enemy.TrashCards.Clone();

                    if (returnedSources.Count >= 1)
                    {
                        if (returnedSources.Count == 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(returnedSources));

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(returnedSources, "Deck Bottom Cards", true, true));
                        }
                        else
                        {
                            int maxCount = returnedSources.Count;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                            canTargetCondition: (cardSource) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine1,
                            message: "Select cards to place at the bottom of the deck\n(cards will be placed back to the bottom of the deck so that cards with lower numbers are on top).",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: false,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: returnedSources,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                            selectCardEffect.SetNotShowCard();
                            selectCardEffect.SetNotAddLog();

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(cardSources));

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(cardSources, "Deck Bottom Cards", true, true));
                            }
                        }
                    }

                    if (returnedSources.Count(HasWhiteLevelSeven) > 0)
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(3, activateClass));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}