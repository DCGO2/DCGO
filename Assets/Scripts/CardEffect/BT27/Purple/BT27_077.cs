using System.Collections;
using System.Collections.Generic;

// NightChiropmon
namespace DCGO.CardEffects.BT27
{
    public class BT27_077 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - Lv.3 w/[Glowing Dawn] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasGlowingDawnTraits;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 3));
            }
            #endregion

            #region Shared Security / When Digivolving

            string SharedEffectName = "If 1 or fewer Tamers, may play 1 [Makoto Kuonji] in name Tamer from hand for free";

            string SharedEffectDescription(string tag)
                => $"[{tag}] If you have 1 or fewer Tamers, you may play 1 Tamer card with [Makoto Kuonji] in its name from your hand without paying the cost.";

            bool IsOwnTamer(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card);

            bool CanSelectCardCondition(CardSource cardSource, ActivateClass activateClass)
                => cardSource.IsTamer
                    && cardSource.ContainsCardName("Makoto Kuonji")
                    && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, SelectCardEffect.Root.Hand);

            bool SharedCanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.MatchConditionPermanentCount(IsOwnTamer) <= 1
                    && CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => CanSelectCardCondition(cardSource, activateClass));

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.MatchConditionPermanentCount(IsOwnTamer) > 1) yield break;

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                    canTargetCondition: cardSource => CanSelectCardCondition(cardSource, activateClass),
                    root: SelectCardEffect.Root.Hand,
                    cardEffect: activateClass,
                    payCost: false));
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                isSkippable: true,
                additionalActivateCondition: SharedCanActivateCondition,
                whenDigivolving: true,
                security: true);

            #endregion

            #region Inherited - Retaliation
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.RetaliationSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
