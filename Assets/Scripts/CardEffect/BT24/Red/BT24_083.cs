using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

// Hiroko Sagisaka
namespace DCGO.CardEffects.BT24
{
    public class BT24_083 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Turn
            if (timing == EffectTiming.OnStartTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return to deck to play another or a 5k- [TS]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription() => "[Start of Your Turn] If you have 4 or less memory, by returning this Tamer to the bottom of the deck, you may play 1 [Hiroko Sagisaka] or 1 [TS] trait Digimon card with 5000 DP or less from your hand without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && card.Owner.MemoryForPlayer <= 4;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return (cardSource.EqualsCardName("Hiroko Sagisaka")
                        || (cardSource.IsDigimon
                            && cardSource.HasDP
                            && cardSource.CardDP <= 5000
                            && cardSource.HasTSTraits))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.DeckBouncePeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                            activateClass: activateClass,
                            successProcess: SuccessProcess(),
                            failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        int maxCount = Math.Min(1, card.Owner.HandCards.Count(CanSelectCardCondition));

                        List<CardSource> selectedCards = new List<CardSource>();

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
                            payCost: false, 
                            isTapped: false, 
                            root: SelectCardEffect.Root.Hand, 
                            activateETB: true));
                    }
                }
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal 3, Add 1, Bottom Deck the rest", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription() => "[On Play] Reveal the top 3 cards of your deck. Add 1 card with the [TS] trait among them to the hand. Return the rest to the bottom of the deck.";

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.HasTSTraits;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition:CanSelectCardCondition,
                                message: "Select 1 card with the [TS] trait.",
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

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}