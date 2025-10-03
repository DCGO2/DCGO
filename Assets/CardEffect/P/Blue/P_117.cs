using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.P
{
    public class P_117 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Activate Cost Reduciton
            ActivateClass costReduceClass = new ActivateClass();
            costReduceClass.SetUpICardEffect("Digivolution Cost -1", CanUseReduceCondition, card);
            costReduceClass.SetUpActivateClass(CanActivateCostReductionCondition, ActivateCostReductionCoroutine, 2, false, EffectDiscription2());
            //costReduceClass.SetIsInheritedEffect(true);
            //costReduceClass.SetNotShowUI(true);
            //costReduceClass.SetIsBackgroundProcess(true);
            costReduceClass.SetHashString("DigivolutionCost-1_P_117");

            string EffectDiscription2()
            {
                return "[Your Turn] [Once Per Turn] When this Digimon would digivolve into a Digimon card with the [Free] trait, if you have a Tamer, reduce the digivolution cost by 1.";
            }

            bool CardSourceCondition(CardSource cardSource)
            {
                return cardSource.CardTraits.Contains("Free");
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




            if (timing == EffectTiming.BeforePayCost)
            {
                cardEffects.Add(costReduceClass);
            }
            #endregion

            #region Cost reduction
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
                            if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsTamer))
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

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] If this Digimon has 2 or more colors, <Draw 1> (Draw 1 card from your deck).";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().TopCard.CardColors.Count >= 2)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                }
            }

            return cardEffects;
        }
    }
}