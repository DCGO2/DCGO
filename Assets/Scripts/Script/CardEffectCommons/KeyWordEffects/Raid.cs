using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
public partial class CardEffectCommons
{
    #region Can activate [Raid]
    public static bool CanActivateRaid(Permanent targetPermanent)
    {
        if (targetPermanent == null) return false;
        if (targetPermanent.TopCard == null) return false;

        bool CanSelectPermanentCondition(Permanent permanent)
        {
            bool PermanentCondition(Permanent permanent1) => permanent1 != GManager.instance.attackProcess.DefendingPermanent && !permanent1.IsSuspended;

            return IsMaxDP(permanent, targetPermanent.TopCard.Owner.Enemy, PermanentCondition);
        }

        if (IsPermanentExistsOnBattleArea(targetPermanent))
        {
            if (GManager.instance.attackProcess.IsAttacking)
            {
                if (GManager.instance.attackProcess.AttackingPermanent == targetPermanent)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    #endregion

    #region Effect process of [Raid]
    public static IEnumerator RaidProcess(Permanent targetPermanent, ICardEffect activateClass)
    {
        if (targetPermanent == null) yield break;
        if (targetPermanent.TopCard == null) yield break;

        bool CanSelectPermanentCondition(Permanent permanent)
        {
            bool PermanentCondition(Permanent permanent1) => !permanent1.IsSuspended;

            return IsMaxDP(permanent, targetPermanent.TopCard.Owner.Enemy, PermanentCondition);
        }

        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

        selectPermanentEffect.SetUp(
            selectPlayer: targetPermanent.TopCard.Owner,
            canTargetCondition: CanSelectPermanentCondition,
            canTargetCondition_ByPreSelecetedList: null,
            canEndSelectCondition: null,
            maxCount: maxCount,
            canNoSelect: true,
            canEndNotMax: false,
            selectPermanentCoroutine: SelectPermanentCoroutine,
            afterSelectPermanentCoroutine: null,
            mode: SelectPermanentEffect.Mode.Custom,
            cardEffect: activateClass);

        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that this Digimon attacks to.", "The opponent is selecting 1 Digimon that this Digimon attacks to.");

        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

        IEnumerator SelectPermanentCoroutine(Permanent permanent)
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SwitchDefender(activateClass, false, permanent));
        }
    }
    #endregion

    #region Target 1 Digimon gains [Raid]
    public static IEnumerator GainRaid(Permanent targetPermanent, EffectDuration effectDuration, ICardEffect activateClass)
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

        ActivateClass raid = CardEffectFactory.RaidEffect(
            targetPermanent: targetPermanent,
            isInheritedEffect: false,
            condition: CanUseCondition,
            rootCardEffect: activateClass,
            targetPermanent.TopCard);

        AddEffectToPermanent(
            targetPermanent: targetPermanent,
            effectDuration: effectDuration,
            card: card,
            cardEffect: raid,
            timing: EffectTiming.OnAllyAttack);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
        }
    }
    #endregion
}