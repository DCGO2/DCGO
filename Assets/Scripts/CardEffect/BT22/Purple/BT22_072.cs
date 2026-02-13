using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//BT22 Lekismon
namespace DCGO.CardEffects.BT22
{
    public class BT22_072 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt digivolution
            if(timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.Level == 3 && (permanent.TopCard.HasLightFangOrNightClawTraits || permanent.TopCard.HasCSTraits);
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 2, false, card, null));
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription() => 
                    "[When Digivolving] If this Digimon's stack has 2 or more same-level cards, you may play 1 Tamer card with the [Night Claw] or [Light Fang] trait from your hand without paying the cost. This effect can't play cards with the same name as any of your Tamers.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool HasSameLevelDigivolutionCards(Permanent permanent)
                {
                    return permanent.StackCards
                        .Filter(x => !x.IsFlipped)
                        .GroupBy(x => x.Level)
                        .Any(g => g.Count() >= 2);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) && HasSameLevelDigivolutionCards(card.PermanentOfThisCard());
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource != null &&
                        cardSource.HasLightFangOrNightClawTraits &&
                        cardSource.IsTamer &&
                        cardSource.Owner == card.Owner &&
                        CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass) &&
                        !card.Owner.GetBattleAreaPermanents().Any((permanent) => permanent.IsTamer && permanent.TopCard.HasSameCardName(cardSource));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                    {
                        CardSource selectedCard = null;
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, CanSelectCardCondition));

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
                            selectedCard = cardSource;
                            yield return null;
                        }

                        if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: new List<CardSource>() { selectedCard }, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Hand, activateETB: true));
                    }
                }
            }
            #endregion

            #region Inherit
            //This section may contain outdated ways of doing things as it was largely copy/pasted from Gracenova and is at the moment more complicated than is worth fiddling with 

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Prevent this Digimon from being deleted", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Inherit-BT22-072");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon with the [Night Claw], [Light Fang] or [Galaxy] trait would be deleted, by trashing 2 same-level cards from its digivolution cards, it isn't deleted.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (!cardSource.CanNotTrashFromDigivolutionCards(activateClass))
                    {
                        if (CardEffectCommons.IsExistOnBattleArea(card))
                        {
                            if (card.PermanentOfThisCard().DigivolutionCards.Contains(cardSource))
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
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                        CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card) &&
                        (card.PermanentOfThisCard().TopCard.EqualsTraits("Galaxy") || card.PermanentOfThisCard().TopCard.HasLightFangOrNightClawTraits);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
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
            #endregion

            return cardEffects;
        }
    }
}