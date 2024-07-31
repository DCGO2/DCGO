using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DCGO.CardEffects.BT17
{
    public class ImperialdramonPaladinMode_BT17_077 : CEntity_Effect
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

            #region Rule - Trait: Also has [Free]
            if (timing == EffectTiming.None)
            {
                ChangeTraitsClass changeTraitsClass = new ChangeTraitsClass();
                changeTraitsClass.SetUpICardEffect("Trait: Also has [Free]", CanUseCondition, card);
                changeTraitsClass.SetUpChangeTraitsClass(changeeTraits: ChangeTraits);
                cardEffects.Add(changeTraitsClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                List<string> ChangeTraits(CardSource cardSource, List<string> cardTraits)
                {
                    if (cardSource == card)
                        cardTraits.Add("Free");

                    return cardTraits;
                }
            }
            #endregion

            #region Blast Digivolve
            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.None)
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
                    if(CardEffectCommons.IsOpponentPermanent(permanent, card))
                        return permanent.DigivolutionCards.Count == 0;

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanTriggerOnAttack(hashtable, card))
                        return CardEffectCommons.HasMatchConditionPermanent(IsOpponentsDigimon);

                    return true;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
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
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: SelectedBottomDeck,
                            mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to bottom deck.", "The opponent is selecting 1 Digimon to bottom deck.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectedBottomDeck(List<Permanent> untappedPermanents)
                        {
                            if(untappedPermanents.Count > 0)
                                yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                                    untappedPermanents,
                                    activateClass).Unsuspend());
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
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
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

                    List<CardSource> returnedSources = willReturnYourTrash ? card.Owner.TrashCards : card.Owner.Enemy.TrashCards;

                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(returnedSources));

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(returnedSources, "Deck Bottom Cards", true, true));

                    if(returnedSources.Count(HasWhiteLevelSeven) > 0)
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(3, activateClass));
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash digivolution sources, Return all cards in trash, Memory +3", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
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

                    List<CardSource> returnedSources = willReturnYourTrash ? card.Owner.TrashCards : card.Owner.Enemy.TrashCards;

                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(returnedSources));

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(returnedSources, "Deck Bottom Cards", true, true));

                    if (returnedSources.Count(HasWhiteLevelSeven) > 0)
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(3, activateClass));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}