using System.Collections;
using System.Collections.Generic;

// Liskmon
namespace DCGO.CardEffects.ST25
{
    public class ST25_03 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - Lv.3 w/[Gemmon] in text
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasText("Gemmon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 3));
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "If you have 1 or fewer Tamers, may play 1 [Kanata Hondo] from hand for free";

            string SharedEffectDescription(string tag)
                => $"[{tag}] If you have 1 or fewer Tamers, you may play 1 [Kanata Hondo] from your hand without paying the cost.";

            bool OwnTamerCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card);

            bool KanataCondition(CardSource cardSource, ICardEffect cardEffect)
                => cardSource.EqualsCardName("Kanata Hondo")
                    && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, cardEffect);

            bool SharedAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.MatchConditionPermanentCount(OwnTamerCondition) <= 1
                    && CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => KanataCondition(cardSource, activateClass));

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                    canTargetCondition: cardSource => KanataCondition(cardSource, activateClass),
                    root: SelectCardEffect.Root.Hand,
                    cardEffect: activateClass,
                    payCost: false));
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                additionalActivateCondition: SharedAdditionalActivateCondition,
                optional: false,
                isSkippable: true,
                onPlay: true,
                whenDigivolving: true);
            #endregion

            #region Raid - Inherited
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
