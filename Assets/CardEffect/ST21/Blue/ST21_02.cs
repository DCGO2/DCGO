using System.Collections;
using System.Collections.Generic;

//ST21 Gomamon
namespace DCGO.CardEffects.ST21
{
    public class ST21_02 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.Level == 2 && permanent.TopCard.HasAdventureTraits;
                }
                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 0, false, card, null));
            }
            #endregion

            #region All turns memory block
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