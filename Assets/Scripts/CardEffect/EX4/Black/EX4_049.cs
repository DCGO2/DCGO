using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.EX4
{
    public class EX4_049 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("WereGarurumon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Choose 1 effect", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Activate 1 of the effects below. -Choose any number of your opponent's Digimon so that their play cost total is up to 6 and return them to the bottom of the deck. -1 of your other Digimon digivolves into a level 6 or lower Digimon card with [Greymon] in its name in your hand without paying the cost. -This Digimon and one of your other Digimon may DNA digivolve into a Digimon card in your hand for the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                            {
                                new SelectionElement<int>(message: $"Deck Bounce", value : 0, spriteIndex: 0),
                                new SelectionElement<int>(message: $"Your other 1 Digimon digivolves", value : 1, spriteIndex: 0),
                                new SelectionElement<int>(message: $"DNA Digivolution", value : 2, spriteIndex: 0),
                            };

                            string selectPlayerMessage = "Which effect will you activate?";
                            string notSelectPlayerMessage = "The opponent is choosing which effect to activate.";

                            GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            int actionID = GManager.instance.userSelectionManager.SelectedIntValue;

                            switch (actionID)
                            {
                                case 0:
                                    bool CanSelectPermanentCondition(Permanent permanent)
                                    {
                                        if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                                        {
                                            if (permanent.TopCard.GetCostItself <= 6)
                                            {
                                                if (permanent.TopCard.HasPlayCost)
                                                {
                                                    return true;
                                                }
                                            }
                                        }

                                        return false;
                                    }

                                    if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                                    {
                                        List<Permanent> selectedPermanents = new List<Permanent>();

                                        int maxCount = card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition);

                                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                        selectPermanentEffect.SetUp(
                                            selectPlayer: card.Owner,
                                            canTargetCondition: CanSelectPermanentCondition,
                                            canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                            canEndSelectCondition: CanEndSelectCondition,
                                            maxCount: maxCount,
                                            canNoSelect: false,
                                            canEndNotMax: true,
                                            selectPermanentCoroutine: SelectPermanentCoroutine,
                                            afterSelectPermanentCoroutine: null,
                                            mode: SelectPermanentEffect.Mode.Custom,
                                            cardEffect: activateClass);

                                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                        {
                                            selectedPermanents.Add(permanent);
                                            yield return null;
                                        }

                                        bool CanEndSelectCondition(List<Permanent> permanents)
                                        {
                                            if (permanents.Count <= 0)
                                            {
                                                return false;
                                            }

                                            int sumCost = 0;

                                            foreach (Permanent permanent1 in permanents)
                                            {
                                                sumCost += permanent1.TopCard.GetCostItself;
                                            }

                                            if (sumCost > 6)
                                            {
                                                return false;
                                            }

                                            return true;
                                        }

                                        bool CanTargetCondition_ByPreSelecetedList(List<Permanent> permanents, Permanent permanent)
                                        {
                                            int sumCost = 0;

                                            foreach (Permanent permanent1 in permanents)
                                            {
                                                sumCost += permanent1.TopCard.GetCostItself;
                                            }

                                            sumCost += permanent.TopCard.GetCostItself;

                                            if (sumCost > 6)
                                            {
                                                return false;
                                            }

                                            return true;
                                        }

                                        if (selectedPermanents.Count >= 1)
                                        {
                                            Hashtable hashtable = new Hashtable();
                                            hashtable.Add("CardEffect", activateClass);

                                            if (selectedPermanents.Count == 1)
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(selectedPermanents, hashtable).DeckBounce());
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
                                                        DeckBottomBounceClass putLibraryBottomPermanent = new DeckBottomBounceClass(libraryPermanets, hashtable);

                                                        putLibraryBottomPermanent.SetNotShowCards();

                                                        yield return ContinuousController.instance.StartCoroutine(putLibraryBottomPermanent.DeckBounce());
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;

                                case 1:
                                    bool CanSelectPermanentCondition1(Permanent permanent)
                                    {
                                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                                        {
                                            if (permanent != card.PermanentOfThisCard())
                                            {
                                                foreach (CardSource cardSource in card.Owner.HandCards)
                                                {
                                                    if (CanSelectCardCondition(cardSource))
                                                    {
                                                        if (cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, false, activateClass))
                                                        {
                                                            return true;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        return false;
                                    }

                                    bool CanSelectCardCondition(CardSource cardSource)
                                    {
                                        return cardSource.IsDigimon && cardSource.HasGreymonName && cardSource.Level <= 6 && cardSource.HasLevel;
                                    }

                                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                                    {
                                        Permanent selectedPermanent = null;

                                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition1));

                                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                        selectPermanentEffect.SetUp(
                                            selectPlayer: card.Owner,
                                            canTargetCondition: CanSelectPermanentCondition1,
                                            canTargetCondition_ByPreSelecetedList: null,
                                            canEndSelectCondition: null,
                                            maxCount: maxCount,
                                            canNoSelect: true,
                                            canEndNotMax: false,
                                            selectPermanentCoroutine: SelectPermanentCoroutine,
                                            afterSelectPermanentCoroutine: null,
                                            mode: SelectPermanentEffect.Mode.Custom,
                                            cardEffect: activateClass);

                                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will digivolve.", "The opponent is selecting 1 Digimon that will digivolve.");

                                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                        {
                                            selectedPermanent = permanent;

                                            yield return null;
                                        }

                                        if (selectedPermanent != null)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                                targetPermanent: selectedPermanent,
                                                cardCondition: CanSelectCardCondition,
                                                payCost: false,
                                                reduceCostTuple: null,
                                                fixedCostTuple: null,
                                                ignoreDigivolutionRequirementFixedCost: -1,
                                                isHand: true,
                                                activateClass: activateClass,
                                                successProcess: null));
                                        }
                                    }
                                    break;

                                case 2:
                                    bool CanSelectCardCondition1(CardSource cardSource)
                                    {
                                        if (cardSource != null)
                                        {
                                            if (cardSource.IsDigimon)
                                            {
                                                if (cardSource.Owner == card.Owner)
                                                {
                                                    if (cardSource.CanPlayJogress(true))
                                                    {
                                                        if (isExistOnField(card))
                                                        {
                                                            if (cardSource.CanJogressFromTargetPermanent(card.PermanentOfThisCard(), true))
                                                            {
                                                                return true;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        return false;
                                    }

                                    if (card.Owner.HandCards.Count(CanSelectCardCondition1) >= 1)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(
                                                                 CardEffectCommons.DNADigivolvePermanentsIntoHandOrTrashCard(
                                                                 CanSelectCardCondition1,
                                                                 payCost: true,
                                                                 isHand: true,
                                                                 activateClass,
                                                                 permanentConditions: new Func<Permanent, bool>[] { (permanent) => permanent == card.PermanentOfThisCard() }));
                                    }
                                    break;
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 level 5 or lower Digimon to the bottom of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("DeckBounce_EX4_049");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking][Once Per Turn] If this Digimon has [Omnimon] in its name, return 1 of your opponent's level 5 or lower Digimon to the bottom of its owner's deck.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.Level <= 5)
                        {
                            if (permanent.TopCard.HasLevel)
                            {
                                return true;
                            }
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
                        if (card.PermanentOfThisCard().TopCard.ContainsCardName("Omnimon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
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
                            mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            return cardEffects;
        }
    }
}