using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class SelectBurstDigivolutionEffect : MonoBehaviour
{
    public void SetUp_SelectWheterToBurst
        (CardSource card,
        CardSource evoRoot,
        bool canNoSelect,
        Func<IEnumerator> endSelectCoroutine_Digivolve,
        Func<IEnumerator> endSelectCoroutine_Burst,
        Func<IEnumerator> noSelectCoroutine)
    {
        _card = card;
        _evoRoot = evoRoot;
        _canNoSelect = canNoSelect;
        _endSelectCoroutine_Digivolve = endSelectCoroutine_Digivolve;
        _endSelectCoroutine_Burst = endSelectCoroutine_Burst;
        _noSelectCoroutine = noSelectCoroutine;
    }

    public void SetUp_SelectTamer
        (CardSource card,
        bool isLocal,
        bool isPayCost,
        bool canNoSelect,
        Func<Permanent, IEnumerator> endSelectCoroutine_SelectTamer,
        Func<IEnumerator> noSelectCoroutine)
    {
        _card = card;
        _isLocal = isLocal;
        _isPayCost = isPayCost;
        _canNoSelect = canNoSelect;
        _endSelectCoroutine_SelectTamer = endSelectCoroutine_SelectTamer;
        _noSelectCoroutine = noSelectCoroutine;
    }

    CardSource _card = null;
    CardSource _evoRoot = null;
    bool _isLocal = false;

    bool _isPayCost = false;
    bool _canNoSelect = false;
    Func<IEnumerator> _endSelectCoroutine_Digivolve = null;
    Func<IEnumerator> _endSelectCoroutine_Burst = null;
    Func<Permanent, IEnumerator> _endSelectCoroutine_SelectTamer = null;
    Func<IEnumerator> _noSelectCoroutine = null;
    public bool TamerBounced { get; private set; } = false;
    public IEnumerator SelectWheterToBurst()
    {
        if (_card != null)
        {
            if (_evoRoot != null)
            {
                yield return StartCoroutine(GManager.instance.selectCardPanel.OpenSelectCardPanel(
                            Message: "With which method would you like to Digivolve?",
                            RootCardSources: new List<CardSource>() { _card, _card },
                            _CanTargetCondition: (cardSource) => true,
                            _CanTargetCondition_ByPreSelecetedList: null,
                            _CanEndSelectCondition: null,
                            _MaxCount: 1,
                            _CanEndNotMax: false,
                            _CanNoSelect: () => _canNoSelect,
                            CanLookReverseCard: true,
                            skillInfos: null,
                            root: SelectCardEffect.Root.None,
                            isCenter: true,
                            evoRootsArray: new CardSource[][] { new CardSource[] { _evoRoot }, new CardSource[] { _evoRoot } },
                            titleStrings: new List<string>() { "Normal Digivolution", "<color=#FF633E>Burst</color> Digivollution" }));

                if (GManager.instance.selectCardPanel.SelectedIndex.Count > 0)
                {
                    int index = GManager.instance.selectCardPanel.SelectedIndex[0];

                    switch (index)
                    {
                        case 0:
                            if (_endSelectCoroutine_Digivolve != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(_endSelectCoroutine_Digivolve());
                            }
                            break;

                        case 1:
                            if (_endSelectCoroutine_Burst != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(_endSelectCoroutine_Burst());
                            }
                            break;
                    }
                }

                else
                {
                    if (_noSelectCoroutine != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(_noSelectCoroutine());
                    }
                }
            }
        }
    }

    public IEnumerator SelectTamer()
    {
        bool active = false;
        SelectPermanentEffect selectPermanentEffect = null;

        if (_card != null)
        {
            if (_card.CanPlayBurst(_isPayCost))
            {
                if (_card.burstDigivolutionCondition != null)
                {
                    if (GManager.instance != null)
                    {
                        if (GManager.instance.turnStateMachine != null)
                        {
                            if (GManager.instance.turnStateMachine.gameContext != null)
                            {
                                if (GManager.instance.turnStateMachine.gameContext.ActiveCardList.Count >= 1)
                                {
                                    selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    if (selectPermanentEffect != null)
                                    {
                                        active = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (active)
        {
            Permanent selectedTamer = null;

            BurstDigivolutionCondition burstDigivolutionCondition = _card.burstDigivolutionCondition;

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (permanent != null)
                {
                    if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                    {
                        if (burstDigivolutionCondition.tamerCondition != null)
                        {
                            if (burstDigivolutionCondition.tamerCondition(permanent))
                            {
                                if (!permanent.CannotReturnToHand(null))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            }

            int maxCount = Math.Min(1, _card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

            if (maxCount >= 1)
            {
                selectPermanentEffect.SetUp(
                selectPlayer: _card.Owner,
                canTargetCondition: CanSelectPermanentCondition,
                canTargetCondition_ByPreSelecetedList: null,
                canEndSelectCondition: null,
                maxCount: maxCount,
                canNoSelect: _canNoSelect,
                canEndNotMax: false,
                selectPermanentCoroutine: SelectPermanentCoroutine,
                afterSelectPermanentCoroutine: null,
                mode: SelectPermanentEffect.Mode.Custom,
                cardEffect: null);

                selectPermanentEffect.SetUpCustomMessage($"Select {burstDigivolutionCondition.selectTamerMessage}.", $"The opponent is selecting {burstDigivolutionCondition.selectTamerMessage}.");

                if (this._isLocal)
                {
                    selectPermanentEffect.SetIsLocal();
                }

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    selectedTamer = permanent;

                    yield return null;
                }
            }

            //バースト進化しない
            if (selectedTamer == null)
            {
                if (_noSelectCoroutine != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(_noSelectCoroutine());
                }
            }

            //バースト進化する
            else
            {
                if (_endSelectCoroutine_Burst != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(_endSelectCoroutine_SelectTamer(selectedTamer));
                }
            }
        }
    }

    public IEnumerator BounceTamer(Permanent tamer)
    {
        TamerBounced = false;

        if (tamer != null)
        {
            if (tamer.TopCard != null)
            {
                if (tamer.TopCard.Owner.GetBattleAreaPermanents().Contains(tamer))
                {
                    if (!tamer.CannotReturnToHand(null))
                    {
                        Hashtable hashtable = new Hashtable();
                        hashtable.Add("IsBurst", true);

                        yield return ContinuousController.instance.StartCoroutine(new HandBounceClaass(new List<Permanent>() { tamer }, hashtable).Bounce());

                        if (tamer.TopCard == null && tamer.IsReturnedToHandByBurstDigivolution)
                        {
                            TamerBounced = true;
                        }
                    }
                }
            }
        }
    }

    public void AddTrashTopCardAtTurnEnd(Permanent permanent)
    {
        Permanent selectedPermanent = permanent;

        if (selectedPermanent != null)
        {
            if (selectedPermanent.TopCard != null)
            {
                ActivateClass activateClass1 = new ActivateClass();

                activateClass1.SetUpICardEffect("Trash this Digimon's top card\n(Burst Digivolution)", CanUseCondition2, selectedPermanent.TopCard);
                activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, 1, false, EffectDiscription1());
                activateClass1.SetEffectSourcePermanent(selectedPermanent);
                activateClass1.SetHashString("TrashBurstDigivolution");
                selectedPermanent.UntilEachTurnEndEffects.Add(GetCardEffect);

                string EffectDiscription1()
                {
                    return "At the end of the burst digivolution turn, trash this Digimon's top card";
                }

                ChangeDPClass rootEffect = new ChangeDPClass();
                rootEffect.SetUpICardEffect("", null, selectedPermanent.TopCard);
                activateClass1.SetRootCardEffect(rootEffect);

                bool CanUseCondition2(Hashtable hashtable1)
                {
                    if (selectedPermanent.TopCard != null)
                    {
                        if (selectedPermanent.TopCard.Owner.GetFieldPermanents().Contains(selectedPermanent))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition1(Hashtable hashtable1)
                {
                    if (selectedPermanent.TopCard != null)
                    {
                        if (selectedPermanent.TopCard.Owner.GetFieldPermanents().Contains(selectedPermanent))
                        {
                            if (selectedPermanent.DigivolutionCards.Count >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                {
                    if (selectedPermanent.TopCard != null)
                    {
                        if (selectedPermanent.TopCard.Owner.GetFieldPermanents().Contains(selectedPermanent))
                        {
                            if (selectedPermanent.DigivolutionCards.Count >= 1)
                            {
                                Permanent permanent = selectedPermanent;

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(permanent));

                                CardSource cardSource = permanent.TopCard;

                                yield return ContinuousController.instance.StartCoroutine(new AceOverflowClass(new List<CardSource>() { cardSource }).Overflow());

                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(cardSource));

                                if (!cardSource.IsToken)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(cardSource));
                                }

                                permanent.ShowingPermanentCard.ShowPermanentData(true);
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().RemoveDigivolveRootEffect(cardSource, permanent));
                            }
                        }
                    }
                }

                ICardEffect GetCardEffect(EffectTiming _timing)
                {
                    if (_timing == EffectTiming.OnEndTurn)
                    {
                        return activateClass1;
                    }

                    return null;
                }
            }
        }
    }
}
