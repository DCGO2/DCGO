using System.Collections.Generic;

namespace DCGO.CardEffects.EX1
{
    public class EX1_038 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (card.PermanentOfThisCard().TopCard.ContainsCardName("Imperialdramon"))
                            {
                                return true;
                            }

                            if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("Free"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: true, card: card, condition: Condition));
            }

            return cardEffects;
        }
    }
}