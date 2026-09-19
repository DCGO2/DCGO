using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT18
{
    public class BT18_059 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region All Turns
            if (timing == EffectTiming.None)
            {
                CannotAddMemoryClass cannotAddMemoryClass = new CannotAddMemoryClass();
                cannotAddMemoryClass.SetUpICardEffect("Opponent can't gain Memory", CanUseCondition, card);
                cannotAddMemoryClass.SetUpCannotAddMemoryClass(PlayerCondition: PlayerCondition, CardEffectCondition: CardEffectCondition);
                cardEffects.Add(cannotAddMemoryClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                bool PlayerCondition(Player player)
                {
                    if (player == card.Owner.Enemy)
                    {
                        return true;
                    }

                    return false;
                }

                bool CardEffectCondition(ICardEffect cardEffect)
                {
                    if (cardEffect != null)
                    {
                        if (cardEffect.EffectSourceCard != null)
                        {
                            if (!cardEffect.IsTamerEffect)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}