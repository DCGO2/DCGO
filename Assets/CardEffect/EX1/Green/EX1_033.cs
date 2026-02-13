using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.EX1
{
    public class EX1_033 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("The next time digivolution cost -1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] The next time one of your Digimon digivolves into a Digimon card with [Insectoid] or [Ancient Insect] in its traits this turn, reduce the memory cost of the digivolution by 1.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
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
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                        yield return new WaitForSeconds(0.2f);

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                        changeCostClass.SetUpICardEffect("Digivolution Cost -1", CanUseCondition1, card);
                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                        card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);
                        //CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilCalculateFixedCost, card: card, cardEffect: null, timing: EffectTiming.None, getCardEffect: getCardEffect);

                        ActivateClass activateClass1 = new ActivateClass();
                        Func<EffectTiming, ICardEffect> getCardEffect1 = GetCardEffect1;
                        activateClass1.SetUpICardEffect("Remove Effect", CanUseCondition1, card);
                        activateClass1.SetUpActivateClass(null, ActivateCoroutine1, -1, false, "");
                        activateClass1.SetIsBackgroundProcess(true);
                        CardEffectCommons.AddEffectToPlayer(
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            card: card,
                            cardEffect: null,
                            timing: EffectTiming.None,
                            getCardEffect: getCardEffect1);

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
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(targetPermanent, card);
                        }

                        bool CardSourceCondition(CardSource cardSource)
                        {
                            if (cardSource.IsDigimon)
                            {
                                if (cardSource.CardTraits.Contains("Insectoid"))
                                {
                                    return true;
                                }

                                if (cardSource.CardTraits.Contains("Ancient Insect"))
                                {
                                    return true;
                                }

                                if (cardSource.CardTraits.Contains("AncientInsect"))
                                {
                                    return true;
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

                        ICardEffect GetCardEffect(EffectTiming _timing)
                        {
                            return changeCostClass;
                        }

                        IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                        {
                            if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(
                                hashtable: _hashtable1,
                                permanentCondition: PermanentCondition,
                                cardCondition: CardSourceCondition))
                            {
                                card.Owner.UntilEachTurnEndEffects.Remove(getCardEffect);
                                card.Owner.UntilEachTurnEndEffects.Remove(getCardEffect1);
                                yield return null;
                            }
                        }

                        ICardEffect GetCardEffect1(EffectTiming _timing)
                        {
                            if (_timing == EffectTiming.AfterPayCost)
                            {
                                return activateClass1;
                            }

                            return null;
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}