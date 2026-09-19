using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CanAttackTargetDefendingPermanentClass : ICardEffect, ICanAttackTargetDefendingPermanentEffect
{
    Func<Permanent, bool> AttackerCondition { get; set; }
    Func<Permanent, bool> DefenderCondition { get; set; }
    Func<ICardEffect, bool> CardEffectCondition { get; set; }
    public void SetUpCanAttackTargetDefendingPermanentClass(Func<Permanent, bool> attackerCondition, Func<Permanent, bool> defenderCondition, Func<ICardEffect, bool> cardEffectCondition)
    {
        this.AttackerCondition = attackerCondition;
        this.DefenderCondition = defenderCondition;
        this.CardEffectCondition = cardEffectCondition;
    }

    public bool CanAttackTargetDefendingPermanent(Permanent Attacker, Permanent Defender, ICardEffect cardEffect)
    {
        if (Attacker != null && Defender != null)
        {
            if (Attacker.TopCard != null && Defender.TopCard != null)
            {
                if (AttackerCondition != null && DefenderCondition != null && CardEffectCondition != null)
                {
                    if (AttackerCondition(Attacker) && DefenderCondition(Defender) && CardEffectCondition(cardEffect))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}