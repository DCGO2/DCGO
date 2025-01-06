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
        if (IsPermanentExistsOnBattleArea(permanent))
        {
            if (permanent.CanBeDestroyedBySkill(activateClass))
            {
                return (permanent.DigivolutionCards.Count < trashValue);
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

        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
            permanentCondition: (target => target == permanent),
            cardCondition: (CardSource) =>  true,
            maxCount: 3,
            canNoTrash: true,
            isFromOnly1Permanent: false,
            activateClass: activateClass,
            afterSelectionCoroutine: AfterTrashedCards
        ));

        IEnumerator AfterTrashedCards(Permanent permanent, List<CardSource> cards)
        {
            if (cards.Count == trashValue)
                cardsTrashed = true;

            yield return null;
        }

        if (cardsTrashed)
        {
            permanent.willBeRemoveField = false;

            permanent.HideDeleteEffect();
        }
    }
    #endregion
}