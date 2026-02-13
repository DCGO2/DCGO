using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT17
{
    public class BT17_004 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Blocker
            if (timing == EffectTiming.None)
            {
                bool CanUseCondition()
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            if (card.PermanentOfThisCard().TopCard.CardNames.Contains("Argomon"))
                                return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: true, card: card, condition: CanUseCondition));
            }
            #endregion

            return cardEffects;
        }
    }
}