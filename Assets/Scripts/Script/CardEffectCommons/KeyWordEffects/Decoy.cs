using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can activate [Decoy]
    public static bool CanActivateDecoy(Permanent permanent, ICardEffect activateClass)
    {
        if (IsPermanentExistsOnBattleArea(permanent))
        {
            if (permanent.CanBeDestroyedBySkill(activateClass))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Effect process of [Decoy]
    public static IEnumerator DecoyProcess(ICardEffect activateClass, Permanent permanent, Func<Permanent, bool> CanSelectPermanentCondition)
    {
        if (permanent == null) yield break;
        if (permanent.TopCard == null) yield break;

        Player owner = permanent.TopCard.Owner;

        yield return ContinuousController.instance.StartCoroutine(DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { permanent }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

        IEnumerator SuccessProcess()
        {
            if (HasMatchConditionPermanent(CanSelectPermanentCondition))
            {
                int maxCount = Math.Min(1, MatchConditionPermanentCount(CanSelectPermanentCondition));

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: owner,
                    canTargetCondition: CanSelectPermanentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to prevent deletion.", "The opponent is selecting 1 Digimon to prevent deletion.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    permanent.willBeRemoveField = false;

                    permanent.HideDeleteEffect();

                    yield return null;
                }
            }
        }
    }
    #endregion
}