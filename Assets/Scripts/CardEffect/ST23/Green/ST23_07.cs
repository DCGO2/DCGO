using System;
using System.Collections;
using System.Collections.Generic;

// Armalizamon
namespace DCGO.CardEffects.ST23
{
    public class ST23_07 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Glowing Dawn");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(level: 3, permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "May play 1 [Glowing Dawn] Tamer from hand for free";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    additionalActivateCondition: AdditionalActivateCondition,
                    optional: false,
                    onPlay: true,
                    whenDigivolving: true);

            string SharedEffectDescription(string tag) => $"[{tag}] If you have 1 or fewer Tamers, you may play 1 Tamer card with the [Glowing Dawn] trait from your hand without paying the cost.";

            bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
            {
                return CardEffectCommons.MatchConditionOwnersPermanentCount(card, HasTamersInBattleArea) <= 1;
            }

            bool HasTamersInBattleArea(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                    && permanent.IsTamer;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool HasTamerInHand(CardSource source)
                {
                    return source.EqualsTraits("Glowing Dawn")
                        && source.IsTamer
                        && CardEffectCommons.CanPlayAsNewPermanent(source, false, activateClass);
                }

                if (CardEffectCommons.HasMatchConditionOwnersHand(card, HasTamerInHand))
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                        canTargetCondition: HasTamerInHand,
                        SelectCardEffect.Root.Hand,
                        activateClass,
                        payCost: false
                    ));
                }
            }
            #endregion

            #region ESS - Piercing

            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}
