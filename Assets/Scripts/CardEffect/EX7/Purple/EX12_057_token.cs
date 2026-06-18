using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.Tokens
{
    public class EX12_057_token : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region Guard
            if (timing == EffectTiming.WhenRemoveField)
            {
                cardEffects.Add(CardEffectFactory.GuardSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}