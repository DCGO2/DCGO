using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using DG.Tweening;
using TMPro;

public class SelectCardPanel : MonoBehaviour
{
    [Header("Message Text")]
    public TextMeshProUGUI MessageText;

    [Header("ScrollRect")]
    public ScrollRect scrollRect;

    [Header("Do not select button")]
    public GameObject NoSelectButton;

    [Header("Select Decision button")]
    public GameObject EndSelectButton;

    [Header("BackGround")]
    public GameObject BackGround;

    [Header("Parents other than background")]
    public GameObject Parent;

    [Header("Return to card selection button")]
    public Button ReturnToSelectCardButton;

    [Header("Text of the button not to be selected")]
    public Text NotSelectButtonText;

    [Header("Text of the selection end button")]
    public Text EndSelectButtonText;

    public Text CountText;

    //Selected card list
    public List<CardSource> SelectedList { get; set; } = new List<CardSource>();
    public List<int> SelectedIndex { get; set; } = new List<int>();
    //Eligibility Criteria for Eligible Cards
    Func<CardSource, bool> _canTargetCondition = null;

    //Whether the unit can be selected with the current selection list status
    Func<List<CardSource>, CardSource, bool> _canTargetCondition_ByPreSelecetedList = null;

    //Conditions under which a selection can be terminated (see list of selection termination points)
    Func<List<CardSource>, bool> _canEndSelectCondition = null;

    //Maximum number of cards that can be selected
    int _maxCount = 0;
    //Whether it can be terminated with less than the maximum number of cards
    bool _canEndNotMax = false;
    //Whether you can choose not to choose
    Func<bool> _canNoSelect = () => false;

    //provisional selection card list
    List<HandCard> _preSelectedHandCardList = new List<HandCard>();
    //Whether the selection has been completed or not
    bool _isEndSelection = false;
    //List of cards with scroll view
    List<HandCard> _handCards = new List<HandCard>();

    UnityAction _onClickNotSelectButtonAction;
    UnityAction _onClickEndSelectButtonAction;

    public string _customCountText = null;

    #region Notification that selection has been made
    public void SetIsEndSelection(bool isEndSelection)
    {
        _isEndSelection = isEndSelection;
    }
    #endregion

    //Select cards normally
    public IEnumerator OpenSelectCardPanel(string Message, List<CardSource> RootCardSources, Func<CardSource, bool> _CanTargetCondition, Func<List<CardSource>, CardSource, bool> _CanTargetCondition_ByPreSelecetedList, Func<List<CardSource>, bool> _CanEndSelectCondition, int _MaxCount, bool _CanEndNotMax, Func<bool> _CanNoSelect, bool CanLookReverseCard, List<SkillInfo> skillInfos, SelectCardEffect.Root root, bool isCenter = false, CardSource[][] evoRootsArray = null, List<string> titleStrings = null)
    {
        yield return ContinuousController.instance.StartCoroutine(OpenSelectCardPanel(Message, "No Selection", "End Selection", null, null, RootCardSources, _CanTargetCondition, _CanTargetCondition_ByPreSelecetedList, _CanEndSelectCondition, _MaxCount, _CanEndNotMax, _CanNoSelect, CanLookReverseCard, skillInfos, root, isCenter: isCenter, evoRootsArray: evoRootsArray, titleStrings: titleStrings));
    }

