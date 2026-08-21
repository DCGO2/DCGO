using System;
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
                    return targetPermanent.TopCard.ContainsTraits("Insectoid") || targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Reduce Play Cost
            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reduce play cost by 4", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetHashString("BT26_045_ReducePlayCost");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "When this card would be played, if your hand has fewer cards than your opponent's, reduce the cost by 4.";

                bool CardCondition(CardSource cardSource) => cardSource == card;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => card.Owner.HandCards.Count < card.Owner.Enemy.HandCards.Count
                        && card.Owner.CanReduceCost(null, card);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect("Play Cost -4", CanUseCondition1, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardCondition, rootCondition: RootCondition, isUpDown: IsUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);

                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                    card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                    bool CanUseCondition1(Hashtable hashtable) => true;

                    bool RootCondition(SelectCardEffect.Root root) => true;

                    bool IsUpDown() => true;

                    int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                    {
                        if (CardCondition(cardSource) && card.Owner.HandCards.Count < card.Owner.Enemy.HandCards.Count)
                        {
                            cost -= 4;
                        }

                        return cost;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));
                }
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
                    && (cardSource.ContainsTraits("Insectoid") || cardSource.EqualsTraits("Titan"))
                    && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);

            bool SharedAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => CanSelectHandCardCondition(cardSource, activateClass));

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool isUsed = false;

                bool CanSelectHandCardConditionBound(CardSource cardSource) => CanSelectHandCardCondition(cardSource, activateClass);

                if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectHandCardConditionBound))
                {
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
                maxCountPerTurn: 1,
                hashValue: "BT26_045_OP_WD_WA",
                additionalActivateCondition: SharedAdditionalActivateCondition,
                onPlay: true,
                whenDigivolving: true,
                whenAttacking: true);

            #region Your Turn - Grant Alliance/Piercing/Vortex
            if (timing == EffectTiming.None)
            {
                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("Your [Insectoid]/[Titan] Digimon gain Alliance, Piercing and Vortex", CanUseCondition, card);
                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                cardEffects.Add(addSkillClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card);

                bool PermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.ContainsTraits("Insectoid") || permanent.TopCard.EqualsTraits("Titan"));

                bool CardSourceCondition(CardSource cardSource)
                    => cardSource.PermanentOfThisCard() != null
                        && PermanentCondition(cardSource.PermanentOfThisCard())
                        && cardSource == cardSource.PermanentOfThisCard().TopCard;

                bool GrantCondition() => CardEffectCommons.IsOwnerTurn(card);

                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                {
                    if (_timing == EffectTiming.OnAllyAttack)
                    {
                        cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: false, card: cardSource, condition: GrantCondition));
                    }

                    if (_timing == EffectTiming.OnDetermineDoSecurityCheck)
                    {
                        cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: cardSource, condition: GrantCondition));
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
