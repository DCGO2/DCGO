using System.Collections;
using System.Collections.Generic;

// GranKuwagamon
namespace DCGO.CardEffects.BT26
{
    public class BT26_045 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Insectoid") || targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Reduce Play Cost
            if (timing == EffectTiming.None)
            {
                bool Condition() => card.Owner.HandCards.Count < card.Owner.Enemy.HandCards.Count;

                cardEffects.Add(CardEffectFactory.MandatorySelfPlayCostReduction(4, card, Condition));
            }
            #endregion

            #region Shared On Play / When Digivolving / When Attacking
            string SharedEffectName()
                => "May play 1 level 4 or lower [Insectoid]/[Titan] Digimon from hand free";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] You may play 1 level 4 or lower Digimon card with the [Insectoid] or [Titan] trait from your hand without paying the cost.";

            bool CanSelectHandCardCondition(CardSource cardSource, ICardEffect activateClass)
                => cardSource.IsDigimon
                    && cardSource.HasLevel && cardSource.Level <= 4
                    && (cardSource.EqualsTraits("Insectoid") || cardSource.EqualsTraits("Titan"))
                    && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);

            bool SharedAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => CanSelectHandCardCondition(cardSource, activateClass));

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool isUsed = false;

                bool CanSelectHandCardConditionBound(CardSource cardSource) => CanSelectHandCardCondition(cardSource, activateClass);

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                    canTargetCondition: CanSelectHandCardConditionBound,
                    root: SelectCardEffect.Root.Hand,
                    cardEffect: activateClass,
                    payCost: false,
                    afterSelectCardCoroutine: AfterSelectCardCoroutine));

                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                {
                    if (cardSources != null && cardSources.Count > 0) isUsed = true;
                    yield return null;
                }

                if (!isUsed) activateClass.RemoveUse();
            }

            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                isSkippable: true,
                maxCountPerTurn: 1,
                hashValue: "BT26_045_OP_WD_WA",
                additionalActivateCondition: SharedAdditionalActivateCondition,
                onPlay: true,
                whenDigivolving: true,
                whenAttacking: true);

            #region Grant Alliance/Piercing/Vortex

            bool GrantPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && (permanent.TopCard.EqualsTraits("Insectoid") || permanent.TopCard.EqualsTraits("Titan"));

            bool GrantCondition() 
                => CardEffectCommons.IsExistOnBattleArea(card)
                    && CardEffectCommons.IsOwnerTurn(card);

            if (timing == EffectTiming.None)
            {
                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("Your [Insectoid]/[Titan] Digimon gain Vortex", CanUseCondition, card);
                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                cardEffects.Add(addSkillClass);

                bool CanUseCondition(Hashtable hashtable) => GrantCondition();

                bool CardSourceCondition(CardSource cardSource)
                    => cardSource.PermanentOfThisCard() != null
                        && GrantPermanentCondition(cardSource.PermanentOfThisCard())
                        && cardSource == cardSource.PermanentOfThisCard().TopCard;

                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                {
                    if (timing == EffectTiming.OnAllyAttack)
                    {
                        cardEffects.Add(CardEffectFactory.AllianceSelfEffect(false, cardSource, GrantCondition));
                    }

                    if (timing == EffectTiming.OnDetermineDoSecurityCheck)
                    {
                        cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, condition: GrantCondition, card: cardSource));
                    }
                    
                    if (_timing == EffectTiming.OnEndTurn)
                    {
                        cardEffects.Add(CardEffectFactory.VortexSelfEffect(isInheritedEffect: false, card: cardSource, condition: GrantCondition));
                    }

                    return cardEffects;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
