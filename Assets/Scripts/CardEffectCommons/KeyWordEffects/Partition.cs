using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger [Partition]
    public static bool CanTriggerPartition(Hashtable hashtable, CardSource card)
    {
        if (CanTriggerWhenPermanentRemoveField(hashtable, (permanent) => permanent.cardSources.Contains(card)))
        {
            if (!IsByBattle(hashtable))
            {
                if (!IsByEffect(hashtable, cardEffect => IsOwnerEffect(cardEffect, card)))
                {
                    return true;
                }
            }
        }

        return false;
    }
    #endregion
    
    #region Can activate [Partition]
    public static bool CanActivatePartition(Permanent permanent)
    {
        if (IsPermanentExistsOnBattleArea(permanent))
        {
            if (permanent.DigivolutionCards.Count >= 2)
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Effect process of [Partition]
    public static IEnumerator PartitionProcess(ICardEffect activateClass, Permanent permanent, List<CardSource> firstSources, List<CardSource> secondSources,  List<PartitionCondition> partitionConditions)
    {
        yield return ContinuousController.instance.StartCoroutine(new PartitionClass(permanent, partitionConditions).Partition(activateClass, firstSources, secondSources));
    }
    #endregion

    #region Partition class
    public class PartitionClass
    {
        public PartitionClass(Permanent permanent, List<PartitionCondition> conditions)
        {
            _permanent = permanent;
            _conditions = conditions;
        }

        Permanent _permanent = null;
        List<PartitionCondition> _conditions = new List<PartitionCondition>();

        string GetConditionColors(PartitionCondition condition)
        {
            string str = condition.Color.ToString();

            if (condition.hasTwoColor)
                str += "/" + condition.Color2.ToString();

            return str;
        }

        public IEnumerator Partition(ICardEffect activateClass, List<CardSource> firstSources, List<CardSource> secondSources)
        {
            if (_permanent != null)
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(_permanent));
                CardSource topCard = _permanent.TopCard;

                List<CardSource> selectedCards = new List<CardSource>();

                bool CanSelectCondition(CardSource source)
                {
                    return true;
                }

                string colorsToGrab = "";

                if (_permanent.TopCard != null)
                {
                    if (firstSources.Count > 1)
                    {
                        colorsToGrab = GetConditionColors(_conditions[0]);

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                        selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => false,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: $"Select 1 {colorsToGrab} Digimon to play",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: false,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: firstSources,
                                canLookReverseCard: true,
                                selectPlayer: topCard.Owner,
                                cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }
                    else
                        selectedCards.AddRange(firstSources);

                    if(secondSources.Count > 1)
                    {
                        colorsToGrab = GetConditionColors(_conditions[1]);

                        SelectCardEffect selectSecondCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                        selectSecondCardEffect.SetUp(
                                canTargetCondition: CanSelectCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => false,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: $"Select 1 {colorsToGrab} Digimon to play",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: false,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: secondSources,
                                canLookReverseCard: true,
                                selectPlayer: topCard.Owner,
                                cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectSecondCardEffect.Activate());
                    }
                    else
                        selectedCards.AddRange(secondSources);

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);
                        secondSources = secondSources.Except(selectedCards).ToList();
                        yield return null;
                    }
                }

                if(selectedCards.Count == 2)
                {
                    yield return ContinuousController.instance.StartCoroutine(PlayPermanentCards(
                        cardSources: selectedCards,
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.DigivolutionCards,
                        activateETB: true));
                }

                #region Play Log
                string log = "";

                log += $"\nPartition :";

                log += $"\n{topCard.BaseENGCardNameFromEntity}({topCard.CardID})";

                log += "\n";

                PlayLog.OnAddLog?.Invoke(log);
                #endregion
            }
        }
    }
    #endregion
}