using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;
using System.Threading.Tasks;
public class HandCard : MonoBehaviour
{
    [Header("カード画像")]
    public Image CardImage;

    [Header("選択アウトライン")]
    public GameObject Outline_Select;

    [Header("表画像表示")]
    public Image ShowFaceCard;

    [Header("スキル名")]
    public TextMeshProUGUI SkillNameText;

    [Header("カード場所Text")]
    public Text cardPositionText;

    [Header("コマンドパネル")]
    public CommandPanel handCardCommandPanel;

    [Header("コストアイコン")]
    public List<Image> CostIcons;

    [Header("コストテキスト")]
    public TextMeshProUGUI CostText;

    [Header("レベルアイコン")]
    public List<Image> LevelIcons;

    [Header("レベルテキスト")]
    public TextMeshProUGUI LevelText;

    [Header("進化コストアイコン")]
    public List<Image> EvoCostIcons;

    [Header("進化コストテキスト_レベル")]
    public List<TextMeshProUGUI> EvoCostTexts_Level;

    [Header("進化コストテキスト_メモリー")]
    public List<TextMeshProUGUI> EvoCostTexts_Memory;

    [Header("プレイテキスト")]
    public TextMeshProUGUI PlayText;

    [Header("ジョグレスプレイテキスト")]
    public TextMeshProUGUI JogressPlayText;

    [Header("バーストプレイテキスト")]
    public TextMeshProUGUI BurstPlayText;

    [Header("App Fusion play text")]
    public TextMeshProUGUI AppFusionPlayText;

    [Header("タイトルテキスト")]
    public TextMeshProUGUI titleText;

    [Header("1体進化矢印")]
    public GameObject singleDigivolutionArrow;

    [Header("ジョグレス進化矢印")]
    public GameObject jogressDigivolutionArrow;

    [Header("ジョグレスエフェクト")]
    public GameObject jogressEffect;

    [Header("DPテキスト")]
    public Text DPText;

    [Header("DP枠")]
    public List<Image> DPBackground_color = new List<Image>();

    [Header("進化元")]
    public List<Image> EvoRootCardImages = new List<Image>();

    [Header("白丸")]
    public Sprite WhiteCircle;

    [Header("虹丸")]
    public Sprite RainbowCircle;

    [Header("クリックテキスト")]
    public TextMeshProUGUI ClickText;
    public CardSource cardSource { get; set; }

    public UnityAction<HandCard> OnClickAction;

    public UnityAction<HandCard> BeginDragAction;
    public UnityAction<List<DropArea>> OnDragAction;
    public UnityAction<List<DropArea>> EndDragAction;
    public bool CanDrag;

    public List<Image> Outline_SelectImages { get; set; } = new List<Image>();
    public bool notHideSelectedIndexText { get; set; } = false;
    private void Start()
    {
        if (!notHideSelectedIndexText)
        {
            OffSelectedIndexText();
        }

        RemoveSelectEffect();

        for (int i = 0; i < Outline_Select.transform.childCount; i++)
        {
            if (Outline_Select.transform.GetChild(i).GetComponent<Image>() != null)
            {
                Outline_SelectImages.Add(Outline_Select.transform.GetChild(i).GetComponent<Image>());
            }
        }

        if (handCardCommandPanel != null)
        {
            handCardCommandPanel.CloseCommandPanel();
        }


        if (CostText != null)
        {
            CostText.transform.parent.gameObject.SetActive(false);
        }


        if (LevelText != null)
        {
            LevelText.transform.parent.gameObject.SetActive(false);
        }


        if (EvoCostIcons != null)
        {
            if (EvoCostIcons.Count >= 1)
            {
                for (int i = 0; i < EvoCostIcons.Count; i++)
                {
                    EvoCostIcons[i].transform.parent.gameObject.SetActive(false);
                    EvoCostIcons[i].transform.parent.parent.gameObject.SetActive(true);
                }
            }
        }

        OffPlayText();

        OffJogressPlayText();

        OffBurstPlayText();

        OffAppFusionPlayText();

        OffDP();

        OffEvoRootCardImage();

        OffClickText();

        if (singleDigivolutionArrow != null)
        {
            singleDigivolutionArrow.SetActive(false);
        }

        if (jogressDigivolutionArrow != null)
        {
            jogressDigivolutionArrow.SetActive(false);
        }

        if (jogressEffect != null)
        {
            jogressEffect.SetActive(false);
        }

        if (titleText != null)
        {
            titleText.gameObject.SetActive(false);
        }
    }

