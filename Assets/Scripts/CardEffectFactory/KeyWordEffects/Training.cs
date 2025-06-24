using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Trigger effect of [Training]
    public static ActivateClass TrainingEffect(CardSource card)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Training", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, DataBase.TrainingEffectDiscription());

        bool CanUseCondition(Hashtable hashtable)
        {
            return true;
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanActivateSuspendCostEffect(card, true);
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            Permanent thisPermanent = card.PermanentOfThisCard();

            yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(
                    new List<Permanent>() { thisPermanent },
                    CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

            if(card.Owner.LibraryCards.Count > 0)
                yield return ContinuousController.instance.StartCoroutine(thisPermanent.AddDigivolutionCardsBottom(
                            new List<CardSource> { card.Owner.LibraryCards[0] }, activateClass, isFacedown: true));
        }

        return activateClass;
    }
    #endregion
}