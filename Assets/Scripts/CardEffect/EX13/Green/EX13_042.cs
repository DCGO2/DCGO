using System.Collections;
using System.Collections.Generic;

// Bastemon
namespace DCGO.CardEffects.EX13
{
    public class EX13_042 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alliance
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared When Digivolving / When Attacking

            string SharedEffectName = "May play 1 play cost 4 or lower [Beast]/[Animal]/[Sovereign] Digimon from hand for free";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] You may play 1 play cost 4 or lower Digimon card with [Beast], [Animal] or [Sovereign], other than [Sea Animal], in any of its traits from your hand without paying the cost.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                    canTargetCondition: cardSource => CanSelectCardCondition(cardSource, activateClass),
                    root: SelectCardEffect.Root.Hand,
                    cardEffect: activateClass,
                    payCost: false,
                    afterSelectCardCoroutine: AfterSelectCardCoroutine));

                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                {
                    if (cardSources.Count == 0) activateClass.RemoveUse();

                    yield return null;
                }
            }

            bool CanSelectCardCondition(CardSource cardSource, ActivateClass activateClass)
                => cardSource.IsDigimon
                    && cardSource.HasPlayCost
                    && cardSource.GetCostItself <= 4
                    && cardSource.HasBeastTraits
                    && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);

            bool SharedAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => CanSelectCardCondition(cardSource, activateClass));

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                isSkippable: true,
                additionalActivateCondition: SharedAdditionalActivateCondition,
                maxCountPerTurn: 1,
                hashValue: "EX13_042_WD_WA",
                whenDigivolving: true,
                whenAttacking: true);

            #endregion

            #region Alliance - ESS
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
