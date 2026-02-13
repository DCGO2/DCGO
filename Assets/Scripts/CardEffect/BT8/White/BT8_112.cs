using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT8_112 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.BeforePayCost)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Digivolution Cost -4", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            activateClass.SetHashString("DigivolutionCost-4_BT8_112");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "When one of your Digimon would digivolve into this card in your hand, you may return 1 white level 7 Digimon card from your trash to the bottom of your deck to reduce the digivolution cost by 4.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.CardColors.Contains(CardColor.White))
                {
                    if (cardSource.Level == 7)
                    {
                        if (cardSource.IsDigimon)
                        {
                            if (cardSource.HasLevel)
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
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card);
            }

            bool CardCondition(CardSource cardSource)
            {
                return cardSource == card && card.Owner.HandCards.Contains(card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(hashtable, PermanentCondition, CardCondition);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                {
                    return true;
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                {
                    int maxCount = Math.Min(1, card.Owner.TrashCards.Count(CanSelectCardCondition));

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        message: "Select 1 card to return to the bottom of deck.",
                        maxCount: maxCount,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetNotAddLog();
                    selectCardEffect.SetUpCustomMessage_ShowCard("Deck Bottom Card");
                    selectCardEffect.SetUpCustomMessage(
                        "Select 1 card to put on bottom of the deck.",
                        "The opponent is selecting 1 card to put on bottom of the deck.");

                    yield return StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(
                            new List<CardSource>() { cardSource }));
                    }

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count >= 1)
                        {
                            yield return null;

                            if (card.Owner.CanReduceCost(new List<Permanent>() { new Permanent(new List<CardSource>()) }, card))
                            {
                                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                            }

                            ChangeCostClass changeCostClass = new ChangeCostClass();
                            changeCostClass.SetUpICardEffect("Digivolution Cost -4", CanUseCondition1, card);
                            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                            card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                return true;
                            }

                            int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                            {
                                if (CardSourceCondition(cardSource))
                                {
                                    if (RootCondition(root))
                                    {
                                        if (PermanentsCondition(targetPermanents))
                                        {
                                            Cost -= 4;
                                        }
                                    }
                                }

                                return Cost;
                            }

                            bool PermanentsCondition(List<Permanent> targetPermanents)
                            {
                                if (targetPermanents != null)
                                {
                                    if (targetPermanents.Count(PermanentCondition) >= 1)
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            bool PermanentCondition(Permanent targetPermanent)
                            {
                                if (targetPermanent.TopCard != null)
                                {
                                    if (targetPermanent.TopCard.Owner == card.Owner)
                                    {
                                        if (targetPermanent.TopCard.Owner.GetBattleAreaPermanents().Contains(targetPermanent))
                                        {
                                            return true;
                                        }
                                    }
                                }

                                return false;
                            }

                            bool CardSourceCondition(CardSource cardSource)
                            {
                                if (cardSource != null)
                                {
                                    if (cardSource == card)
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            bool RootCondition(SelectCardEffect.Root root)
                            {
                                return true;
                            }

                            bool isUpDown()
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Trash digivolution cards and return Digimons to the bottom of deck", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] You may return 1 2-color card from this Digimon's digivolution cards to the bottom of its owner's deck to trash all of the digivolution cards of 1 of your opponent's Digimon. Then, return all of your opponent's Digimon with no digivolution cards to the bottom of their owners' decks in any order.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource != null)
                {
                    if (cardSource.CardColors.Count == 2)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.DigivolutionCards.Count((cardSource) => !cardSource.CanNotTrashFromDigivolutionCards(activateClass)) >= 1)
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
                    if (card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                bool returned = false;

                int maxCount = Math.Min(1, card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => CanSelectCardCondition(cardSource)));

                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to return to the bottom of deck.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                selectCardEffect.SetNotAddLog();
                selectCardEffect.SetNotShowCard();
                selectCardEffect.SetUpCustomMessage(
                    "Select 1 card to put on bottom of the deck.",
                    "The opponent is selecting 1 card to put on bottom of the deck.");

                yield return StartCoroutine(selectCardEffect.Activate());

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    yield return ContinuousController.instance.StartCoroutine(new ReturnToLibraryBottomDigivolutionCardsClass(
                        permanent: card.PermanentOfThisCard(),
                        cardSources: new List<CardSource>() { cardSource },
                        hashtable: CardEffectCommons.CardEffectHashtable(activateClass)
                    ).ReturnToLibraryBottomDigivolutionCards());

                    returned = true;
                }

                if (returned)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage(
                            "Select 1 Digimon that will trash digivolution cards.",
                            "The opponent is selecting 1 Digimon that will trash digivolution cards.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                                targetPermanent: selectedPermanent,
                                trashCount: selectedPermanent.DigivolutionCards.Count,
                                isFromTop: true,
                                activateClass: activateClass));
                        }
                    }
                }

                List<Permanent> selectedPermanents = new List<Permanent>();

                foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaDigimons())
                {
                    if (permanent.HasNoDigivolutionCards)
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
                        yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(
                            selectedPermanents,
                            CardEffectCommons.CardEffectHashtable(activateClass)).DeckBounce());
                    }

                    else
                    {
                        List<CardSource> cardSources = new List<CardSource>();

                        foreach (Permanent permanent in selectedPermanents)
                        {
                            cardSources.Add(permanent.TopCard);
                        }

                        List<SkillInfo> skillInfos = new List<SkillInfo>();

                        foreach (CardSource cardSource in cardSources)
                        {
                            ICardEffect cardEffect = new ChangeBaseDPClass();
                            cardEffect.SetUpICardEffect(" ", null, cardSource);

                            skillInfos.Add(new SkillInfo(cardEffect, null, EffectTiming.None));
                        }

                        List<CardSource> selectedCards = new List<CardSource>();

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
                        selectCardEffect.SetUpSkillInfos(skillInfos);

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1)
                            {
                                foreach (CardSource cardSource in cardSources)
                                {
                                    selectedCards.Add(cardSource);
                                }

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(cardSources, "Deck Bottom Cards", true, true));
                            }
                        }

                        if (selectedCards.Count >= 1)
                        {
                            List<Permanent> libraryPermanets = new List<Permanent>();

                            foreach (CardSource cardSource in selectedCards)
                            {
                                libraryPermanets.Add(cardSource.PermanentOfThisCard());
                            }

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

        if (timing == EffectTiming.OnAllyAttack)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Trash digivolution cards and return Digimons to the bottom of deck", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Attacking] You may return 1 2-color card from this Digimon's digivolution cards to the bottom of its owner's deck to trash all of the digivolution cards of 1 of your opponent's Digimon. Then, return all of your opponent's Digimon with no digivolution cards to the bottom of their owners' decks in any order.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.CardColors.Count == 2;
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.DigivolutionCards.Count((cardSource) => !cardSource.CanNotTrashFromDigivolutionCards(activateClass)) >= 1)
                    {
                        return true;
                    }
                }

                return false;
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
                bool returned = false;

                int maxCount = Math.Min(1, card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => CanSelectCardCondition(cardSource)));

                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to return to the bottom of deck.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                selectCardEffect.SetNotAddLog();
                selectCardEffect.SetNotShowCard();
                selectCardEffect.SetUpCustomMessage(
                    "Select 1 card to put on bottom of the deck.",
                    "The opponent is selecting 1 card to put on bottom of the deck.");

                yield return StartCoroutine(selectCardEffect.Activate());

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    yield return ContinuousController.instance.StartCoroutine(new ReturnToLibraryBottomDigivolutionCardsClass(
                        permanent: card.PermanentOfThisCard(),
                        cardSources: new List<CardSource>() { cardSource },
                        hashtable: CardEffectCommons.CardEffectHashtable(activateClass)
                    ).ReturnToLibraryBottomDigivolutionCards());

                    returned = true;
                }

                if (returned)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will trash digivolution cards.", "The opponent is selecting 1 Digimon that will trash digivolution cards.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedPermanent, trashCount: selectedPermanent.DigivolutionCards.Count, isFromTop: true, activateClass: activateClass));
                        }
                    }
                }

                List<Permanent> selectedPermanents = new List<Permanent>();

                foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaDigimons())
                {
                    if (permanent.HasNoDigivolutionCards)
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
                        List<CardSource> cardSources = new List<CardSource>();

                        foreach (Permanent permanent in selectedPermanents)
                        {
                            cardSources.Add(permanent.TopCard);
                        }

                        List<SkillInfo> skillInfos = new List<SkillInfo>();

                        foreach (CardSource cardSource in cardSources)
                        {
                            ICardEffect cardEffect = new ChangeBaseDPClass();
                            cardEffect.SetUpICardEffect(" ", null, cardSource);

                            skillInfos.Add(new SkillInfo(cardEffect, null, EffectTiming.None));
                        }

                        List<CardSource> selectedCards = new List<CardSource>();

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
                        selectCardEffect.SetUpSkillInfos(skillInfos);

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1)
                            {
                                foreach (CardSource cardSource in cardSources)
                                {
                                    selectedCards.Add(cardSource);
                                }

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(cardSources, "Deck Bottom Cards", true, true));
                            }
                        }

                        if (selectedCards.Count >= 1)
                        {
                            List<Permanent> libraryPermanets = new List<Permanent>();

                            foreach (CardSource cardSource in selectedCards)
                            {
                                libraryPermanets.Add(cardSource.PermanentOfThisCard());
                            }

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

        return cardEffects;
    }
}