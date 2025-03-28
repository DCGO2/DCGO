using System.Collections;
using System.Collections.Generic;

// Gaiamon
namespace DCGO.CardEffects.BT21
{
    public class BT21_101 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None) cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));

            //Need Link Logic to implement other effects

            return cardEffects;
        }
    }
}