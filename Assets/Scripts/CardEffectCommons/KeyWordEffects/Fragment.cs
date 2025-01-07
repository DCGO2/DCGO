using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can activate [Fragment]
    public static bool CanActivateFragment(Permanent permanent, int trashValue, ICardEffect activateClass)
    {
        Debug.Log($"FRAGMENT CAN ACTIVATE: {IsPermanentExistsOnBattleArea(permanent)}");
        if (IsPermanentExistsOnBattleArea(permanent))
        {
            Debug.Log($"FRAGMENT CAN ACTIVATE: {permanent.CanBeDestroyedBySkill(activateClass)}");
            if (permanent.CanBeDestroyedBySkill(activateClass))
            {
                Debug.Log($"FRAGMENT CAN ACTIVATE: {permanent.DigivolutionCards.Count} <= {trashValue}");
                return (permanent.DigivolutionCards.Count <= trashValue);
            }
        }

        return false;
    }
    #endregion

    #region Effect process of [Fragment]
    public static IEnumerator FragmentProcess(ICardEffect activateClass, Permanent permanent, int trashValue)
    {
        if (permanent == null) yield break;
        if (permanent.TopCard == null) yield break;
        if (permanent.DigivolutionCards.Count < trashValue) yield break;

        bool cardsTrashed = false;

        List<CardSource> selectedCards = new List<CardSource>();

        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

        selectCardEffect.SetUp(
                    canTargetCondition: (CardSource) => true,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    canNoSelect: () => false,
                    selectCardCoroutine: SelectCardCoroutine,
                    afterSelectCardCoroutine: null,
                    message: "Select digivolution cards to trash.",
                    maxCount: 3,
                    canEndNotMax: false,
                    isShowOpponent: true,
                    mode: SelectCardEffect.Mode.Custom,
                    root: SelectCardEffect.Root.Custom,
                    customRootCardList: permanent.DigivolutionCards,
                    canLookReverseCard: true,
                    selectPlayer: activateClass.EffectSourceCard.Owner,
                    cardEffect: activateClass);

        selectCardEffect.SetUpCustomMessage("Select digivolution cards to trash.", "The opponent is selecting digivolution cards to trash.");

        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

        IEnumerator SelectCardCoroutine(CardSource cardSource)
        {
            selectedCards.Add(cardSource);

            yield return null;
        }

        if (selectedCards.Count == trashValue)
        {
            yield return ContinuousController.instance.StartCoroutine(new ITrashDigivolutionCards(
                permanent,
                selectedCards,
                activateClass).TrashDigivolutionCards());

            cardsTrashed = true;
        }

        if (cardsTrashed)
        {
            permanent.willBeRemoveField = false;

            permanent.HideDeleteEffect();
        }
    }
    #endregion
}