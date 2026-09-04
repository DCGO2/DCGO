using System;
using System.Collections;
using System.Collections.Generic;

public partial class CardEffectFactory
{
    /// <summary>
    /// Creates an AddSkillClass static effect that dynamically copies non-inherited effects 
    /// from digivolution cards under this Permanent.
    /// </summary>
    public static AddSkillClass CopyDigivolutionCardEffects(
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

        bool DefaultCanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.IsExistOnBattleArea(card);
        }

        bool DefaultPermanentCondition(Permanent permanent)
        {
            return permanent != null && permanent == card.PermanentOfThisCard();
        }

        bool DefaultCardSourceCondition(CardSource cardSource)
        {
            // This wrapper only ever grants its copy to its own host (card) -- never to
            // whatever else happens to be the probed candidate. Without this identity check,
            // a completely unrelated card could satisfy "permanentCondition(permanent) &&
            // cardSource == permanent.TopCard" purely by being the top card of card's own
            // permanent, and get treated as if it were the intended recipient.
            if (cardSource == null || cardSource != card) return false;

            Permanent permanent = cardSource.PermanentOfThisCard();
            if (permanent == null) return false;

            if (!permanentCondition(permanent)) return false;

            // A non-inherited/non-linked copy only applies while card is still the actual
            // active top card of its permanent. An inherited/linked copy is specifically meant
            // to keep applying once card is buried/attached -- requiring cardSource ==
            // permanent.TopCard here would make that impossible, since a buried/linked card is
            // by definition never the top card.
            return isInheritedEffect || isLinkedEffect || cardSource == permanent.TopCard;
        }

        List<CardSource> validSources(List<CardSource> availableSources) => availableSources.Filter(
            cardSource => cardCondition == null || cardCondition(cardSource)
        );

        canUseCondition ??= DefaultCanUseCondition;
        permanentCondition ??= DefaultPermanentCondition;
        cardSourceCondition ??= DefaultCardSourceCondition;

        AddSkillClass addSkillClass = new AddSkillClass();
        addSkillClass.SetUpICardEffect(isSuccession ? "Succession" : "Copy Digivolution Card Effects", canUseCondition, card);
        addSkillClass.SetIsInheritedEffect(isInheritedEffect);
        addSkillClass.SetIsLinkedEffect(isLinkedEffect);

        List<ICardEffect> GetEffects(CardSource sourceCard, List<ICardEffect> getCardEffects, EffectTiming _timing)
        {
            if (getCardEffects == null)
                getCardEffects = new List<ICardEffect>();

            if (sourceCard == null)
                return getCardEffects;

            if (cardSourceCondition != null && !cardSourceCondition(sourceCard))
                return getCardEffects;

            Permanent thisPermanent = sourceCard.PermanentOfThisCard();

            if (targetSources == null)
            {
                if (thisPermanent == null || thisPermanent.DigivolutionCards == null)
                    return getCardEffects;
                targetSources = cardSources => cardSources;
            } 

            foreach (CardSource cardSource in validSources(targetSources(sourceCard.PermanentOfThisCard().DigivolutionCards)))
            {
                List<ICardEffect> toCopyEffects = cardSource.cEntity_EffectController.GetCardEffects_ExceptAddedEffects(_timing, sourceCard);

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
                            ValidCardSources = validSources(targetSources(sourceCard.PermanentOfThisCard().DigivolutionCards));
                            return ValidCardSources.Contains(activateClass.OriginalEffectSourceCard);
                        }

                        bool ValidCardSourceAtActivate()
                        {
                            Permanent thisPermanent = sourceCard.PermanentOfThisCard();
                            if (thisPermanent != null)
                            {
                                ValidCardSources = validSources(targetSources(sourceCard.PermanentOfThisCard().DigivolutionCards));
                            }
                            return ValidCardSources.Contains(activateClass.OriginalEffectSourceCard);
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

                        copiedActivateClass.SetOriginalEffectSourceCard(activateClass.OriginalEffectSourceCard);
                        copiedActivateClass.SetHashString(GenerateHashString(card, activateClass.OriginalEffectSourceCard, activateClass.HashString, isInheritedEffect, isLinkedEffect));
                        copiedActivateClass.SetIsInheritedEffect(isInheritedEffect);
                        copiedActivateClass.SetIsLinkedEffect(isLinkedEffect);

                        getCardEffects.Add(copiedActivateClass);

                        getCardEffects.Add(PermanentEffectFactory.AddDetailClass(
                            thisPermanent,
                            copiedActivateClass.EffectDescription,
                            true,
                            copiedActivateClass));
                    }
                    else if (!isSuccession || cardEffect.EffectName != "Succession") // Succession can never copy another succession skill
                    {
                        getCardEffects.Add(cardEffect);
                        getCardEffects.Add(PermanentEffectFactory.AddDetailClass(
                            thisPermanent,
                            cardEffect.EffectDescription,
                            false,
                            cardEffect));
                    }
                }
                // If succession, break loop after first valid card to only copy topmost.
                // break instead of return in case we ever need to perform more actions before returning
                if (isSuccession) break; 
            }

            return getCardEffects;
        }

        addSkillClass.SetUpAddSkillClass(
            cardSourceCondition: cardSourceCondition,
            getEffects: GetEffects,
            limitTiming: null
        );

        return addSkillClass;
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