using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EX5_065 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnAddDigivolutionCards)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Your Turn] When an effect places the top card of one of your Digimon in a Digimon's digivolution cards, by suspending this Tamer, gain 1 memory.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (CardEffectCommons.CanTriggerOnAddDigivolutionCard(
                            hashtable: hashtable,
                            permanentCondition: permanent => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card),
                            cardEffectCondition: cardEffect => cardEffect.EffectSourceCard != null,
                            cardCondition: null))
                        {
                            if (CardEffectCommons.IsFromDigimon(hashtable))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.CanActivateSuspendCostEffect(card))
                {
                    return true;
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(
                    new List<Permanent>() { card.PermanentOfThisCard() },
                    CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
            }
        }

        if (timing == EffectTiming.OnStartTurn)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Play Digimon from digivolution cards to DNA digivolve", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Start of Opponent's Turn] By playing 1 card with the same level as one of your [Night Claw]/[Light Fang] trait Digimon from that Digimon's digivolution cards withotu paying the cost, 2 of your Digimon may DNA Digivolve into a Digimon card in your hand. At the end of the turn, return the Digimon played by this effect to hand.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    if (permanent.DigivolutionCards.Count(cardSource => CanSelectCardCondition(cardSource, permanent)) >= 1)
                    {
                        if (permanent.TopCard.CardTraits.Contains("Light Fang"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardTraits.Contains("LightFung"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardTraits.Contains("Night Claw"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardTraits.Contains("NightClaw"))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanSelectCardCondition(CardSource cardSource, Permanent permanent)
            {
                if (cardSource.IsDigimon)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                    {
                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                        {
                            if (cardSource.HasLevel)
                            {
                                if (permanent.TopCard.HasLevel)
                                {
                                    if (cardSource.Level == permanent.Level)
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

            bool CanSelectCardCondition1(CardSource cardSource)
            {
                if (cardSource.IsDigimon)
                {
                    if (cardSource.CanPlayJogress(true))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOpponentTurn(card))
                    {
                        return true;
                    }
                }

                return false;
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
                bool played = false;

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    Permanent selectedPermanent = null;

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
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that has digivolution cards.", "The opponent is selecting 1 Digimon that has digivolution cards.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        if (selectedPermanent.DigivolutionCards.Count((cardSource) => CanSelectCardCondition(cardSource, selectedPermanent)) >= 1)
                        {
                            maxCount = Math.Min(1, selectedPermanent.DigivolutionCards.Count((cardSource) => CanSelectCardCondition(cardSource, selectedPermanent)));

                            List<CardSource> selectedCards = new List<CardSource>();

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                        canTargetCondition: cardSource => CanSelectCardCondition(cardSource, selectedPermanent),
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

                            selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to play.",
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
                            activateETB: true));

                            foreach (CardSource selectedCard in selectedCards)
                            {
                                if (CardEffectCommons.IsExistOnBattleArea(selectedCard))
                                {
                                    played = true;

                                    Permanent selectedPermanent1 = selectedCard.PermanentOfThisCard();

                                    ActivateClass activateClass1 = new ActivateClass();
                                    activateClass1.SetUpICardEffect("Return the Digimon to hand", CanUseCondition1, card);
                                    activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, "");
                                    CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilEachTurnEnd, card: card, cardEffect: activateClass1, timing: EffectTiming.OnEndTurn);

                                    bool CanUseCondition1(Hashtable hashtable)
                                    {
                                        return true;
                                    }

                                    bool CanActivateCondition1(Hashtable hashtable)
                                    {
                                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent1))
                                        {
                                            if (!selectedPermanent1.CannotReturnToHand(activateClass))
                                            {
                                                if (!selectedPermanent1.TopCard.CanNotBeAffected(activateClass1))
                                                {
                                                    return true;
                                                }
                                            }
                                        }

                                        return false;
                                    }

                                    IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(new HandBounceClaass(new List<Permanent>() { selectedPermanent1 }, CardEffectCommons.CardEffectHashtable(activateClass1)).Bounce());
                                    }
                                }
                            }
                        }
                    }
                }

                if (played)
                {
                    // DNA digivolve
                    yield return ContinuousController.instance.StartCoroutine(
                                         CardEffectCommons.DNADigivolvePermanentsIntoHandOrTrashCard(
                                             CanSelectCardCondition1,
                                             payCost: true,
                                             isHand: true,
                                             activateClass
                                         ));
                }
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
        }

        return cardEffects;
    }
}
