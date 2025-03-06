using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT21
{
    public class Tsumemon_BT21_006 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Inherit
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        int vemmon_in_source = 0;

                        foreach (CardSource card_in_source in card.PermanentOfThisCard().cardSources)
                        {
                            if (card_in_source.PermanentOfThisCard().TopCard.ContainsCardName("Vemmon"))
                            {
                                vemmon_in_source += 1;
                            }
                        }

                        if (vemmon_in_source >= 4)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 3000, isInheritedEffect: true, card: card, condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}