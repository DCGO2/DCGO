using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class PartitionCondition
{
    public int Level;
    public CardColor Color;
    public CardColor Color2;
    public string Name;
    public bool hasTwoColor = false;

    public PartitionCondition(int level, CardColor color)
    {
        Level = level;
        Color = color;
    }

    public PartitionCondition(int level, CardColor color, CardColor color2)
    {
        Level = level;
        Color = color;
        Color2 = color2;
        hasTwoColor = true;
    }

    public PartitionCondition(string cardName)
    {
        Name = cardName;
    }
}

public partial class CardEffectFactory
{
    #region Trigger effect of [Partition] on oneself
    public static ActivateClass PartitionSelfEffect(bool isInheritedEffect, CardSource card, Func<bool> condition, List<PartitionCondition> cardSourceConditions)
    {
        Permanent targetPermanent = card.PermanentOfThisCard();
        
        bool CanUseCondition()
        {
            if (condition == null || condition())
            {
                return true;
            }

            return false;
        }

        return PartitionEffect(targetPermanent: targetPermanent, isInheritedEffect: isInheritedEffect, condition: CanUseCondition, partitionConditions: cardSourceConditions, card);
    }
    #endregion

    #region Trigger effect of [Partition]
    public static ActivateClass PartitionEffect(Permanent targetPermanent, bool isInheritedEffect, Func<bool> condition, List<PartitionCondition> partitionConditions, CardSource card)
    {
        if (targetPermanent == null) return null;
        if (targetPermanent.TopCard == null) return null;
        if (card == null) return null;

        List<CardSource> sourceOneCard = new List<CardSource>();
        List<CardSource> sourceTwoCard = new List<CardSource>();

        sourceOneCard = targetPermanent.DigivolutionCards.Clone();
        sourceTwoCard = targetPermanent.DigivolutionCards.Clone();

        if (!String.IsNullOrEmpty(partitionConditions[0].Name))
            sourceOneCard = sourceOneCard.Filter(source => source.EqualsCardName(partitionConditions[0].Name));

        if (!String.IsNullOrEmpty(partitionConditions[1].Name))
            sourceOneCard = sourceOneCard.Filter(source => source.EqualsCardName(partitionConditions[1].Name));

        if (partitionConditions[0].hasTwoColor)
        {
            sourceOneCard = sourceOneCard.Filter(source =>
                    (source.CardColors.Contains(partitionConditions[0].Color) || source.CardColors.Contains(partitionConditions[0].Color2))
                    && (source.HasLevel && source.Level == partitionConditions[0].Level));
        }
        else
        {
            sourceOneCard = sourceOneCard.Filter(source =>
                    source.CardColors.Contains(partitionConditions[0].Color)
                    && (source.HasLevel && source.Level == partitionConditions[0].Level));
        }

        if (partitionConditions[1].hasTwoColor)
        {
            sourceTwoCard = sourceTwoCard.Filter(source =>
                    (source.CardColors.Contains(partitionConditions[1].Color) || source.CardColors.Contains(partitionConditions[1].Color2))
                    && (source.HasLevel && source.Level == partitionConditions[1].Level));
        }
        else
        {
            sourceTwoCard = sourceTwoCard.Filter(source =>
                    source.CardColors.Contains(partitionConditions[1].Color) && 
                    (source.HasLevel && source.Level == partitionConditions[1].Level));
        }

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Partition", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
        activateClass.SetHashString($"Partition_{card.CardID}" + (isInheritedEffect ? "_inherited" : ""));
        activateClass.SetIsInheritedEffect(isInheritedEffect);

        string EffectDiscription()
        {
            return DataBase.PartitionEffectDiscription();
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            if (CardEffectCommons.CanTriggerPartition(hashtable, card))
            {
                if (condition == null || condition())
                {
                    return true;
                }
            }

            return false;
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            if (CardEffectCommons.CanActivatePartition(targetPermanent))
            {
                if (sourceOneCard == null || sourceOneCard.Count > 0)
                {
                    if (sourceTwoCard == null || sourceTwoCard.Count > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            if (sourceOneCard.Count == 1)
                sourceTwoCard = sourceTwoCard.Except(sourceOneCard).ToList();

            if (sourceTwoCard.Count == 1)
                sourceOneCard = sourceOneCard.Except(sourceTwoCard).ToList();

            return CardEffectCommons.PartitionProcess(activateClass, targetPermanent, sourceOneCard, sourceTwoCard, partitionConditions);
        }

        return activateClass;
    }
    #endregion
}