using System.Collections;
using System.Collections.Generic;

// Kanata Hondo
namespace DCGO.CardEffects.BD01
{
    public class BD01_10 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared PermanentCondition
            bool PermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.HasPermanentColor(CardColor.Red)
                    && permanent.TopCard.EqualsTraits("ALYSIAN");
            }
            #endregion

            #region Start of Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                cardEffects.Add(CardEffectFactory.Gain1MemoryTamerOwnerDigimonConditionalEffect("IF you have a red Digimon with the [ALYSIAN] trait, gain 1 memory", PermanentCondition, null, card));
            }
            #endregion

            #region All turns
            if (timing == EffectTiming.None)
            {
                string EffectDiscription()
                {
                    return "[All Turns] All of your red [ALYSIAN] trait Digimon get +1000 DP.";
                }

                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(
                permanentCondition: PermanentCondition,
                changeValue: 1000,
                isInheritedEffect: false,
                card: card,
                condition: Condition,
                effectName: EffectDiscription));
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}
