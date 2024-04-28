using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
public partial class CardEffectCommons
{
    #region Can trigger [Pierce]
    public static bool CanTriggerPierce(Hashtable hashtable, Permanent piercePermanent)
    {
        if (piercePermanent == null) return false;
        if (piercePermanent.TopCard == null) return false;

        return CanTriggerWhenDeleteOpponentDigimonByBattle(hashtable: hashtable, winnerCondition: (permanent) => permanent.cardSources.Contains(piercePermanent.TopCard), loserCondition: (permanent) => IsOpponentPermanent(permanent, piercePermanent.TopCard), isOnlyWinnerSurvive: true);
    }
    #endregion

    #region Can activate [Pierce]
    public static bool CanActivatePierce(Permanent permanent)
    {
        if (permanent == null) return false;
        if (permanent.TopCard == null) return false;
        if (GManager.instance.attackProcess.AttackingPermanent == null) return false;
        if (GManager.instance.attackProcess.AttackingPermanent.TopCard == null) return false;

        if (IsPermanentExistsOnBattleArea(permanent))
        {
            if (permanent.TopCard.Owner.Enemy.SecurityCards.Count >= 1)
            {
                if (GManager.instance.attackProcess.AttackingPermanent.cardSources.Contains(permanent.TopCard))
                {
                    if (!GManager.instance.attackProcess.DoSecurityCheck)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    #endregion

    #region Effect process of [Pierce]
    public static IEnumerator PierceProcess()
    {
        GManager.instance.attackProcess.DoSecurityCheck = true;

        yield return null;
    }
    #endregion

    #region Target 1 Digimon gains [Pierce]
    public static IEnumerator GainPierce(Permanent targetPermanent, EffectDuration effectDuration, ICardEffect activateClass)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        bool CanUseCondition()
        {
            if (IsPermanentExistsOnBattleArea(targetPermanent))
            {
                if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
                {
                    return true;
                }
            }

            return false;
        }

        ActivateClass pierce = CardEffectFactory.PierceEffect(targetPermanent: targetPermanent, isInheritedEffect: false, condition: CanUseCondition, rootCardEffect: activateClass, targetPermanent.TopCard);

        AddEffectToPermanent(targetPermanent: targetPermanent, effectDuration: effectDuration, card: card, cardEffect: pierce, timing: EffectTiming.OnDetermineDoSecurityCheck);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
        }
    }
    #endregion
}