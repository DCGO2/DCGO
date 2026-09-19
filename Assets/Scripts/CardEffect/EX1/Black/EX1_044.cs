using System;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX1
{
    public class EX1_044 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                int count()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return card.Owner.GetBattleAreaDigimons().Count((permanent) => permanent != card.PermanentOfThisCard() && permanent.TopCard.HasSameCardName(card.PermanentOfThisCard().TopCard));
                    }

                    return 0;
                }

                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (count() >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect<Func<int>>(changeValue: () => 1000 * count(), isInheritedEffect: true, card: card, condition: Condition));
            }

            return cardEffects;
        }
    }
}