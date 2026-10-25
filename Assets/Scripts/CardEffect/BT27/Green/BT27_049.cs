using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Liamon
namespace DCGO.CardEffects.BT27
{
    public class BT27_049 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - [Elecmon]
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Elecmon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Alternate Digivolution Requirement - Lv.3 w/[Holy Beast]/[ACCEL] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasHolyBeastTraits
                        || targetPermanent.TopCard.EqualsTraits("ACCEL");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 3));
            }
            #endregion

            #region Rule - Name: Treated as including [Leomon]
            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("Treated as including [Leomon] in its name", CanUseCondition, card);
                changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: ChangeCardNames);
                cardEffects.Add(changeCardNamesClass);

                bool CanUseCondition(Hashtable hashtable)
                    => true;

                List<string> ChangeCardNames(CardSource cardSource, List<string> cardNames)
                {
                    if (cardSource == card)
                    {
                        cardNames.Add("Leomon");
                    }

                    return cardNames;
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into [Leomon] in name or [ACCEL] trait Digimon in hand, cost reduced per opponent's Digimon/Tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] This Digimon may digivolve into a Digimon card with [Leomon] in its name or the [ACCEL] trait in the hand. For each of your opponent's Digimon and Tamers, reduce this effect's paid cost by 1.";

                bool DigivolveCardCondition(CardSource cardSource)
                    => cardSource.IsDigimon
                        && (cardSource.ContainsCardName("Leomon") || cardSource.EqualsTraits("ACCEL"));

                // Cost is checked by DigivolveIntoHandOrTrashCard after the reduction, so only the requirement is checked here.
                bool CanDigivolveFromHandCondition(CardSource cardSource)
                    => DigivolveCardCondition(cardSource)
                        && cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, false, activateClass, SelectCardEffect.Root.Hand);

                int OpponentDigimonAndTamerCount()
                    => card.Owner.Enemy.GetBattleAreaPermanents()
                        .Count(permanent => permanent.IsDigimon || permanent.IsTamer);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanDigivolveFromHandCondition);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardCondition: DigivolveCardCondition,
                        payCost: true,
                        reduceCostTuple: (reduceCost: OpponentDigimonAndTamerCount(), reduceCostCardCondition: null),
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: true,
                        activateClass: activateClass,
                        successProcess: null));
                }
            }
            #endregion

            #region Inherited - Barrier
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
