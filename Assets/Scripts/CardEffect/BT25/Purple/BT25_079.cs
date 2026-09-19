using System.Collections;
using System.Collections.Generic;

// Hyemon
namespace DCGO.CardEffects.BT25
{
    public class BT25_079 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Can't Gain Memory
            if (timing == EffectTiming.None)
            {
                CannotAddMemoryClass cannotAddMemoryClass = new CannotAddMemoryClass();
                cannotAddMemoryClass.SetUpICardEffect("Players can't gain memory other than by Tamer effects", CanUseCondition, card);
                cannotAddMemoryClass.SetUpCannotAddMemoryClass(PlayerCondition: PlayerCondition, CardEffectCondition: CardEffectCondition);
                cardEffects.Add(cannotAddMemoryClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool PlayerCondition(Player player)
                {
                    return player == true;
                }

                bool CardEffectCondition(ICardEffect cardEffect)
                {
                    return cardEffect != null
                        && cardEffect.EffectSourceCard != null
                        && !cardEffect.IsTamerEffect;
                }
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.RetaliationSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
