using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class EditDeck : MonoBehaviour
{
    [Header("Deck creation object")]
    public GameObject CreateDeckObject;

    [Header("card prefab")]
    public CardPrefab_CreateDeck cardPrefab_CreateDeck;

    [Header("card pool scroll rect")]
    public ScrollRect CardPoolScroll;

    [Header("Deck card scroll rect")]
    public ScrollRect DeckScroll;

    [Header("Deck card number display text")]
    public Text DeckCountText;

    [Header("loading object")]
    public LoadingObject LoadingObjec;

    [Header("Card details display")]
    public DetailCard_DeckEditor DetailCard;

    [Header("filter")]
    public FilterCardList filterCardList;

    /*
    [Header("フィルターパネル")]
    public FilterPanel filterPanel;
    */

    [Header("Deck name input field")]
    public InputField DeckNameInputField;

    //Deck data being edited
    public DeckData EdittingDeckData { get; set; }

    bool _isFromSelectDeck = false;

    public UnityAction EndEditAction;

    public int DisplayPageIndex = 0;

    public Button UpDisplayPageIndexButton;
    public Button DownDisplayPageIndexButton;
    public Text ShowDisplayPageIndexText;
    public LoadingObject isSearchingObject;

    [Header("Page switching SE")]
    public AudioClip SwitchPageSE;

    [Header("Card classification display")]
    [SerializeField] CardDistribution cardDistribution;
    int _maxDisplayPageIndex;
    string _oldDeckName = "";
    List<CEntity_Base> _oldDeckCards = new List<CEntity_Base>();
    List<CEntity_Base> _oldDigitamaDeckCards = new List<CEntity_Base>();
    int _oldKeyCardId = -1;
    bool _isFromClipboard = false;

    private void Start()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        CardPoolScroll.content.GetComponent<GridLayoutGroup>().constraintCount = 5;
#endif
    }

    void CheckButtonEnabled()
    {
        UpDisplayPageIndexButton.interactable = DisplayPageIndex < _maxDisplayPageIndex;
        UpDisplayPageIndexButton.transform.GetChild(0).gameObject.SetActive(DisplayPageIndex < _maxDisplayPageIndex);
        DownDisplayPageIndexButton.interactable = DisplayPageIndex > 0;
        DownDisplayPageIndexButton.transform.GetChild(0).gameObject.SetActive(DisplayPageIndex > 0);

        ShowDisplayPageIndexText.text = $"{DisplayPageIndex + 1}/{_maxDisplayPageIndex + 1}";

        CardPoolScroll.verticalNormalizedPosition = 1;
    }

    public void UpDisplayPageIndex()
    {
        ContinuousController.instance.PlaySE(SwitchPageSE);

        DisplayPageIndex++;

        if (DisplayPageIndex > _maxDisplayPageIndex)
        {
            DisplayPageIndex = _maxDisplayPageIndex;
        }

        SetCardData();
        CheckButtonEnabled();
        ReflectDeckData();
    }

    public void DownDisplayPageIndex()
    {
        ContinuousController.instance.PlaySE(SwitchPageSE);

        DisplayPageIndex--;

        if (DisplayPageIndex < 0)
        {
            DisplayPageIndex = 0;
        }

        SetCardData();
        CheckButtonEnabled();
        ReflectDeckData();
    }

    List<List<CEntity_Base>> SprittedCardLists = new List<List<CEntity_Base>>();

    //List<CEntity_Base> ActiveCardList = new List<CEntity_Base>();

    IEnumerator SetSprittedCardLists()
    {
        SprittedCardLists = new List<List<CEntity_Base>>();

        for (int i = 0; i < _maxDisplayPageIndex + 1; i++)
        {
            SprittedCardLists.Add(new List<CEntity_Base>());
        }

        List<CEntity_Base> cEntity_Bases = new List<CEntity_Base>();

        foreach (CEntity_Base cEntity_Base in ContinuousController.instance.CardList)
        {
            if (MatchCondition(cEntity_Base))
            {
                cEntity_Bases.Add(cEntity_Base);
            }
        }

        for (int i = 0; i < cEntity_Bases.Count; i++)
        {
            CEntity_Base cEntity_Base = cEntity_Bases[i];

            int Quotient = i / _cardPoolPrefabs.Count;

            if (0 <= Quotient && Quotient < SprittedCardLists.Count)
            {
                SprittedCardLists[Quotient].Add(cEntity_Base);
            }
        }

        yield return null;
    }

    void SetCardData()
    {
        for (int i = 0; i < _cardPoolPrefabs.Count; i++)
        {
            if (i < SprittedCardLists[DisplayPageIndex].Count)
            {
                _cardPoolPrefabs[i].isActive = true;
                _cardPoolPrefabs[i].SetUpCardPrefab_CreateDeck(SprittedCardLists[DisplayPageIndex][i]);
            }

            else
            {
                _cardPoolPrefabs[i].isActive = false;
            }
        }
    }

    #region Card details display
    public void OffDetailCard()
    {
        DetailCard.OffDetailCard();
    }

    public void OnDetailCard(CEntity_Base cEntity_Base)
    {
        if (isDragging)
        {
            return;
        }

        DetailCard.SetUpDetailCard(cEntity_Base);
    }
    #endregion

    int _frameCount = 0;
    int _updateFrame = 3;

    private void Update()
    {
        #region Updates only once every few frames
        _frameCount++;

        if (_frameCount < _updateFrame)
        {
            return;
        }

        else
        {
            _frameCount = 0;
        }
        #endregion

        //TODO: need to find a better way, this shouldn't happen every 3rd frame, should only happen on changes. MikeB
        ShowOnlyVisibleObjects();

        if (EdittingDeckData != null)
        {
            TrialDrawButton.gameObject.SetActive(EdittingDeckData.DeckCards().Count >= 1);
            ClearDeckButton.gameObject.SetActive(EdittingDeckData.AllDeckCards().Count >= 1);
        }
    }

    #region Show only visible cards
    public void ShowOnlyVisibleObjects()
    {
        if (DoneSetUp)
        {
            foreach (CardPrefab_CreateDeck cardPrefab_CreateDeck in _cardPoolPrefabs)
            {
                if (!cardPrefab_CreateDeck.isActive)
                {
                    cardPrefab_CreateDeck.gameObject.SetActive(false);
                }

                else
                {
                    cardPrefab_CreateDeck.gameObject.SetActive(true);

                    if (!((RectTransform)cardPrefab_CreateDeck.transform).IsVisibleFrom(Opening.instance.MainCamera))
                    {
                        cardPrefab_CreateDeck.CardImage.transform.parent.gameObject.SetActive(false);
                    }

                    else
                    {
                        cardPrefab_CreateDeck.CardImage.transform.parent.gameObject.SetActive(true);

                        if (cardPrefab_CreateDeck.cEntity_Base != null)
                        {
                            //if (cardPrefab_CreateDeck.cEntity_Base.CardSprite != null)
                            {
                                cardPrefab_CreateDeck.ShowCardImage();
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion

    public bool isEditting { get; set; } = false;

    #region Close deck editing screen
    public void OnClickCompleteEditButton()
    {
        ContinuousController.instance.PlaySE(Opening.instance.DecisionSE);
        ContinuousController.instance.StartCoroutine(CloseCreateDeckCoroutine(false));
    }

    public void OnClickCancelEditButton()
    {
        ContinuousController.instance.PlaySE(Opening.instance.CancelSE);
        ContinuousController.instance.StartCoroutine(CloseCreateDeckCoroutine(true));
    }

    public IEnumerator CloseCreateDeckCoroutine(bool Canceled)
    {
        isEditting = false;
        yield return ContinuousController.instance.StartCoroutine(LoadingObjec.StartLoading("Now Loading"));

        Opening.instance.OffYesNoObjects();

        Opening.instance.deck.trialDraw.Close();

        Opening.instance.deck.deckListPanel.Close();

        for (int i = 0; i < Opening.instance.deck.selectDeck.deckInfoPrefabParentScroll.content.childCount; i++)
        {
            CreateNewDeckButton createNewDeckButton = Opening.instance.deck.selectDeck.deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<CreateNewDeckButton>();

            if (createNewDeckButton != null)
            {
                createNewDeckButton.CreateNewDeckWayObject.Close_(false);
                break;
            }
        }

        if (Canceled)
        {
            EdittingDeckData.DeckCardIDs = DeckData.GetDeckCardCodes(DeckData.SortedDeckCardsList(_oldDeckCards));
            EdittingDeckData.DigitamaDeckCardIDs = DeckData.GetDeckCardCodes(DeckData.SortedDeckCardsList(_oldDigitamaDeckCards));
            EdittingDeckData.DeckName = _oldDeckName;
            EdittingDeckData.KeyCardId = _oldKeyCardId;
        }

        if (!EdittingDeckData.AllDeckCards().Contains(EdittingDeckData.KeyCard))
        {
            EdittingDeckData.KeyCardId = -1;
        }

        #region not save if deck is empty or canceled when importing from clipboard
        if (EdittingDeckData != null && (EdittingDeckData.DeckCards().Count == 0 && EdittingDeckData.DigitamaDeckCards().Count == 0 || (Canceled && _isFromClipboard)))
        {
            ContinuousController.instance.DeckDatas.Remove(EdittingDeckData);
        }
        #endregion

        ContinuousController.instance.SaveDeckDatas();
        ContinuousController.instance.SaveDeckData(EdittingDeckData);

        for (int i = 0; i < DeckScroll.content.childCount; i++)
        {
            Destroy(DeckScroll.content.GetChild(i).gameObject);
        }

        if (!Canceled)
        {
            EdittingDeckData.DeckCardIDs = DeckData.GetDeckCardCodes(DeckData.SortedDeckCardsList(EdittingDeckData.DeckCards()));
            EdittingDeckData.DigitamaDeckCardIDs = DeckData.GetDeckCardCodes(DeckData.SortedDeckCardsList(EdittingDeckData.DigitamaDeckCards()));
        }

        foreach (CardPrefab_CreateDeck cardPrefab_CreateDeck in _cardPoolPrefabs)
        {
            if (cardPrefab_CreateDeck._OnEnterCoroutine != null)
            {
                cardPrefab_CreateDeck.StopCoroutine(cardPrefab_CreateDeck._OnEnterCoroutine);
                cardPrefab_CreateDeck._OnEnterCoroutine = null;
            }

            cardPrefab_CreateDeck.StopAllCoroutines();

            cardPrefab_CreateDeck.gameObject.SetActive(false);
        }

        yield return new WaitUntil(() => DeckScroll.content.childCount == 0);

        yield return new WaitForSeconds(0.1f);

        CreateDeckObject.SetActive(false);

        #region Set up previous screen
        if (_isFromSelectDeck)
        {
            Opening.instance.deck.selectDeck.ResetDeckInfoPanel();

            yield return ContinuousController.instance.StartCoroutine(Opening.instance.deck.selectDeck.SetDeckList(false));

            for (int i = 0; i < Opening.instance.deck.selectDeck.deckInfoPrefabParentScroll.content.childCount; i++)
            {
                if (Opening.instance.deck.selectDeck.deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<DeckInfoPrefab>() != null)
                {
                    if (Opening.instance.deck.selectDeck.deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<DeckInfoPrefab>().thisDeckData == EdittingDeckData)
                    {
                        if (Opening.instance.deck.selectDeck.deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<DeckInfoPrefab>().thisDeckData != null)
                        {
                            if (Opening.instance.deck.selectDeck.deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<DeckInfoPrefab>().thisDeckData.DeckCards().Count > 0)
                            {
                                Opening.instance.deck.selectDeck.deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<DeckInfoPrefab>().OnClick_DeckInfoPrefab(false);
                            }
                        }
                    }
                }
            }

            if (EdittingDeckData != null)
            {
                ContinuousController.instance.BattleDeckData = EdittingDeckData;
            }
        }

        else
        {
            Opening.instance.battle.selectBattleDeck.ResetDeckInfoPanel();

            yield return ContinuousController.instance.StartCoroutine(Opening.instance.battle.selectBattleDeck.SetDeckList(false));

            for (int i = 0; i < Opening.instance.battle.selectBattleDeck.deckInfoPrefabParentScroll.content.childCount; i++)
            {
                if (Opening.instance.battle.selectBattleDeck.deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<DeckInfoPrefab>() != null)
                {
                    if (Opening.instance.battle.selectBattleDeck.deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<DeckInfoPrefab>().thisDeckData == EdittingDeckData)
                    {
                        Opening.instance.battle.selectBattleDeck.deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<DeckInfoPrefab>().OnClick_DeckInfoPrefab(false);
                    }
                }
            }
        }
        #endregion

        EndEditAction?.Invoke();

        EndEditAction = null;

        yield return ContinuousController.instance.StartCoroutine(LoadingObjec.EndLoading());
    }
    #endregion

    #region Open deck editing screen
    public void SetUpCreateDeck(DeckData deckData, bool isFromSelectDeck, bool isFromClipboard = false)
    {
        if (deckData == null)
        {
            return;
        }

        _isFromClipboard = isFromClipboard;

        Opening.instance.OffYesNoObjects();

        Opening.instance.deck.trialDraw.Close();

        Opening.instance.deck.deckListPanel.Close();

        _oldDeckName = deckData.DeckName;

        _oldDeckCards = new List<CEntity_Base>();

        foreach (CEntity_Base cEntity_Base in deckData.DeckCards())
        {
            _oldDeckCards.Add(cEntity_Base);
        }

        _oldDigitamaDeckCards = new List<CEntity_Base>();

        foreach (CEntity_Base cEntity_Base in deckData.DigitamaDeckCards())
        {
            _oldDigitamaDeckCards.Add(cEntity_Base);
        }

        _oldKeyCardId = deckData.KeyCardId;

        _checkCoverCoroutine = null;
        this._isFromSelectDeck = isFromSelectDeck;
        DisplayPageIndex = 0;
        SetCardData();
        CheckButtonEnabled();
        isSearchingObject.Off();
        ContinuousController.instance.StartCoroutine(SetUpCreateDeckCoroutine(deckData));
    }

    public IEnumerator SetUpCreateDeckCoroutine(DeckData deckData)
    {
        yield return ContinuousController.instance.StartCoroutine(LoadingObjec.StartLoading("Now Loading"));

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < draggable_CardParent.childCount; i++)
        {
            Destroy(draggable_CardParent.GetChild(i).gameObject);
        }

        DraggingCover.SetActive(false);

        CardPoolScroll.verticalNormalizedPosition = 1;
        DeckScroll.verticalNormalizedPosition = 1;

        deckData.DeckCardIDs = DeckData.GetDeckCardCodes(DeckData.SortedDeckCardsList(deckData.DeckCards()));
        deckData.DigitamaDeckCardIDs = DeckData.GetDeckCardCodes(DeckData.SortedDeckCardsList(deckData.DigitamaDeckCards()));

        EdittingDeckData = deckData;

        ContinuousController.instance.SaveDeckDatas();
        //ContinuousController.instance.SaveDeckData(EdittingDeckData);

        OffDetailCard();

        for (int i = 0; i < DeckScroll.content.childCount; i++)
        {
            Destroy(DeckScroll.content.GetChild(i).gameObject);
        }

        yield return new WaitUntil(() => DeckScroll.content.childCount == 0);

        for (int i = 0; i < DeckScroll.viewport.childCount; i++)
        {
            if (i != 0)
            {
                Destroy(DeckScroll.viewport.GetChild(i).gameObject);
            }
        }

        yield return new WaitUntil(() => DeckScroll.viewport.childCount == 1);

        _maxDisplayPageIndex = (ContinuousController.instance.CardList.Length - 1) / _cardPoolPrefabs.Count;

        if (deckData != null)
        {
            List<CEntity_Base> DeckCards = new List<CEntity_Base>();

            foreach (CEntity_Base cEntity_Base in deckData.AllDeckCards())
            {
                DeckCards.Add(cEntity_Base);
            }

            foreach (CEntity_Base cEntity_Base in DeckCards)
            {
                CreateDeckCard(cEntity_Base);
            }
        }

        CreateDeckObject.SetActive(true);

        foreach (CardPrefab_CreateDeck _CardPrefab_CreateDeck in _cardPoolPrefabs)
        {
            _CardPrefab_CreateDeck.isActive = true;

            if (_CardPrefab_CreateDeck._OnEnterCoroutine != null)
            {
                _CardPrefab_CreateDeck.StopCoroutine(_CardPrefab_CreateDeck._OnEnterCoroutine);
                _CardPrefab_CreateDeck._OnEnterCoroutine = null;
            }
        }

        filterCardList.Init(() => ContinuousController.instance.StartCoroutine(ShowPoolCard_MatchCondition()));

        DeckNameInputField.onEndEdit.RemoveAllListeners();
        DeckNameInputField.text = EdittingDeckData.DeckName;
        DeckNameInputField.onEndEdit.AddListener(OnEndEdit);

        yield return ContinuousController.instance.StartCoroutine(SetSprittedCardLists());

        SetCardData();

        CheckButtonEnabled();

        ReflectDeckData();

        cardDistribution.SetCardDistribution(EdittingDeckData);

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < DeckScroll.content.childCount; i++)
        {
            DeckScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>().ShowCardImage();
        }

        yield return StartCoroutine(LoadingObjec.EndLoading());
        isEditting = true;
    }

    public void OnEndEdit(string text)
    {
        ContinuousController.instance.RenameDeck(EdittingDeckData, text);

        DeckNameInputField.onEndEdit.RemoveAllListeners();
        DeckNameInputField.text = text;
        DeckNameInputField.onEndEdit.AddListener(OnEndEdit);
    }
    #endregion

    public List<CardPrefab_CreateDeck> CardPoolCardPrefabs_CreateDeck = new List<CardPrefab_CreateDeck>();

    #region Initialize the deck editing screen at the start of the game
    public List<CardPrefab_CreateDeck> cardPoolPrefabs_all = new List<CardPrefab_CreateDeck>();
    List<CardPrefab_CreateDeck> _cardPoolPrefabs = new List<CardPrefab_CreateDeck>();
    bool DoneSetUp { get; set; } = false;
    public IEnumerator InitEditDeck()
    {
        cardDistribution.Init();

        for (int i = 0; i < CardPoolScroll.content.childCount; i++)
        {
            CardPrefab_CreateDeck cardPrefab_CreateDeck = CardPoolScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>();

            if (cardPrefab_CreateDeck != null)
            {
                CardPoolCardPrefabs_CreateDeck.Add(cardPrefab_CreateDeck);
            }
        }

        for (int i = 0; i < DeckScroll.content.childCount; i++)
        {
            Destroy(DeckScroll.content.GetChild(i).gameObject);
        }

        foreach (CardPrefab_CreateDeck cardPrefab_CreateDeck in cardPoolPrefabs_all)
        {
            if (cardPrefab_CreateDeck.gameObject.activeSelf)
            {
                cardPrefab_CreateDeck.gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < cardPoolPrefabs_all.Count; i++)
        {
            if (i < ContinuousController.instance.CardList.Length)
            {
                CardPrefab_CreateDeck _cardPrefab_CreateDeck = cardPoolPrefabs_all[i];
                _cardPoolPrefabs.Add(_cardPrefab_CreateDeck);
                CEntity_Base cEntity_Base = ContinuousController.instance.CardList[i];

                foreach (ScrollRect _scroll in _cardPrefab_CreateDeck.scroll)
                {
                    _scroll.content = CardPoolScroll.content;

                    _scroll.viewport = CardPoolScroll.viewport;

                    _scroll.enabled = false;
                }

                _cardPrefab_CreateDeck.OnClickAction = () =>
                {
                    StartCoroutine(AddDeckCardCoroutine_OnClick(_cardPrefab_CreateDeck));
                };

                _cardPrefab_CreateDeck.OnBeginDragAction = (cardPrefab_CreateDeck) => { StartCoroutine(OnBeginDrag(cardPrefab_CreateDeck)); };

                _cardPrefab_CreateDeck.OnEnterAction = (cardPrefab) =>
                {
                    OnDetailCard(_cardPrefab_CreateDeck.cEntity_Base);
                };
            }
        }

        yield return ContinuousController.instance.StartCoroutine(SetSprittedCardLists());

        yield return new WaitForSeconds(0.1f);

        DoneSetUp = true;
    }
    #endregion

    #region Changed deck content changes to UI
    public void ReflectDeckData()
    {
        if (EdittingDeckData != null)
        {
            //Turn on covers for cards that cannot be included in the deck
            if (_checkCoverCoroutine != null)
            {
                StopCoroutine(_checkCoverCoroutine);
            }

            _checkCoverCoroutine = StartCoroutine(CheckCoverIEnumerator());

            SetDeckCountText();
        }
    }

    Coroutine _checkCoverCoroutine = null;
    IEnumerator CheckCoverIEnumerator()
    {
        foreach (CardPrefab_CreateDeck cardPrefab_CreateDeck in _cardPoolPrefabs)
        {
            yield return null;
            cardPrefab_CreateDeck.CheckCover(EdittingDeckData);
        }
    }
    #endregion

    #region Show deck number text
    public void SetDeckCountText()
    {
        DeckCountText.text = $"{EdittingDeckData.DeckCards().Count}+{EdittingDeckData.DigitamaDeckCards().Count}/50+5";

        if (EdittingDeckData.IsValidDeckData())
        {
            DeckCountText.color = new Color32(69, 255, 69, 255);
        }

        else
        {
            DeckCountText.color = new Color32(255, 64, 64, 255);
        }
    }
    #endregion

    #region Generate cards for deck
    public CardPrefab_CreateDeck CreateDeckCard(CEntity_Base cEntity_Base)
    {
        CardPrefab_CreateDeck _cardPrefab_CreateDeck = Instantiate(cardPrefab_CreateDeck, DeckScroll.content);

        SetUpDeckCard(_cardPrefab_CreateDeck, cEntity_Base);

        return _cardPrefab_CreateDeck;
    }

    public void SetUpDeckCard(CardPrefab_CreateDeck _cardPrefab_CreateDeck, CEntity_Base cEntity_Base)
    {
        foreach (ScrollRect _scroll in _cardPrefab_CreateDeck.scroll)
        {
            _scroll.content = DeckScroll.content;

            _scroll.viewport = DeckScroll.viewport;
        }

        _cardPrefab_CreateDeck.SetUpCardPrefab_CreateDeck(cEntity_Base);
        _cardPrefab_CreateDeck.ShowCardImage();

        _cardPrefab_CreateDeck.OnClickAction = () =>
        {
            StartCoroutine(RemoveDeckCardCoroutine_OnClick(_cardPrefab_CreateDeck));
        };

        _cardPrefab_CreateDeck.OnBeginDragAction = (cardPrefab_CreateDeck) => { StartCoroutine(OnBeginDrag(cardPrefab_CreateDeck)); };

        _cardPrefab_CreateDeck.OnEnterAction = (cardPrefab) =>
        {
            OnDetailCard(cEntity_Base);
            cardPrefab.SetupAddRemoveButton(EdittingDeckData);
        };

        _cardPrefab_CreateDeck.OnExitAction = (cardPrefab) =>
        {
            cardPrefab.OffAddRemoveButton();
        };

        _cardPrefab_CreateDeck.Parent.localScale = new Vector3(0.76f, 0.76f, 0.76f);

        _cardPrefab_CreateDeck.OnClickAddButtonAction = (cardPrefab) => StartCoroutine(AddDeckCardCoroutine_OnClick(cardPrefab));
        _cardPrefab_CreateDeck.OnClickRemoveButtonAction = (cardPrefab) => StartCoroutine(RemoveDeckCardCoroutine_OnClick(cardPrefab));
    }
    #endregion

    #region Display only cards that meet search/filter conditions
    bool MatchCondition(CEntity_Base cEntity_Base)
    {
        if (!filterCardList.OnlyContainsName()(cEntity_Base))
        {
            return false;
        }

        if (!filterCardList.OnlyContainsColor()(cEntity_Base))
        {
            return false;
        }

        if (!filterCardList.OnlyMatchRarity()(cEntity_Base))
        {
            return false;
        }

        if (!filterCardList.OnlyMatchCardKind()(cEntity_Base))
        {
            return false;
        }

        if (!filterCardList.OnlyMatchCardSet()(cEntity_Base))
        {
            return false;
        }

        if (!filterCardList.OnlyMatchPlayCost()(cEntity_Base))
        {
            return false;
        }

        if (!filterCardList.OnlyMatchLevel()(cEntity_Base))
        {
            return false;
        }

        if (!filterCardList.OnlyMatchSpecialCardKind()(cEntity_Base))
        {
            return false;
        }

        if (!filterCardList.OnlyMatchParallelCondition()(cEntity_Base))
        {
            return false;
        }

        return true;
    }

    public IEnumerator ShowPoolCard_MatchCondition()
    {
        DraggingCover.SetActive(true);

        DisplayPageIndex = 0;

        _maxDisplayPageIndex = (ContinuousController.instance.CardList.Count((cEntity_Base) => MatchCondition(cEntity_Base)) - 1) / _cardPoolPrefabs.Count;

        yield return ContinuousController.instance.StartCoroutine(SetSprittedCardLists());

        SetCardData();

        CheckButtonEnabled();

        ReflectDeckData();

        DraggingCover.SetActive(false);

        for (int i = 0; i < CardPoolCardPrefabs_CreateDeck.Count; i++)
        {
            CardPrefab_CreateDeck cardPrefab_CreateDeck = CardPoolCardPrefabs_CreateDeck[i];

            if (cardPrefab_CreateDeck != null)
            {
                cardPrefab_CreateDeck.uIShiny.enabled = false;

                if (cardPrefab_CreateDeck._OnEnterCoroutine != null)
                {
                    cardPrefab_CreateDeck.StopCoroutine(cardPrefab_CreateDeck._OnEnterCoroutine);
                    cardPrefab_CreateDeck._OnEnterCoroutine = null;
                }

                cardPrefab_CreateDeck.StopAllCoroutines();
            }
        }
    }
    #endregion

    #region Manipulating card objects

    [Header("draggable card object")]
    public Draggable_Card draggable_CardPrefab;

    [Header("draggable card object parent")]
    public Transform draggable_CardParent;

    [Header("Deck card dropArea")]
    public DropArea DeckCardsDropArea;

    [Header("card pool dropArea")]
    public DropArea CardPoolDropArea;

    [Header("Cover while dragging")]
    public GameObject DraggingCover;

    [Header("Coordinates of the disappearing deck card")]
    public Transform DisappearDeckCardTransform;

    //Is it draggable?
    public bool CanDrag { get; set; } = true;

    //Whether you are dragging
    public bool isDragging { get; set; } = false;

    #region Generate draggable objects
    public Draggable_Card CreateDraggable_Card()
    {
        Draggable_Card draggable_Card = Instantiate(draggable_CardPrefab, draggable_CardParent);

        draggable_Card.DefaultParent = draggable_CardParent;
        draggable_Card.transform.localScale = new Vector3(1, 1, 1);
        draggable_Card.CardImage.color = new Color(1, 1, 1, 1);

        return draggable_Card;
    }
    #endregion

    #region At the start of drag
    public IEnumerator OnBeginDrag(CardPrefab_CreateDeck _cardPrefab_CreateDeck)
    {
        if (_cardPrefab_CreateDeck.transform.parent == CardPoolScroll.content)
        {
            if (!DeckBuildingRule.CanAddCard(_cardPrefab_CreateDeck.cEntity_Base, EdittingDeckData))
            {
                yield break;
            }
        }

        if (Input.GetMouseButton(1))
        {
            yield break;
        }

        if (!isDragging && CanDrag)
        {
            Draggable_Card draggable_Card = CreateDraggable_Card();

            SetStartDrag(_cardPrefab_CreateDeck, draggable_Card);

            draggable_Card.OnDropAction = (dropAreas) => { OnEndDrag(dropAreas, _cardPrefab_CreateDeck, draggable_Card); };

            draggable_Card.OnDragAction = (dropAreas) =>
            {
                DeckCardsDropArea.OnPointerExit();
                CardPoolDropArea.OnPointerExit();

                if (_cardPrefab_CreateDeck.transform.parent == DeckScroll.content)
                {
                    if (dropAreas.Count((dropArea) => dropArea == CardPoolDropArea) > 0)
                    {
                        CardPoolDropArea.OnPointerEnter();
                    }
                }

                else if (_cardPrefab_CreateDeck.transform.parent == CardPoolScroll.content)
                {
                    if (dropAreas.Count((dropArea) => dropArea == DeckCardsDropArea) > 0)
                    {
                        DeckCardsDropArea.OnPointerEnter();
                    }
                }
            };

            if (_cardPrefab_CreateDeck.transform.parent == DeckScroll.content)
            {
                DeckCardsDropArea.OffDropPanel();
                CardPoolDropArea.OnDropPanel();

                _cardPrefab_CreateDeck.HideDeckCardTab();
                _cardPrefab_CreateDeck.OffAddRemoveButton();
            }

            else if (_cardPrefab_CreateDeck.transform.parent == CardPoolScroll.content)
            {
                DeckCardsDropArea.OnDropPanel();
                CardPoolDropArea.OffDropPanel();
            }

            DeckScroll.enabled = false;

            for (int i = 0; i < DeckScroll.content.childCount; i++)
            {
                foreach (ScrollRect scroll in DeckScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>().scroll)
                {
                    scroll.enabled = false;
                }
            }

            CardPoolScroll.enabled = false;

            for (int i = 0; i < _cardPoolPrefabs.Count; i++)
            {
                foreach (ScrollRect scroll in _cardPoolPrefabs[i].scroll)
                {
                    scroll.enabled = false;
                }
            }
        }
    }

    public void SetStartDrag(CardPrefab_CreateDeck _cardPrefab_CreateDeck, Draggable_Card draggable_Card)
    {
        isDragging = true;

        CanDrag = false;

        DraggingCover.SetActive(true);

        draggable_Card.SetUpDraggable_Card(_cardPrefab_CreateDeck.cEntity_Base, _cardPrefab_CreateDeck.transform.position);
    }
    #endregion

    #region At the end of dragging
    public void OnEndDrag(List<DropArea> dropAreas, CardPrefab_CreateDeck _cardPrefab_CreateDeck, Draggable_Card draggable_Card)
    {
        DeckCardsDropArea.OffDropPanel();
        CardPoolDropArea.OffDropPanel();

        StartCoroutine(OnEndDragCoroutine(dropAreas, _cardPrefab_CreateDeck, draggable_Card));

        DeckScroll.enabled = true;

        for (int i = 0; i < DeckScroll.content.childCount; i++)
        {
            foreach (ScrollRect scroll in DeckScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>().scroll)
            {
                scroll.enabled = true;
            }
        }

        CardPoolScroll.enabled = true;

        for (int i = 0; i < _cardPoolPrefabs.Count; i++)
        {
            foreach (ScrollRect scroll in _cardPoolPrefabs[i].scroll)
            {
                scroll.enabled = true;
            }
        }
    }

    IEnumerator OnEndDragCoroutine(List<DropArea> dropAreas, CardPrefab_CreateDeck _cardPrefab_CreateDeck, Draggable_Card draggable_Card)
    {
        if (_cardPrefab_CreateDeck.transform.parent == DeckScroll.content)
        {
            if (dropAreas.Count((dropArea) => dropArea == CardPoolDropArea) > 0)
            {
                yield return StartCoroutine(RemoveDeckCardCoroutine(_cardPrefab_CreateDeck, draggable_Card));
            }

            else
            {
                _cardPrefab_CreateDeck.ShowCardImage();
            }
        }

        else if (_cardPrefab_CreateDeck.transform.parent == CardPoolScroll.content)
        {
            if (dropAreas.Count((dropArea) => dropArea == DeckCardsDropArea) > 0)
            {
                yield return StartCoroutine(AddDeckCardCoroutine(_cardPrefab_CreateDeck, draggable_Card));
            }
        }

        Reset_EndDrag(draggable_Card);
    }

    public void Reset_EndDrag(Draggable_Card draggable_Card)
    {
        isDragging = false;
        DestroyImmediate(draggable_Card.gameObject);
        CanDrag = true;
        DraggingCover.SetActive(false);
    }
    #endregion

    float _addCardAnimationTime = 0.01f;

    #region Animation to add cards to deck when dropped
    IEnumerator AddDeckCardCoroutine(CardPrefab_CreateDeck _cardPrefab_CreateDeck, Draggable_Card draggable_Card)
    {
        if (!DeckBuildingRule.CanAddCard(_cardPrefab_CreateDeck.cEntity_Base, EdittingDeckData))
        {
            yield break;
        }

        ContinuousController.instance.PlaySE(Opening.instance.DrawSE);

        for (int i = 0; i < CardPoolCardPrefabs_CreateDeck.Count; i++)
        {
            CardPrefab_CreateDeck cardPrefab_CreateDeck = CardPoolCardPrefabs_CreateDeck[i];

            if (cardPrefab_CreateDeck != null)
            {
                cardPrefab_CreateDeck.uIShiny.enabled = false;
                cardPrefab_CreateDeck.OffAddRemoveButton();
            }
        }
        Debug.Log($"CARD TO ADD: {_cardPrefab_CreateDeck.cEntity_Base}");
        yield return StartCoroutine(AddDeckCards(_cardPrefab_CreateDeck.cEntity_Base));

        List<CEntity_Base> DeckCards = new List<CEntity_Base>();

        foreach (CEntity_Base cEntity_Base in EdittingDeckData.AllDeckCards())
        {
            //Debug.Log($"ADD: {cEntity_Base.CardIndex} || {cEntity_Base.CardID}");
            DeckCards.Add(cEntity_Base);
        }
        Debug.Log($"ADDED EDITTING DECK DATA ENTITIES TO DECK CARDS LIST: {DeckCards.Count} || {DeckCards.IndexOf(_cardPrefab_CreateDeck.cEntity_Base)}  || {DeckCards.Count((cEntity_Base) => cEntity_Base == _cardPrefab_CreateDeck.cEntity_Base)}");
        int index = DeckCards.IndexOf(_cardPrefab_CreateDeck.cEntity_Base) + DeckCards.Count((cEntity_Base) => cEntity_Base == _cardPrefab_CreateDeck.cEntity_Base) - 1;
        Debug.Log("GETTING INDEX OF LAST ADDED CARD: " + index);
        CardPrefab_CreateDeck newCardPrefab = CreateDeckCard(_cardPrefab_CreateDeck.cEntity_Base);

        newCardPrefab.HideDeckCardTab();

        newCardPrefab.transform.SetSiblingIndex(index);

        List<CardPrefab_CreateDeck> cardPrefabs = new List<CardPrefab_CreateDeck>();

        for (int i = 0; i < DeckScroll.content.childCount; i++)
        {
            CardPrefab_CreateDeck cardPrefab = DeckScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>();

            if (cardPrefab != null)
            {
                cardPrefabs.Add(cardPrefab);

                if (cardPrefab.AddRemoveButtonParent.activeSelf)
                {
                    cardPrefab.OffAddRemoveButton();
                    cardPrefab.SetupAddRemoveButton(EdittingDeckData);
                }
            }
        }

        foreach (CardPrefab_CreateDeck cardPrefab in cardPrefabs)
        {
            cardPrefab.transform.SetParent(null);
        }

        foreach (CEntity_Base cEntity_Base in EdittingDeckData.AllDeckCards())
        {
            for (int i = 0; i < cardPrefabs.Count; i++)
            {
                if (cardPrefabs[i].cEntity_Base == cEntity_Base)
                {
                    cardPrefabs[i].transform.SetParent(DeckScroll.content);
                    cardPrefabs.Remove(cardPrefabs[i]);

                    break;
                }
            }
        }

        yield return new WaitForSeconds(Time.deltaTime);

        bool end = false;

        var sequence = DOTween.Sequence();

        sequence
            .Append(draggable_Card.transform.DOMove(newCardPrefab.transform.position, _addCardAnimationTime))
            .Join(draggable_Card.transform.DOScale(new Vector3(0.5f, 0.5f, 0.5f), _addCardAnimationTime))
            .AppendCallback(() => end = true);

        sequence.Play();

        yield return new WaitWhile(() => !end);
        end = false;

        yield return new WaitForSeconds(_addCardAnimationTime);
        newCardPrefab.ShowCardImage();
        draggable_Card.gameObject.SetActive(false);
    }
    #endregion

    #region Animation to remove cards on drop
    IEnumerator RemoveDeckCardCoroutine(CardPrefab_CreateDeck _cardPrefab_CreateDeck, Draggable_Card draggable_Card)
    {
        ContinuousController.instance.PlaySE(Opening.instance.DrawSE);

        bool end = false;

        yield return StartCoroutine(RemoveDeckCards(_cardPrefab_CreateDeck.cEntity_Base));

        var sequence = DOTween.Sequence();

        sequence
            .Append(draggable_Card.transform.DOMove(DisappearDeckCardTransform.position, _addCardAnimationTime))
            .Join(draggable_Card.transform.DOScale(new Vector3(0.5f, 0.5f, 0.5f), _addCardAnimationTime))
            .AppendCallback(() => end = true);

        sequence.Play();

        yield return new WaitWhile(() => !end);
        end = false;

        yield return new WaitForSeconds(_addCardAnimationTime);
        draggable_Card.gameObject.SetActive(false);

        if (_cardPrefab_CreateDeck.transform.parent != CardPoolScroll.content)
        {
            DestroyImmediate(_cardPrefab_CreateDeck.gameObject);
        }
    }
    #endregion

    #region Add card to deck animation when right-clicking
    public IEnumerator AddDeckCardCoroutine_OnClick(CardPrefab_CreateDeck _cardPrefab_CreateDeck)
    {
        if (!DeckBuildingRule.CanAddCard(_cardPrefab_CreateDeck.cEntity_Base, EdittingDeckData))
        {
            yield break;
        }

        Draggable_Card draggable_Card = CreateDraggable_Card();

        SetStartDrag(_cardPrefab_CreateDeck, draggable_Card);

        draggable_Card.IsDragging = false;

        yield return StartCoroutine(AddDeckCardCoroutine(_cardPrefab_CreateDeck, draggable_Card));

        Reset_EndDrag(draggable_Card);
    }
    #endregion

    #region Animation to remove cards from deck when right-clicking
    public IEnumerator RemoveDeckCardCoroutine_OnClick(CardPrefab_CreateDeck _cardPrefab_CreateDeck)
    {
        Draggable_Card draggable_Card = CreateDraggable_Card();

        _cardPrefab_CreateDeck.HideDeckCardTab();

        SetStartDrag(_cardPrefab_CreateDeck, draggable_Card);

        draggable_Card.IsDragging = false;

        yield return StartCoroutine(RemoveDeckCardCoroutine(_cardPrefab_CreateDeck, draggable_Card));

        Reset_EndDrag(draggable_Card);
    }
    #endregion

    #region add cards to deck
    public IEnumerator AddDeckCards(CEntity_Base cEntity_Base)
    {
        Debug.Log($"ADD DECK CARD: {cEntity_Base.CardID}: {DeckBuildingRule.CanAddCard(cEntity_Base, EdittingDeckData)}");
        if (!DeckBuildingRule.CanAddCard(cEntity_Base, EdittingDeckData))
        {
            yield break;
        }
        Debug.Log($"ADD DECK CARD: {EdittingDeckData.DeckCardIDs.Count}");
        EdittingDeckData.AddCard(cEntity_Base);

        Debug.Log($"ADD DECK CARD: {EdittingDeckData.DeckCardIDs.Count}");
        Debug.Log($"ADD DECK CARD: {EdittingDeckData.DeckCards().Count}");
        EdittingDeckData.DeckCardIDs = DeckData.GetDeckCardCodes(DeckData.SortedDeckCardsList(EdittingDeckData.DeckCards()));
        EdittingDeckData.DigitamaDeckCardIDs = DeckData.GetDeckCardCodes(DeckData.SortedDeckCardsList(EdittingDeckData.DigitamaDeckCards()));
        
        ReflectDeckData();
        cardDistribution.SetCardDistribution(EdittingDeckData);
    }
    #endregion

    #region remove cards from deck
    public IEnumerator RemoveDeckCards(CEntity_Base cEntity_Base)
    {
        if (cEntity_Base.cardKind == CardKind.DigiEgg)
        {
            if (EdittingDeckData.DigitamaDeckCards().Count((cardData) => cardData.CardID == cEntity_Base.CardID) <= 0)
            {
                yield break;
            }
        }

        else
        {
            if (EdittingDeckData.DeckCards().Count((cardData) => cardData.CardID == cEntity_Base.CardID) <= 0)
            {
                yield break;
            }
        }

        EdittingDeckData.RemoveCard(cEntity_Base);

        EdittingDeckData.DeckCardIDs = DeckData.GetDeckCardCodes(DeckData.SortedDeckCardsList(EdittingDeckData.DeckCards()));
        EdittingDeckData.DigitamaDeckCardIDs = DeckData.GetDeckCardCodes(DeckData.SortedDeckCardsList(EdittingDeckData.DigitamaDeckCards()));

        ReflectDeckData();
        cardDistribution.SetCardDistribution(EdittingDeckData);
    }
    #endregion



    #endregion

    public void OnPointerEnterBackground()
    {
        List<CardPrefab_CreateDeck> cardPrefabs = new List<CardPrefab_CreateDeck>();

        for (int i = 0; i < DeckScroll.content.childCount; i++)
        {
            CardPrefab_CreateDeck cardPrefab = DeckScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>();

            if (cardPrefab != null)
            {
                cardPrefabs.Add(cardPrefab);
            }
        }

        foreach (CardPrefab_CreateDeck cardPrefab in cardPrefabs)
        {
            if (cardPrefab != null)
            {
                cardPrefab._OnExit();
            }
        }
    }

    public Button TrialDrawButton;

    public void OnClickTrial5DrawButton()
    {
        Opening.instance.deck.trialDraw.Off();
        ContinuousController.instance.PlaySE(Opening.instance.DecisionSE);
        ContinuousController.instance.StartCoroutine(Opening.instance.deck.trialDraw.SetUpTrialDraw(EdittingDeckData.DeckCards()));
    }

    public Button ClearDeckButton;

    public void OnClickClearDeckButton()
    {
        List<UnityAction> Commands = new List<UnityAction>()
            {
                () =>
                {
                    ContinuousController.instance.StartCoroutine(ClearDeckCoroutine());

                        IEnumerator ClearDeckCoroutine()
                        {
                            DraggingCover.SetActive(true);

                            List<CEntity_Base> cEntity_Bases = new List<CEntity_Base>();

                            foreach(CEntity_Base cEntity_Base in EdittingDeckData.DeckCards())
                            {
                               cEntity_Bases.Add(cEntity_Base);
                            }

                            foreach(CEntity_Base cEntity_Base in EdittingDeckData.DigitamaDeckCards())
                            {
                                cEntity_Bases.Add(cEntity_Base);
                            }

                            foreach(CEntity_Base cEntity_Base in cEntity_Bases)
                            {
                                yield return ContinuousController.instance.StartCoroutine(RemoveDeckCards(cEntity_Base));
                            }

                            for (int i = 0; i < DeckScroll.content.childCount; i++)
                            {
                                Destroy(DeckScroll.content.GetChild(i).gameObject);
                            }

                            yield return new WaitWhile(() => DeckScroll.content.childCount >= 1);

                            DraggingCover.SetActive(false);
                        }
                },

                null,
            };

        List<string> CommandTexts = new List<string>()
        {
            LocalizeUtility.GetLocalizedString(
                    EngMessage: "Yes",
                    JpnMessage: "はい"
                ),
                LocalizeUtility.GetLocalizedString(
                    EngMessage: "No",
                    JpnMessage: "いいえ"
                ),
        };

        Opening.instance.SetUpActiveYesNoObject(
            Commands,
            CommandTexts,
            LocalizeUtility.GetLocalizedString(
            EngMessage: "Remove all cards from the deck?",
            JpnMessage: "全てのカードをデッキから除きますか?"
            ),
            true);
    }

    public void OnClickSetDeckIconButton()
    {
        ContinuousController.instance.StartCoroutine(Opening.instance.deck.deckListPanel.SetUpDeckListPanel(
            EdittingDeckData,
            OnClick,
            LocalizeUtility.GetLocalizedString(
            EngMessage: "Choose a deck icon card.",
            JpnMessage: "デッキアイコン選択"
        )));

        void OnClick(CEntity_Base cEntity_Base)
        {
            EdittingDeckData.KeyCardId = cEntity_Base.CardIndex;
            Opening.instance.deck.deckListPanel.Close();
            Opening.instance.PlayDecisionSE();
        }
    }
}


