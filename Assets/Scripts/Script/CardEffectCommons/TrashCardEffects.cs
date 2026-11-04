using System;
using System.Collections;
using System.Collections.Generic;

public partial class CardEffectCommons
{
    public static IEnumerator Trash1ToDraw1Gain1Memory(
        CardSource card,
        Func<CardSource, bool> canSelectCardCondition,
        ICardEffect cardEffect,
        string customMessage = null,
        bool optional = true,
        IEnumerator successProcess = null,
        IEnumerator failureProcess = null)
    {
        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

        selectHandEffect.SetUp(
            selectPlayer: card.Owner,
            canTargetCondition: canSelectCardCondition,
            canTargetCondition_ByPreSelecetedList: null,
            canEndSelectCondition: null,
            maxCount: 1,
            canNoSelect: optional,
            canEndNotMax: false,
            isShowOpponent: true,
            selectCardCoroutine: null,
            afterSelectCardCoroutine: AfterSelectCardCoroutine,
            mode: SelectHandEffect.Mode.Discard,
            cardEffect: cardEffect);

        if (customMessage != null)
        {
            selectHandEffect.SetUpCustomMessage(customMessage, "The opponent is selecting 1 card to trash.");
        }

        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
        {
            if (cardSources.Count >= 1)
            {
                yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, cardEffect).Draw());
                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, cardEffect));
                yield return ContinuousController.instance.StartCoroutine(successProcess);
            }
            else
            {
                yield return ContinuousController.instance.StartCoroutine(failureProcess);
            }
        }
    }
}