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

            #region Inherited Your Turn
            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolution Cost -1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("EX9_003_Inherit");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] [Once Per Turn] When this Digimon with face-down digivolution cards would digivolve into a [Ver.3] trait Digimon card, reduce the digivolution cost by 1.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent == card.PermanentOfThisCard()
                        && permanent.HasFaceDownDigivolutionCards;
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           cardSource.EqualsTraits("Ver.3");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(hashtable, PermanentCondition, CardCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Hashtable hashtable = new Hashtable{{ "CardEffect", activateClass }};

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
                        if (CardSourceCondition(cardSource)
                        && RootCondition(root)
                        && PermanentsCondition(targetPermanents))
                        {
                            Cost -= 1;
                        }
                        
                        return Cost;
                    }

                    bool PermanentsCondition(List<Permanent> targetPermanents)
                    {
                        return targetPermanents != null
                            && targetPermanents.Count(PermanentCondition) >= 1;
                    }

                    bool PermanentCondition(Permanent targetPermanent)
                    {
                        return targetPermanent.TopCard != null
                            && targetPermanent.TopCard.Owner == card.Owner
                            && targetPermanent.TopCard.Owner.GetBattleAreaPermanents().Contains(targetPermanent);
                    }

                    bool CardSourceCondition(CardSource cardSource)
                    {
                        return cardSource != null
                            && cardSource.Owner == card.Owner
                            && cardSource.EqualsTraits("Ver.3");
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
            #endregion

            return cardEffects;
        }
    }
}