using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT21
{
    //Tsumemon
    public class BT21_006 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Inherit
            if (timing == EffectTiming.None)
            {
                bool VemmonCondition(CardSource source)
                {
                    return source.EqualsCardName("Vemmon");
                }

                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        return card.PermanentOfThisCard().DigivolutionCards.Count(VemmonCondition) >= 4;

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 3000, isInheritedEffect: true, card: card, condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}