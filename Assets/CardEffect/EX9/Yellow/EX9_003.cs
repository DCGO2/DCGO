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

            #region Your Turn
            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolution Cost -1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("DigivoltuionCost-1_EX9_003");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] [Once Per Turn] When this Digimon with face-down digivolution cards would digivolve into a [Ver.3] trait Digimon card, reduce the digivolution cost by 1.";
                }

                bool PermanentCondition(Permanent permanent)
                {

                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent == card.PermanentOfThisCard())
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           cardSource.EqualsTraits("Ver.3");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(hashtable, PermanentCondition, CardCondition))
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
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (isExistOnField(card))
                    {
                        Hashtable hashtable = new Hashtable();
                        hashtable.Add("CardEffect", activateClass);

                        ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect("Digivolution Cost -1", CanUseCondition1, card);
                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                        card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

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
                            if (targetPermanent.TopCard != null)
                            {
                                if (targetPermanent.TopCard.Owner == card.Owner)
                                {
                                    if (targetPermanent.TopCard.Owner.GetBattleAreaPermanents().Contains(targetPermanent))
                                    {
                                        return true;
                                    }
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
                                    return cardSource.EqualsTraits("Ver.3");
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

                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}