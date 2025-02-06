using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX1
{
    public class EX1_016 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                CanAttackTargetDefendingPermanentClass canAttackTargetDefendingPermanentClass = new CanAttackTargetDefendingPermanentClass();
                canAttackTargetDefendingPermanentClass.SetUpICardEffect($"Can attack to unsuspended Digimon with no digivolution cards", CanUseCondition, card);
                canAttackTargetDefendingPermanentClass.SetUpCanAttackTargetDefendingPermanentClass(
                    attackerCondition: AttackerCondition,
                    defenderCondition: DefenderCondition,
                    cardEffectCondition: CardEffectCondition);
                cardEffects.Add(canAttackTargetDefendingPermanentClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool AttackerCondition(Permanent attacker)
                {
                    return attacker == card.PermanentOfThisCard();
                }

                bool DefenderCondition(Permanent defender)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(defender, card))
                    {
                        if (!defender.IsSuspended)
                        {
                            if (defender.HasNoDigivolutionCards)
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

            return cardEffects;
        }
    }
}