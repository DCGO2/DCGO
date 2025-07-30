using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCGO.CardEffects.BT15
{
    public class BT15_082 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }

            if (timing == EffectTiming.OnReturnCardsToHandFromTrash)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return this Tamer to your hand to play a Digimon from your hand.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When a red Digimon card returns from your trash to the hand, by returning this Tamer to the hand, you may play 1 13000 DP or less red Digimon card with [Avian], [Bird], [Beast], [Animal] or [Sovereign], other than [Sea Animal] in one of its traits from your hand without paying the cost. For each of your opponent's security cards, remove 2000 from this effect's playable card's DP maximum.";
                }

                bool CardReturnedCondition(CardSource cardSource)
                {
                    if (cardSource.Owner == card.Owner)
                    {
                        return cardSource.CardColors.Contains(CardColor.Red);
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenCardsReturnToHandFromTrash(hashtable, CardReturnedCondition, card))
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
                        return true;
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    int subAmount = card.Owner.Enemy.SecurityCards.Count * 2000;
                    int cardDP = 13000 - subAmount;

                    if (cardSource.CardDP <= cardDP && cardSource.CardColors.Contains(CardColor.Red) && cardSource.Owner == card.Owner)
                    {
                        if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false,
                                cardEffect: activateClass))
                        {
                            if (cardSource.HasAvianBeastAnimalTraits)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent bounceTargetPermanent = card.PermanentOfThisCard();

                    if (bounceTargetPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.BouncePeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent>() { bounceTargetPermanent },
                            activateClass: activateClass,
                            successProcess: SuccessProcess(),
                            failureProcess: null));

                        IEnumerator SuccessProcess()
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCardCondition,
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

                            selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                            yield return StartCoroutine(selectHandEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Hand, activateETB: true));
                        }
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
}