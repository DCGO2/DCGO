using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT13
{
    public class BT13_055 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Your 1 [Angoramon] digivolves into this card", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Hand][Main] If you have [Ruli Tsukiyono], by placing 1 [SymbareAngoramon] from your hand as 1 of your [Angoramon]'s bottom digivolution card, that Digimon digivolves into this card for a digivolution cost of 3, ignoring digivolution requirements.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.CardNames.Contains("SymbareAngoramon"))
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
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.CardNames.Contains("Angoramon"))
                        {
                            if (!permanent.IsToken)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Contains(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.TopCard.CardNames.Contains("Ruli Tsukiyono") || permanent.TopCard.CardNames.Contains("RuliTsukiyono")))
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.HandCards.Contains(card))
                    {
                        bool added = false;

                        Permanent selectedPermanent = null;

                        if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
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

                                selectHandEffect.SetUpCustomMessage("Select 1 card to place at the bottom of digivolution cards.", "The opponent is selecting 1 card to place at the bottom of digivolution cards.");

                                yield return StartCoroutine(selectHandEffect.Activate());

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCard = cardSource;

                                    yield return null;
                                }

                                if (selectedCard != null)
                                {
                                    maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectPermanentCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: true,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: SelectPermanentCoroutine,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 [Angoramon] that will get a digivolution card.", "The opponent is selecting 1 [Angoramon] that will get a digivolution card.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        selectedPermanent = permanent;

                                        if (selectedPermanent != null)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass));

                                            added = true;
                                        }
                                    }
                                }
                            }
                        }

                        if (added)
                        {
                            if (selectedPermanent != null)
                            {
                                if (card.Owner.HandCards.Contains(card))
                                {
                                    #region ignore digivolution requirement

                                    AddDigivolutionRequirementClass addEvolutionConditionClass = new AddDigivolutionRequirementClass();
                                    addEvolutionConditionClass.SetUpICardEffect("Ignore Digivolution requirements", CanUseCondition1, card);
                                    addEvolutionConditionClass.SetUpAddDigivolutionRequirementClass(getEvoCost: GetEvoCost);
                                    Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                                    card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                                    ICardEffect GetCardEffect(EffectTiming _timing)
                                    {
                                        if (_timing == EffectTiming.None)
                                        {
                                            return addEvolutionConditionClass;
                                        }

                                        return null;
                                    }

                                    bool CanUseCondition1(Hashtable hashtable)
                                    {
                                        return true;
                                    }

                                    int GetEvoCost(Permanent permanent, CardSource cardSource, bool checkAvailability)
                                    {
                                        if (card.Owner.CanIgnoreDigivolutionRequirement(permanent, cardSource))
                                        {
                                            if (CardSourceCondition(cardSource) && PermanentCondition(permanent))
                                            {
                                                return 3;
                                            }
                                        }

                                        return -1;
                                    }

                                    bool PermanentCondition(Permanent targetPermanent)
                                    {
                                        return targetPermanent == selectedPermanent;
                                    }

                                    bool CardSourceCondition(CardSource cardSource)
                                    {
                                        return cardSource == card;
                                    }

                                    #endregion

                                    if (card.CanPlayCardTargetFrame(selectedPermanent.PermanentFrame, true, activateClass))
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(new PlayCardClass(
                                            cardSources: new List<CardSource>() { card },
                                            hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                            payCost: true,
                                            targetPermanent: selectedPermanent,
                                            isTapped: false,
                                            root: SelectCardEffect.Root.Hand,
                                            activateETB: true).PlayCard());
                                    }

                                    #region release effect

                                    card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);

                                    #endregion
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnEndBattle)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash the top card of opponent's security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("TrashSecurity_BT13_055");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn][Once Per Turn] When this Digimon deletes an opponent's Digimon in battle, trash the top card of your opponent's security stack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            bool WinnerCondition(Permanent permanent) => permanent.cardSources.Contains(card);
                            bool LoserCondition(Permanent permanent) => CardEffectCommons.IsOpponentPermanent(permanent, card);

                            if (CardEffectCommons.CanTriggerWhenDeleteOpponentDigimonByBattle(hashtable: hashtable, winnerCondition: WinnerCondition, loserCondition: LoserCondition, isOnlyWinnerSurvive: false))
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
                        if (card.Owner.Enemy.SecurityCards.Count >= 1)
                        {
                            if (CardEffectCommons.IsOwnerTurn(card))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                }
            }

            return cardEffects;
        }
    }
}