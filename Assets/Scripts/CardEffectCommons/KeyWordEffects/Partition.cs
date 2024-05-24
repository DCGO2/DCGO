using System.Collections;
using System.Collections.Generic;
using System;
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
    public static IEnumerator PartitionProcess(ICardEffect activateClass, Permanent permanent, Func<CardSource, bool> CanSelectFirstSourceCondition, Func<CardSource, bool> CanSelectSecondSourceCondition)
    {
        yield return ContinuousController.instance.StartCoroutine(new PartitionClass(permanent).Partition(activateClass, CanSelectFirstSourceCondition, CanSelectSecondSourceCondition));
    }
    #endregion

    #region Partition class
    public class PartitionClass
    {
        public PartitionClass(Permanent permanent)
        {
            _permanent = permanent;
        }

        Permanent _permanent = null;


        public IEnumerator Partition(ICardEffect activateClass, Func<CardSource, bool> CanSelectFristSourceCondition, Func<CardSource, bool> CanSelectSecondSourceCondition)
        {
            if (_permanent != null)
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(_permanent));
                CardSource topCard = _permanent.TopCard;

                List<CardSource> selectedCards = new List<CardSource>();

                if (_permanent.TopCard != null)
                {
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                    selectCardEffect.SetUp(
                            canTargetCondition: CanSelectFristSourceCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select card to play",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: false,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: _permanent.DigivolutionCards,
                            canLookReverseCard: true,
                            selectPlayer: topCard.Owner,
                            cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    SelectCardEffect selectSecondCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                    selectSecondCardEffect.SetUp(
                            canTargetCondition: CanSelectSecondSourceCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select card to play",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: false,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: _permanent.DigivolutionCards,
                            canLookReverseCard: true,
                            selectPlayer: topCard.Owner,
                            cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectSecondCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

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

                GManager.instance.playLog.AddLogString(log);
                #endregion
            }
        }
    }
    #endregion
}