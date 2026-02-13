using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can activate [Alliance]
    public static bool CanActivateAlliance(Hashtable hashtable, CardSource card)
    {
        bool CanSelectPermanentCondition(Permanent permanent)
        {
            if (IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
            {
                if (permanent != card.PermanentOfThisCard())
                {
                    if (CanActivateSuspendCostEffect(permanent.TopCard))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        if (CardEffectCommons.IsExistOnBattleArea(card))
        {
            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Effect process of [Alliance]
    public static IEnumerator AllianceProcess(Hashtable hashtable, ICardEffect activateClass, CardSource card)
    {
        bool CanSelectPermanentCondition(Permanent permanent)
        {
            if (IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
            {
                if (permanent != card.PermanentOfThisCard())
                {
                    if (CanActivateSuspendCostEffect(permanent.TopCard))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
        {
            Permanent selectedPermanent = null;

            int maxCount = Math.Min(1, MatchConditionPermanentCount(CanSelectPermanentCondition));

            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

            selectPermanentEffect.SetUp(
                selectPlayer: card.Owner,
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

            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to suspend.", "The opponent is selecting 1 Digimon to suspend.");

            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

            IEnumerator SelectPermanentCoroutine(Permanent permanent)
            {
                selectedPermanent = permanent;

                yield return null;
            }

            if (selectedPermanent != null)
            {
                if (selectedPermanent.TopCard != null)
                {
                    if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                    {
                        if (CanActivateSuspendCostEffect(selectedPermanent.TopCard))
                        {
                            Permanent tapPermanent = selectedPermanent;

                            yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(
                                new List<Permanent>() { tapPermanent },
                                CardEffectHashtable(activateClass)).Tap());

                            if (tapPermanent.TopCard != null)
                            {
                                if (tapPermanent.IsSuspended)
                                {
                                    if (IsExistOnBattleArea(card))
                                    {
                                        int plusDP = tapPermanent.DP;

                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                            targetPermanent: card.PermanentOfThisCard(),
                                            changeValue: plusDP,
                                            effectDuration: EffectDuration.UntilEndAttack,
                                            activateClass: activateClass));

                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(
                                            targetPermanent: card.PermanentOfThisCard(),
                                            changeValue: 1,
                                            effectDuration: EffectDuration.UntilEndAttack,
                                            activateClass: activateClass));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region Target 1 Digimon gains [Alliance]
    public static IEnumerator GainAlliance(Permanent targetPermanent, EffectDuration effectDuration, ICardEffect activateClass)
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

        ActivateClass retaliation = CardEffectFactory.AllianceEffect(
            targetPermanent: targetPermanent,
            isInheritedEffect: false,
            condition: CanUseCondition,
            rootCardEffect: activateClass,
            card: targetPermanent.TopCard);

        AddEffectToPermanent(
            targetPermanent: targetPermanent,
            effectDuration: effectDuration,
            card: card,
            cardEffect: retaliation,
            timing: EffectTiming.OnAllyAttack);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
        }
    }
    #endregion

    #region Player gains effect to have Digimon gains [Alliance]
    public static IEnumerator GainAlliancePlayerEffect(Func<Permanent, bool> permanentCondition, EffectDuration effectDuration, ICardEffect activateClass)
    {
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        bool PermanentCondition(Permanent permanent)
        {
            if (IsPermanentExistsOnBattleArea(permanent))
            {
                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                {
                    if (permanentCondition == null || permanentCondition(permanent))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool CanUseCondition()
        {
            return true;
        }

        AllianceClass alliance = CardEffectFactory.AllianceStaticEffect(permanentCondition: PermanentCondition, isInheritedEffect: false, card: card, condition: CanUseCondition);

        AddEffectToPlayer(effectDuration: effectDuration, card: card, cardEffect: alliance, timing: EffectTiming.None);

        foreach (Permanent permanent in GManager.instance.turnStateMachine.gameContext.PermanentsForTurnPlayer)
        {
            if (PermanentCondition(permanent))
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(permanent));
            }
        }
    }
    #endregion
}