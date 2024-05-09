using System.Collections;
using System.Collections.Generic;
using System;

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
            if (permanent.DigivolutionCards.Count >= 1)
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

                    //int maxCount = Math.Min(1, MatchConditionPermanentCount(CanSelectCardCondition));

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
                            root: SelectCardEffect.Root.DigivolutionCards,
                            customRootCardList: _permanent.cardSources,
                            canLookReverseCard: true,
                            selectPlayer: topCard.Owner,
                            cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    selectCardEffect.SetUp(
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
                            root: SelectCardEffect.Root.DigivolutionCards,
                            customRootCardList: _permanent.cardSources,
                            canLookReverseCard: true,
                            selectPlayer: topCard.Owner,
                            cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }
                }
                
                foreach(CardSource card in selectedCards)
                {
                    yield return ContinuousController.instance.StartCoroutine(PlayPermanentCards(
                        cardSources: new List<CardSource>() { card },
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Trash,
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