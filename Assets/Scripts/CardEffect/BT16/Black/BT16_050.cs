using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class BT16_050 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared Effect

            string EffectDiscription()
            {
                return "[All Turns] All of your other Digimon with the [D-Brigade] or [DigiPolice] trait get +1000 DP.";
            }

            bool Condition()
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    return true;
                }

                return false;
            }

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                {
                    if (permanent != card.PermanentOfThisCard())
                    {
                        if (permanent.TopCard.CardTraits.Contains("D-Brigade") || permanent.TopCard.CardTraits.Contains("DigiPolice"))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(
                permanentCondition: PermanentCondition,
                changeValue: 1000,
                isInheritedEffect: false,
                card: card,
                condition: Condition,
                effectName: EffectDiscription));
            }

            #endregion

            #region Inherited Effect

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(
                permanentCondition: PermanentCondition,
                changeValue: 1000,
                isInheritedEffect: true,
                card: card,
                condition: Condition,
                effectName: EffectDiscription));
            }

            #endregion

            return cardEffects;
        }
    }
}