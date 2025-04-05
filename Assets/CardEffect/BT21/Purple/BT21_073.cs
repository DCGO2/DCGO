using System.Collections;
using System.Collections.Generic;

//BT21_073 Charismon
namespace DCGO.CardEffects.BT21
{
    public class BT21_073 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region On Play/When Digivolving shared
            bool CanLinkCondition(CardSource card)
            {
                return card.HasLevel && card.Level <= 4;
                card.LinkDP
            }
            #endregion
            return cardEffects;
        }
    }
}