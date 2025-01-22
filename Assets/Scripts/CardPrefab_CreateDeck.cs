using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;
using Shapes2D;
using System;
using Coffee.UIEffects;
using UnityEngine.EventSystems;
using TMPro;
public class CardPrefab_CreateDeck : MonoBehaviour
{
    [Header("Card Image")]
    public Image CardImage;

    [Header("Scroll Rect")]
    public List<ScrollRect> scroll = new List<ScrollRect>();

    [Header("Cover")]
    public List<GameObject> Cover = new List<GameObject>();

    [Header("Cover Standard")]
    public GameObject Cover_Standard;

    [Header("Outline")]
    public GameObject Outline;

    [Header("Anim")]
    public Animator anim;

    [Header("Cost Icons")]
    public List<Image> CostIcons = new List<Image>();

    [Header("Cost Text")]
    public TextMeshProUGUI CostText;

    [Header("Level Icons")]
    public List<Image> LevelIcons;

    [Header("Level Text")]
    public TextMeshProUGUI LevelText;

    [Header("Evo Cost Icons")]
    public List<Image> EvoCostIcons;

    [Header("Evo Cost Text Level")]
    public List<TextMeshProUGUI> EvoCostTexts_Level;

    [Header("Evo Cost Text Memory")]
    public List<TextMeshProUGUI> EvoCostTexts_Memory;

    [Header("Quantity Limit")]
    public GameObject LimitObject;
    public Text LimitText;

    [Header("Ban")]
    public GameObject BanObject;

    [Header("White Circle")]
    public Sprite WhiteCircle;

    [Header("Rainbow Circle")]
    public Sprite RainbowCircle;

    public Transform Parent;
    public CEntity_Base cEntity_Base { get; set; }

    public UnityAction OnClickAction;

    public UnityAction<CardPrefab_CreateDeck> OnEnterAction;

    public UnityAction<CardPrefab_CreateDeck> OnExitAction;

    public UnityAction<CardPrefab_CreateDeck> OnBeginDragAction;

    public bool isActive = true;

    public GameObject AddRemoveButtonParent;
    public Button AddButton;
    public Button RemoveButton;

    public UIShiny uIShiny;
    private void Start()
    {
        Outline.SetActive(false);

        if (CardImage.GetComponent<UIShiny>() != null)
        {
            CardImage.GetComponent<UIShiny>().enabled = false;
        }

        OffAddRemoveButton();

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

        if (LimitObject != null)
        {
            LimitObject.SetActive(false);
        }

        if (BanObject != null)
        {
            BanObject.SetActive(false);
        }
    }

    public UnityAction<CardPrefab_CreateDeck> OnClickAddButtonAction;

    public void OnClickAddButton()
    {
        OnClickAddButtonAction?.Invoke(this);
    }

    public UnityAction<CardPrefab_CreateDeck> OnClickRemoveButtonAction;

    public void OnClickRemoveButton()
    {
        OnClickRemoveButtonAction?.Invoke(this);
    }

    public void SetupAddRemoveButton(DeckData deckData)
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        return;
#endif
        if (AddRemoveButtonParent != null)
        {
            AddRemoveButtonParent.SetActive(true);
        }

        bool AddButtonInteractable = false;

        if (deckData != null && cEntity_Base != null)
        {
            AddButtonInteractable = !CanNotAddThisCard(deckData);
        }

        if (AddButton != null)
        {
            AddButton.interactable = AddButtonInteractable;

            AddButton.transform.parent.gameObject.SetActive(AddButtonInteractable);
        }

