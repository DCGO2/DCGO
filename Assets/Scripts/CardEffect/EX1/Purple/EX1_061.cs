using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX1
{
    public class EX1_061 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsOwnerTurn(card) && CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent == card.PermanentOfThisCard();
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.ContainsCardName("Myotismon") && cardSource.Owner.HandCards.Contains(cardSource);
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return root == SelectCardEffect.Root.Hand;
                }

                cardEffects.Add(CardEffectFactory.ChangeDigivolutionCostStaticEffect(
                    changeValue: -1,
                    permanentCondition: PermanentCondition,
                    cardCondition: CardSourceCondition,
                    rootCondition: RootCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition,
                    setFixedCost: false));
            }

            if (timing == EffectTiming.None)
            {
                CanAttackTargetDefendingPermanentClass canAttackTargetDefendingPermanentClass = new CanAttackTargetDefendingPermanentClass();
                canAttackTargetDefendingPermanentClass.SetUpICardEffect($"Your Digimon can attack to unsuspended Digimon", CanUseCondition, card);
                canAttackTargetDefendingPermanentClass.SetUpCanAttackTargetDefendingPermanentClass(
                    attackerCondition: AttackerCondition,
                    defenderCondition: DefenderCondition,
                    cardEffectCondition: CardEffectCondition);
                canAttackTargetDefendingPermanentClass.SetIsInheritedEffect(true);
                cardEffects.Add(canAttackTargetDefendingPermanentClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (card.PermanentOfThisCard().TopCard.ContainsCardName("Myotismon"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool AttackerCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.HasRetaliation)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool DefenderCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (!permanent.IsSuspended)
                        {
                            if (permanent.Level <= 4)
                            {
                                if (permanent.TopCard.HasLevel)
                                {
                                    return true;
                                }
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