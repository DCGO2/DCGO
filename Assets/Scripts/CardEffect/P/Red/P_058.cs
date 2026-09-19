using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;


public class P_058 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            CanAttackTargetDefendingPermanentClass canAttackTargetDefendingPermanentClass = new CanAttackTargetDefendingPermanentClass();
            canAttackTargetDefendingPermanentClass.SetUpICardEffect($"Can attack to unsuspended Digimon", CanUseCondition, card);
            canAttackTargetDefendingPermanentClass.SetUpCanAttackTargetDefendingPermanentClass(attackerCondition: AttackerCondition, defenderCondition: DefenderCondition, cardEffectCondition: CardEffectCondition);

            cardEffects.Add(canAttackTargetDefendingPermanentClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                if (isExistOnField(card))
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.TopCard.CardColors.Contains(CardColor.Red) && permanent.IsTamer))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }



            bool AttackerCondition(Permanent permanent)
            {
                return permanent == card.PermanentOfThisCard();
            }

            bool DefenderCondition(Permanent permanent)
            {
                if (permanent != null)
                {
                    if (permanent.TopCard != null)
                    {
                        if (permanent.TopCard.Owner == card.Owner.Enemy)
                        {
                            if (permanent.IsDigimon)
                            {
                                if (!permanent.IsSuspended)
                                {
                                    if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                    {
                                        return true;
                                    }
                                }
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
