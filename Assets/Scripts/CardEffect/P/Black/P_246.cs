using System.Collections;
using System.Collections.Generic;

// Motimon
namespace DCGO.CardEffects.P
{
    public class P_246 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Inherited YT
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into a [Sukamon]/[Etemon]/[Mamemon] in name from hand for 2 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("P_246_Inherited");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Your Turn] [Once Per Turn] When any of your other Digimon with [Sukamon] or [Mamemon] in their names are deleted, this Digimon may digivolve into a Digimon card with [Sukamon], [Etemon] or [Mamemon] in its name in the hand with the cost reduced by 2.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, TriggerPermanentCondition, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectTargetCardCondition);

                bool TriggerPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && (permanent.TopCard.ContainsCardName("Sukamon")
                            || permanent.TopCard.ContainsCardName("Mamemon"));

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.IsDigimon
                        && (cardSource.ContainsCardName("Sukamon")
                            || cardSource.ContainsCardName("Etemon")
                            || cardSource.ContainsCardName("Mamemon"));

                bool CanSelectTargetCardCondition(CardSource cardSource)
                    => CanSelectCardCondition(cardSource)
                        && cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, true, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool isUsed = false;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardCondition: CanSelectCardCondition,
                        payCost: true,
                        reduceCostTuple: (reduceCost: 2, reduceCostCardCondition: null),
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: true,
                        activateClass: activateClass,
                        successProcess: SuccessProcess(),
                        isOptional: true));

                    IEnumerator SuccessProcess()
                    {
                        isUsed = true;
                        yield return null;
                    }

                    if (!isUsed) activateClass.RemoveUse();
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
