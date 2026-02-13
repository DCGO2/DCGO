using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.P
{
    public class P_158 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 4 cards of your deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Reveal the top 4 cards of your deck. Add 1 card with the [D-Reaper] trait among them to the hand. Return the rest to the bottom of the deck.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("D-Reaper");
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

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 4,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 card with the [D-Reaper] trait.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null)
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                        activateClass: activateClass
                    ));
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Digimon card with the [D-Reaper] trait", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] By returning this Tamer to the bottom of the deck, you may play 1 Digimon card with the [D-Reaper] trait and play cost of 3 or less from your hand without paying the cost. For each digivolution card of 1 of your [Mother D-Reaper], add 1 to the maximum play cost you may play with this effect.";
                }

                int getMaxCost()
                {
                    int cost = 3;
                    int maxCost = 0;

                    foreach(Permanent permanent in card.Owner.GetBattleAreaPermanents().Filter(IsMotherDReaper))
                    {
                        if(permanent.DigivolutionCards.Count > maxCost)
                            maxCost = permanent.DigivolutionCards.Count;
                    }

                    return cost + maxCost;
                }

                bool IsMotherDReaper(Permanent permanent)
                {
                    return permanent.TopCard.EqualsCardName("Mother D-Reaper");
                }

                bool HasDReaper(CardSource cardSource)
                {
                    if (cardSource.EqualsTraits("D-Reaper"))
                    {
                        if (cardSource.IsDigimon)
                        {
                            if (cardSource.GetCostItself <= getMaxCost())
                                return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeckBouncePeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass: activateClass,
                        successProcess: SuccessProcess(),
                        failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, HasDReaper))
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: HasDReaper,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: SelectCardCoroutine,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectHandEffect.SetUpCustomMessage(
                                "Select 1 Digimon to play.",
                                "The opponent is selecting 1 Digimon to play.");

                            yield return StartCoroutine(selectHandEffect.Activate());

                            IEnumerator SelectCardCoroutine(List<CardSource> cardSources)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: cardSources,
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Hand,
                                    activateETB: true));
                            }
                        }
                    }
                }
            }
            #endregion

            #region Security Effect
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}