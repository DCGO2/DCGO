using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
public partial class CardEffectCommons
{
    #region Can trigger [Fortitude]
    public static bool CanTriggerFortitude(Hashtable hashtable, CardSource card)
    {
        return CanTriggerOnDeletion(hashtable, card);
    }
    #endregion

    #region Can activate [Fortitude]
    public static bool CanActivateFortitude(Hashtable hashtable, CardSource card, bool isInheritedEffect, ICardEffect activateClass)
    {
        if (IsExistOnTrash(card))
        {
            if (!isInheritedEffect || CanActivateOnDeletionInherited(hashtable, card))
            {
                List<Hashtable> hashtables = CardEffectCommons.GetHashtablesFromHashtable(hashtable);

                if (hashtables != null)
                {
                    foreach (Hashtable hashtable1 in hashtables)
                    {
                        List<CardSource> CardStack = CardEffectCommons.GetCardSourcesFromHashtable(hashtable1);
                        List<CardSource> CardSources = CardEffectCommons.GetDigivolutionSourcesFromHashtable(hashtable1);

                        if (CardStack != null && CardSources != null)
                        {
                            if (CardStack.Contains(card))
                            {
                                if (CardSources.Count >= 1)
                                {
                                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: card, payCost: false, cardEffect: activateClass, root: SelectCardEffect.Root.Trash))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return false;
    }
    #endregion

    #region Effect process of [Fortitude]
    public static IEnumerator FortitudeProcess(Hashtable hashtable, CardSource card, ICardEffect activateClass)
    {
        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: new List<CardSource>() { card },
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Trash,
                        activateETB: true));
    }
    #endregion

    #region Target 1 Digimon gains [Fortitude]
    public static IEnumerator GainFortitude(Permanent targetPermanent, EffectDuration effectDuration, ICardEffect activateClass)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        bool CanUseCondition()
        {
            if (IsPermanentExistsOnBattleArea(targetPermanent))
            {
                if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
                {
                    return true;
                }
            }

            return false;
        }

        ActivateClass evade = CardEffectFactory.EvadeEffect(targetPermanent: targetPermanent, isInheritedEffect: false, condition: CanUseCondition, rootCardEffect: activateClass, targetPermanent.TopCard);

        AddEffectToPermanent(targetPermanent: targetPermanent, effectDuration: effectDuration, card: card, cardEffect: evade, timing: EffectTiming.OnDestroyedAnyone);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
        }
    }
    #endregion
}