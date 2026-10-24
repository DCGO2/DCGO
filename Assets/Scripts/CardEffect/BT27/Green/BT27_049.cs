using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Liamon — BT27-049
namespace DCGO.CardEffects.BT27
{
    public class BT27_049 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    CardSource source = permanent.TopCard;
                    return source.EqualsCardName("Elecmon") ||
                           (source.IsLevel3 &&
                            (source.EqualsTraits("Holy Beast") || source.EqualsTraits("ACCEL")));
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into [Leomon] or [ACCEL] with reduced cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    "[When Attacking] This Digimon may digivolve into a Digimon card with [Leomon] in its name or the [ACCEL] trait in the hand. For each of your opponent's Digimon and Tamers, reduce this effect's paid cost by 1.");
                cardEffects.Add(activateClass);

                bool EligibleCard(CardSource source)
                    => source.IsDigimon &&
                       (source.ContainsCardName("Leomon") || source.EqualsTraits("ACCEL"));

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                       CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                       CardEffectCommons.HasMatchConditionOwnersHand(card, EligibleCard);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent digimon = card.PermanentOfThisCard();
                    if (digimon == null)
                        yield break;

                    // Count the opponent's Digimon and Tamers at resolution.
                    int reduction = card.Owner.Enemy.GetBattleAreaPermanents()
                        .Count(permanent => permanent.IsDigimon || permanent.IsTamer);

                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: digimon,
                            cardCondition: EligibleCard,
                            payCost: true,
                            reduceCostTuple: (reduceCost: reduction, reduceCostCardCondition: null),
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: null));
                }
            }
            #endregion

            #region Inherited Barrier
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(
                    isInheritedEffect: true,
                    card: card,
                    condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
