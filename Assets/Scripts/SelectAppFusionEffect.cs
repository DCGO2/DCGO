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
        EvoRoot = evoRoot;
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
    public CardSource EvoRoot = null;
    bool _isLocal = false;

    bool _isPayCost = false;
    bool _canNoSelect = false;
    Func<IEnumerator> _endSelectCoroutine_Digivolve = null;
    Func<IEnumerator> _endSelectCoroutine_AppFusion = null;
    Func<CardSource, IEnumerator> _endSelectCoroutine_SelectLink = null;
    Func<IEnumerator> _noSelectCoroutine = null;
    public bool LinkAdded { get; private set; } = false;
    public CardSource selectedLink = null;

    public IEnumerator SelectWheterToAppFusion()
    {
        if (_card != null)
        {
            if (EvoRoot != null)
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
                            evoRootsArray: new CardSource[][] { new CardSource[] { EvoRoot }, new CardSource[] { EvoRoot } },
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

        if (_card != null && EvoRoot != null)
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

        if (active)
        {
            AppFusionCondition appFusionCondition = _card.appFusionCondition;

            bool CanSelectSourceCondition(CardSource link)
            {
                if(appFusionCondition != null)
                {
                    if (appFusionCondition.linkedCondition != null)
                    {
                        if (link != null)
                        {
                            if (appFusionCondition.linkedCondition(targetPermanent, link))
                            {
                                return true;
                            }
                        }
                    }
                }
                
                
                return false;
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
                message: $"Select 1 card to add as source.",
                maxCount: maxCount,
                canEndNotMax: false,
                isShowOpponent: true,
                mode: SelectCardEffect.Mode.Custom,
                root: SelectCardEffect.Root.Custom,
                customRootCardList: targetPermanent.LinkedCards,
                canLookReverseCard: false,
                selectPlayer: _card.Owner,
                cardEffect: null);

                if (this._isLocal)
                {
                    selectCardEffect.SetIsLocal();
                }

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

    public IEnumerator AddToSources(CardSource link)
    {
        LinkAdded = false;

        if (link != null)
        {
            if(link.PermanentOfThisCard() != null)
            {
                Permanent linkPermanent = link.PermanentOfThisCard();

                if (link.Owner.GetBattleAreaDigimons().Contains(linkPermanent))
                {
                    UnityEngine.Debug.Log($"ADD SOURCES: {linkPermanent}, {link}");
                    yield return ContinuousController.instance.StartCoroutine(linkPermanent.RemoveLinkedCard(link));
                    yield return ContinuousController.instance.StartCoroutine(linkPermanent.AddDigivolutionCardsTop(new List<CardSource> { link, linkPermanent.TopCard }, null));

                    LinkAdded = true;
                }
            }
        }
    }
}
