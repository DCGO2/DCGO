using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Tokomon
namespace DCGO.CardEffects.EX9
{
    public class EX9_003 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Cost Reduciton Shared

            ActivateClass costReduceClass = new ActivateClass();
            costReduceClass.SetUpICardEffect("Digivolution Cost -1", CanUseReduceCondition, card);
            costReduceClass.SetUpActivateClass(CanActivateCostReductionCondition, ActivateCostReductionCoroutine, 1, false, EffectDiscription2());
            costReduceClass.SetIsInheritedEffect(true);
            costReduceClass.SetHashString("DigivolutionCost_EX9_003");

            string EffectDiscription2()
            {
                return "[Your Turn] [Once Per Turn] When this Digimon with face-down digivolution cards would digivolve into a [Ver.3] trait Digimon card, reduce the digivolution cost by 1.";
            }

            bool CardSourceCondition(CardSource cardSource)
            {
                return cardSource.EqualsTraits("Ver.3");
            }

            bool CanUseReduceCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolveOfCard(hashtable, CardSourceCondition, card))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanActivateCostReductionCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsOwnerTurn(card))
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCostReductionCoroutine(Hashtable _hashtable)
            {
                yield return null;
            }

            #endregion

            #region Cost Reduction Effect

            if (timing == EffectTiming.BeforePayCost)
            {
                cardEffects.Add(costReduceClass);
            }

            #endregion

            #region Cost Reduction Result

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect($"Digivolution Cost -1", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (card.PermanentOfThisCard().TopCard.PermanentOfThisCard().DigivolutionCards.Exists(x => x.IsFlipped))
                            {
                                if (!card.cEntity_EffectController.isOverMaxCountPerTurn(costReduceClass, costReduceClass.MaxCountPerTurn))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource))
                    {
                        if (RootCondition(root))
                        {
                            if (PermanentsCondition(targetPermanents))
                            {
                                Cost -= 1;
                            }
                        }
                    }

                    return Cost;
                }

                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    if (targetPermanents != null)
                    {
                        if (targetPermanents.Count(PermanentCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent == card.PermanentOfThisCard();
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                bool isUpDown()
                {
                    return true;
                }
            }

            #endregion

            return cardEffects;
        }
    }
}