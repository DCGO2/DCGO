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
        Func<ICardEffect, bool> effectCondition = null
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
            if (cardSource == null) return false;

            Permanent permanent = cardSource.PermanentOfThisCard();
            if (permanent == null) return false;

            if (permanentCondition(permanent))
            {
                if (cardSource == permanent.TopCard)
                {
                    return true;
                }
            }
            return false;
        }

        List<CardSource> validSources(List<CardSource> availableSources) => availableSources.Filter(
            cardSource => cardCondition == null || cardCondition(cardSource)
        );

        canUseCondition ??= DefaultCanUseCondition;
        permanentCondition ??= DefaultPermanentCondition;
        cardSourceCondition ??= DefaultCardSourceCondition;

        AddSkillClass addSkillClass = new AddSkillClass();
        addSkillClass.SetUpICardEffect("Copy Digivolution Card Effects", canUseCondition, card);
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
                toCopyEffects.ForEach(eff => eff.SetOriginalEffectSourceCard(cardSource));
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
                        getCardEffects.Add(activateClass);

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
                        activateClass.SetCanUseCondition(
                            hashtable => ValidCardSourceAtTrigger() 
                            && (originalUseCondition is null || originalUseCondition(hashtable))
                        );

                        var originalActivateCondition = activateClass.CanActivateCondition;
                        activateClass.SetCanActivateCondition(
                            hashtable => ValidCardSourceAtActivate()
                            && (originalActivateCondition is null || originalActivateCondition(hashtable))
                        );

                        activateClass.SetHashString(GenerateHashString(card, activateClass.OriginalEffectSourceCard, activateClass.HashString, isInheritedEffect, isLinkedEffect));

                        getCardEffects.Add(PermanentEffectFactory.AddDetailClass(
                            thisPermanent,
                            activateClass.EffectDescription,
                            true,
                            activateClass));
                    }
                    else
                    {
                        getCardEffects.Add(cardEffect);
                        getCardEffects.Add(PermanentEffectFactory.AddDetailClass(
                            thisPermanent,
                            cardEffect.EffectDescription,
                            false,
                            cardEffect));
                    }
                }
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
        string sourceHashString = source ??= "";
        string inherited = isInherited ? "-inherited" : "";
        string linked = isLinked ? "-linked" : "";
        return $"{card.CardIndex}-copying-{cardSource.CardIndex}-effect-{sourceHashString}{inherited}{linked}";
    }
}