    public async void SetEvoRootCardImages(CardSource[] cardSources)
    {
        if (cardSources != null)
        {
            if (EvoRootCardImages != null)
            {
                if (cardSources.Length == 1)
                {
                    if (singleDigivolutionArrow != null)
                    {
                        singleDigivolutionArrow.SetActive(true);
                    }
                }

                else if (cardSources.Length == 2)
                {
                    if (jogressDigivolutionArrow != null)
                    {
                        jogressDigivolutionArrow.SetActive(true);
                    }

                    if (jogressEffect != null)
                    {
                        jogressEffect.SetActive(true);
                    }
                }

                for (int i = 0; i < cardSources.Length; i++)
                {
                    if (EvoRootCardImages.Count > i)
                    {
                        if (EvoRootCardImages[i].transform.childCount >= 1)
                        {
                            EvoRootCardImages[i].transform.GetChild(0).gameObject.SetActive(false);
                        }

                        EvoRootCardImages[i].gameObject.SetActive(true);

                        if (cardSources[i] != null)
                        {
                            EvoRootCardImages[i].sprite = await cardSources[i].GetCardSprite();
                        }

                        else
                        {
                            if (EvoRootCardImages[i].transform.childCount >= 1)
                            {
                                EvoRootCardImages[i].transform.GetChild(0).gameObject.SetActive(true);
                            }
                        }
                    }
                }
            }
        }
    }

