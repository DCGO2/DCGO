using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can activate [Save]
    public static bool CanActivateSave(Hashtable hashtable, Func<Permanent, bool> CanSelectPermanentCondition)
    {
        if (IsTopCardInTrashOnDeletion(hashtable))
        {
            if (HasMatchConditionPermanent(CanSelectPermanentCondition))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Effect process of [Save]
    public static IEnumerator SaveProcess(Hashtable hashtable, ICardEffect activateClass, CardSource card, Func<Permanent, bool> CanSelectPermanentCondition)
    {
        if (!CanActivateSave(hashtable, CanSelectPermanentCondition)) yield break;

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

        selectPermanentEffect.SetUpCustomMessage(customMessageArray: customPermanentMessageArrayTemplate(customText: "that will get a digivolution card", maxCount: 1, CanSelectDigimon: false, CanSelectTamer: true));

        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

        IEnumerator SelectPermanentCoroutine(Permanent permanent)
        {
            Permanent selectedPermanent = permanent;

            if (selectedPermanent != null)
            {
                if (card.Owner.TrashHandCard.gameObject.activeSelf)
                {
                    card.Owner.TrashHandCard.gameObject.SetActive(false);
                }

                yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { card }, activateClass));
            }
        }
    }
    #endregion

}