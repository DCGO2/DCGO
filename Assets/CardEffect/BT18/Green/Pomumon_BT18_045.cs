using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT18
{
    public class Pomumon_BT18_045 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region All Turns
            if (timing == EffectTiming.None)
            {
                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.Owner == card.Owner;
                }

                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (card.PermanentOfThisCard().IsSuspended)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSecurityDigimonCardDPStaticEffect(
                    cardCondition: CardCondition,
                    changeValue: 1000,
                    isInheritedEffect: true,
                    card: card,
                    condition: Condition,
                    effectName: "All of your other Digimon get +1000 DP."));
            }
            #endregion

            return cardEffects;
        }
    }
}