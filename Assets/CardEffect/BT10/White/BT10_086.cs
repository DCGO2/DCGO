using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.BT10
{
    public class BT10_086 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("Omnimon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return !CardEffectCommons.IsExistOnField(card);
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent != null)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(targetPermanent, card))
                        {
                            if (targetPermanent.DigivolutionCards.Some((cardSource) =>
                            cardSource.CardNames.Contains("XAntibody") || cardSource.CardNames.Contains("X Antibody")))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                cardEffects.Add(
                    CardEffectFactory.ChangeDigivolutionCostStaticEffect(
                        changeValue: -2,
                        permanentCondition: PermanentCondition,
                        cardCondition: CardSourceCondition,
                        rootCondition: RootCondition,
                        isInheritedEffect: false,
                        card: card,
                        condition: Condition,
                        setFixedCost: false));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return oppponent's all Digimons with the highest level to the bottom of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Return all of your opponent's Digimon with the highest level to the bottom of their owners' decks in any order.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsMaxLevel(permanent, card.Owner.Enemy);
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
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            List<Permanent> selectedPermanents = new List<Permanent>();

                            foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaDigimons())
                            {
                                if (CanSelectPermanentCondition(permanent))
                                {
                                    if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                    {
                                        if (!permanent.CannotReturnToLibrary(activateClass))
                                        {
                                            selectedPermanents.Add(permanent);
                                        }
                                    }
                                }
                            }

                            if (selectedPermanents.Count >= 1)
                            {
                                if (selectedPermanents.Count == 1)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(selectedPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).DeckBounce());
                                }
                                else
                                {
                                    List<CardSource> cardSources = selectedPermanents.Map(permanent => permanent.TopCard);

                                    List<SkillInfo> skillInfos = cardSources.Map(cardSource =>
                                    {
                                        ChangeBaseDPClass cardEffect = new ChangeBaseDPClass();
                                        cardEffect.SetUpICardEffect(" ", null, cardSource);

                                        return new SkillInfo(cardEffect, null, EffectTiming.None);
                                    });

                                    List<CardSource> selectedCards = new List<CardSource>();

                                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                    selectCardEffect.SetUp(
                                        canTargetCondition: (cardSource) => true,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => false,
                                        selectCardCoroutine: null,
                                        afterSelectCardCoroutine: AfterSelectCardCoroutine1,
                                        message: "Specify the order to place the card at the bottom of the deck\n(cards will be placed back to the bottom of the deck so that cards with lower numbers are on top).",
                                        maxCount: cardSources.Count,
                                        canEndNotMax: false,
                                        isShowOpponent: false,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Custom,
                                        customRootCardList: cardSources,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                                    selectCardEffect.SetNotShowCard();
                                    selectCardEffect.SetNotAddLog();
                                    selectCardEffect.SetUpSkillInfos(skillInfos);

                                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                    IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
                                    {
                                        if (cardSources.Count >= 1)
                                        {
                                            selectedCards = cardSources.Clone();

                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(cardSources, "Deck Bottom Cards", true, true));
                                        }
                                    }

                                    if (selectedCards.Count >= 1)
                                    {
                                        List<Permanent> libraryPermanets = selectedCards.Map(cardSource => cardSource.PermanentOfThisCard());

                                        if (libraryPermanets.Count >= 1)
                                        {
                                            DeckBottomBounceClass putLibraryBottomPermanent = new DeckBottomBounceClass(libraryPermanets, CardEffectCommons.CardEffectHashtable(activateClass));

                                            putLibraryBottomPermanent.SetNotShowCards();

                                            yield return ContinuousController.instance.StartCoroutine(putLibraryBottomPermanent.DeckBounce());
                                        }
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
                activateClass.SetUpICardEffect("Return 1 digivolution card to bottom of deck and trash Security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("TrashSecurity_BT10_086");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving][Once Per Turn] By placing 1 [X Antibody] or level 6 card from this Digimon's digivolution cards at the bottom of its owner's deck, reveal all of your opponent's security cards, and trash 1 of them. Place the rest in your opponent's security stack face down. Then, your opponent shuffles their security stack.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardNames.Contains("X Antibody") || cardSource.CardNames.Contains("XAntibody"))
                    {
                        return true;
                    }

                    if (cardSource.Level == 6)
                    {
                        if (cardSource.HasLevel)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    return true;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool returnedToLibrary = false;

                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => false,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 digivolution card to return to the bottom of deck.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: null);

                    selectCardEffect.SetUpCustomMessage(
                        "Select 1 digivolution card to return to the bottom of deck.",
                        "The opponent is selecting 1 digivolution card to return to the bottom of deck.");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    foreach (CardSource cardSource in selectedCards)
                    {
                        if (!cardSource.IsToken)
                        {
                            returnedToLibrary = true;

                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(
                                new List<CardSource>() { cardSource }));
                        }
                    }

                    if (returnedToLibrary)
                    {
                        if (card.Owner.Enemy.SecurityCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(card.Owner.Enemy.SecurityCards, "Security Cards", true, true));

                            int maxCount = 1;

                            CardSource selectedCard = null;

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition1,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => false,
                                selectCardCoroutine: SelectCardCoroutine1,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                message: "Select 1 card to discard.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Security,
                                customRootCardList: card.Owner.Enemy.SecurityCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetIsSecurity();
                            selectCardEffect.SetUpCustomMessage_ShowCard("Trash card");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine1(CardSource cardSource)
                            {
                                selectedCard = cardSource;
                                yield return null;
                            }

                            IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                            {
                                if (cardSources.Count >= 1)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                                        player: card.Owner.Enemy,
                                        refSkillInfos: ref ContinuousController.instance.nullSkillInfos).ReduceSecurity());
                                }
                            }

                            if (selectedCard != null)
                            {
                                #region
                                selectedCard.Owner.securityObject.securityBreakGlass.ShowBlueMatarial();

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().BreakSecurityEffect(selectedCard.Owner));

                                yield return new WaitForSeconds(0.1f);

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().EnterSecurityCardEffect(selectedCard));

                                yield return new WaitForSeconds(0.5f);

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().DestroySecurityEffect(selectedCard));
                                #endregion

                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(selectedCard));
                            }

                            ContinuousController.instance.PlaySE(GManager.instance.ShuffleSE);

                            card.Owner.Enemy.SecurityCards = RandomUtility.ShuffledDeckCards(card.Owner.Enemy.SecurityCards);
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 digivolution card to bottom of deck and trash Security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("TrashSecurity_BT10_086");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking][Once Per Turn] By placing 1 [X Antibody] or level 6 card from this Digimon's digivolution cards at the bottom of its owner's deck, reveal all of your opponent's security cards, and trash 1 of them. Place the rest in your opponent's security stack face down. Then, your opponent shuffles their security stack.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardNames.Contains("X Antibody") || cardSource.CardNames.Contains("XAntibody"))
                    {
                        return true;
                    }

                    if (cardSource.Level == 6)
                    {
                        if (cardSource.HasLevel)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    return true;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool returnedToLibrary = false;

                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => false,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 digivolution card to return to the bottom of deck.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: null);

                    selectCardEffect.SetUpCustomMessage(
                        "Select 1 digivolution card to return to the bottom of deck.",
                        "The opponent is selecting 1 digivolution card to return to the bottom of deck.");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    foreach (CardSource cardSource in selectedCards)
                    {
                        if (!cardSource.IsToken)
                        {
                            returnedToLibrary = true;

                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(new List<CardSource>() { cardSource }));
                        }
                    }

                    if (returnedToLibrary)
                    {
                        if (card.Owner.Enemy.SecurityCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(
                                card.Owner.Enemy.SecurityCards,
                                "Security Cards",
                                true,
                                true));

                            int maxCount = 1;

                            CardSource selectedCard = null;

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition1,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => false,
                                selectCardCoroutine: SelectCardCoroutine1,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                message: "Select 1 card to discard.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Security,
                                customRootCardList: card.Owner.Enemy.SecurityCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage_ShowCard("Trash card");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine1(CardSource cardSource)
                            {
                                selectedCard = cardSource;
                                yield return null;
                            }

                            IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                            {
                                if (cardSources.Count >= 1)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                                        player: card.Owner.Enemy,
                                        refSkillInfos: ref ContinuousController.instance.nullSkillInfos).ReduceSecurity());
                                }
                            }

                            if (selectedCard != null)
                            {
                                #region
                                selectedCard.Owner.securityObject.securityBreakGlass.ShowBlueMatarial();

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().BreakSecurityEffect(selectedCard.Owner));

                                yield return new WaitForSeconds(0.1f);

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().EnterSecurityCardEffect(selectedCard));

                                yield return new WaitForSeconds(0.5f);

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().DestroySecurityEffect(selectedCard));
                                #endregion

                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(selectedCard));
                            }

                            ContinuousController.instance.PlaySE(GManager.instance.ShuffleSE);

                            card.Owner.Enemy.SecurityCards = RandomUtility.ShuffledDeckCards(card.Owner.Enemy.SecurityCards);
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}