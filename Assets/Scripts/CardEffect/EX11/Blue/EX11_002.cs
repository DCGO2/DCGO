using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Hiyarimon
namespace DCGO.CardEffects.EX11
{
    public class EX11_002 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                CanAttackTargetDefendingPermanentClass canAttackTargetDefendingPermanentClass = new CanAttackTargetDefendingPermanentClass();
                canAttackTargetDefendingPermanentClass.SetUpICardEffect($"Can attack to unsuspended Digimon", CanUseCondition, card);
                canAttackTargetDefendingPermanentClass.SetUpCanAttackTargetDefendingPermanentClass(attackerCondition: AttackerCondition, defenderCondition: DefenderCondition, cardEffectCondition: CardEffectCondition);
                canAttackTargetDefendingPermanentClass.SetIsInheritedEffect(true);

                cardEffects.Add(canAttackTargetDefendingPermanentClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && card.Owner.Enemy.GetBattleAreaDigimons().Count((permanent) => permanent.DigivolutionCards.Count >= 1) == 0;
                }

                bool AttackerCondition(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                bool DefenderCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && !permanent.IsSuspended;
                }

                bool CardEffectCondition(ICardEffect cardEffect)
                {
                    return true;
                }
            }
            
            return cardEffects;
        }
    }
}
