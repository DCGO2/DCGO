using System.Collections;
using System.Collections.Generic;

// Nosferamon / Rampage Bullet (Dual Digimon / Option)
// INTEGRATION CANDIDATE: See README for required unresolved mechanics.
namespace DCGO.CardEffects.BT27
{
    public class BT27_082 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            // Digimon side: alternate digivolution, Lv.5 [Glowing Dawn] cost 3.
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                    => permanent.TopCard.EqualsTraits("Glowing Dawn");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 5));
            }

            // Digimon side: <Execute>.
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.ExecuteSelfEffect(
                    isInheritedEffect: false, card: card, condition: null));
            }

            // Digimon side: <Ascension>.
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.AscensionSelfEffect(
                    isInheritedEffect: false, card: card, condition: null));
            }

            // Option side: [Use Req. ([Glowing Dawn] trait)] and [Arts Digivolve].
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(
                    card, source => source.EqualsTraits("Glowing Dawn")));
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            }

            // TODO: [Security] [On Deletion] — deletion of lowest-level enemy
            // Digimon, then conditional opponent top-security trash if no deletion.
            // Requires verification of the dual-card security/on-deletion dispatch.

            // TODO: [When Digivolving] [When Attacking] [Once Per Turn]
            // Trash 2 BOTTOM face-down cards from under any of your Tamers as cost;
            // activate this card's Option-side [Main] effect without using the Option.
            // Needs validated source-selection, trashing, and effect invocation APIs.

            // TODO: Option-side [Main] — delete one enemy Lv.6+ Digimon, then
            // optionally play one [BEATBREAK] trait card costing <= 4 from trash.
            // Needs validated dual-side option activation and play-card type handling.

            return cardEffects;
        }
    }
}
