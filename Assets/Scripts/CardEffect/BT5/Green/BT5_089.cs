using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT5_089 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnStartTurn)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Memory +2", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Start of Your Turn] If your opponent has a suspended Digimon in play, gain 2 memory.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
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
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, (permanent) => permanent.IsDigimon && permanent.IsSuspended))
                    {
                        if (card.Owner.CanAddMemory(activateClass))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(2, activateClass));
            }
        }

        if (timing == EffectTiming.OnAllyAttack)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Reveal the top 3 cards of deck", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Your Turn] When you attack with a level 5 green Digimon, you may suspend this Tamer to reveal 3 cards from the top of your deck. You may digivolve 1 green level 6 Digimon card among them onto the attacking Digimon without paying its memory cost. Place the remaining cards at the bottom of your deck in any order.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.IsDigimon)
                {
                    if (cardSource.Level == 6)
                    {
                        if (cardSource.CardColors.Contains(CardColor.Green))
                        {
                            if (GManager.instance.attackProcess.AttackingPermanent != null)
                            {
                                if (GManager.instance.attackProcess.AttackingPermanent.TopCard != null)
                                {
                                    if (cardSource.CanPlayCardTargetFrame(GManager.instance.attackProcess.AttackingPermanent.PermanentFrame, false, activateClass))
                                    {
                                        if (cardSource.HasLevel)
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

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    if (permanent.TopCard.CardColors.Contains(CardColor.Green))
                    {
                        if (permanent.Level == 5)
                        {
                            if (permanent.TopCard.HasLevel)
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
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, PermanentCondition))
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
                    if (CardEffectCommons.CanActivateSuspendCostEffect(card))
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                List<CardSource> selectedCards = new List<CardSource>();

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                    revealCount: 3,
                    simplifiedSelectCardConditions:
                    new SimplifiedSelectCardConditionClass[]
                    {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 green level 6 Digimon card.",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: 1,
                            selectCardCoroutine: SelectCardCoroutine),
                    },
                    remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                    activateClass: activateClass,
                    canNoSelect: true
                ));

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCards.Add(cardSource);
                    yield return null;
                }

                yield return ContinuousController.instance.StartCoroutine(new PlayCardClass(
                                    cardSources: selectedCards,
                                    hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                    payCost: false,
                                    targetPermanent: GManager.instance.attackProcess.AttackingPermanent,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Library,
                                    activateETB: true).PlayCard());
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
        }

        return cardEffects;
    }
}
