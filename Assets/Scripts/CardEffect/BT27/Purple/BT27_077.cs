using System.Collections;
using System.Collections.Generic;

// NightChiropmon — BT27-077
namespace DCGO.CardEffects.BT27
{
    public class BT27_077 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                    => permanent.TopCard != null && permanent.TopCard.EqualsTraits("Glowing Dawn");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    level: 3,
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region Security / When Digivolving — Play Makoto Kuonji
            // The [Security] tag is an additional trigger for this shared effect,
            // rather than a separate security-only card effect.
            if (timing == EffectTiming.SecuritySkill || timing == EffectTiming.OnEnterFieldAnyone)
            {
                bool isSecurityTiming = timing == EffectTiming.SecuritySkill;
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Play 1 Makoto Kuonji Tamer from hand for free if you have 1 or fewer Tamers",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetHashString(isSecurityTiming ? "BT27_077_Security" : "BT27_077_WhenDigivolving");
                if (isSecurityTiming) activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Security] [When Digivolving] If you have 1 or fewer Tamers, you may play 1 Tamer card with [Makoto Kuonji] in its name from your hand without paying the cost.";

                bool IsOwnTamer(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card);

                bool IsEligibleTamer(CardSource candidate)
                    => candidate.IsTamer
                        && candidate.ContainsCardName("Makoto Kuonji")
                        && CardEffectCommons.CanPlayAsNewPermanent(candidate, false, activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => isSecurityTiming
                        ? CardEffectCommons.CanTriggerSecurityEffect(hashtable, card)
                        : CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card)
                            && CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => (isSecurityTiming || CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass))
                        && CardEffectCommons.MatchConditionPermanentCount(IsOwnTamer) <= 1
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, IsEligibleTamer);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.MatchConditionPermanentCount(IsOwnTamer) > 1)
                        yield break;

                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.PlayByEffect(
                            canTargetCondition: IsEligibleTamer,
                            root: SelectCardEffect.Root.Hand,
                            cardEffect: activateClass,
                            payCost: false));
                }
            }
            #endregion

            #region Inherited — Retaliation
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.RetaliationSelfEffect(
                    isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
