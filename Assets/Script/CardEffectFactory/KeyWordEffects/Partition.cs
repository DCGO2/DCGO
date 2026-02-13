using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PartitionCondition
{
    public int Level;
    public CardColor Color;
    public CardColor Color2;
    public string Name;
    public bool HasOneColour = false;
    public bool hasTwoColor = false;
    public bool hasName = false;

    public PartitionCondition(int level, CardColor color)
    {
        Level = level;
        Color = color;
        HasOneColour = true;
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
        hasName = true;
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

        #region Setup Partition Cards

        // First Card

        if (partitionConditions[0].HasOneColour)
        {
            sourceOneCard = sourceOneCard.Filter(source =>
                    source.CardColors.Contains(partitionConditions[0].Color)
                    && (source.HasLevel && source.Level == partitionConditions[0].Level));
        }
        else if (partitionConditions[0].hasTwoColor)
        {
            sourceOneCard = sourceOneCard.Filter(source =>
                    (source.CardColors.Contains(partitionConditions[0].Color) || source.CardColors.Contains(partitionConditions[0].Color2))
                    && (source.HasLevel && source.Level == partitionConditions[0].Level));
        }
        else if (partitionConditions[0].hasName)
        {
            sourceOneCard = sourceOneCard.Filter(source => source.EqualsCardName(partitionConditions[0].Name));
        }
        else
        {
            sourceOneCard = null;
        }


        if (partitionConditions[1].HasOneColour)
        {
            sourceTwoCard = sourceTwoCard.Filter(source =>
                    source.CardColors.Contains(partitionConditions[1].Color)
                    && (source.HasLevel && source.Level == partitionConditions[1].Level));
        }
        else if (partitionConditions[1].hasTwoColor)
        {
            sourceTwoCard = sourceTwoCard.Filter(source =>
                    (source.CardColors.Contains(partitionConditions[1].Color) || source.CardColors.Contains(partitionConditions[1].Color2))
                    && (source.HasLevel && source.Level == partitionConditions[1].Level));
        }
        else if (partitionConditions[1].hasName)
        {
            sourceTwoCard = sourceTwoCard.Filter(source => source.EqualsCardName(partitionConditions[1].Name));
        }
        else
        {
            sourceTwoCard = null;
        }

        #endregion

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
