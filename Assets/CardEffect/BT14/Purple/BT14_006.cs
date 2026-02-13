using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT14
{
    public class BT14_006 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnDiscardHand)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Digimon digivolves into discarded card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Digivolve_BT14_006");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When a Digimon card with the [Dark Animal] or [SoC] trait is trashed from your hand, this Digimon may digivolve into that card.";
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource.Owner == card.Owner)
                    {
                        if (cardSource.IsDigimon)
                        {
                            if (cardSource.CardTraits.Contains("Dark Animal") || cardSource.CardTraits.Contains("DarkAnimal"))
                            {
                                return true;
                            }

                            if (cardSource.CardTraits.Contains("SoC"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource, Hashtable hashtable)
                {
                    if (CardCondition(cardSource))
                    {
                        List<CardSource> DiscardedCards = CardEffectCommons.GetDiscardedCardsFromHashtable(hashtable);

                        if (DiscardedCards != null)
                        {
                            if (DiscardedCards.Contains(cardSource))
                            {
                                if (CardEffectCommons.IsExistOnTrash(cardSource))
                                {
                                    if (CardEffectCommons.IsExistOnBattleArea(card))
                                    {
                                        if (cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, true, activateClass))
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
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnTrashHand(hashtable, null, CardCondition))
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
                        if (card.Owner.TrashCards.Count(cardSource => CanSelectCardCondition(cardSource, hashtable)) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardCondition: cardSource => CanSelectCardCondition(cardSource, _hashtable),
                        payCost: true,
                        reduceCostTuple: null,
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: false,
                        activateClass: activateClass,
                        successProcess: null));
                }
            }

            return cardEffects;
        }
    }
}