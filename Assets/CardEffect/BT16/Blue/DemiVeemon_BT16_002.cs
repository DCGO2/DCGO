using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class DemiVeemon_BT16_002 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            switch (timing)
            {
                case EffectTiming.None:
                {
                    string EffectDiscription()
                    {
                        return "[All Turns] While this Digimon has 2 or more colors, it gets +1000 DP.";
                    }
                    
                    bool PermanentCondition()
                    {
                        return CardEffectCommons.IsExistOnBattleArea(card)
                               && card.PermanentOfThisCard().TopCard.CardColors.Count >= 2;
                    }
                    
                    cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(1000, true, card, PermanentCondition));
                    break;
                }
            }
            
            return cardEffects;
        }
    }
}