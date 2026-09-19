using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can activate [Scapegoat]
    public static bool CanActivateScapegoat(Permanent permanent, Func<Permanent, bool> permanentCondition)
    {
        if (IsPermanentExistsOnBattleArea(permanent))
        {
            if (HasMatchConditionPermanent(permanentCondition))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Effect process of [Scapegoat]
    public static IEnumerator ScapegoatProcess(ICardEffect activateClass, Permanent permanent, Func<Permanent, bool> CanSelectPermanentCondition)
    {
        if (permanent == null) yield break;
        if (permanent.TopCard == null) yield break;

        Player owner = permanent.TopCard.Owner;

        if (HasMatchConditionPermanent(CanSelectPermanentCondition))
        {
            int maxCount = 1;

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

            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

            IEnumerator SelectPermanentCoroutine(Permanent _permanent)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { _permanent }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                IEnumerator SuccessProcess()
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