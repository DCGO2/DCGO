using System.Collections;
using System.Collections.Generic;

// Bokomon
namespace DCGO.CardEffects.EX13
{
    public class EX13_050 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region All Turns
            if (timing == EffectTiming.None)
            {
                CannotAddMemoryClass cannotAddMemoryClass = new CannotAddMemoryClass();
                cannotAddMemoryClass.SetUpICardEffect("Players can't gain memory other than by Tamer effects", CanUseCondition, card);
                cannotAddMemoryClass.SetUpCannotAddMemoryClass(PlayerCondition: PlayerCondition, CardEffectCondition: CardEffectCondition);
                cardEffects.Add(cannotAddMemoryClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card);

                bool PlayerCondition(Player player)
                    => true;

                bool CardEffectCondition(ICardEffect cardEffect)
                    => cardEffect != null
                        && cardEffect.EffectSourceCard != null
                        && !cardEffect.IsTamerEffect;
            }
            #endregion

            #region Blocker - ESS
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
