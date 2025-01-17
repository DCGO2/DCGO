using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class BT16_002 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().TopCard.CardColors.Count >= 2)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(1000, true, card, PermanentCondition));
            }

            return cardEffects;
        }
    }
}