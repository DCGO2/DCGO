using System.Collections.Generic;

namespace DCGO.CardEffects.BT11
{
    public class BT11_060 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                bool CardEffectCondition(ICardEffect cardEffect)
                {
                    return CardEffectCommons.IsOpponentEffect(cardEffect, card);
                }

                bool CanUseCondition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                string effectName = "Can't return to hand by opponent's effects";

                cardEffects.Add(CardEffectFactory.CannotReturnToHandStaticEffect(
                    permanentCondition: PermanentCondition,
                    cardEffectCondition: CardEffectCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseCondition,
                    effectName: effectName
                ));
            }

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                bool CardEffectCondition(ICardEffect cardEffect)
                {
                    return CardEffectCommons.IsOpponentEffect(cardEffect, card);
                }

                bool CanUseCondition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                string effectName = "Can't return to deck by opponent's effects";

                cardEffects.Add(CardEffectFactory.CannotReturnToDeckStaticEffect(
                    permanentCondition: PermanentCondition,
                    cardEffectCondition: CardEffectCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseCondition,
                    effectName: effectName
                ));
            }

            return cardEffects;
        }
    }
}