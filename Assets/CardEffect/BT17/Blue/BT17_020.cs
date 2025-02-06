using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DCGO.CardEffects.BT17
{
    public class BT17_020 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 3 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Play] Reveal the top 3 cards of your deck. Add 1 card with the [Hybrid]/[Ten Warriors] trait and 1 Tamer card with an inherited effect among them to the hand. Return the rest to the bottom of the deck.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.HasHybridTenWarriorsTraits;
                }

                bool CanSelectTamerCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsTamer)
                    {
                        if (cardSource.HasInheritedEffect)
                        {
                            return true;
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
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                            revealCount: 3,
                            simplifiedSelectCardConditions:
                            new SimplifiedSelectCardConditionClass[]
                            {
                                new(
                                    canTargetCondition: CanSelectCardCondition,
                                    message: "Select 1 card with the [Hybrid]/[Ten Warriors] trait.",
                                    mode: SelectCardEffect.Mode.AddHand,
                                    maxCount: 1,
                                    selectCardCoroutine: null),
                                new(
                                    canTargetCondition: CanSelectTamerCardCondition,
                                    message: "Select 1 Tamer card with an inherited effect.",
                                    mode: SelectCardEffect.Mode.AddHand,
                                    maxCount: 1,
                                    selectCardCoroutine: null),
                            },
                            remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                            activateClass: activateClass
                        ));
                }
            }

            #endregion

            #region When Attacking - ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play Tamer card from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true,
                    EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Attacking] [Once Per Turn] You may play 1 Tamer card with an inherited effect from your hand with the play cost reduced by 2.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanSelectHandCardCondition(CardSource cardSource)
                {
                    if (card.Owner.HandCards.Contains(cardSource))
                    {
                        if (cardSource.IsTamer)
                        {
                            if (cardSource.HasInheritedEffect)
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
                        if (card.Owner.HandCards.Count(CanSelectHandCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    #region Reduce Play Cost

                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect($"Play Cost -2", _ => true, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CanSelectHandCardCondition,
                        rootCondition: RootCondition, isUpDown: () => true, isCheckAvailability: () => false,
                        isChangePayingCost: () => true);
                    Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                    card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                    ICardEffect GetCardEffect(EffectTiming rcTiming)
                    {
                        if (rcTiming == EffectTiming.None)
                        {
                            return changeCostClass;
                        }

                        return null;
                    }

                    int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                    {
                        if (CanSelectHandCardCondition(cardSource))
                        {
                            if (RootCondition(root))
                            {
                                if (PermanentsCondition(targetPermanents))
                                {
                                    cost -= 2;
                                }
                            }
                        }

                        return cost;
                    }

                    bool PermanentsCondition(List<Permanent> targetPermanents)
                    {
                        if (targetPermanents == null)
                        {
                            return true;
                        }

                        return targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0;
                    }

                    bool RootCondition(SelectCardEffect.Root root)
                    {
                        return true;
                    }

                    #endregion

                    if (card.Owner.HandCards.Count(CanSelectHandCardCondition) >= 1)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectHandCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 Tamer card to play.",
                            "The opponent is selecting 1 Tamer card to play.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass,
                                payCost: true, isTapped: false, root: SelectCardEffect.Root.Hand, activateETB: true));
                    }

                    #region Remove Reduce Cost effect

                    card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);

                    #endregion
                }
            }

            #endregion

            return cardEffects;
        }
    }
}