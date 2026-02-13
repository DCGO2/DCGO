using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT13
{
    public class BT13_110 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] <Draw 1>. You may place 1 Digimon card from your hand as the bottom digivolution card of 1 of your [King Drasil_7D6] in the breeding area. Then, place this card in the battle area.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBreedingArea(permanent, card))
                    {
                        if (!permanent.IsToken)
                        {
                            if (permanent.TopCard.CardNames.Contains("King Drasil_7D6"))
                            {
                                return true;
                            }

                            if (permanent.TopCard.CardNames.Contains("KingDrasil_7D6"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                    if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                    {
                        if (card.Owner.GetBreedingAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                        {
                            CardSource selectedCard = null;

                            int maxCount = 1;

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

                            selectHandEffect.SetUpCustomMessage(
                                "Select 1 card to place at the bottom of digivolution cards.",
                                "The opponent is selecting 1 card to place at the bottom of digivolution cards.");

                            yield return StartCoroutine(selectHandEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCard = cardSource;

                                yield return null;
                            }

                            if (selectedCard != null)
                            {
                                if (card.Owner.GetBreedingAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                                {
                                    Permanent selectedPermanent = card.Owner.GetBreedingAreaPermanents()[0];

                                    if (CanSelectPermanentCondition(selectedPermanent))
                                    {
                                        if (selectedPermanent != null)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(
                                                new List<CardSource>() { selectedCard },
                                                activateClass));
                                        }
                                    }
                                }
                            }
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Digimon from breeding area", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] <Delay> - Play 1 [Royal Knight] trait card from the digivolution cards of your Digimon in the breeding area without paying the cost. [On Play] effects on Digimon played by this effect don't activate, and they gain <Rush> for the turn.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBreedingArea(permanent, card))
                    {
                        if (permanent.IsDigimon)
                        {
                            if (permanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.HasRoyalKnightTraits)
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
                    return CardEffectCommons.CanDeclareOptionDelayEffect(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool deleted = false;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass: activateClass,
                        successProcess: permanents => SuccessProcess(),
                        failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        deleted = true;

                        yield return null;
                    }

                    if (deleted)
                    {
                        if (card.Owner.GetBreedingAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                        {
                            Permanent selectedPermanent = card.Owner.GetBreedingAreaPermanents()[0];

                            if (CanSelectPermanentCondition(selectedPermanent))
                            {
                                if (selectedPermanent != null)
                                {
                                    if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                                    {
                                        int maxCount = Math.Min(1, selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition));

                                        List<CardSource> selectedCards = new List<CardSource>();

                                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                        selectCardEffect.SetUp(
                                                    canTargetCondition: CanSelectCardCondition,
                                                    canTargetCondition_ByPreSelecetedList: null,
                                                    canEndSelectCondition: null,
                                                    canNoSelect: () => false,
                                                    selectCardCoroutine: SelectCardCoroutine,
                                                    afterSelectCardCoroutine: null,
                                                    message: "Select 1 digivolution card to play.",
                                                    maxCount: maxCount,
                                                    canEndNotMax: false,
                                                    isShowOpponent: true,
                                                    mode: SelectCardEffect.Mode.Custom,
                                                    root: SelectCardEffect.Root.Custom,
                                                    customRootCardList: selectedPermanent.DigivolutionCards,
                                                    canLookReverseCard: true,
                                                    selectPlayer: card.Owner,
                                                    cardEffect: activateClass);

                                        selectCardEffect.SetUpCustomMessage(
                                            "Select 1 digivolution card to play.",
                                            "The opponent is selecting 1 digivolution card to play.");
                                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                                        yield return StartCoroutine(selectCardEffect.Activate());

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
                                            root: SelectCardEffect.Root.DigivolutionCards,
                                            activateETB: false));

                                        foreach (CardSource selectedCard in selectedCards)
                                        {
                                            if (CardEffectCommons.IsExistOnBattleArea(selectedCard))
                                            {
                                                selectedPermanent = selectedCard.PermanentOfThisCard();

                                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRush(
                                                    targetPermanent: selectedPermanent,
                                                    effectDuration: EffectDuration.UntilEachTurnEnd,
                                                    activateClass: activateClass));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaceSelfDelayOptionSecurityEffect(card));
            }

            return cardEffects;
        }
    }
}