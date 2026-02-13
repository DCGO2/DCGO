using System.Collections.Generic;

namespace DCGO.CardEffects.BT14
{
    public class BT14_062 : CEntity_Effect
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
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                string effectName = "Can't be deleted by opponent's effects";

                cardEffects.Add(CardEffectFactory.CanNotBeDestroyedBySkillStaticEffect(
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