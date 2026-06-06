using System;
using System.Collections.Generic;

public partial class CardEffectFactory
{
    /// <summary>
    /// Effect to copy through the effects of digivolution cards as effect of the top card. Done directly in CEntity_Effect.CardEffects for efficiency
    /// </summary>
    /// <param name="cardEffects">Reference to the list of effects for this card to add effects to</param>
    /// <param name="timing">timing being queried</param>
    /// <param name="card">Card with the copy effect</param>
    /// <param name="canUseCondition">Condition to use this effect (Current turn, This digimon with X trait, etc.)</param>
    /// <param name="cardCondition">Condition of the cards to copy from (gammamon in name, MachineDramon or Chaosdramon, etc.)</param>
    /// <param name="effectCondition">Conditions on the effects to copy (Only On Plays, only activate effects, etc.)</param>
    public static void CopyDigivolutionCardEffects(ref List<ICardEffect> cardEffects,
                                                   EffectTiming timing, 
                                                   CardSource card,
                                                   bool isInheritedEffect = false,
                                                   bool isLinkedEffect = false,
                                                   Func<bool> canUseCondition = null,
                                                   Func<CardSource, bool> cardCondition = null,
                                                   Func<ICardEffect, bool> effectCondition = null
                                                   )
    {
        if (card.PermanentOfThisCard() == null) return;
        if (canUseCondition != null && !canUseCondition()) return;

        Permanent thisPermanent = card.PermanentOfThisCard();

        //If it is an inherited effect it should be in digivolution cards, if link effect should be linked, if neither it should not be in either (leaving only topcard)
        if (isInheritedEffect != thisPermanent.DigivolutionCards.Contains(card)) return;
        if (isLinkedEffect != thisPermanent.LinkedCards.Contains(card)) return;

        List<CardSource> validSources = thisPermanent.DigivolutionCards.Filter(cardSource => cardSource != card && (cardCondition == null || cardCondition(cardSource)));

        foreach (CardSource cardSource in validSources)
        {
            List<ICardEffect> toCopyEffects = cardSource.EffectList(timing);
            int i = 0;
            foreach (ICardEffect cardEffect in toCopyEffects)
            {
                if (cardEffect.IsInheritedEffect || cardEffect.IsLinkedEffect) continue;

                if (cardEffect is ActivateClass)
                {
                    ActivateClass activateClass = (ActivateClass)cardEffect;
                    activateClass.SetHashString(GenerateHashString(card, cardSource, i));//Set unique hashstring so OPTs work correctly
                    cardEffects.Add(activateClass);
                    cardEffects.Add(PermanentEffectFactory.AddDetailClass(thisPermanent, activateClass.EffectDiscription, true, activateClass));
                }
                else//If any other effect types need their can use condition changed to work correctly those can be handled here, otherwise just copied through
                {
                    cardEffects.Add(cardEffect);
                }
            }
        }
    }

    private static string GenerateHashString(CardSource card, CardSource cardSource, int i) => $"Card-{card.CardIndex}-Copying-Card-{cardSource.CardIndex}-effect-{i}";
}