using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX8
{
    public class EX8_030 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Alternate Digivolution Conditions
            
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 2)
                        return targetPermanent.TopCard.EqualsTraits("NSo");

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 0,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }
            
            #endregion
            
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