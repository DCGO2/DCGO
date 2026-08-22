using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Threading.Tasks;

[RequireComponent(typeof(OpenURL))]
public class Opening : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds0_15 = new WaitForSeconds(0.15f);
    private static WaitForSeconds _waitForSeconds0_1 = new WaitForSeconds(0.1f);
    [Header("読み込み中表示オブジェクト")]
    public LoadingObject LoadingObject;

    [Header("読み込み中表示オブジェクト(明るい)")]
    public LoadingObject LoadingObject_light;

    [Header("読み込み中表示オブジェクト_アンロード")]
    public LoadingObject LoadingObject_Unload;

    [Header("ホーム")]
    public HomeMode home;

    [Header("デッキ")]
    public DeckMode deck;

    [Header("バトル")]
    public BattleMode battle;

    [Header("クリック時エフェクト")]
    public GameObject OnClickEffect;

    [Header("キャンバス")]
    public RectTransform canvasRect;

    [Header("YesNoオブジェクト")]
    [SerializeField] List<YesNoObject> YesNoObjects = new List<YesNoObject>();
    [Header("Background Particle effects")]
    [SerializeField] List<ParticleSystem> _backgroundParticles = new List<ParticleSystem>();
    [SerializeField] CardImagePanel _cardImagePanel;

    public CheckUpdate checkUpdate;

    public static Opening instance = null;

    public Text VerText;
    // === DCGO-CUSTOM:onlinecount begin ===
    Text OnlineCountText;
    Text RankedCountText;
    // === DCGO-CUSTOM:onlinecount end ===
    // === DCGO-CUSTOM:friends begin ===
    Button FriendsButton;
    // === DCGO-CUSTOM:recovery begin ===
    Button AccountButton;
    // === DCGO-CUSTOM:recovery end ===
    // === DCGO-CUSTOM:friends end ===

    public OptionPanel optionPanel;
    public PatchNotes patchNotesPanel;

    public GameObject ModeButtons;

    public Vector3 DeckInfoPrefabStartScale;

    public Vector3 DeckInfoPrefabExpandScale;

    [SerializeField] Transform camerasParent;

    public Title title;

    [Header("背景Image")]
    public List<Image> BackgroundImages = new List<Image>();

    public GameObject openingObject;

    [Header("TitleButtonSE")]
    public AudioClip TitleButtonSE;

    [Header("DrawSE")]
    public AudioClip DrawSE;

    [Header("MoveSE")]
    public AudioClip MoveSE;

    [Header("DecisionSE")]
    public AudioClip DecisionSE;

    [Header("CancelSE")]
    public AudioClip CancelSE;

    [Header("BGM")]
    public AudioClip bgm;

    [Header("BGMObject")]
    public BGMObject OpeningBGM;
    private void Awake()
    {
        instance = this;

        if (openingCameras.Count >= 1)
        {
            // === DCGO-CUSTOM:reconnect begin ===
            PhotonUtility.DisconnectImmediate();
            // === DCGO-CUSTOM:reconnect end ===
            MainCamera = openingCameras[0];
        }
    // === DCGO-CUSTOM:onlinecount begin ===
    void OnDestroy()
    {
        var service = OnlinePlayerCountService.Instance;
        if (service != null)
        {
            service.Changed -= RefreshOnlinePlayerCountText;
            service.SetMenuPresenceEnabled(false);
        }
    }

    public void EnsureOnlinePlayerCountUi()
    {
        if (VerText == null)
        {
            return;
        }

        Transform parent = VerText.transform.parent != null ? VerText.transform.parent : VerText.transform;
        var verRt = VerText.GetComponent<RectTransform>();

        if (OnlineCountText == null)
        {
            OnlineCountText = CreateVerSiblingText("OnlineCountText", parent, verRt, yOffset: 70f);
        }

        if (RankedCountText == null)
        {
            // Above the online counter
            RankedCountText = CreateVerSiblingText("RankedCountText", parent, verRt, yOffset: 140f);
        }

        var svc = OnlinePlayerCountService.EnsureExists();
        svc.Changed -= RefreshOnlinePlayerCountText;
        svc.Changed += RefreshOnlinePlayerCountText;
        svc.SetMenuPresenceEnabled(true);
        RefreshOnlinePlayerCountText();
    }

    // === DCGO-CUSTOM:friends begin ===
    public void EnsureFriendsButton()
    {
        // Place next to Online/Ranked counters (bottom-right), not under ModeButtons —
        // ModeButtons is the left Battle/Deck strip and clipped the old button.
        if (VerText == null && canvasRect == null)
        {
            return;
        }

        if (FriendsButton != null)
        {
            // Recreate if a previous Play Mode left it under ModeButtons (clipped).
            Transform expectedParent = VerText != null && VerText.transform.parent != null
                ? VerText.transform.parent
                : (canvasRect != null ? canvasRect.transform : null);
            if (FriendsButton.transform.parent == expectedParent)
            {
                FriendsButton.gameObject.SetActive(true);
                return;
            }

            Destroy(FriendsButton.gameObject);
            FriendsButton = null;
        }

        Transform parent = VerText != null && VerText.transform.parent != null
            ? VerText.transform.parent
            : (canvasRect != null ? canvasRect.transform : null);
        if (parent == null)
        {
            return;
        }

        Font font = VerText != null && VerText.font != null
            ? VerText.font
            : Resources.GetBuiltinResource<Font>("Arial.ttf");

        var go = new GameObject("FriendsButton", typeof(RectTransform), typeof(Image), typeof(Button));
        go.layer = VerText != null ? VerText.gameObject.layer : parent.gameObject.layer;
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        if (VerText != null)
        {
            var verRt = VerText.GetComponent<RectTransform>();
            rt.localRotation = verRt.localRotation;
            rt.localScale = Vector3.one;
            // Same corner as Ver / Online counters, above Ranked count
            rt.anchorMin = verRt.anchorMin;
            rt.anchorMax = verRt.anchorMax;
            rt.pivot = verRt.pivot;
            rt.anchoredPosition = verRt.anchoredPosition + new Vector2(-20f, 210f);
        }
        else
        {
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-40f, 220f);
        }

        rt.sizeDelta = new Vector2(200f, 56f);
        go.GetComponent<Image>().color = new Color(0.18f, 0.42f, 0.85f, 0.95f);

        FriendsButton = go.GetComponent<Button>();
        FriendsButton.onClick.AddListener(() =>
        {
            PlayDecisionSE();
            FriendListPanel.ShowFromHome();
        });

        var labelGo = new GameObject("Label", typeof(RectTransform));
        labelGo.transform.SetParent(go.transform, false);
        var text = labelGo.AddComponent<Text>();
        text.font = font;
        text.fontSize = 24;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = LocalizeUtility.GetLocalizedString(EngMessage: "Friends", JpnMessage: "フレンド");
        text.raycastTarget = false;
        if (VerText != null && VerText.material != null)
        {
            text.material = VerText.material;
        }

        var lrt = text.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
    }
    // === DCGO-CUSTOM:friends end ===

    // === DCGO-CUSTOM:recovery begin ===
    public void EnsureAccountButton()
    {
        // Same bottom-right stack as Friends, one row above it.
        if (VerText == null && canvasRect == null)
        {
            return;
        }

        if (AccountButton != null)
        {
            Transform expectedParent = VerText != null && VerText.transform.parent != null
                ? VerText.transform.parent
                : (canvasRect != null ? canvasRect.transform : null);
            if (AccountButton.transform.parent == expectedParent)
            {
                AccountButton.gameObject.SetActive(true);
                return;
            }

            Destroy(AccountButton.gameObject);
            AccountButton = null;
        }

        Transform parent = VerText != null && VerText.transform.parent != null
            ? VerText.transform.parent
            : (canvasRect != null ? canvasRect.transform : null);
        if (parent == null)
        {
            return;
        }

        Font font = VerText != null && VerText.font != null
            ? VerText.font
            : Resources.GetBuiltinResource<Font>("Arial.ttf");

        var go = new GameObject("AccountButton", typeof(RectTransform), typeof(Image), typeof(Button));
        go.layer = VerText != null ? VerText.gameObject.layer : parent.gameObject.layer;
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        if (VerText != null)
        {
            var verRt = VerText.GetComponent<RectTransform>();
            rt.localRotation = verRt.localRotation;
            rt.localScale = Vector3.one;
            rt.anchorMin = verRt.anchorMin;
            rt.anchorMax = verRt.anchorMax;
            rt.pivot = verRt.pivot;
            // Friends is at +210; button height 56 + 12 gap → +278
            rt.anchoredPosition = verRt.anchoredPosition + new Vector2(-20f, 278f);
        }
        else
        {
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-40f, 288f);
        }

        rt.sizeDelta = new Vector2(200f, 56f);
        go.GetComponent<Image>().color = new Color(0.18f, 0.42f, 0.85f, 0.95f);

        AccountButton = go.GetComponent<Button>();
        AccountButton.onClick.AddListener(() =>
        {
            PlayDecisionSE();
            AccountRecoveryPanel.ShowFromHome();
        });

        var labelGo = new GameObject("Label", typeof(RectTransform));
        labelGo.transform.SetParent(go.transform, false);
        var text = labelGo.AddComponent<Text>();
        text.font = font;
        text.fontSize = 24;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = LocalizeUtility.GetLocalizedString(EngMessage: "Account", JpnMessage: "アカウント");
        text.raycastTarget = false;
        if (VerText != null && VerText.material != null)
        {
            text.material = VerText.material;
        }

        var lrt = text.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
    }
    // === DCGO-CUSTOM:recovery end ===

    Text CreateVerSiblingText(string name, Transform parent, RectTransform verRt, float yOffset)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = VerText.gameObject.layer;
        go.transform.SetParent(parent, false);

        var text = go.AddComponent<Text>();
        text.raycastTarget = false;
        ApplyOnlineCountTextStyle(text);

        var rt = text.GetComponent<RectTransform>();
        if (rt != null && verRt != null)
        {
            rt.localRotation = verRt.localRotation;
            rt.localScale = verRt.localScale;
            rt.anchorMin = verRt.anchorMin;
            rt.anchorMax = verRt.anchorMax;
            rt.pivot = verRt.pivot;
            rt.anchoredPosition = verRt.anchoredPosition + new Vector2(0f, yOffset);
            rt.sizeDelta = new Vector2(Mathf.Max(verRt.sizeDelta.x, 320f), verRt.sizeDelta.y);
        }

        return text;
    }

    void ApplyOnlineCountTextStyle(Text text)
    {
        if (text == null || VerText == null)
        {
            return;
        }

        if (VerText.font != null)
        {
            text.font = VerText.font;
            text.material = VerText.material;
        }

        text.fontSize = VerText.fontSize;
        text.fontStyle = VerText.fontStyle;
        text.alignment = TextAnchor.UpperRight;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = VerText.color;
        text.resizeTextForBestFit = false;
    }

    void RefreshOnlinePlayerCountText()
    {
        var svc = OnlinePlayerCountService.Instance;

        if (OnlineCountText != null)
        {
            OnlineCountText.text = svc != null
                ? svc.FormatDisplayString()
                : LocalizeUtility.GetLocalizedString("Online: —", "オンライン: —");
        }

        if (RankedCountText != null)
        {
            RankedCountText.text = svc != null
                ? svc.FormatRankedDisplayString()
                : LocalizeUtility.GetLocalizedString("Ranked: —", "ランク: —");
        }
    }
    // === DCGO-CUSTOM:onlinecount end ===
        // === DCGO-CUSTOM:recovery begin ===
        if (AccountButton != null)
        {
            AccountButton.gameObject.SetActive(false);
        }
        // === DCGO-CUSTOM:recovery end ===
    }

    int count = 0;
    int UpdateFrame = 5;
    private void Update()
    {
        #region 数フレームに一度だけ更新
        count++;

        if (count < UpdateFrame)
        {
            return;
        }

        else
        {
            count = 0;
        }
        #endregion

        GetRayCast();

        if (ContinuousController.instance != null)
        {
            foreach (ParticleSystem particleSystem in _backgroundParticles)
            {
                if (particleSystem != null)
                {
                    particleSystem.gameObject.SetActive(ContinuousController.instance.showBackgroundParticle);
                }
            }
        }
    }

    YesNoObject ActiveYesNoObject()
    {
        foreach (YesNoObject yesNoObject in YesNoObjects)
        {
            if (!yesNoObject.gameObject.activeSelf)
            {
                return yesNoObject;
            }
        }

        if (YesNoObjects.Count >= 1)
        {
            return YesNoObjects[0];
        }

        return null;
    }

    public void OffYesNoObjects()
    {
        foreach (YesNoObject yesNoObject in YesNoObjects)
        {
            yesNoObject.Close_(false);
        }
    }

    public void SetUpActiveYesNoObject(List<UnityAction> OnClickActions, List<string> CommandTexts, string _InfoText, bool CanClose)
    {
        YesNoObject activeYesNoObject = ActiveYesNoObject();

        if (activeYesNoObject != null)
        {
            activeYesNoObject.Off();
            activeYesNoObject.transform.parent.gameObject.SetActive(true);
            activeYesNoObject.transform.SetSiblingIndex(activeYesNoObject.transform.parent.childCount - 1);
            activeYesNoObject.SetUpYesNoObject(OnClickActions, CommandTexts, _InfoText, CanClose);
        }
    }

    void GetRayCast()
    {
        if (GManager.instance != null)
        {
            return;
        }

        if (ContinuousController.instance == null)
        {
            return;
        }

        //bool isRay = false;

        List<RaycastResult> results = new List<RaycastResult>();
        PointerEventData pointer = new PointerEventData(EventSystem.current)
        {
            // マウスポインタの位置にレイ飛ばし、ヒットしたものを保存
            position = Input.mousePosition
        };
        EventSystem.current.RaycastAll(pointer, results);

        // ヒットしたUIの名前
        //isRay = true;

        CardPrefab_CreateDeck cardPrefab = null;

        foreach (RaycastResult target in results)
        {
            if (target.gameObject.CompareTag("CardPrefab_CreateDeck_CardImage"))
            {
                cardPrefab = target.gameObject.transform.parent.parent.parent.GetComponent<CardPrefab_CreateDeck>();

                /*
                if (cardPrefab != null)
                {
                    if (Opening.instance.deck.trialDraw.gameObject.activeSelf)
                    {
                        if (cardPrefab.transform.parent != Opening.instance.deck.trialDraw.CardScroll.content)
                        {
                            cardPrefab = null;
                        }
                    }

                    else if (Opening.instance.deck.deckListPanel.gameObject.activeSelf)
                    {
                        if (cardPrefab.transform.parent != Opening.instance.deck.deckListPanel.DeckScroll.content)
                        {
                            cardPrefab = null;
                        }
                    }
                }
                */

                if (cardPrefab != null)
                {
                    break;
                }
            }
        }

        //isRay = cardPrefab != null;

        if (cardPrefab != null)
        {
            if (deck.editDeck.isDragging)
            {
                cardPrefab = null;
            }

            else
            {
                foreach (RaycastResult target in results)
                {
                    if (target.gameObject.layer == LayerMask.NameToLayer("IgnoreCardPrefabRaycast"))
                    {
                        if (DropArea.IsChild(Opening.instance.deck.trialDraw.gameObject, target.gameObject))
                        {
                            if (cardPrefab.transform.parent == Opening.instance.deck.trialDraw.CardScroll.content)
                            {
                                continue;
                            }
                        }

                        else if (DropArea.IsChild(Opening.instance.deck.deckListPanel.gameObject, target.gameObject))
                        {
                            if (cardPrefab.transform.parent == Opening.instance.deck.deckListPanel.DeckScroll.content)
                            {
                                continue;
                            }

                            if (cardPrefab.transform.parent == Opening.instance.deck.trialDraw.CardScroll.content)
                            {
                                continue;
                            }
                        }

                        cardPrefab = null;
                        break;
                    }
                }
            }
        }

        if (cardPrefab != null)
        {
            if (Opening.instance.deck.editDeck.gameObject.activeSelf && Opening.instance.deck.editDeck.isEditting && !Opening.instance.deck.editDeck.isDragging)
            {
                for (int i = 0; i < Opening.instance.deck.editDeck.CardPoolCardPrefabs_CreateDeck.Count; i++)
                {
                    if (Opening.instance.deck.editDeck.CardPoolCardPrefabs_CreateDeck[i] != cardPrefab)
                    {
                        Opening.instance.deck.editDeck.CardPoolCardPrefabs_CreateDeck[i]._OnExit();
                    }
                }

                for (int i = 0; i < Opening.instance.deck.editDeck.DeckScroll.content.childCount; i++)
                {
                    if (Opening.instance.deck.editDeck.DeckScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>() != cardPrefab)
                    {
                        Opening.instance.deck.editDeck.DeckScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>()._OnExit();
                    }
                }
            }

            if (Opening.instance.deck.deckListPanel.gameObject.activeSelf)
            {
                for (int i = 0; i < Opening.instance.deck.deckListPanel.DeckScroll.content.childCount; i++)
                {
                    if (Opening.instance.deck.deckListPanel.DeckScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>() != cardPrefab)
                    {
                        Opening.instance.deck.deckListPanel.DeckScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>()._OnExit();
                    }
                }
            }

            if (Opening.instance.deck.trialDraw.gameObject.activeSelf)
            {
                for (int i = 0; i < Opening.instance.deck.trialDraw.CardScroll.content.childCount; i++)
                {
                    if (Opening.instance.deck.trialDraw.CardScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>() != cardPrefab)
                    {
                        Opening.instance.deck.trialDraw.CardScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>()._OnExit();
                    }
                }
            }

            cardPrefab.OnEnter();
        }

        //if (!isRay)
        else
        {
            if (Opening.instance.deck.editDeck.gameObject.activeSelf && Opening.instance.deck.editDeck.isEditting)
            {
                for (int i = 0; i < Opening.instance.deck.editDeck.CardPoolCardPrefabs_CreateDeck.Count; i++)
                {
                    Opening.instance.deck.editDeck.CardPoolCardPrefabs_CreateDeck[i]._OnExit();
                }

                for (int i = 0; i < Opening.instance.deck.editDeck.DeckScroll.content.childCount; i++)
                {
                    Opening.instance.deck.editDeck.DeckScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>()._OnExit();
                }
            }

            if (Opening.instance.deck.deckListPanel.gameObject.activeSelf)
            {
                for (int i = 0; i < Opening.instance.deck.deckListPanel.DeckScroll.content.childCount; i++)
                {
                    Opening.instance.deck.deckListPanel.DeckScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>()._OnExit();
                }
            }

            if (Opening.instance.deck.trialDraw.gameObject.activeSelf)
            {
                for (int i = 0; i < Opening.instance.deck.trialDraw.CardScroll.content.childCount; i++)
                {
                    Opening.instance.deck.trialDraw.CardScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>()._OnExit();
                }
            }
        }
    }

    public void OpenRuleBook()
    {
        OpenURL openURL = GetComponent<OpenURL>();

        openURL.Open();
    }

    public List<Camera> openingCameras
    {
        get
        {
            List<Camera> openingCameras = new List<Camera>();

            for (int i = 0; i < camerasParent.childCount; i++)
            {             
                if (camerasParent.GetChild(i).TryGetComponent<Camera>(out var camera))
                {
                    openingCameras.Add(camera);
                }
            }

            return openingCameras;
        }
    }

    public Camera MainCamera { get; set; }

    public void PlayDecisionSE()
    {
        ContinuousController.instance.PlaySE(Opening.instance.DecisionSE);
    }

    public void PlayCancelSE()
    {
        ContinuousController.instance.PlaySE(Opening.instance.CancelSE);
    }
    public void OffModeButtons()
    {
        ModeButtons.SetActive(false);
        // === DCGO-CUSTOM:friends begin ===
        if (FriendsButton != null)
        {
            FriendsButton.gameObject.SetActive(false);
        }
        // === DCGO-CUSTOM:friends end ===
    }

    public void OnModeButtons()
    {
        ModeButtons.SetActive(true);
        // === DCGO-CUSTOM:friends begin ===
        EnsureFriendsButton();
        // === DCGO-CUSTOM:recovery begin ===
        EnsureAccountButton();
        // === DCGO-CUSTOM:recovery end ===
        // === DCGO-CUSTOM:friends end ===
    }

    // === DCGO-CUSTOM:friends begin ===
    public void EnsureFriendsButton()
    {
        if (VerText == null && canvasRect == null)
        {
            return;
        }

        if (FriendsButton != null)
        {
            Transform expectedParent = VerText != null && VerText.transform.parent != null
                ? VerText.transform.parent
                : (canvasRect != null ? canvasRect.transform : null);
            if (FriendsButton.transform.parent == expectedParent)
            {
                FriendsButton.gameObject.SetActive(true);
                return;
            }

            Destroy(FriendsButton.gameObject);
            FriendsButton = null;
        }

        Transform parent = VerText != null && VerText.transform.parent != null
            ? VerText.transform.parent
            : (canvasRect != null ? canvasRect.transform : null);
        if (parent == null)
        {
            return;
        }

        Font font = VerText != null && VerText.font != null
            ? VerText.font
            : Resources.GetBuiltinResource<Font>("Arial.ttf");

        var go = new GameObject("FriendsButton", typeof(RectTransform), typeof(Image), typeof(Button));
        go.layer = VerText != null ? VerText.gameObject.layer : parent.gameObject.layer;
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        if (VerText != null)
        {
            var verRt = VerText.GetComponent<RectTransform>();
            rt.localRotation = verRt.localRotation;
            rt.localScale = Vector3.one;
            rt.anchorMin = verRt.anchorMin;
            rt.anchorMax = verRt.anchorMax;
            rt.pivot = verRt.pivot;
            rt.anchoredPosition = verRt.anchoredPosition + new Vector2(-20f, 210f);
        }
        else
        {
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-40f, 220f);
        }

        rt.sizeDelta = new Vector2(200f, 56f);
        go.GetComponent<Image>().color = new Color(0.18f, 0.42f, 0.85f, 0.95f);

        FriendsButton = go.GetComponent<Button>();
        FriendsButton.onClick.AddListener(() =>
        {
            PlayDecisionSE();
            FriendListPanel.ShowFromHome();
        });

        var labelGo = new GameObject("Label", typeof(RectTransform));
        labelGo.transform.SetParent(go.transform, false);
        var text = labelGo.AddComponent<Text>();
        text.font = font;
        text.fontSize = 24;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = LocalizeUtility.GetLocalizedString(EngMessage: "Friends", JpnMessage: "フレンド");
        text.raycastTarget = false;
        if (VerText != null && VerText.material != null)
        {
            text.material = VerText.material;
        }

        var lrt = text.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
    }
    // === DCGO-CUSTOM:friends end ===

    public void CreateOnClickEffect()
    {
        GameObject effect = Instantiate(OnClickEffect, canvasRect.transform);

        var mousePos = Input.mousePosition;
        var magnification = canvasRect.sizeDelta.x / Screen.width;
        mousePos.x = mousePos.x * magnification - canvasRect.sizeDelta.x / 2;
        mousePos.y = mousePos.y * magnification - canvasRect.sizeDelta.y / 2;
        mousePos.z = 0;// -0.5f;// transform.localPosition.z;

        effect.transform.SetLocalPositionAndRotation(mousePos, Quaternion.Euler(new Vector3(77, 0, 0)));

        StartCoroutine(Effects.DeleteCoroutine(effect, null));
    }

    private void Start()
    {
        foreach (YesNoObject yesNoObject in YesNoObjects)
        {
            yesNoObject.Close_(false);
        }

        ChangeBackground();

        home.OffHome();

        deck.OffDeck();

        battle.OffBattle();

        _cardImagePanel.Close_(false);

        StartCoroutine(Init());
    }

    async void ChangeBackground()
    {
        Sprite backgroundSprite = await StreamingAssetsUtility.GetSprite("Background_home");

        if (backgroundSprite != null)
        {
            foreach (Image BackgroundImage in BackgroundImages)
            {
                BackgroundImage.sprite = backgroundSprite;
            }
        }
    }

    public IEnumerator Init()
    {
        OpeningBGM.StopPlayBGM();
        yield return StartCoroutine(LoadingObject.StartLoading("Now Loading"));

        LoadingObject_light.gameObject.SetActive(false);
        LoadingObject_Unload.gameObject.SetActive(false);

        yield return StartCoroutine(ContinuousController.LoadCoroutine());

        yield return new WaitWhile(() => ContinuousController.instance == null);

        // ContinuousController.instance.LoadVolume();

        home.OffHome();

        optionPanel.Init();

        patchNotesPanel.Init();

        yield return StartCoroutine(deck.editDeck.InitEditDeck());

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();

            while (PhotonNetwork.IsConnected)
            {
                yield return null;
            }
        }

        yield return _waitForSeconds0_1;

        VerText.text = $"Ver{ContinuousController.instance.GameVerString}";

        deck.SetUpDeckMode();

        deck.OffDeck();
        title.SetUpTitle();
        // === DCGO-CUSTOM:onlinecount begin ===
        EnsureOnlinePlayerCountUi();
        OnlinePlayerCountService.EnsureExists().SetMenuPresenceEnabled(true);
        // === DCGO-CUSTOM:onlinecount end ===

        LoadCardImages();

        yield return _waitForSeconds0_15;

        yield return StartCoroutine(LoadingObject.EndLoading());
    }

    async void LoadCardImages()
    {
#if UNITY_EDITOR
        foreach (CEntity_Base cEntity_Base in ContinuousController.instance.CardList)
        {
            cEntity_Base.HasLoadStarted = false;
        }
#endif

        foreach (DeckData deckData in ContinuousController.instance.DeckDatas)
        {
            CEntity_Base keyCard = deckData.KeyCard;

            if (keyCard != null)
            {
                keyCard.HasLoadStarted = false;
                await keyCard.LoadCardImage();
            }
        }
    }
}
