using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEditor.Rendering;

public partial class CardEffectCommons
{
    #region Can trigger

    #region Can trigger [On Deletion] effect
    public static bool CanTriggerOnDeletion(Hashtable hashtable, CardSource card)
    {
        return CanTriggerOnPermanentDeleted(hashtable, (permanent) => permanent.cardSources.Contains(card));
    }
    #endregion

    #region Can trigger "when permanent is deleted" effect
    public static bool CanTriggerOnPermanentDeleted(Hashtable hashtable, Func<Permanent, bool> permanentCondition)
    {
        List<Hashtable> hashtables = GetHashtablesFromHashtable(hashtable);

        if (hashtables != null)
        {
            foreach (Hashtable hashtable1 in hashtables)
            {
                Permanent permanent = GetPermanentFromHashtable(hashtable1);

                if (permanent != null)
                {
                    if (permanent.TopCard != null)
                    {
                        if (permanentCondition != null)
                        {
                            if (permanentCondition(permanent))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }
    #endregion

    #region Can trigger "when permanent is deleted by battle" effects
    public static bool IsByBattle(Hashtable hashtable)
    {
        return GetBattleFromHashtable(hashtable) != null;
    }
    #endregion

    #region Can trigger "when permanent is deleted or played by effect that satisfies the condition" effects
    public static bool IsByEffect(Hashtable hashtable, Func<ICardEffect, bool> cardEffectCondition)
    {
        ICardEffect CardEffect = GetCardEffectFromHashtable(hashtable);

        if (CardEffect != null)
        {
            if (CardEffect.EffectSourceCard != null)
            {
                if (cardEffectCondition == null || cardEffectCondition(CardEffect))
                {
                    return true;
                }
            }
        }

        return false;
    }
    #endregion

    #endregion

    #region Can activate

    #region Can activate [On Deletion] effect(not inherited)
    public static bool CanActivateOnDeletion(CardSource card)
    {
        return IsExistOnTrash(card);
    }
    #endregion

    #region Can activate [On Deletion] effect(inherited)
    public static bool CanActivateOnDeletionInherited(Hashtable hashtable, CardSource card)
    {
        if (IsTopCardInTrashOnDeletion(hashtable))
        {
            if (IsTopCardSamePermanent(hashtable, card))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Whether TopCard is in trash when check [On Deletion] effect
    public static bool IsTopCardInTrashOnDeletion(Hashtable hashtable)
    {
        List<Hashtable> hashtables = GetHashtablesFromHashtable(hashtable);

        if (hashtables != null)
        {
            foreach (Hashtable hashtable1 in hashtables)
            {
                CardSource TopCard = GetTopCardFromOneHashtable(hashtable1);

                if (TopCard != null)
                {
                    if (IsExistOnTrash(TopCard) || TopCard.IsToken)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    #endregion

    #region Whether the card that uses the effect and the top card belonged to the same permanent
    public static bool IsTopCardSamePermanent(Hashtable hashtable, CardSource card)
    {
        if (card == null) return false;
        if (card.PermanentJustBeforeRemoveField == null) return false;

        List<Hashtable> hashtables = GetHashtablesFromHashtable(hashtable);

        if (hashtables != null)
        {
            foreach (Hashtable hashtable1 in hashtables)
            {
                CardSource TopCard = GetTopCardFromOneHashtable(hashtable1);

                if (TopCard != null)
                {
                    if (TopCard.PermanentJustBeforeRemoveField != null)
                    {
                        if (card.PermanentJustBeforeRemoveField == TopCard.PermanentJustBeforeRemoveField)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
    #endregion

    #region Can activate [On Deletion] effect that can activate if the permanent conains specific name
    public static bool CanActivateSelfOnDeletionWithContainingCardName(Hashtable hashtable, string name, CardSource card)
    {
        return CanActivateOnDeletionWithContainingCardName(
            hashtable: hashtable,
            name: name,
            cardCondition: cardSource => cardSource == card
        );
    }

    public static bool CanActivateOnDeletionWithContainingCardName(
        Hashtable hashtable,
        string name,
        Func<CardSource, bool> cardCondition)
    {
        List<Hashtable> hashtables = GetHashtablesFromHashtable(hashtable);

        if (hashtables != null)
        {
            foreach (Hashtable hashtable1 in hashtables)
            {
                if (hashtable1 != null)
                {
                    List<CardSource> CardSources = GetCardSourcesFromHashtable(hashtable1);

                    if (CardSources != null)
                    {
                        if (CardSources.Some(cardSource => cardCondition == null || cardCondition(cardSource)))
                        {
                            if (hashtable1.ContainsKey("CardNames"))
                            {
                                if (hashtable1["CardNames"] is List<string>)
                                {
                                    List<string> CardNames = (List<string>)hashtable1["CardNames"];

                                    if (CardNames != null)
                                    {
                                        if (CardNames.Count((cardName) => cardName.Contains(name)) >= 1)
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
        }

        return false;
    }
    #endregion

    #region Can activate [On Deletion] effect that can activate if the permanent conains specific trait
    public static bool CanActivateSelfOnDeletionWithContainingTrait(Hashtable hashtable, string name, CardSource card)
    {
        return CanActivateOnDeletionWithContainingTrait(
            hashtable: hashtable,
            name: name,
            cardCondition: cardSource => cardSource == card
        );
    }

    public static bool CanActivateOnDeletionWithContainingTrait(
        Hashtable hashtable,
        string name,
        Func<CardSource, bool> cardCondition)
    {
        List<Hashtable> hashtables = GetHashtablesFromHashtable(hashtable);
        if (hashtables != null)
        {
            foreach (Hashtable hashtable1 in hashtables)
            {
                if (hashtable1 != null)
                {
                    List<CardSource> CardSources = GetCardSourcesFromHashtable(hashtable1);
                    if (CardSources != null)
                    {
                        if (CardSources.Some(cardSource => cardCondition == null || cardCondition(cardSource)))
                        {
                            if (hashtable1.ContainsKey("TopCard"))
                            {
                                if (hashtable1["TopCard"] is CardSource)
                                {
                                    CardSource topCard = (CardSource)hashtable1["TopCard"];
                                    if (topCard != null)
                                    {
                                        if (topCard.ContainsTraits(name))
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
        }

        return false;
    }
    #endregion

    #region Can activate [On Deletion] effect that can activate if the permanent has specific colors
    public static bool CanActivateSelfOnDeletionWithCardColors(Hashtable hashtable, Func<List<CardColor>, bool> cardColorCondition, CardSource card)
    {
        return CanActivateOnDeletionWithCardColors(
            hashtable: hashtable,
            cardColorCondition: cardColorCondition,
            cardCondition: cardSource => cardSource == card
        );
    }

    public static bool CanActivateOnDeletionWithCardColors(
        Hashtable hashtable,
        Func<List<CardColor>, bool> cardColorCondition,
        Func<CardSource, bool> cardCondition)
    {
        if (hashtable != null)
        {
            List<Hashtable> hashtables = GetHashtablesFromHashtable(hashtable);

            if (hashtables != null)
            {
                foreach (Hashtable hashtable1 in hashtables)
                {
                    List<CardSource> CardSources = GetCardSourcesFromHashtable(hashtable1);

                    if (CardSources != null)
                    {
                        if (CardSources.Some(cardSource => cardCondition == null || cardCondition(cardSource)))
                        {
                            if (hashtable1.ContainsKey("CardColors"))
                            {
                                if (hashtable1["CardColors"] is List<CardColor>)
                                {
                                    List<CardColor> CardColors = (List<CardColor>)hashtable1["CardColors"];

                                    if (CardColors != null)
                                    {
                                        if (cardColorCondition == null || cardColorCondition(CardColors))
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
        }

        return false;
    }
    #endregion

    #region Can activate [On Deletion] effect that can activate if the permanent has Save text
    public static bool CanActivateSelefOnDeletionWithSaveText(Hashtable hashtable, CardSource card)
    {
        return CanActivateOnDeletionWithSaveText(
            hashtable: hashtable,
            cardCondition: cardSource => cardSource == card
        );
    }

    public static bool CanActivateOnDeletionWithSaveText(
        Hashtable hashtable,
        Func<CardSource, bool> cardCondition)
    {
        List<Hashtable> hashtables = GetHashtablesFromHashtable(hashtable);

        if (hashtables != null)
        {
            foreach (Hashtable hashtable1 in hashtables)
            {
                List<CardSource> CardSources = GetCardSourcesFromHashtable(hashtable1);

                if (CardSources != null)
                {
                    if (CardSources.Some(cardSource => cardCondition == null || cardCondition(cardSource)))
                    {
                        if (hashtable1.ContainsKey("HasSaveText"))
                        {
                            if (hashtable1["HasSaveText"] is bool)
                            {
                                bool HasSaveText = (bool)hashtable1["HasSaveText"];

                                if (HasSaveText)
                                {
                                    return true;
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

    #endregion
}