    //Select cards(Custom button text and click handling)
    public IEnumerator OpenSelectCardPanel(string Message, string NotSelectButtonMessage, string EndSelectButtonMessage, UnityAction _OnClickNotSelectButtonAction, UnityAction _OnClickEndSelectButtonAction, List<CardSource> RootCardSources, Func<CardSource, bool> _CanTargetCondition, Func<List<CardSource>, CardSource, bool> _CanTargetCondition_ByPreSelecetedList, Func<List<CardSource>, bool> _CanEndSelectCondition, int _MaxCount, bool _CanEndNotMax, Func<bool> _CanNoSelect, bool CanLookReverseCard, List<SkillInfo> skillInfos, SelectCardEffect.Root root, bool isCenter = false, CardSource[][] evoRootsArray = null, List<string> titleStrings = null)
    {
        #region Initialization
        this.gameObject.SetActive(true);

        _onClickNotSelectButtonAction = _OnClickNotSelectButtonAction;
        _onClickEndSelectButtonAction = _OnClickEndSelectButtonAction;

        NotSelectButtonText.text = NotSelectButtonMessage;
        EndSelectButtonText.text = EndSelectButtonMessage;

        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
        {
            GManager.instance.turnStateMachine.OffFieldCardTarget(player);
            GManager.instance.turnStateMachine.OffHandCardTarget(player);
        }

        GManager.instance.turnStateMachine.IsSelecting = true;

        BackGround.SetActive(true);

        Parent.SetActive(false);

        SetIsEndSelection(false);

        ReturnToSelectCardButton.gameObject.SetActive(false);

        _preSelectedHandCardList = new List<HandCard>();

        SelectedList = new List<CardSource>();

        SelectedIndex = new List<int>();

        _handCards = new List<HandCard>();

        MessageText.text = Message;

        _canTargetCondition = _CanTargetCondition;

        _canTargetCondition_ByPreSelecetedList = _CanTargetCondition_ByPreSelecetedList;

        _canEndSelectCondition = _CanEndSelectCondition;

        _maxCount = _MaxCount;

        _canEndNotMax = _CanEndNotMax;

        _canNoSelect = _CanNoSelect;

        if (!GManager.instance.turnStateMachine.DoneStartGame)
        {
            CountText.text = "";
        }

        else
        {
            if (!string.IsNullOrEmpty(_customCountText))
            {
                CountText.text = _customCountText;
            }

            else
            {
                if (_canEndNotMax)
                {
                    CountText.text = $"Select up to {_maxCount} card{Utils.PluralFormSuffix(_maxCount)}.";
                }

                else
                {
                    CountText.text = $"Select {_maxCount} card{Utils.PluralFormSuffix(_maxCount)}.";
                }
            }
        }
        #endregion

        yield return StartCoroutine(OpenSelectCardPanelAnimation(root));

        yield return StartCoroutine(OpenSelectCardPanelCoroutine(RootCardSources, CanLookReverseCard, skillInfos, isCenter: isCenter, evoRootsArray: evoRootsArray, titleStrings: titleStrings));

        CheckSelection();

        yield return new WaitWhile(() => !_isEndSelection);
        SetIsEndSelection(false);
    }

    IEnumerator OpenSelectCardPanelAnimation(SelectCardEffect.Root root)
    {
        bool end = false;
        var sequence = DOTween.Sequence();
        float time = 0.4f;

        this.transform.localScale = new Vector3(0.1f, 0.1f, 1f);

        if (root == SelectCardEffect.Root.Security)
        {
            this.transform.localPosition = new Vector3(-580f, -110f, 0f);
        }

        else
        {
            this.transform.localPosition = new Vector3(0f, 0f, 0f);
        }

        sequence
            .Append(this.transform.DOLocalMove(new Vector3(0f, 0f, 0f), time))
            .Join(this.transform.DOScale(new Vector3(1f, 1f, 1f), time))
            .AppendCallback(() => { end = true; });

        sequence.Play();

        yield return new WaitWhile(() => !end);
        end = false;
    }

