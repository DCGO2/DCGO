using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Add effects to 1 target permanent

    public static void AddEffectToPermanent(Permanent targetPermanent, EffectDuration effectDuration, CardSource card, ICardEffect cardEffect, EffectTiming timing)
    {
        Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffectByEffectTiming(timing: timing, cardEffect: cardEffect);

        switch (effectDuration)
        {
            case EffectDuration.UntilOpponentTurnEnd:
                if (IsOwnerPermanent(targetPermanent, card))
                {
                    targetPermanent.UntilOpponentTurnEndEffects.Add(getCardEffect);
                }
                else
                {
                    targetPermanent.UntilOwnerTurnEndEffects.Add(getCardEffect);
                }
                break;

            case EffectDuration.UntilOwnerTurnEnd:
                if (IsOwnerPermanent(targetPermanent, card))
                {
                    targetPermanent.UntilOwnerTurnEndEffects.Add(getCardEffect);
                }
                else
                {
                    targetPermanent.UntilOpponentTurnEndEffects.Add(getCardEffect);
                }
                break;

            case EffectDuration.UntilEachTurnEnd:
                targetPermanent.UntilEachTurnEndEffects.Add(getCardEffect);
                break;

            case EffectDuration.UntilEndAttack:
                targetPermanent.UntilEndAttackEffects.Add(getCardEffect);
                break;

            case EffectDuration.UntilNextUntap:
                targetPermanent.UntilNextUntapEffects.Add(getCardEffect);
                break;
        }
    }

    #endregion

    #region Add effects to 1 target player

    public static void AddEffectToPlayer(EffectDuration effectDuration, CardSource card, ICardEffect cardEffect, EffectTiming timing, Func<EffectTiming, ICardEffect> getCardEffect = null)
    {
        Player player = card.Owner;

        getCardEffect ??= GetCardEffectByEffectTiming(timing: timing, cardEffect: cardEffect);

        switch (effectDuration)
        {
            case EffectDuration.UntilOpponentTurnEnd:
                player.UntilOpponentTurnEndEffects.Add(getCardEffect);
                break;

            case EffectDuration.UntilOwnerTurnEnd:
                player.UntilOwnerTurnEndEffects.Add(getCardEffect);
                break;

            case EffectDuration.UntilEachTurnEnd:
                player.UntilEachTurnEndEffects.Add(getCardEffect);
                break;

            case EffectDuration.UntilEndBattle:
                player.UntilEndBattleEffects.Add(getCardEffect);
                break;

            case EffectDuration.UntilOwnerActivePhase:
                player.Enemy.UntilOwnerActivePhaseEffects.Add(getCardEffect);
                break;

            case EffectDuration.UntilCalculateFixedCost:
                player.UntilCalculateFixedCostEffect.Add(getCardEffect);
                break;
        }
    }

    #endregion
}