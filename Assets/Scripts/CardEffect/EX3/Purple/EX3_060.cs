using System.Collections.Generic;

namespace DCGO.CardEffects.EX3
{
    public class EX3_060 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().HasNoDigivolutionCards)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.CanNotAttackSelfStaticEffect(defenderCondition: null, isInheritedEffect: false, card: card, condition: Condition, effectName: "Can't Attack"));
            }

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().HasNoDigivolutionCards)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.CanNotBlockStaticSelfEffect(attackerCondition: null, isInheritedEffect: false, card: card, condition: Condition, effectName: "Can't Block"));
            }

            return cardEffects;
        }
    }
}