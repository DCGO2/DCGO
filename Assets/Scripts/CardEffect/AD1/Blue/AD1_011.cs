using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Paildramon
namespace DCGO.CardEffects.AD1
{
    public class AD1_011 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolve Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel4
                        && (targetPermanent.TopCard.EqualsTraits("Free")
                            || targetPermanent.TopCard.EqualsTraits("Hero"));
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region DNA
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.GetJogressConditionClass(
                    PermanentCondition1, 
                    "a level 4 Blue Digimon", 
                    PermanentCondition2, 
                    "a level 4 Green Digimon", 
                    card
                ));

                bool PermanentCondition1(Permanent permanent)
                {
                    return permanent.TopCard.CardColors.Contains(CardColor.Blue)
                        && permanent.Levels_ForJogress(card).Contains(4);
                }

                bool PermanentCondition2(Permanent permanent)
                {
                    return permanent.TopCard.CardColors.Contains(CardColor.Green)
                        && permanent.Levels_ForJogress(card).Contains(4);
                }
            }
            #endregion

            #region Shared Partition + Inherited

            List<PartitionCondition> partitionConditions = new List<PartitionCondition>();
            partitionConditions.Add(new PartitionCondition(4, CardColor.Blue));
            partitionConditions.Add(new PartitionCondition(4, CardColor.Green));

            if (timing == EffectTiming.WhenRemoveField)
            {
                cardEffects.Add(CardEffectFactory.PartitionSelfEffect
                    (isInheritedEffect: false,
                    card: card,
                    condition: null,
                    cardSourceConditions: partitionConditions));

                cardEffects.Add(CardEffectFactory.PartitionSelfEffect
                    (isInheritedEffect: true,
                    card: card,
                    condition: null,
                    cardSourceConditions: partitionConditions));
            }

            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Can't be deleted in battle. If DNA: Attack target can't be changed", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Digivolving] Until your opponent's turn ends, this Digimon can't be deleted in battle. Then, if DNA digivolving, this Digimon's attack target can't change for the turn.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleArea(card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool CanNotBeDestroyedByBattleCondition(Permanent permanent1, Permanent attackingPermanent,
                                                    Permanent defendingPermanent, CardSource defendingCard)
                    {
                        return permanent1 == attackingPermanent || permanent1 == defendingPermanent;
                    }

                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.GainCanNotBeDeletedByBattle(
                            targetPermanent: card.PermanentOfThisCard(),
                            canNotBeDestroyedByBattleCondition: CanNotBeDestroyedByBattleCondition,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass,
                            effectName: "Can't be deleted in battle"));

                    if (CardEffectCommons.IsJogress(hashtable))
                    {
                        card.PermanentOfThisCard().UntilEachTurnEndEffects.Add(_timing => PermanentEffectFactory.CanNotSwitchAttackTargetEffect(card.PermanentOfThisCard()));
                    }
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into Imperialdramon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] This Digimon may digivolve into a Digimon card with [Imperialdramon] in its name in the hand with the digivolution cost reduced by 2.";

                bool CanSelectCardCondition(CardSource cardSource) => cardSource.CardNames.Contains("Imperialdramon");

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleArea(card);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardCondition: CanSelectCardCondition,
                        payCost: true,
                        reduceCostTuple: (reduceCost: 2, reduceCostCardCondition: null),
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
