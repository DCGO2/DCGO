using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class SelectAppFusionEffect : MonoBehaviour
{
    public void SetUp_SelectWheterToAppFusion
        (CardSource card,
        CardSource evoRoot,
        bool canNoSelect,
        Func<IEnumerator> endSelectCoroutine_Digivolve,
        Func<IEnumerator> endSelectCoroutine_AppFusion,
        Func<IEnumerator> noSelectCoroutine)
    {
        _card = card;
        _evoRoot = evoRoot;
        _canNoSelect = canNoSelect;
        _endSelectCoroutine_Digivolve = endSelectCoroutine_Digivolve;
        _endSelectCoroutine_AppFusion = endSelectCoroutine_AppFusion;
        _noSelectCoroutine = noSelectCoroutine;
    }

    public void SetUp_SelectLink
        (CardSource card,
        bool isLocal,
        bool isPayCost,
        bool canNoSelect,
        Func<CardSource, IEnumerator> endSelectCoroutine_SelectLink,
        Func<IEnumerator> noSelectCoroutine)
    {
        _card = card;
        _isLocal = isLocal;
        _isPayCost = isPayCost;
        _canNoSelect = canNoSelect;
        _endSelectCoroutine_SelectLink = endSelectCoroutine_SelectLink;
        _noSelectCoroutine = noSelectCoroutine;
    }

    CardSource _card = null;
    CardSource _evoRoot = null;
    bool _isLocal = false;

    bool _isPayCost = false;
    bool _canNoSelect = false;
    Func<IEnumerator> _endSelectCoroutine_Digivolve = null;
    Func<IEnumerator> _endSelectCoroutine_AppFusion = null;
    Func<CardSource, IEnumerator> _endSelectCoroutine_SelectLink = null;
    Func<IEnumerator> _noSelectCoroutine = null;
    public bool TamerBounced { get; private set; } = false;

    public IEnumerator SelectWheterToAppFusion()
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
                            titleStrings: new List<string>() { "Normal Digivolution", "<color=#FF633E>App Fusion</color>" }));

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
                            if (_endSelectCoroutine_AppFusion != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(_endSelectCoroutine_AppFusion());
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

    public IEnumerator SelectLink(Permanent targetPermanent)
    {
        bool active = false;
        SelectCardEffect selectCardEffect = null;

        if (_card != null && targetPermanent != null)
        {
            if (_card.CanPlayBurst(_isPayCost))
            {
                if (_card.appFusionCondition != null)
                {
                    if (GManager.instance != null)
                    {
                        if (GManager.instance.turnStateMachine != null)
                        {
                            if (GManager.instance.turnStateMachine.gameContext != null)
                            {
                                if (GManager.instance.turnStateMachine.gameContext.ActiveCardList.Count >= 1)
                                {
                                    selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                    if (selectCardEffect != null)
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
            CardSource selectedLink = null;

            AppFusionCondition appFusionCondition = _card.appFusionCondition;

            bool CanSelectSourceCondition(CardSource link)
            {
                return (link != null);
            }

            int maxCount = Math.Min(1, targetPermanent.LinkedCards.Count(CanSelectSourceCondition));

            if (maxCount >= 1)
            {
                selectCardEffect.SetUp(
                canTargetCondition: CanSelectSourceCondition,
                canTargetCondition_ByPreSelecetedList: null,
                canEndSelectCondition: null,
                canNoSelect: () => _canNoSelect,
                selectCardCoroutine: SelectCardCoroutine,
                afterSelectCardCoroutine: null,
                message: $"Select {appFusionCondition.selectLinkMessage}.",
                maxCount: maxCount,
                canEndNotMax: false,
                isShowOpponent: true,
                mode: SelectCardEffect.Mode.Custom,
                root: SelectCardEffect.Root.Custom,
                customRootCardList: targetPermanent.LinkedCards,
                canLookReverseCard: false,
                selectPlayer: _card.Owner,
                cardEffect: null);

                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                IEnumerator SelectCardCoroutine(CardSource source)
                {
                    selectedLink = source;

                    yield return null;
                }
            }

            //バースト進化しない
            if (selectedLink == null)
            {
                if (_noSelectCoroutine != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(_noSelectCoroutine());
                }
            }

            //バースト進化する
            else
            {
                if (_endSelectCoroutine_AppFusion != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(_endSelectCoroutine_SelectLink(selectedLink));
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

                        if (tamer.TopCard == null && tamer.IsAddedAsSourceByAppFusion)
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
                activateClass1.SetHashString("TrashAppFusion");
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
