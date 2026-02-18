using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ST10_04 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Reveal the top 3 cards of deck", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Play] Reveal the top 3 cards of your deck. Add 1 yellow Digimon card and 1 purple Digimon card among them to your hand. Place the remaining cards at the bottom of your deck in any order.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.IsDigimon)
                {
                    if (cardSource.CardColors.Contains(CardColor.Yellow))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanSelectCardCondition1(CardSource cardSource)
            {
                if (cardSource.IsDigimon)
                {
                    if (cardSource.CardColors.Contains(CardColor.Purple))
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

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                    revealCount: 3,
                    simplifiedSelectCardConditions:
                    new SimplifiedSelectCardConditionClass[]
                    {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 yellow Digimon card.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition1,
                            message: "Select 1 purple Digimon card.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                    },
                    remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                    activateClass: activateClass
                ));
            }
        }

        if (timing == EffectTiming.None)
        {
            bool Condition()
            {
                return CardEffectCommons.IsOwnerTurn(card) && CardEffectCommons.IsExistOnBattleArea(card);
            }

            bool PermanentCondition(Permanent targetPermanent)
            {
                return targetPermanent == card.PermanentOfThisCard();
            }

            bool CardSourceCondition(CardSource cardSource)
            {
                if (cardSource.CardTraits.Contains("Archangel"))
                {
                    return true;
                }

                if (cardSource.CardTraits.Contains("Fallen Angel"))
                {
                    return true;
                }

                if (cardSource.CardTraits.Contains("FallenAngel"))
                {
                    return true;
                }

                return false;
            }

            bool RootCondition(SelectCardEffect.Root root)
            {
                return true;
            }

            cardEffects.Add(CardEffectFactory.ChangeDigivolutionCostStaticEffect(
                changeValue: -2,
                permanentCondition: PermanentCondition,
                cardCondition: CardSourceCondition,
                rootCondition: RootCondition,
                isInheritedEffect: false,
                card: card,
                condition: Condition,
                setFixedCost: false));
        }

        if (timing == EffectTiming.OnEndTurn)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("DNA digivolve this Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            activateClass.SetIsInheritedEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[End of Your Turn] You may DNA digivolve this Digimon and one of your other Digimon in play into a Digimon card in your hand by paying its DNA digivolve cost.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource != null)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.CanPlayJogress(true))
                            {
                                if (isExistOnField(card))
                                {
                                    if (cardSource.CanJogressFromTargetPermanent(card.PermanentOfThisCard(), true))
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

            bool CanUseCondition(Hashtable hashtable)
            {
                if (isExistOnField(card))
                {
                    return true;
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (card.Owner.HandCards.Count >= 1)
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && permanent != card.PermanentOfThisCard()))
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
                if (isExistOnField(card))
                {
                    if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                         CardEffectCommons.DNADigivolvePermanentsIntoHandOrTrashCard(
                             CanSelectCardCondition,
                             payCost: true,
                             isHand: true,
                             activateClass,
                             permanentConditions: new Func<Permanent, bool>[] { (permanent) => permanent == card.PermanentOfThisCard() }
                         ));
                    }
                }
            }
        }

        return cardEffects;
    }
}