    #region Open panel to generate card prefab
    IEnumerator OpenSelectCardPanelCoroutine(List<CardSource> RootCardSources, bool CanLookReverseCard, List<SkillInfo> skillInfos, bool isCenter, CardSource[][] evoRootsArray, List<string> titleStrings)
    {
        List<CardSource> root = new List<CardSource>();

        foreach (CardSource cardSource in RootCardSources)
        {
            root.Add(cardSource);
        }

        GridLayoutGroup grid = scrollRect.content.GetComponent<GridLayoutGroup>();

        if (grid != null)
        {
            if (isCenter)
            {
                grid.padding.left = 550;
                grid.spacing = new Vector2(600f, 0);
            }

            else
            {
                grid.padding.left = 248;
                grid.spacing = new Vector2(191.7f, 0);
            }
        }

        yield return new WaitForSeconds(Time.deltaTime);

        #region Do not select button
        NoSelectButton.SetActive(_canNoSelect());
        #endregion

        #region Initialize card list
        while (scrollRect.content.childCount > 0)
        {
            for (int i = 0; i < scrollRect.content.childCount; i++)
            {
                if (scrollRect.content.GetChild(i) != null)
                {
                    if (scrollRect.content.GetChild(i).gameObject != null)
                    {
                        Destroy(scrollRect.content.GetChild(i).gameObject);
                        yield return null;
                    }
                }
            }
        }

        yield return new WaitWhile(() => scrollRect.content.childCount > 0);
        #endregion

        #region card generation
        foreach (CardSource cardSource in root)
        {
            HandCard handCard = Instantiate(GManager.instance.handCardPrefab, scrollRect.content);
            handCard.gameObject.name = $"selectCardPanel_{cardSource.Owner.PlayerName}";

            handCard.GetComponent<Draggable_HandCard>().startScale = new Vector3(2.7f, 2.7f, 1);

            handCard.GetComponent<Draggable_HandCard>().DefaultY = -292;

            EventTrigger eventTrigger = handCard.CardImage.GetComponent<EventTrigger>();

            eventTrigger.triggers.Clear();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((x) => { PointerClick(cardSource); });

            #region Processing on click
            void PointerClick(CardSource cardSource1)
            {
                #region right click
                if (Input.GetMouseButtonUp(1))
                {
                    if (cardSource1 != null)
                    {
                        //If you can't see the face down card
                        if (!CanLookReverseCard)
                        {
                            if (!cardSource1.IsFlipped)
                            {
                                GManager.instance.cardDetail.OpenCardDetail(cardSource1, true);

                                GManager.instance.PlayDecisionSE();
                            }
                        }

                        //If you can see the card face down
                        else
                        {
                            GManager.instance.cardDetail.OpenCardDetail(cardSource1, true);

                            GManager.instance.PlayDecisionSE();
                        }
                    }
                }
                #endregion

                #region left click
                else if (Input.GetMouseButtonUp(0))
                {
                    handCard.OnClickAction?.Invoke(handCard);
                }
                #endregion
            }
            #endregion

            eventTrigger.triggers.Add(entry);

            handCard.SetUpHandCard(cardSource);

            if (!cardSource.IsFlipped || CanLookReverseCard)
            {
                handCard.SetUpHandCardImage();
            }

            else
            {
                handCard.SetUpReverseCard();
            }

            handCard.AddClickTarget(OnClickHandCard);

            _handCards.Add(handCard);
        }

        yield return new WaitWhile(() => scrollRect.content.childCount < root.Count);
        yield return new WaitWhile(() => scrollRect.content.childCount != _handCards.Count);

        yield return new WaitForSeconds(Time.deltaTime * 2);

        for (int i = 0; i < scrollRect.content.childCount; i++)
        {
            scrollRect.content.GetChild(i).localScale = new Vector3(2.7f, 2.7f, 1);
        }
        #endregion

        CheckSelection();

        Parent.SetActive(true);

        ReturnToSelectCardButton.gameObject.SetActive(false);

        scrollRect.horizontalNormalizedPosition = 0;

        yield return new WaitForSeconds(Time.deltaTime * 1f);

        #region 効果名を表示
        if (skillInfos != null)
        {
            List<Permanent> permanents = new List<Permanent>();

            foreach (HandCard handCard in _handCards)
            {
                int index = _handCards.IndexOf(handCard);

                if (0 <= index && index < skillInfos.Count)
                {
                    if (skillInfos[index] != null)
                    {
                        Permanent permanent = handCard.cardSource.PermanentOfThisCard();

                        if (permanent != null)
                        {
                            if (!permanents.Contains(permanent))
                            {
                                permanents.Add(permanent);
                            }
                        }
                    }
                }
            }

            foreach (HandCard handCard in _handCards)
            {
                int index = _handCards.IndexOf(handCard);

                if (0 <= index && index < skillInfos.Count)
                {
                    if (skillInfos[index] != null)
                    {
                        if (!string.IsNullOrEmpty(skillInfos[index].CardEffect.EffectName))
                        {
                            handCard.SetSkillName(skillInfos[index].CardEffect);
                            handCard.SetUpCardPositionText(permanents);
                        }

                        if (handCard.cardSource.PermanentOfThisCard() != null)
                        {
                            foreach (FieldPermanentCard fieldPermanentCard in handCard.cardSource.Owner.FieldPermanentObjects)
                            {
                                if (fieldPermanentCard.ThisPermanent != null)
                                {
                                    if (fieldPermanentCard.ThisPermanent.cardSources.Contains(handCard.cardSource))
                                    {
                                        fieldPermanentCard.SetPermanentIndexText(permanents);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region 進化元画像を表示
        if (evoRootsArray != null)
        {
            for (int i = 0; i < evoRootsArray.Length; i++)
            {
                if (i < _handCards.Count)
                {
                    _handCards[i].SetEvoRootCardImages(evoRootsArray[i]);
                }
            }
        }
        #endregion

        #region タイトルテキストを表示
        if (titleStrings != null)
        {
            for (int i = 0; i < titleStrings.Count; i++)
            {
                if (i < _handCards.Count)
                {
                    _handCards[i].SetTitleText(titleStrings[i]);
                }
            }
        }
        #endregion
    }
    #endregion

    #region Left-click processing
    public void OnClickHandCard(HandCard handCard)
    {
        if (_canTargetCondition(handCard.cardSource))
        {
            if (_preSelectedHandCardList.Contains(handCard))
            {
                _preSelectedHandCardList.Remove(handCard);
                SelectedIndex.Remove(_handCards.IndexOf(handCard));
            }

            else
            {
                if (_canTargetCondition_ByPreSelecetedList != null)
                {
                    List<CardSource> _PreSelectedList = new List<CardSource>();

                    for (int i = 0; i < _preSelectedHandCardList.Count; i++)
                    {
                        if (_preSelectedHandCardList.Count >= _maxCount && i == _preSelectedHandCardList.Count - 1)
                        {
                            continue;
                        }

                        _PreSelectedList.Add(_preSelectedHandCardList[i].cardSource);
                    }

                    if (!_canTargetCondition_ByPreSelecetedList(_PreSelectedList, handCard.cardSource))
                    {
                        return;
                    }
                }

                if (_preSelectedHandCardList.Count < _maxCount)
                {
                    _preSelectedHandCardList.Add(handCard);
                    SelectedIndex.Add(_handCards.IndexOf(handCard));
                }

                else
                {
                    if (_preSelectedHandCardList.Count > 0)
                    {
                        _preSelectedHandCardList.RemoveAt(_preSelectedHandCardList.Count - 1);
                        SelectedIndex.RemoveAt(SelectedIndex.Count - 1);
                        _preSelectedHandCardList.Add(handCard);
                        SelectedIndex.Add(_handCards.IndexOf(handCard));
                    }
                }

                if (!ContinuousController.instance.checkBeforeEndingSelection
                && !_canNoSelect()
                && !_canEndNotMax
                && _maxCount == _preSelectedHandCardList.Count)
                {
                    OnClickEndSelectButton();
                    return;
                }
            }

            CheckSelection();
        }
    }
    #endregion

    #region UI reflects whether the selection can be terminated.
    public void CheckSelection()
    {
        EndSelectButton.SetActive(CanEndSelection());

        foreach (HandCard handCard in _handCards)
        {
            handCard.RemoveSelectEffect();
            handCard.OffSelectedIndexText();

            if (_canTargetCondition(handCard.cardSource))
            {
                if (_preSelectedHandCardList.Contains(handCard))
                {
                    handCard.SetOrangeOutline();
                    handCard.OnOutline();
                    handCard.SetSelectedIndexText(_preSelectedHandCardList.IndexOf(handCard) + 1);
                }

                else
                {
                    handCard.SetBlueOutline();
                    handCard.OnOutline();

                    if (_canTargetCondition_ByPreSelecetedList != null)
                    {
                        List<CardSource> _PreSelectedList = new List<CardSource>();

                        for (int i = 0; i < _preSelectedHandCardList.Count; i++)
                        {
                            if (_preSelectedHandCardList.Count >= _maxCount && i == _preSelectedHandCardList.Count - 1)
                            {
                                continue;
                            }

                            _PreSelectedList.Add(_preSelectedHandCardList[i].cardSource);
                        }

                        if (!_canTargetCondition_ByPreSelecetedList(_PreSelectedList, handCard.cardSource))
                        {
                            handCard.RemoveSelectEffect();
                        }
                    }

                    if (_preSelectedHandCardList.Count >= _maxCount)
                    {
                        handCard.RemoveSelectEffect();
                    }
                }
            }
        }

        if (_preSelectedHandCardList.Count >= _maxCount)
        {
            NoSelectButton.SetActive(false);
        }

        else
        {
            NoSelectButton.SetActive(_canNoSelect());
        }
    }
    #endregion

    #region Determines if selection can be terminated
    bool CanEndSelection()
    {
        if (_maxCount <= 0) return true;

        List<CardSource> _PreSelectedList = new List<CardSource>();

        foreach (HandCard handCard1 in _preSelectedHandCardList)
        {
            _PreSelectedList.Add(handCard1.cardSource);
        }

        if (_canEndSelectCondition != null)
        {
            if (!_canEndSelectCondition(_PreSelectedList))
            {
                return false;
            }
        }

        if (_canEndNotMax)
        {
            return true;
        }

        else
        {
            if (_PreSelectedList.Count == _maxCount)
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Exit card selection
    public void CloseSelectCardPanel()
    {
        this.gameObject.SetActive(false);

        ReturnToSelectCardButton.gameObject.SetActive(false);
    }
    #endregion

    #region Processing when a button is pressed that does not select anything
    public void OnClickNotSelectButton()
    {
        GManager.instance.PlayDecisionSE();

        SelectedList = new List<CardSource>();
        CloseSelectCardPanel();

        SetIsEndSelection(true);

        ContinuousController.instance.StartCoroutine(OnClickButtonActionCoroutine(_onClickNotSelectButtonAction));
    }
    #endregion

    #region Processing when the Select End button is pressed
    public void OnClickEndSelectButton()
    {
        if (CanEndSelection())
        {
            GManager.instance.PlayDecisionSE();

            List<CardSource> _PreSelectedList = new List<CardSource>();

            foreach (HandCard handCard1 in _preSelectedHandCardList)
            {
                _PreSelectedList.Add(handCard1.cardSource);
            }

            SelectedList = new List<CardSource>();

            foreach (CardSource cardSource in _PreSelectedList)
            {
                SelectedList.Add(cardSource);
            }

            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
            {
                foreach (FieldPermanentCard fieldPermanentCard in player.FieldPermanentObjects)
                {
                    fieldPermanentCard.OffPermanentIndexText();
                }
            }

            CloseSelectCardPanel();

            SetIsEndSelection(true);

            ContinuousController.instance.StartCoroutine(OnClickButtonActionCoroutine(_onClickEndSelectButtonAction));
        }
    }


    IEnumerator OnClickButtonActionCoroutine(UnityAction Action)
    {
        yield return new WaitForSeconds(0.1f);

        Action?.Invoke();
    }
    #endregion

    #region Temporarily display/hide panel
    public void OnClickReturnToSelectCardButton()
    {
        this.gameObject.SetActive(true);
        ReturnToSelectCardButton.gameObject.SetActive(false);
    }

    public void OnClickCheckFieldButton()
    {
        this.gameObject.SetActive(false);
        ReturnToSelectCardButton.gameObject.SetActive(true);
        ReturnToSelectCardButton.onClick.RemoveAllListeners();
        ReturnToSelectCardButton.onClick.AddListener(() => OnClickReturnToSelectCardButton());
    }
    #endregion
}
