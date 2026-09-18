using System;
using System.Collections;
using System.Collections.Generic;

public partial class CardEffectFactory
{
    /// <summary>
    /// Copies every effect from the digivolution cards to the effects returned for the card with the copy effect
    /// </summary>
    public static void CopyDigivolutionCardEffects(
        ref List<ICardEffect> cardEffects, 
        EffectTiming timing,
        CardSource card,
        bool isInheritedEffect = false,
        bool isLinkedEffect = false,
        Func<List<CardSource>, List<CardSource>> targetSources = null,
        Func<Hashtable, bool> canUseCondition = null,
        Func<Permanent, bool> permanentCondition = null,
        Func<CardSource, bool> cardSourceCondition = null,
        Func<CardSource, bool> cardCondition = null,
        Func<ICardEffect, bool> effectCondition = null,
        bool isSuccession = false
        )
    {
        Permanent thisPermanent = card.PermanentOfThisCard();

        if (targetSources == null)
        {
            if (thisPermanent == null || thisPermanent.DigivolutionCards == null)
                return;
            targetSources = cardSources => cardSources;
        }

        List<CardSource> validSources(List<CardSource> availableSources) => availableSources.Filter(
            cardSource => cardCondition == null || cardCondition(cardSource)
        );

        foreach (CardSource cardSource in validSources(targetSources(thisPermanent.DigivolutionCards)))
        {
            List<ICardEffect> toCopyEffects = cardSource.cEntity_EffectController.GetCardEffects_ExceptAddedEffects(timing, cardSource);

            toCopyEffects.ForEach(eff =>
                {
                    eff.SetOriginalEffectSourceCard(cardSource);
                }
            );

            toCopyEffects = toCopyEffects.Filter(
                cardEffect => effectCondition == null || effectCondition(cardEffect)
            );
            
            foreach (ICardEffect cardEffect in toCopyEffects)
            {
                if (cardEffect.IsInheritedEffect || cardEffect.IsLinkedEffect)
                {
                    continue;
                }

                if (cardEffect is ActivateClass activateClass)
                {
                    // Build a brand-new ActivateClass rather than mutating/reusing the source
                    // card's own instance. The source card's copy needs its own independent
                    // EffectSourceCard/HashString so per-turn-use tracking (ICardEffect.IsSameEffect,
                    // which short-circuits on reference equality) doesn't treat "the original card
                    // already used this ability this turn" as also covering "the Digimon that just
                    // gained this ability via Succession/copy already used it" -- per game rules,
                    // gaining another card's effects this way grants an independently-tracked copy,
                    // not a shared use-count with the original (real bug: a [Once Per Turn] When
                    // Digivolving effect used earlier the same turn on the source card silently
                    // couldn't trigger again when copied onto the new top card via Succession, even
                    // though it's a fresh instance from the new card's perspective).

                    List<CardSource> ValidCardSources = null;

                    bool ValidCardSourceAtTrigger()
                    {
                        ValidCardSources = validSources(targetSources(thisPermanent.DigivolutionCards));
                        return ValidCardSources.Contains(cardSource);
                    }

                    bool ValidCardSourceAtActivate()
                    {
                        if (thisPermanent != null)
                        {
                            ValidCardSources = validSources(targetSources(thisPermanent.DigivolutionCards));
                        }
                        return ValidCardSources.Contains(cardSource);
                    }

                    var originalUseCondition = activateClass.CanUseCondition;
                    var originalActivateCondition = activateClass.CanActivateCondition;

                    ActivateClass copiedActivateClass = new ActivateClass();

                    copiedActivateClass.SetUpICardEffect(
                        activateClass.EffectName,
                        hashtable => ValidCardSourceAtTrigger()
                            && (originalUseCondition is null || originalUseCondition(hashtable)),
                        card);

                    // activateClass's own coroutine body may self-reference activateClass to
                    // adjust its own OPT usage mid-effect (e.g. "if (!isUsed) activateClass.
                    // RemoveUse();" -- real precedent: BT26_016 Chronomon: Holy Mode). Since
                    // that closure still targets activateClass (the source), redirect
                    // RemoveUse()/AddUse() to affect copiedActivateClass instead for the
                    // duration of this one call, so the source card's own OPT tracking isn't
                    // touched by a copy's activation. Push/pop (not set/clear) and a finally
                    // block: activateClass may already be mid-activation for a different copy
                    // (e.g. awaiting player input) when this one starts, and the underlying
                    // coroutine could throw or be stopped externally -- both must not leave a
                    // stale redirect in place for the source's own later activations.
                    IEnumerator ActivateWithRedirectedUseTracking(Hashtable hashtable)
                    {
                        activateClass.SetIsDigimonEffect(true);//Copy effects were for some reason refusing to act as Digimon effects. Explicitly setting here to bypass this being unset

                        activateClass.PushUseTrackingRedirectTarget(copiedActivateClass);
                        try
                        {
                            yield return ContinuousController.instance.StartCoroutine(activateClass.Activate(hashtable));
                        }
                        finally
                        {
                            activateClass.PopUseTrackingRedirectTarget();
                        }
                    }

                    copiedActivateClass.SetUpActivateClass(
                        hashtable => ValidCardSourceAtActivate()
                            && (originalActivateCondition is null || originalActivateCondition(hashtable)),
                        ActivateWithRedirectedUseTracking,
                        activateClass.MaxCountPerTurn,
                        activateClass.IsOptional,
                        activateClass.EffectDescription);

                    copiedActivateClass.SetOriginalEffectSourceCard(cardSource);
                    copiedActivateClass.SetHashString(GenerateHashString(card, cardSource, activateClass.HashString, isInheritedEffect, isLinkedEffect));
                    copiedActivateClass.SetIsInheritedEffect(isInheritedEffect);
                    copiedActivateClass.SetIsLinkedEffect(isLinkedEffect);

                    cardEffects.Add(copiedActivateClass);
                    cardEffects.Add(PermanentEffectFactory.AddDetailClass(
                        thisPermanent,
                        copiedActivateClass.EffectDescription,
                        true,
                        copiedActivateClass));
                }
                else if (!isSuccession || cardEffect.EffectName != "Succession") // Succession can never copy another succession skill
                {
                    cardEffects.Add(cardEffect);
                }
            }
            // If succession, break loop after first valid card to only copy topmost.
            // break instead of return in case we ever need to perform more actions before returning
            if (isSuccession) break; 
        }
    }

    private static string GenerateHashString(CardSource card, CardSource cardSource, string source, bool isInherited, bool isLinked)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(card is not null ? card.GetHashCode() : 0 );
        sb.Append($"//copy//{cardSource.GetHashCode()}//effect");
        sb.Append(source is not null && !source.Equals(string.Empty) ? $"//{source}" : "");
        sb.Append(isInherited ? "//inherited" : "");
        sb.Append(isLinked ? "//linked" : "");
        return sb.ToString();
    }
}