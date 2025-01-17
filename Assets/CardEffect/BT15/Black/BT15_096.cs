using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT15
{
    public class BT15_096 : CEntity_Effect
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
                    return "[Main] Reveal the top 5 cards of your deck. Among them, add 1 card with the [Machine] or [Cyborg] trait to the hand and trash 1 such card. Return the rest to the top of the deck. Then, place this card in the battle area.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("Machine"))
                    {
                        return true;
                    }

                    if (cardSource.CardTraits.Contains("Cyborg"))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                                revealCount: 5,
                                simplifiedSelectCardConditions:
                                new SimplifiedSelectCardConditionClass[]
                                {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 card with [Machine] or [Cyborg] in its traits.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 card with [Machine] or [Cyborg] in its traits.",
                            mode: SelectCardEffect.Mode.Discard,
                            maxCount: 1,
                            selectCardCoroutine: null),
                                },
                                remainingCardsPlace: RemainingCardsPlace.DeckTop,
                                activateClass: activateClass
                            ));

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(
                        card: card,
                        cardEffect: activateClass));
                }
            }

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Digimon from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main][Delay] You may play 1 level 5 or higher Digimon card with the [Machine] or [Cyborg] trait from your hand with the play cost reduced by 3.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.Level >= 5)
                    {
                        if (cardSource.HasLevel)
                        {
                            if (cardSource.IsDigimon)
                            {
                                if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: true, cardEffect: activateClass))
                                {
                                    if (cardSource.CardTraits.Contains("Machine"))
                                    {
                                        return true;
                                    }

                                    if (cardSource.CardTraits.Contains("Cyborg"))
                                    {
                                        return true;
                                    }
                                }
                            }
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

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        deleted = true;

                        yield return null;
                    }

                    if (deleted)
                    {
                        #region reduce play cost

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect("Play Cost -3", CanUseCondition1, card);
                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                        Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                        card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                        ICardEffect GetCardEffect(EffectTiming _timing)
                        {
                            if (_timing == EffectTiming.None)
                            {
                                return changeCostClass;
                            }

                            return null;
                        }

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
                                        Cost -= 3;
                                    }
                                }
                            }

                            return Cost;
                        }

                        bool PermanentsCondition(List<Permanent> targetPermanents)
                        {
                            if (targetPermanents == null)
                            {
                                return true;
                            }
                            else
                            {
                                if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        bool CardSourceCondition(CardSource cardSource)
                        {
                            if (cardSource != null)
                            {
                                if (cardSource.Owner == card.Owner)
                                {
                                    if (cardSource.Level >= 5)
                                    {
                                        if (cardSource.HasLevel)
                                        {
                                            if (cardSource.IsDigimon)
                                            {
                                                if (cardSource.CardTraits.Contains("Machine"))
                                                {
                                                    return true;
                                                }

                                                if (cardSource.CardTraits.Contains("Cyborg"))
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

                        bool RootCondition(SelectCardEffect.Root root)
                        {
                            return true;
                        }

                        bool isUpDown()
                        {
                            return true;
                        }

                        #endregion

                        if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                        {
                            Permanent selectedPermanent = null;

                            List<CardSource> selectedCards = new List<CardSource>();

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

                            selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                            yield return StartCoroutine(selectHandEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: true,
                            isTapped: false,
                            root: SelectCardEffect.Root.Hand,
                            activateETB: true));
                        }

                        #region release reducing play cost

                        card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);

                        #endregion
                    }
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Reveal the top 5 cards of deck and place this card on battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] Reveal the top 5 cards of your deck. Among them, add 1 card with the [Machine] or [Cyborg] trait to the hand and trash 1 such card. Return the rest to the top of the deck. Then, place this card in the battle area.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("Machine"))
                    {
                        return true;
                    }

                    if (cardSource.CardTraits.Contains("Cyborg"))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                                revealCount: 5,
                                simplifiedSelectCardConditions:
                                new SimplifiedSelectCardConditionClass[]
                                {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 card with [Machine] or [Cyborg] in its traits.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 card with [Machine] or [Cyborg] in its traits.",
                            mode: SelectCardEffect.Mode.Discard,
                            maxCount: 1,
                            selectCardCoroutine: null),
                                },
                                remainingCardsPlace: RemainingCardsPlace.DeckTop,
                                activateClass: activateClass
                            ));

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(
                        card: card,
                        cardEffect: activateClass));
                }
            }

            return cardEffects;
        }
    }
}