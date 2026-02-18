using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class ST13_04 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

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
                if (cardSource.Owner.HandCards.Contains(cardSource))
                {
                    if (cardSource.CardTraits.Contains("Legend-Arms"))
                    {
                        return true;
                    }

                    if (cardSource.CardColors.Contains(CardColor.Black))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool RootCondition(SelectCardEffect.Root root)
            {
                return root == SelectCardEffect.Root.Hand;
            }

            cardEffects.Add(CardEffectFactory.ChangeDigivolutionCostStaticEffect(
                changeValue: -1,
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
