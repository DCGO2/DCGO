using System.Collections.Generic;

namespace DCGO.CardEffects.EX3
{
    public class EX3_016 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsOpponentTurn(card))
                    {
                        if (CardEffectCommons.IsExistOnBattleArea(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(targetPermanent, card) && targetPermanent.HasNoDigivolutionCards;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return true;
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                cardEffects.Add(CardEffectFactory.ChangeDigivolutionCostStaticEffect(
                    changeValue: 1,
                    permanentCondition: PermanentCondition,
                    cardCondition: CardSourceCondition,
                    rootCondition: RootCondition,
                    isInheritedEffect: true,
                    card: card,
                    condition: Condition,
                    setFixedCost: false));
            }

            return cardEffects;
        }
    }
}