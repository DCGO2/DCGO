using System;
using System.Collections;
using System.Collections.Generic;

public partial class CardEffectCommons
{
    /// <summary>
    /// "By Trashing X [[Condition]] cards, Draw Y and/or gain Z memory"
    /// </summary>
    /// <param name="card">Card with the effect</param>
    /// <param name="canSelectCardCondition">Condition for which cards may be trashed</param>
    /// <param name="cardEffect">ActivateClass of enclosing effect</param>
    /// <param name="drawCount"># of cards to draw, 0 for gain memory only</param>
    /// <param name="memoryGain"># of memory to gain, 0 for draw only</param>
    /// <param name="customMessage">Message to display to user when they are selecting a card to trash. Example: "Select 1 [Condition] card to trash"</param>
    /// <param name="trashCount">Number of cards to trash, in case of a trash 2 to draw 2 or similar effect. Left with default 1</param>
    /// <param name="optional">Whether the trash is optional. It should be for all "By effects", included in case of "trash a card, if you do draw 1 and gain 1 memory"</param>
    /// <param name="successProcess">Function to use when effect is used. Can set a flag when the operation is carried out.</param>
    /// <param name="failureProcess">Function to use when cards are not chosen. Can be used to RemoveUse() on the cardEffect</param>
    public static IEnumerator TrashToDrawAndGainMemory(
        CardSource card,
        Func<CardSource, bool> canSelectCardCondition,
        ICardEffect cardEffect,
        string customMessage = null,
        int trashCount = 1,
        int drawCount = 1,
        int memoryGain = 1,
        bool optional = true,
        IEnumerator successProcess = null,
        IEnumerator failureProcess = null)
    {
        if (trashCount > 0 && card.Owner.HandCards.Count >= trashCount)
        {
            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

            selectHandEffect.SetUp(
                selectPlayer: card.Owner,
                canTargetCondition: canSelectCardCondition,
                canTargetCondition_ByPreSelecetedList: null,
                canEndSelectCondition: null,
                maxCount: trashCount,
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
                if (cardSources.Count >= trashCount)
                {
                    if (drawCount > 0) yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, drawCount, cardEffect).Draw());
                    if (memoryGain > 0) yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(memoryGain, cardEffect));
                    if (successProcess != null) yield return ContinuousController.instance.StartCoroutine(successProcess);
                }
                else if (failureProcess != null) 
                {
                    yield return ContinuousController.instance.StartCoroutine(failureProcess);
                }
            }
        }
        else if (failureProcess != null) 
        {
            yield return ContinuousController.instance.StartCoroutine(failureProcess);
        }
    }
}