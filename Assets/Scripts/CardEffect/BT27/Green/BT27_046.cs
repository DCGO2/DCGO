using System.Collections;
using System.Collections.Generic;

// Armalizamon
namespace DCGO.CardEffects.BT27
{
    public class BT27_046 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasGlowingDawnTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 3));
            }
            #endregion

            #region Shared Security / When Digivolving
            string SharedEffectName = "If 1 or fewer Tamers, play 1 Tamer with [Tomoro Tenma] in name from hand";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    isSkippable: true,
                    additionalActivateCondition: SharedAdditionalActivateCondition,
                    security: true,
                    whenDigivolving: true);

            string SharedEffectDescription(string tag) => $"[{tag}] If you have 1 or fewer Tamers, you may play 1 Tamer card with [Tomoro Tenma] in its name from your hand without paying the cost.";

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.IsTamer
                    && cardSource.ContainsCardName("Tomoro Tenma");
            }

            bool SharedAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
            {
                return CardEffectCommons.OwnerHas1OrLessTamers(card)
                    && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                    canTargetCondition: CanSelectCardCondition,
                    SelectCardEffect.Root.Hand,
                    activateClass,
                    false
                ));
            }
            #endregion

            #region Inherited Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(true, card, null));
            }
            #endregion

            return cardEffects;
        }
    }
}
