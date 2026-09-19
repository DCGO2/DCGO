using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT18
{
    public class BT18_043 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Your Turn

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolution Cost -1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("ReduceDigivolutionCost-1_BT18_043");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Your Turn] (Once Per Turn) When this Digimon or any of your Tamers would digivolve into a multicolored Digimon that is green or red, reduce the digivolution cost by 1.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsOwnerTurn(card) &&
                           CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(hashtable,
                               DigivolveFromPermanentCondition, DigivolveToCardCondition);
                }

                bool DigivolveFromPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card) &&
                           (permanent == card.PermanentOfThisCard() || permanent.IsTamer);
                }

                bool DigivolveToCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.CardColors.Count > 1 &&
                           (cardSource.CardColors.Contains(CardColor.Green) || cardSource.CardColors.Contains(CardColor.Red));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect("Digivolution Cost -1", CanUseReduceCondition, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition,
                        rootCondition: RootCondition, isUpDown: IsUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                    card.Owner.UntilCalculateFixedCostEffect.Add(_ => changeCostClass);

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(hashtable));

                    bool CanUseReduceCondition(Hashtable reduceHashtable)
                    {
                        return true;
                    }

                    int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                    {
                        if (CardSourceCondition(cardSource) && RootCondition(root) && PermanentsCondition(targetPermanents))
                        {
                            cost -= 1;
                        }

                        return cost;
                    }

                    bool PermanentsCondition(List<Permanent> targetPermanents)
                    {
                        return targetPermanents != null && targetPermanents.Some(PermanentCondition);
                    }

                    bool PermanentCondition(Permanent targetPermanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(targetPermanent, card);
                    }

                    bool CardSourceCondition(CardSource cardSource)
                    {
                        return cardSource.Owner == card.Owner;
                    }

                    bool RootCondition(SelectCardEffect.Root root)
                    {
                        return true;
                    }

                    bool IsUpDown()
                    {
                        return true;
                    }
                }
            }

            #endregion

            #region Piercing - ESS

            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(
                    isInheritedEffect: true,
                    card: card,
                    condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}