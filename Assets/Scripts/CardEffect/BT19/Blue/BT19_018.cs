using System.Collections.Generic;

namespace DCGO.CardEffects.BT19
{
    public class BT19_018 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Evade

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.EvadeSelfEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }

            #endregion

            #region Jamming - ESS

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.JammingSelfStaticEffect(
                    isInheritedEffect: true,
                    card: card,
                    condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}