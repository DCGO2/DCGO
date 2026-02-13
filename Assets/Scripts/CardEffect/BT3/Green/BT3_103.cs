using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;


public class BT3_103 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OptionSkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] The next time one of your green Digimon digivolves this turn, you may suspend 1 of your Digimon to reduce the memory cost of the digivolution by 5.";
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                yield return new WaitForSeconds(0.2f);

                ActivateClass activateClass1 = new ActivateClass();
                Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                activateClass1.SetUpICardEffect("Digivolution Cost -5", CanUseCondition1, card);
                activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, true, EffectDiscription1());
                CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilEachTurnEnd, card: card, cardEffect: null, timing: EffectTiming.None, getCardEffect: getCardEffect);

                ActivateClass activateClass2 = new ActivateClass();
                Func<EffectTiming, ICardEffect> getCardEffect1 = GetCardEffect1;
                activateClass2.SetUpICardEffect("Remove Effect", CanUseCondition1, card);
                activateClass2.SetUpActivateClass(null, ActivateCoroutine2, -1, false, "");
                activateClass2.SetIsBackgroundProcess(true);
                CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilEachTurnEnd, card: card, cardEffect: null, timing: EffectTiming.None, getCardEffect: getCardEffect1);

                string EffectDiscription1()
                {
                    return "You may suspend 1 of your Digimon to reduce the memory cost of the digivolution by 5.";
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(targetPermanent, card))
                    {
                        if (targetPermanent.TopCard.CardColors.Contains(CardColor.Green))
                        {
                            return true;

                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (CardEffectCommons.CanActivateSuspendCostEffect(permanent.TopCard))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition1(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(
                        hashtable: hashtable,
                        permanentCondition: PermanentCondition,
                        cardCondition: null))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanActivateCondition1(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                {
                    int maxCount = 1;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass1);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        yield return null;

                        if (permanents.Count >= 1)
                        {
                            ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                            ChangeCostClass changeCostClass = new ChangeCostClass();
                            changeCostClass.SetUpICardEffect("Digivolution Cost -5", CanUseCondition1, card);
                            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true); card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable1));

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
                                            Cost -= 5;
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
                                return CardEffectCommons.IsPermanentExistsOnBattleArea(targetPermanent);
                            }

                            bool CardSourceCondition(CardSource cardSource)
                            {
                                return true;
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

                ICardEffect GetCardEffect(EffectTiming _timing)
                {
                    if (_timing == EffectTiming.BeforePayCost)
                    {
                        return activateClass1;
                    }

                    return null;
                }

                IEnumerator ActivateCoroutine2(Hashtable _hashtable1)
                {
                    if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(
                        hashtable: _hashtable1,
                        permanentCondition: PermanentCondition,
                        cardCondition: null))
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
                        return activateClass2;
                    }

                    return null;
                }
            }

        }

        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect($"Add this card to hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] Add this card to its owner's hand.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
            }
        }

        return cardEffects;
    }
}
