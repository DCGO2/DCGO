using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class TrainingClass
{
    public TrainingClass(ICardEffect activateClass)
    {
        _activateClass = activateClass;
    }

    ICardEffect _activateClass = null;

    public IEnumerator Training()
    {
        if (_activateClass == null) yield break;
        if (_activateClass.EffectSourceCard == null) yield break;

        CardSource card = _activateClass.EffectSourceCard;
        Permanent thisPermanent = card.PermanentOfThisCard();

        if (thisPermanent.IsSuspended || !thisPermanent.CanSuspend) yield break;

        yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(
                    new List<Permanent>() { thisPermanent },
                    CardEffectCommons.CardEffectHashtable(_activateClass)).Tap());

        yield return ContinuousController.instance.StartCoroutine(thisPermanent.AddDigivolutionCardsBottom(
                        new List<CardSource> { card.Owner.LibraryCards[0] }, _activateClass, isFacedown: true));
    }
}