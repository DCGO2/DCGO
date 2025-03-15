using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT1
{
    public class BT1_027 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.LinkEffect(card, 1, 2000));
            }

            return cardEffects;
        }
    }
}