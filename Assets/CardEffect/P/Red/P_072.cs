using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;


public class P_072 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            bool PermanentCondition(Permanent targetPermanent)
            {
                return targetPermanent.TopCard.CardNames.Contains("MetalGreymon");
            }

            cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                permanentCondition: PermanentCondition,
                digivolutionCost: 0,
                ignoreDigivolutionRequirement: false,
                card: card,
                condition: null));
        }

        if (timing == EffectTiming.None)
        {
            ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
            changeCardNamesClass.SetUpICardEffect("Also treated as [MetalGreymon]", CanUseCondition, card);
            changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: changeCardNames);
            cardEffects.Add(changeCardNamesClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                return true;
            }

            List<string> changeCardNames(CardSource cardSource, List<string> CardNames)
            {
                if (cardSource == card)
                {
                    CardNames.Add("MetalGreymon");
                }

                return CardNames;
            }
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Delete 1 Digimon with 5000 DP or less", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] If you have a Tamer in play, delete 1 of your opponent's Digimon with 5000 DP or less.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(5000, activateClass))
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
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsTamer))
                        {
                            return true;
                        }
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
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
        }

        if (timing == EffectTiming.WhenPermanentWouldBeDeleted || timing == EffectTiming.WhenReturntoHandAnyone || timing == EffectTiming.WhenReturntoLibraryAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Prevent this Digimon from being deleted or returned to hand or deck", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            activateClass.SetIsInheritedEffect(true);
            activateClass.SetHashString("Substitute_P_072");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[All Turns] When this Digimon has [Greymon] or [Omnimon] in its name and an effect would delete it or return it to your hand or deck, you may trash 2 cards of the same level in this Digimonfigivolution cards to prevent it from leaving play.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (!cardSource.CanNotTrashFromDigivolutionCards(activateClass))
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Contains(card))
                        {
                            foreach (CardSource cardSource1 in card.PermanentOfThisCard().DigivolutionCards)
                            {
                                if (cardSource != cardSource1)
                                {
                                    if (cardSource.Level == cardSource1.Level)
                                    {
                                        if (!cardSource1.CanNotTrashFromDigivolutionCards(activateClass))
                                        {
                                            if (cardSource.HasLevel && cardSource1.HasLevel)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card))
                    {
                        if (CardEffectCommons.IsByEffect(hashtable, null))
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
                    if (card.PermanentOfThisCard().TopCard.HasGreymonName || card.PermanentOfThisCard().TopCard.ContainsCardName("Omnimon"))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 2)
                        {
                            List<CardSource> canSelectCards = new List<CardSource>();

                            foreach (CardSource cardSource in card.PermanentOfThisCard().DigivolutionCards)
                            {
                                canSelectCards.Add(cardSource);
                            }

                            if (canSelectCards.Count >= 2)
                            {
                                List<CardSource[]> cardsList = ParameterComparer.Enumerate(canSelectCards, 2).ToList();

                                foreach (CardSource[] cardSources in cardsList)
                                {
                                    if (cardSources.Length == 2)
                                    {
                                        if (cardSources[0].Level == cardSources[1].Level)
                                        {
                                            if (cardSources[0].HasLevel && cardSources[1].HasLevel)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition) >= 2)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = 2;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                    canEndSelectCondition: CanEndSelectCondition,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select cards to discard.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: selectedPermanent.DigivolutionCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetNotShowCard();
                        yield return StartCoroutine(selectCardEffect.Activate());

                        bool CanEndSelectCondition(List<CardSource> cardSources)
                        {
                            if (CardEffectCommons.HasNoElement(cardSources))
                            {
                                return false;
                            }

                            List<int> levels = cardSources
                            .Map(cardSource1 => cardSource1.Level)
                            .Distinct()
                            .ToList();

                            if (levels.Count > 1)
                            {
                                return false;
                            }

                            return true;
                        }

                        bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                        {
                            List<int> levels = cardSources
                            .Map(cardSource1 => cardSource1.Level)
                            .Concat(new List<int>() { cardSource.Level })
                            .Distinct()
                            .ToList();

                            if (levels.Count > 1)
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
                            if (selectedCards.Count == 2)
                            {
                                selectedPermanent.willBeRemoveField = false;
                                selectedPermanent.HideDeleteEffect();
                                selectedPermanent.HideHandBounceEffect();
                                selectedPermanent.HideDeckBounceEffect();
                            }

                            yield return ContinuousController.instance.StartCoroutine(new ITrashDigivolutionCards(
                                selectedPermanent,
                                selectedCards,
                                activateClass).TrashDigivolutionCards());
                        }
                    }
                }
            }
        }

        return cardEffects;
    }
}
