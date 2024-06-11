using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class PartitionCondition
{
    public int Level;
    public CardColor Color;

    public PartitionCondition(int level, CardColor color)
    {
        Level = level;
        Color = color;
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

        List<CardSource> sourceOneCard = targetPermanent.cardSources
            .Clone()
            .Filter(source =>
                source.CardColors.Contains(partitionConditions[0].Color)
                && (source.HasLevel && source.Level == partitionConditions[0].Level));

        List<CardSource> sourceTwoCard = targetPermanent.cardSources
            .Clone()
            .Filter(source =>
                source.CardColors.Contains(partitionConditions[1].Color)
                && (source.HasLevel && source.Level == partitionConditions[1].Level));

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

            return CardEffectCommons.PartitionProcess(activateClass, targetPermanent, sourceOneCard, sourceTwoCard);
        }

        return activateClass;
    }
    #endregion
}