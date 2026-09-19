using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT14
{
    public class BT14_017 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                cardEffects.Add(CardEffectFactory.BlitzSelfEffect(isInheritedEffect: false, card: card, condition: null, isWhenDigivolving: true));
            }

            if (timing == EffectTiming.None)
            {
                CanNotPutFieldClass canNotPutFieldClass = new CanNotPutFieldClass();
                canNotPutFieldClass.SetUpICardEffect("Opponent can't play Digimon card with DP 6000 or less", CanUseCondition, card);
                canNotPutFieldClass.SetUpCanNotPutFieldClass(cardCondition: CardCondition, cardEffectCondition: CardEffectCondition);
                cardEffects.Add(canNotPutFieldClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.Enemy.MemoryForPlayer >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource.Owner == card.Owner.Enemy)
                    {
                        if (!CardEffectCommons.IsExistOnBreedingAreaDigimon(card))
                        {
                            if (cardSource.HasDP && cardSource.CardDP <= 6000)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CardEffectCondition(ICardEffect cardEffect)
                {
                    return true;
                }
            }

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.Enemy.MemoryForPlayer >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 4000, isInheritedEffect: false, card: card, condition: Condition));
            }

            return cardEffects;
        }
    }
}