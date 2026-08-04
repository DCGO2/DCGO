using System;
using System.Collections;
using System.Collections.Generic;

public partial class CardEffectFactory
{
    /// <summary>
    /// Creates an AddSkillClass static effect that dynamically copies non-inherited effects 
    /// from digivolution cards under this Permanent.
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
        Func<ICardEffect, bool> effectCondition = null
        )
    {
        
        if (timing is not EffectTiming.None)
        {
            return;
        }

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
                targetSources = _ => validSources(sourceCard.PermanentOfThisCard().DigivolutionCards);
            } 

            foreach (CardSource cardSource in targetSources(null))
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

                        var originalUseCondition = activateClass.CanUseCondition;
                        activateClass.SetCanUseCondition(
                            hashtable => validSources(targetSources(null)).Contains(activateClass.OriginalEffectSourceCard) 
                            && (originalUseCondition is null || originalUseCondition(hashtable))
                        );

                        var originalActivateCondition = activateClass.CanActivateCondition;
                        activateClass.SetCanActivateCondition(
                            hashtable => validSources(targetSources(null)).Contains(activateClass.OriginalEffectSourceCard) 
                            && (originalActivateCondition is null || originalActivateCondition(hashtable))
                        );

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
                            true,
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

        cardEffects.Add(addSkillClass);
    }

    private static string GenerateHashString(CardSource card, CardSource cardSource, int i)
        => $"Card-{card.CardIndex}-Copying-Card-{cardSource.CardIndex}-effect-{i}";
}