        if (transform.parent != null)
        {
            for (int i = 0; i < transform.parent.childCount; i++)
            {
                CardPrefab_CreateDeck cardPrefab_CreateDeck = transform.parent.GetChild(i).GetComponent<CardPrefab_CreateDeck>();

                if (cardPrefab_CreateDeck != null && cardPrefab_CreateDeck != this)
                {
                    cardPrefab_CreateDeck.OffAddRemoveButton();
                }
            }
        }
    }
    public void OffAddRemoveButton()
    {
        if (AddRemoveButtonParent != null)
        {
            AddRemoveButtonParent.SetActive(false);
        }
    }

    public void SetUpCardPrefab_CreateDeck(CEntity_Base _cEntity_Base)
    {
        cEntity_Base = _cEntity_Base;

        //ShowCardImage();
    }

    public async void ShowCardImage()
    {
        if (this.cEntity_Base != null)
        {
            //カード画像

            CardImage.color = new Color(1, 1, 1, 1);

            CardImage.GetComponent<EventTrigger>().enabled = true;

            CardImage.sprite = await cEntity_Base.GetCardSprite();

            if (CostText != null)
            {
                if (cEntity_Base.cardKind != CardKind.DigiEgg)
                {
                    for (int i = CostIcons.Count-1; i > -1; i--)
                    {
                        CardColor cardColor = CardColor.None;
                        int value = CostIcons.Count - i - 1;

                        if (i >= CostIcons.Count - cEntity_Base.cardColors.Count)
                        {
                            float fillAmount = (float)((CostIcons.Count - i) / (float)cEntity_Base.cardColors.Count);

                            cardColor = cEntity_Base.cardColors[value];
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

                    CostText.text = $"{cEntity_Base.PlayCost}";
                }

                else
                {
                    CostText.transform.parent.gameObject.SetActive(false);
                }
            }

            if (LevelIcons != null)
            {
                if ((cEntity_Base.cardKind == CardKind.Digimon || cEntity_Base.cardKind == CardKind.DigiEgg) && cEntity_Base.Level >= 1)
                {
                    if (LevelIcons.Count >= 1)
                    {
                        for (int i = 0; i < CostIcons.Count; i++)
                        {
                            int cardColorIndex = i < cEntity_Base.cardColors.Count ? i : 0;
                            CardColor cardColor = cEntity_Base.cardColors[cardColorIndex];

                            if (i < cEntity_Base.cardColors.Count)
                            {
                                cardColor = cEntity_Base.cardColors[i];
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
                        LevelText.text = $"Lv.{cEntity_Base.Level}";
                    }
                }

                else
                {
                    LevelText.transform.parent.gameObject.SetActive(false);
                }
            }

            if (EvoCostIcons != null)
            {
                if (cEntity_Base.cardKind == CardKind.Digimon)
                {
                    if (EvoCostIcons.Count >= 1)
                    {
                        for (int i = 0; i < EvoCostIcons.Count; i++)
                        {
                            if (i < cEntity_Base.EvoCosts.Count)
                            {
                                EvoCostIcons[i].sprite = WhiteCircle;
                                EvoCostIcons[i].transform.parent.gameObject.SetActive(true);

                                CardColor cardColor = CardColor.None;

                                cardColor = cEntity_Base.EvoCosts[i].CardColor;

                                EvoCostIcons[i].color = DataBase.CardColor_ColorDarkDictionary[cardColor];

                                EvoCostTexts_Level[i].text = $"Lv.{cEntity_Base.EvoCosts[i].Level}";
                                EvoCostTexts_Memory[i].text = $"{cEntity_Base.EvoCosts[i].MemoryCost}";

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

                        if (cEntity_Base.EvoCosts.Count == 7)
                        {
                            int cost = cEntity_Base.EvoCosts[0].MemoryCost;
                            int level = cEntity_Base.EvoCosts[0].Level;

                            bool sameCost = true;

                            for (int i = 0; i < cEntity_Base.EvoCosts.Count; i++)
                            {
                                if (cEntity_Base.EvoCosts[i].MemoryCost != cost || cEntity_Base.EvoCosts[i].Level != level)
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

                                        EvoCostTexts_Level[i].text = $"Lv.{cEntity_Base.EvoCosts[i].Level}";
                                        EvoCostTexts_Memory[i].text = $"{cEntity_Base.EvoCosts[i].MemoryCost}";

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

            if (LimitObject != null && BanObject != null && LimitText != null)
            {
                LimitObject.SetActive(false);
                BanObject.SetActive(false);

                int maxCount = DeckBuildingRule.MaxCount_BanList(cEntity_Base);

                if (maxCount <= cEntity_Base.MaxCountInDeck)
                {
                    if (maxCount == 1)
                    {
                        LimitObject.SetActive(true);
                        LimitText.gameObject.SetActive(true);
                        LimitText.text = $"{maxCount}";
                    }

                    else if (maxCount == 0)
                    {
                        BanObject.SetActive(true);
                    }
                }
            }
        }
    }

    public void HideDeckCardTab()
    {
        CardImage.color = new Color(1, 1, 1, 0);
        CardImage.GetComponent<EventTrigger>().enabled = false;
        Outline.SetActive(false);

        if (CardImage.GetComponent<UIShiny>() != null)
        {
            CardImage.GetComponent<UIShiny>().enabled = false;
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
                }
            }
        }
    }

    public void SetCover(bool active)
    {
        foreach (GameObject g in Cover)
        {
            g.SetActive(active);
        }
    }

    public void CheckCover(DeckData deckData)
    {
        SetCover(CanNotAddThisCard(deckData));
    }

    public bool CanNotAddThisCard(DeckData deckData)
    {
        if (deckData.DeckCards().Count >= 70)
        {
            return true;
        }

        if (deckData.DigitamaDeckCards().Count >= 10)
        {
            return true;
        }

        if (cEntity_Base != null)
        {
            if (cEntity_Base.cardKind == CardKind.DigiEgg)
            {
                if (cEntity_Base.SameCardIDCount(deckData.DigitamaDeckCards()) >= cEntity_Base.MaxCountInDeck)
                {
                    return true;
                }
            }

            else
            {
                if (cEntity_Base.SameCardIDCount(deckData.DeckCards()) >= cEntity_Base.MaxCountInDeck)
                {
                    return true;
                }
            }

            if (!DeckBuildingRule.CanAddCard(cEntity_Base, deckData))
            {
                return true;
            }
        }

        return false;
    }

    public void OnClick()
    {
        OnClickAction?.Invoke();
    }

    public void OnBeginDrag()
    {
        OnBeginDragAction?.Invoke(this);
    }

    float _timer = 0f;
    private void Update()
    {
        if (AddRemoveButtonParent.activeSelf)
        {
            _OnEnter();
        }

        _timer += Time.deltaTime;

        if (_timer > Time.deltaTime * 6)
        {
            _timer = 0f;

            if (isActive && CardImage.gameObject.activeSelf)
            {
                if (cEntity_Base != null && Cover_Standard != null)
                {
                    Cover_Standard.SetActive(!cEntity_Base.IsStandardValid);
                }
            }
        }
    }

    public Coroutine _OnEnterCoroutine = null;
    public void OnEnter()
    {
        Outline.SetActive(true);
        anim.SetInteger("Open", 1);
        anim.SetInteger("Close", 0);
        uIShiny.enabled = true;

        _OnEnter();
    }

    IEnumerator OnEnterCoroutine()
    {
        int count = 0;

        while (true)
        {
            yield return new WaitForSeconds(Time.deltaTime);

            bool isRay = false;

            List<RaycastResult> results = new List<RaycastResult>();
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            // マウスポインタの位置にレイ飛ばし、ヒットしたものを保存
            pointer.position = Input.mousePosition;
            EventSystem.current.RaycastAll(pointer, results);
            // ヒットしたUIの名前
            foreach (RaycastResult target in results)
            {
                if (target.gameObject == this.CardImage.gameObject)
                {
                    isRay = true;
                }
            }

            if (isRay)
            {
                _OnEnter();

                count++;

                if (count > 10)
                {
                    _OnEnterCoroutine = null;
                    yield break;
                }
            }
        }
    }

    public void _OnEnter()
    {
        OnEnterAction?.Invoke(this);
    }

    public void OnExit()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
         _OnExit();
        return;
#endif
        return;
        bool isRay = false;

        List<RaycastResult> results = new List<RaycastResult>();
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        // マウスポインタの位置にレイ飛ばし、ヒットしたものを保存
        pointer.position = Input.mousePosition;
        EventSystem.current.RaycastAll(pointer, results);
        // ヒットしたUIの名前
        foreach (RaycastResult target in results)
        {
            if (target.gameObject == AddButton.gameObject || target.gameObject == RemoveButton.gameObject)
            {
                return;
            }
        }

        _OnExit();

        //StartCoroutine(OnExitCoroutine());
    }

    IEnumerator OnExitCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Time.deltaTime);

            bool isRay = false;

            List<RaycastResult> results = new List<RaycastResult>();
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            // マウスポインタの位置にレイ飛ばし、ヒットしたものを保存
            pointer.position = Input.mousePosition;
            EventSystem.current.RaycastAll(pointer, results);
            // ヒットしたUIの名前
            foreach (RaycastResult target in results)
            {
                if (target.gameObject == this.CardImage.gameObject)
                {
                    isRay = true;
                }
            }

            if (!isRay)
            {
                _OnExit();
                yield break;
            }
        }
    }

    public void _OnExit()
    {
        Outline.SetActive(false);
        anim.SetInteger("Open", 0);
        anim.SetInteger("Close", 1);
        uIShiny.enabled = false;
        OnExitAction?.Invoke(this);
        OffAddRemoveButton();
    }
}
