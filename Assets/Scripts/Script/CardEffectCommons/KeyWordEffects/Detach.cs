using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
public partial class CardEffectCommons
{
    #region Can trigger [Detach]
    public static bool CanTriggerDetach(Hashtable hashtable, Permanent targetPermanent)
    {
        if (IsPermanentExistsOnBattleArea(targetPermanent))
        {
            if (CanTriggerWhenPermanentRemoveField(hashtable, permanent => permanent == targetPermanent))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Can activate [Detach]
    public static bool CanActivateDetach(Permanent targetPermanent, Func<CardSource, bool> cardCondition)
    {
        if (IsPermanentExistsOnBattleArea(targetPermanent))
        {
            if (targetPermanent.LinkedCards.Any(cardCondition))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Effect process of [Detach]
    public static IEnumerator DetachProcess(Permanent targetPermanent, Func<CardSource, bool> cardCondition, string condition, ICardEffect activateClass)
    {
        if (targetPermanent.TopCard == null) yield break;
        
        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

        selectCardEffect.SetUp(
            canTargetCondition: cardCondition,
            canTargetCondition_ByPreSelecetedList: null,
            canEndSelectCondition: null,
            canNoSelect: () => true,
            selectCardCoroutine: SelectCardCoroutine,
            afterSelectCardCoroutine: null,
            message: $"Select 1 {condition} link card to trash.",
            maxCount: 1,
            canEndNotMax: false,
            isShowOpponent: true,
            mode: SelectCardEffect.Mode.Custom,
            root: SelectCardEffect.Root.LinkedCards,
            customRootCardList: targetPermanent.LinkedCards,
            canLookReverseCard: true,
            selectPlayer: targetPermanent.TopCard.Owner,
            cardEffect: activateClass);

        selectCardEffect.SetUpCustomMessage($"Select 1 {condition} link card to trash.", $"The opponent is selecting 1 {condition} link card to trash.");

        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

        IEnumerator SelectCardCoroutine(CardSource cardSource)
        {
            yield return ContinuousController.instance.StartCoroutine(TrashLinkCardsAndProcessAccordingToResult(targetPermanent, new List<CardSource>() { cardSource }, activateClass, SuccessProcess, null));
        }

        IEnumerator SuccessProcess(List<CardSource> cardSources)
        {
            if (cardSources.Count > 0)
            {
                targetPermanent.willBeRemoveField = false;

                targetPermanent.HideHandBounceEffect();
                targetPermanent.HideDeckBounceEffect();
                targetPermanent.HideWillRemoveFieldEffect();
                targetPermanent.HideDeleteEffect();
            }

            yield return null;
        }
    }
    #endregion
}