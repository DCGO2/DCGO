using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT9_012 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            bool PermanentCondition(Permanent targetPermanent)
            {
                return targetPermanent.TopCard.CardNames.Contains("Greymon");
            }

            cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
        }

        if (timing == EffectTiming.WhenPermanentWouldBeDeleted || timing == EffectTiming.WhenReturntoHandAnyone || timing == EffectTiming.WhenReturntoLibraryAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Prevent this Digimon from being deleted or returned to hand or deck", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            activateClass.SetIsInheritedEffect(true);
            activateClass.SetHashString("Substitute_BT9_012");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[All Turns] When this Digimon has [Greymon] or [Omnimon] in its name and an effect would delete it or return it to your hand or deck, you may trash 2 cards of the same level in this Digimonfdigivolution cards to prevent it from leaving play.";
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
