using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT1_109 : CEntity_Effect
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
                return "[Main] For the turn, the next time you would digivolve one of your green Digimon from level 5 to level 6, decrease the digivolution cost by 4.";
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                yield return new WaitForSeconds(0.2f);

                ChangeCostClass changeCostClass = new ChangeCostClass();
                Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                changeCostClass.SetUpICardEffect("Digivolution Cost -4", CanUseCondition1, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilEachTurnEnd, card: card, cardEffect: null, timing: EffectTiming.None, getCardEffect: getCardEffect);

                ActivateClass activateClass1 = new ActivateClass();
                Func<EffectTiming, ICardEffect> getCardEffect1 = GetCardEffect1;
                activateClass1.SetUpICardEffect("Remove Effect", CanUseCondition1, card);
                activateClass1.SetUpActivateClass(null, ActivateCoroutine1, -1, false, "");
                activateClass1.SetIsBackgroundProcess(true);
                CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilEachTurnEnd, card: card, cardEffect: null, timing: EffectTiming.None, getCardEffect: getCardEffect1);

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
                                Cost -= 4;
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
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(targetPermanent, card))
                    {
                        if (targetPermanent.TopCard.CardColors.Contains(CardColor.Green))
                        {
                            if (targetPermanent.Level == 5 && targetPermanent.TopCard.HasLevel)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    if (cardSource.Level == 6)
                    {
                        if (cardSource.HasLevel)
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

        return cardEffects;
    }
}
