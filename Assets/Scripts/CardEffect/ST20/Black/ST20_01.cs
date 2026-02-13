using System.Collections;
using System.Collections.Generic;

//ST20-01 Koromon
namespace DCGO.CardEffects.ST20
{
    public class ST20_01 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Inherited Effect
            if (timing == EffectTiming.None)
            {
                bool condition()
                {
                    return card.PermanentOfThisCard().TopCard.HasAdventureTraits;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: 1000,
                    isInheritedEffect: true,
                    card: card,
                    condition: condition));
            }
            #endregion

            return cardEffects;
        }
    }
}