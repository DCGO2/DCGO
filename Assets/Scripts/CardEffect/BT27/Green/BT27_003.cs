using System.Collections;
using System.Collections.Generic;

// Frimon — BT27-003
namespace DCGO.CardEffects.BT27
{
    public class BT27_003 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Inherited — When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into [Leomon] or [ACCEL]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT27_003_Inherited");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] [Once Per Turn] This Digimon may digivolve into a Digimon card with [Leomon] in its name or the [ACCEL] trait in the hand.";
                }

                Permanent ThisDigimon()
                {
                    return card.PermanentOfThisCard();
                }

                bool IsEligibleDigivolutionCard(CardSource targetCard)
                {
                    return targetCard.IsDigimon &&
                           (targetCard.ContainsCardName("Leomon") || targetCard.EqualsTraits("ACCEL"));
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionOwnersHand(card, IsEligibleDigivolutionCard);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent attackingDigimon = ThisDigimon();
                    if (attackingDigimon == null)
                        yield break;

                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: attackingDigimon,
                            cardCondition: IsEligibleDigivolutionCard,
                            payCost: true,
                            reduceCostTuple: null,
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: null));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
