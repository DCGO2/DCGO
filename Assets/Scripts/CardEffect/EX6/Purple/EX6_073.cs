using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace DCGO.CardEffects.EX6
{
    public class EX6_073 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsTraits("Seven Great Demon Lords") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level >= 5;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, 
                    digivolutionCost: 6, 
                    ignoreDigivolutionRequirement: false, 
                    card: card, 
                    condition: null));
            }
            #endregion

            #region When Digivolving/When Attacking Shared Conditions
            List<CardSource> digivolutionCards = new List<CardSource>();

            bool CanNoSelect(CardSource cardSource)
            {
                if (cardSource != null)
                {
                    if (cardSource.PayingCost(SelectCardEffect.Root.Hand, null, checkAvailability: false) > cardSource.Owner.MaxMemoryCost)
                    {
                        return false;
                    }
                }

                return true;
            }

            bool HasSevenGreatDemonLordsTrait(CardSource cardSource)
            {
                if (cardSource.IsDigimon || cardSource.IsOption)
                {
                    return cardSource.ContainsTraits("Seven Great Demon Lords");
                }

                return false;
            }

            bool CanSelectOponentsPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                {
                    if (permanent.IsDigimon || permanent.IsTamer)
                        return true;
                }

                return false;
            }
            //if (digivolutionCards.Count((filteredCard) =>  filteredCard.CardNames.Concat(cardSource.CardNames).Distinct().ToList().Count > 0) == 0)
            bool CanSelectTrashCardCondition(CardSource cardSource)
            {
                if (HasSevenGreatDemonLordsTrait(cardSource))
                {
                    if(!digivolutionCards.Some(filteredCard => filteredCard != cardSource && cardSource.HasSameCardName(filteredCard)))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
            {
                List<string> cardNames = GetNamesList(cardSources);

                foreach (string name in cardNames)
                {
                    if (cardSource.CardNames.Contains(name))
                        return false;
                }

                return true;
            }

            bool CanEndSelectCondition(List<CardSource> cardSources)
            {
                if (CardEffectCommons.HasNoElement(cardSources))
                {
                    return false;
                }

                return true;
            }

            List<string> GetNamesList(List<CardSource> cardSources)
            {
                List<string> cardNames = new List<string>();

                foreach (CardSource cardName in cardSources)
                {
                    foreach (string name in cardName.CardNames)
                    {
                        if (!cardNames.Contains(name))
                        {
                            cardNames.Add(name);
                        }
                    }
                }

                return cardNames;
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place up to 7 sources, delete digimon or tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may place up to 7 cards with different names and the [Seven Great Demon Lords] trait from your trash as this Digimon's bottom digivolution cards. If you placed 4 or more cards with this effect, delete 1 of your opponent's Digimon or Tamers.";
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
                    digivolutionCards = new List<CardSource>();

                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectTrashCardCondition(cardSource)))
                    {
                        bool noSelect = CanNoSelect(CardEffectCommons.GetCardFromHashtable(hashtable));
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = Math.Min(7, card.Owner.TrashCards.Count((cardSource) => CanSelectTrashCardCondition(cardSource)));

                        if (maxCount >= 1)
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectTrashCardCondition,
                                canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                canEndSelectCondition: CanEndSelectCondition,
                                canNoSelect: () => noSelect,
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

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);
                                digivolutionCards.Add(cardSource);
                                yield return null;
                            }

                            if (selectedCards.Count >= 1)
                            {
                                yield return StartCoroutine(AfterSelectCardCoroutine(selectedCards));
                            }
                        }
                    }

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(cardSources, activateClass));

                        if (cardSources.Count >= 4)
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectOponentsPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }
                }
            }
            #endregion

            #region When Attacking - Place Sources
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place up to 7 sources, delete digimon or tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] You may place up to 7 cards with different names and the [Seven Great Demon Lords] trait from your trash as this Digimon's bottom digivolution cards. If you placed 4 or more cards with this effect, delete 1 of your opponent's Digimon or Tamers.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    digivolutionCards = new List<CardSource>();

                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectTrashCardCondition(cardSource)))
                    {
                        bool noSelect = CanNoSelect(CardEffectCommons.GetCardFromHashtable(hashtable));
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = Math.Min(7, card.Owner.TrashCards.Count((cardSource) => CanSelectTrashCardCondition(cardSource)));

                        if (maxCount >= 1)
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectTrashCardCondition,
                                canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                canEndSelectCondition: CanEndSelectCondition,
                                canNoSelect: () => noSelect,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select cards to place in Digivolution cards.",
                                maxCount: 7,
                                canEndNotMax: true,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);
                                digivolutionCards.Add(cardSource);
                                yield return null;
                            }

                            if (selectedCards.Count >= 1)
                            {
                                yield return StartCoroutine(AfterSelectCardCoroutine(selectedCards));
                            }
                        }
                    }

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(cardSources, activateClass));

                        if (cardSources.Count >= 4)
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectOponentsPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 7 Digimon/Tamers, Then Trash 7 security. For each card deleted, reduce that number by 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] By returning 7 cards with different names and the [Seven Great Demon Lords] trait from this Digimon's digivolution cards to the bottom of the deck, delete 7 of your opponent's Digimon or Tamers. Then, trash the top 7 cards of your opponent's security stack. For each card deleted by this effect, reduce the cards trashed by 1.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if(card.PermanentOfThisCard().cardSources.Count(HasSevenGreatDemonLordsTrait) >= 7)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool returnedToLibrary = false;
                    List<CardSource> selectedCards = new List<CardSource>();
                    List<Permanent> deletedPermanents = new List<Permanent>();

                    #region Select Digivolution sources to bottom deck
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                                canTargetCondition: HasSevenGreatDemonLordsTrait,
                                canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 7 digivolution card to return to the bottom of deck.",
                                maxCount: 7,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: null);

                    selectCardEffect.SetUpCustomMessage("Select 7 digivolution card to return to the bottom of deck.", "The opponent is selecting 7 digivolution card to return to the bottom of deck.");
                    selectCardEffect.SetNotShowCard();

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    if (selectedCards.Count == 7)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new ReturnToLibraryBottomDigivolutionCardsClass(
                                card.PermanentOfThisCard(),
                                selectedCards, CardEffectCommons.CardEffectHashtable(activateClass)).ReturnToLibraryBottomDigivolutionCards());

                        returnedToLibrary = true;
                    }
                    #endregion

                    #region Delete Digimon/Tamers
                    if (returnedToLibrary)
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        int deleteCount = Math.Min(7, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, CanSelectOponentsPermanentCondition));

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOponentsPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: deleteCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: SelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    IEnumerator SelectPermanentCoroutine(List<Permanent> permanents)
                    {

                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                targetPermanents: permanents, 
                                activateClass: activateClass, 
                                successProcess: SuccessProcess, 
                                failureProcess: null));

                        yield return null;
                    }

                    IEnumerator SuccessProcess(List<Permanent> permanents)
                    {
                        deletedPermanents = permanents;

                        yield return null;
                    }
                    #endregion

                    if (returnedToLibrary && deletedPermanents.Count < 7)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            player: card.Owner.Enemy,
                            destroySecurityCount: 7 - deletedPermanents.Count,
                            cardEffect: activateClass,
                            fromTop: true).DestroySecurity());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}