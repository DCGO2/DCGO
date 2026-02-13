using System;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX2
{
    public class EX2_036 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool DefenderCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card);
                }

                bool Condition()
                {
                    return CardEffectCommons.IsOwnerTurn(card);
                }

                cardEffects.Add(CardEffectFactory.CanNotAttackSelfStaticEffect(defenderCondition: DefenderCondition, isInheritedEffect: false, card: card, condition: Condition, effectName: "Can't Attack to Digimon"));
            }

            if (timing == EffectTiming.None)
            {
                int count()
                {
                    return card.Owner.TrashCards.Count((cardSource) => cardSource.CardTraits.Contains("Cyborg") || cardSource.CardTraits.Contains("Machine"));
                }

                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (count() >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect<Func<int>>(changeValue: () => 1000 * count(), isInheritedEffect: false, card: card, condition: Condition));
            }

            return cardEffects;
        }
    }
}