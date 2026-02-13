using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace DCGO.CardEffects.BT10
{
    public class BT10_084 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play Digimons from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Play 1 purple or yellow Digimon card with 6000 DP or less from your trash without paying its memory cost. If you have 1 or fewer security cards, you may play 1 level 6 or lower Digimon card with [Angel] or [Fallen Angel] in its traits from your trash without paying its memory cost instead.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                    {
                        if (cardSource.IsDigimon)
                        {
                            if (cardSource.Level <= 4 && cardSource.HasLevel)
                            {
                                if (cardSource.CardTraits.Contains("BagraArmy"))
                                {
                                    return true;
                                }

                                if (cardSource.CardTraits.Contains("Bagra Army"))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = Math.Min(2, card.Owner.TrashCards.Count(CanSelectCardCondition));

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select cards to play.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select cards to play.", "The opponent is selecting cards to play.");
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
                                    root: SelectCardEffect.Root.Trash,
                                    activateETB: true));

                        foreach (CardSource selectedCard in selectedCards)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(
                                                                    targetPermanent: selectedCard.PermanentOfThisCard(),
                                                                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                                                    activateClass: activateClass));
                        }
                    }
                }
            }

            #endregion

            #region Opponents Turn

            if (timing == EffectTiming.WhenWouldDigivolutionCardDiscarded)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash digivolution cards instead", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Opponent's Turn] When an effect would trash one of your other Digimon's digivolution cards, you may trash this Digimon's digivolution cards instead.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent != card.PermanentOfThisCard())
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanTriggerOnTrashDigivolutionCard(hashtable, PermanentCondition, cardEffect => true, cardSource => true))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (!CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<CardSource> discardedCards = CardEffectCommons.GetDiscardedCardsFromHashtable(_hashtable);

                    if (discardedCards.Count > 0)
                    {
                        if (card.PermanentOfThisCard().cardSources.Count >= discardedCards.Count)
                        {
                            discardedCards.ForEach(source => source.willBeRemoveSources = false);

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: (CardSource) => true,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => false,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: null,
                                message: "Select card(s) to trash.",
                                maxCount: discardedCards.Count,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Discard,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}