    public void OffEvoRootCardImage()
    {
        if (EvoRootCardImages != null)
        {
            for (int i = 0; i < EvoRootCardImages.Count; i++)
            {
                if (EvoRootCardImages.Count > i)
                {
                    EvoRootCardImages[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void ShowDP()
    {
        if (DPText != null && DPBackground_color != null)
        {
            if (cardSource.IsDigimon && cardSource.CardDP >= 0)
            {
                DPText.transform.parent.gameObject.SetActive(true);
                DPText.text = cardSource.CardDP.ToString();

                for (int i = 0; i < DPBackground_color.Count; i++)
                {
                    CardColor cardColor = CardColor.None;

                    if (i < cardSource.CardColors.Count)
                    {
                        cardColor = cardSource.CardColors[i];
                    }

                    else
                    {
                        cardColor = cardSource.CardColors[0];
                    }

                    DPBackground_color[i].color = DataBase.CardColor_ColorLightDictionary[cardColor];
                }
            }

            else
            {
                DPText.transform.parent.gameObject.SetActive(false);
            }
        }
    }

    public void SetClickText()
    {
        if (ClickText != null)
        {
            ClickText.gameObject.SetActive(true);
        }
    }

    public void OffClickText()
    {
        if (ClickText != null)
        {
            ClickText.gameObject.SetActive(false);
        }
    }

    public void OffDP()
    {
        if (DPText != null)
        {
            DPText.transform.parent.gameObject.SetActive(false);
        }
    }

    public async void SetShowFaceCard()
    {
        if (ShowFaceCard != null && cardSource != null)
        {
            ShowFaceCard.gameObject.SetActive(true);
            ShowFaceCard.sprite = await cardSource.GetCardSprite();
        }
    }

    public void OffShowFaceCard()
    {
        if (ShowFaceCard != null)
        {
            ShowFaceCard.gameObject.SetActive(false);
        }
    }

    public void SetSkillName(ICardEffect cardEffect)
    {
        if (SkillNameText != null)
        {
            if (cardEffect != null)
            {
                if (!string.IsNullOrEmpty(cardEffect.EffectName))
                {
                    SkillNameText.transform.parent.gameObject.SetActive(true);
                    SkillNameText.text = cardEffect.EffectName;
                }
            }
        }
    }

    public void SetTitleText(string titleString)
    {
        if (titleText != null)
        {
            if (!string.IsNullOrEmpty(titleString))
            {
                titleText.gameObject.SetActive(true);
                titleText.text = titleString;
            }
        }
    }

    public void SetOutlineColor(Color color)
    {
        foreach (Image image in Outline_SelectImages)
        {
            image.color = color;
        }
    }

    public void SetBlueOutline()
    {
        SetOutlineColor(DataBase.SelectColor_Blue);
    }

    public void SetOrangeOutline()
    {
        SetOutlineColor(DataBase.SelectColor_Orange);
    }

    public void SetGreenOutline()
    {
        SetOutlineColor(DataBase.SelectColor_Green);
    }

    public void OnOutline()
    {
        if (Outline_Select != null)
            Outline_Select.SetActive(true);
    }

    public void OnSelect()
    {
        OnOutline();

        SetBlueOutline();
    }

    public void RemoveSelectEffect()
    {
        if(Outline_Select!= null)
            Outline_Select.SetActive(false);

        OffClickText();
    }

    public void SetUpHandCard(CardSource _cardSource, bool showCardImage = true)
    {
        cardSource = _cardSource;

        if (CardImage != null)
        {
            CardImage.gameObject.SetActive(true);
        }

        if (_cardSource.Owner.isYou && showCardImage)
        {
            SetUpHandCardImage();
        }

        else
        {
            SetUpReverseCard();
        }
    }

    public void SetUpCardPositionText(List<Permanent> permanents)
    {
        if (cardPositionText != null)
        {
            if (cardSource != null)
            {
                string cardPositionString = "";

                if (cardSource.PermanentOfThisCard() != null)
                {
                    if (cardSource.Owner.GetFieldPermanents().Contains(cardSource.PermanentOfThisCard()))
                    {
                        int index = permanents.IndexOf(cardSource.PermanentOfThisCard()) + 1;
                        cardPositionString = $"Field:{index}";
                    }
                }

                else if (cardSource.Owner.HandCards.Contains(cardSource))
                {
                    cardPositionString = "Hand";
                }

                else if (CardEffectCommons.IsExistOnTrash(cardSource))
                {
                    cardPositionString = "Trash";
                }

                else if (cardSource.Owner.LibraryCards.Contains(cardSource))
                {
                    cardPositionString = "Deck";
                }

                else if (cardSource.Owner.DigitamaLibraryCards.Contains(cardSource))
                {
                    cardPositionString = "Digi - Egg Deck";
                }

                cardPositionText.transform.parent.gameObject.SetActive(true);

                cardPositionText.text = cardPositionString;
            }
        }
    }

    public void SetUpReverseCard()
    {
        CardImage.sprite = ContinuousController.instance.ReverseCard;

        OffShowFaceCard();

        if (SkillNameText != null)
        {
            SkillNameText.transform.parent.gameObject.SetActive(false);
        }

        if (cardPositionText != null)
        {
            cardPositionText.transform.parent.gameObject.SetActive(false);
        }
    }

    public bool ShowOpponent { get; set; } = false;
    bool onYourHand()
    {
        if (GManager.instance != null)
        {
            if (GManager.instance.You.HandCardObjects.Contains(this))
            {
                return true;
            }

            if (ShowOpponent)
            {
                return true;
            }

            if (!GManager.instance.turnStateMachine.DoneStartGame && cardSource.Owner.isYou)
            {
                return true;
            }
        }

        return false;
    }

    int _frameCount = 145;
    int _updateFrame = 150;

    float _validPressTime = 0.5f;
    float _requiredTime = 0.0f;
    bool _pressing = false;
    public bool IsExecuting { get; set; } = false;

    bool _firstRaycastTarget = true;

    private void Awake()
    {
        _firstRaycastTarget = CardImage.raycastTarget;
    }
    private void Update()
    {
        if (CardImage.color.a <= 0.1f)
        {
            CardImage.raycastTarget = false;
        }

        else
        {
            CardImage.raycastTarget = _firstRaycastTarget;
        }

        #region exception check
        if (cardSource == null)
        {
            return;
        }
        #endregion

        #region 長押しの取得
#if !UNITY_EDITOR && UNITY_ANDROID
        if (pressing)
        {
            if(requiredTime < Time.time)
            {
                OnRightClicked();
                pressing = false;
            }
        }
#endif
        #endregion

        #region Update only once every few frames
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

        if (onYourHand())
        {
            SetUpHandCardImage();
        }
    }

    public void SetPlayText(string message, Color color)
    {
        if (PlayText != null)
        {
            PlayText.transform.parent.gameObject.SetActive(true);
            PlayText.transform.parent.parent.gameObject.SetActive(true);
            PlayText.text = message;
            PlayText.color = color;
        }
    }

    public void OffPlayText()
    {
        if (PlayText != null)
        {
            PlayText.transform.parent.gameObject.SetActive(false);
            PlayText.transform.parent.parent.gameObject.SetActive(true);
        }
    }

    public void SetJogressPlayText()
    {
        if (JogressPlayText != null)
        {
            JogressPlayText.transform.parent.gameObject.SetActive(true);
            JogressPlayText.transform.parent.parent.gameObject.SetActive(true);
        }
    }

    public void OffJogressPlayText()
    {
        if (BurstPlayText != null)
        {
            BurstPlayText.transform.parent.gameObject.SetActive(false);
            BurstPlayText.transform.parent.parent.gameObject.SetActive(true);
        }

        if (AppFusionPlayText != null)
        {
            AppFusionPlayText.transform.parent.gameObject.SetActive(false);
            AppFusionPlayText.transform.parent.parent.gameObject.SetActive(true);
        }
    }

    public void SetBurstPlayText()
    {
        if (BurstPlayText != null)
        {
            BurstPlayText.transform.parent.gameObject.SetActive(true);
            BurstPlayText.transform.parent.parent.gameObject.SetActive(true);
        }
    }

    public void OffBurstPlayText()
    {
        if (JogressPlayText != null)
        {
            JogressPlayText.transform.parent.gameObject.SetActive(false);
            JogressPlayText.transform.parent.parent.gameObject.SetActive(true);
        }

        if (AppFusionPlayText != null)
        {
            AppFusionPlayText.transform.parent.gameObject.SetActive(false);
            AppFusionPlayText.transform.parent.parent.gameObject.SetActive(true);
        }
    }

    public void SetAppFusionPlayText()
    {
        if (AppFusionPlayText != null)
        {
            AppFusionPlayText.transform.parent.gameObject.SetActive(true);
            AppFusionPlayText.transform.parent.parent.gameObject.SetActive(true);
        }
    }

    public void OffAppFusionPlayText()
    {
        if (JogressPlayText != null)
        {
            JogressPlayText.transform.parent.gameObject.SetActive(false);
            JogressPlayText.transform.parent.parent.gameObject.SetActive(true);
        }

        if (BurstPlayText != null)
        {
            BurstPlayText.transform.parent.gameObject.SetActive(false);
            BurstPlayText.transform.parent.parent.gameObject.SetActive(true);
        }
    }

    public void PointerDown(BaseEventData eventData)
    {
        if (!_pressing)
        {
            _pressing = true;
            _requiredTime = Time.time + _validPressTime;
        }

        else
        {
            _pressing = false;
        }
    }

    public void PointerUp(BaseEventData eventData)
    {
        if (_pressing)
        {
            _pressing = false;
        }
    }

    public void PointerExit(BaseEventData eventData)
    {
        if (_pressing)
        {
            _pressing = false;
        }
    }

    public async void SetUpHandCardImage()
    {
        if (SkillNameText != null)
        {
            if (!IsExecuting)
            {
                SkillNameText.transform.parent.gameObject.SetActive(false);
            }
        }

        if (cardPositionText != null)
        {
            cardPositionText.transform.parent.gameObject.SetActive(false);
        }

        if (GManager.instance != null)
        {
            if (GManager.instance.You.HandCardObjects.Contains(this) || !GManager.instance.turnStateMachine.DoneStartGame)
            {
                ShowPlayCost();
            }
        }

        //カード画像
        CardImage.sprite = await cardSource.GetCardSprite();
    }

    void ShowPlayCost()
    {
        if (CostText != null)
        {
            for (int i = CostIcons.Count - 1; i > -1; i--)
            {
                CardColor cardColor = CardColor.None;
                int value = CostIcons.Count - i - 1;

                if (i >= CostIcons.Count - cardSource.CardColors.Count)
                {
                    float fillAmount = (float)((CostIcons.Count - i) / (float)cardSource.CardColors.Count);

                    cardColor = cardSource.CardColors[value];
                    CostIcons[i].color = DataBase.CardColor_ColorDarkDictionary[cardColor];
                    CostIcons[i].fillAmount = fillAmount;
                    CostIcons[i].gameObject.SetActive(true);
                }
                else
                {
                    CostIcons[i].gameObject.SetActive(false);
                }
            }

            CostText.color = Color.white;

            CostText.transform.parent.gameObject.SetActive(true);

            CostText.text = $"{cardSource.PayingCost(SelectCardEffect.Root.Hand, null, checkAvailability: false)}";
        }
    }

    public void ShowCostLevel()
    {
        ShowPlayCost();

        if (LevelIcons != null)
        {
            if (LevelText != null)
            {
                if (LevelText.transform.parent != null)
                {
                    if (cardSource.IsDigimon && cardSource.HasLevel)
                    {
                        if (LevelIcons.Count >= 1)
                        {
                            for (int i = 0; i < LevelIcons.Count; i++)
                            {
                                CardColor cardColor = CardColor.None;

                                if (i < cardSource.CardColors.Count)
                                {
                                    cardColor = cardSource.CardColors[i];
                                    LevelIcons[i].color = DataBase.CardColor_ColorDarkDictionary[cardColor];
                                    LevelIcons[i].gameObject.SetActive(true);
                                }

                                else
                                {
                                    LevelIcons[i].gameObject.SetActive(false);
                                }
                            }

                            LevelText.color = Color.white;

                            LevelText.transform.parent.gameObject.SetActive(true);
                            LevelText.text = $"Lv.{cardSource.Level}";
                        }
                    }

                    else
                    {
                        LevelText.transform.parent.gameObject.SetActive(false);
                    }
                }
            }
        }

        if (EvoCostIcons != null)
        {
            if (cardSource.IsDigimon)
            {
                if (EvoCostIcons.Count >= 1)
                {
                    for (int i = 0; i < EvoCostIcons.Count; i++)
                    {
                        if (i < cardSource.BaseEvoCostsFromEntity.Count)
                        {
                            EvoCostIcons[i].sprite = WhiteCircle;
                            EvoCostIcons[i].transform.parent.gameObject.SetActive(true);
                            EvoCostIcons[i].gameObject.SetActive(true);

                            CardColor cardColor = CardColor.None;

                            cardColor = cardSource.BaseEvoCostsFromEntity[i].CardColor;

                            EvoCostIcons[i].color = DataBase.CardColor_ColorDarkDictionary[cardColor];

                            EvoCostTexts_Level[i].text = $"Lv.{cardSource.BaseEvoCostsFromEntity[i].Level}";
                            EvoCostTexts_Memory[i].text = $"{cardSource.BaseEvoCostsFromEntity[i].MemoryCost}";

                            if (cardColor == CardColor.Yellow || cardColor == CardColor.White)
                            {
                                EvoCostTexts_Level[i].color = Color.black;
                                EvoCostTexts_Memory[i].color = Color.black;
                            }

                            else
                            {
                                EvoCostTexts_Level[i].color = Color.white;
                                EvoCostTexts_Memory[i].color = Color.white;
                            }
                        }

                        else
                        {
                            EvoCostIcons[i].transform.parent.gameObject.SetActive(false);
                        }
                    }

                    if (cardSource.BaseEvoCostsFromEntity.Count == 7)
                    {
                        int cost = cardSource.BaseEvoCostsFromEntity[0].MemoryCost;
                        int level = cardSource.BaseEvoCostsFromEntity[0].Level;

                        bool sameCost = true;

                        for (int i = 0; i < cardSource.BaseEvoCostsFromEntity.Count; i++)
                        {
                            if (cardSource.BaseEvoCostsFromEntity[i].MemoryCost != cost || cardSource.BaseEvoCostsFromEntity[i].Level != level)
                            {
                                sameCost = false;
                                break;
                            }
                        }

                        if (sameCost)
                        {
                            for (int i = 0; i < EvoCostIcons.Count; i++)
                            {
                                EvoCostIcons[i].transform.parent.gameObject.SetActive(false);

                                if (i == 0)
                                {
                                    EvoCostIcons[i].sprite = RainbowCircle;
                                    EvoCostIcons[i].transform.parent.gameObject.SetActive(true);

                                    EvoCostIcons[i].color = Color.white;

                                    EvoCostTexts_Level[i].text = $"Lv.{cardSource.BaseEvoCostsFromEntity[i].Level}";
                                    EvoCostTexts_Memory[i].text = $"{cardSource.BaseEvoCostsFromEntity[i].MemoryCost}";

                                    EvoCostTexts_Level[i].color = Color.white;
                                    EvoCostTexts_Memory[i].color = Color.white;
                                }
                            }
                        }
                    }
                }
            }

            else
            {
                for (int i = 0; i < EvoCostIcons.Count; i++)
                {
                    EvoCostIcons[i].transform.parent.gameObject.SetActive(false);
                }
            }
        }
    }

    public void AddClickTarget(UnityAction<HandCard> _OnClickAction)
    {
        OnClickAction = _OnClickAction;

        OnSelect();
    }

    public void RemoveOnClickAction()
    {
        OnClickAction = null;
    }


    public void RemoveClickTarget()
    {

        RemoveOnClickAction();
        RemoveSelectEffect();
    }

    public void AddDragTarget(UnityAction<HandCard> _BeginDragAction, UnityAction<List<DropArea>> _OnDropAction, UnityAction<List<DropArea>> _OnDragAction)
    {
        BeginDragAction = _BeginDragAction;
        EndDragAction = _OnDropAction;
        OnDragAction = _OnDragAction;

        CanDrag = true;

        OnSelect();
    }

    public void RemoveDragTarget()
    {
        BeginDragAction = null;
        EndDragAction = null;
        OnDragAction = null;

        CanDrag = false;

        RemoveSelectEffect();
    }

    public void PointerClick(BaseEventData eventData)
    {
        #region right click
        if (Input.GetMouseButtonUp(1))
        {
            OnRightClicked();
        }
        #endregion

        #region left click
        else if (Input.GetMouseButtonUp(0))
        {
            OnClickAction?.Invoke(this);
        }
        #endregion
    }

    #region 右クリック

    void OnRightClicked()
    {
        if (transform.parent.GetComponent<HandContoller>() != null)
        {
            if (transform.parent.GetComponent<HandContoller>().isDragging)
            {
                return;
            }
        }

        if (cardSource != null)
        {
            bool CanLook = false;

            if (!GManager.instance.Opponent.HandCardObjects.Contains(this))
            {
                if (!cardSource.IsFlipped)
                {
                    CanLook = true;
                }

                else
                {
                    if (cardSource.Owner.isYou)
                    {
                        if (cardSource.Owner.LostCards.Contains(cardSource))
                        {
                            CanLook = true;
                        }
                    }
                }
            }

            if (ShowOpponent)
            {
                CanLook = true;
            }

            if (CanLook)
            {
                //Debug.Log($"OnClickHandCard_{cardSource.BaseCardNameFromEntity}, parent:{transform.parent.gameObject.name}, gameObject:{this.gameObject.name}");
                GManager.instance.cardDetail.OpenCardDetail(cardSource, true);

                if (GManager.instance != null)
                {
                    GManager.instance.PlayDecisionSE();
                }
            }
        }
    }
    #endregion

    [Header("selected-sequence-index text")]
    public Text SelectedIndexText;

    public void OffSelectedIndexText()
    {
        if (SelectedIndexText != null)
        {
            SelectedIndexText.transform.parent.gameObject.SetActive(false);
        }
    }

    public void SetSelectedIndexText(int index)
    {
        if (SelectedIndexText != null)
        {
            SelectedIndexText.transform.parent.gameObject.SetActive(true);
            SelectedIndexText.text = $"{index}";
        }
    }
}