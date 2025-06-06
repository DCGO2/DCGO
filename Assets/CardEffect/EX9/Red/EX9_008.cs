using System.Collections;
using System.Collections.Generic;

// Biyomon
namespace DCGO.CardEffects.EX9
{
    public class EX9_008 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.IsLevel2)
                    {
                        if (targetPermanent.TopCard.EqualsTraits("DM"))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Training

            if (timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.TrainingEffect(card: card));
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}