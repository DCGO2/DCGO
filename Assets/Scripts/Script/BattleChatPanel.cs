using System.Collections;
using System.Collections.Generic;
using System.Text;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Free-text PvP chat for BattleScene. Plain text log: green name = you, red name = opponent.
/// </summary>
public class BattleChatPanel : MonoBehaviour
{
    const int MaxStoredMessages = 80;
    const int MaxDisplayCharacters = 8000;

    const string YouNameColor = "#3DDC84";
    const string EnemyNameColor = "#FF5A5A";

    static readonly Color PanelBg = new Color(0.07f, 0.09f, 0.12f, 0.96f);
    static readonly Color HeaderBg = new Color(0.12f, 0.16f, 0.20f, 1f);
    static readonly Color ThreadBg = new Color(0.05f, 0.07f, 0.09f, 1f);
    static readonly Color InputBg = new Color(0.12f, 0.16f, 0.20f, 1f);
    static readonly Color SendBg = new Color(0.00f, 0.55f, 0.45f, 1f);

    struct ChatMessage
    {
        public string Text;
        public bool IsMine;
    }

    [SerializeField] GameObject _panelRoot;
    [SerializeField] TMP_Text _logText;
    [SerializeField] ScrollRect _scroll;
    [SerializeField] TMP_InputField _inputField;
    [SerializeField] Button _sendButton;
    [SerializeField] Button _toggleButton;
    [SerializeField] GameObject _unreadBadge;
    [SerializeField] TMP_Text _unreadBadgeText;

    readonly List<ChatMessage> _messages = new List<ChatMessage>();
    bool _built;
    bool _panelOpen;
    bool _skipCancelSe;
    int _unreadCount;

    public void Init()
    {
        ContinuousController.OnBattleChatReceived -= OnChatReceived;
        ContinuousController.OnBattleChatReceived += OnChatReceived;

        bool enableChat = ContinuousController.instance != null
            && !ContinuousController.instance.isAI
            && PhotonNetwork.InRoom;

        if (!enableChat)
        {
            Clear();
            if (_toggleButton != null)
                _toggleButton.gameObject.SetActive(false);
            OffChat(playSe: false);
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        EnsureUi();

        if (_toggleButton != null)
            _toggleButton.gameObject.SetActive(true);

        Clear();
        _skipCancelSe = true;
        OffChat(playSe: false);
        _skipCancelSe = false;
        SetUnread(0);
    }

    public void Clear()
    {
        _messages.Clear();
        if (_logText != null)
            _logText.text = string.Empty;
        if (_inputField != null)
            _inputField.text = string.Empty;
        SetUnread(0);
    }

    public void OnClickToggleButton()
    {
        if (_panelOpen)
            OffChat();
        else
            SetUpChat();
    }

    public void SetUpChat()
    {
        EnsureUi();
        if (_panelRoot != null)
            _panelRoot.SetActive(true);

        _panelOpen = true;
        SetUnread(0);

        if (GManager.instance != null)
            GManager.instance.PlayDecisionSE();

        RefreshLogText();
        StartCoroutine(ScrollToBottomNextFrame());

        if (_inputField != null)
            _inputField.ActivateInputField();
    }

    public void OffChat(bool playSe = true)
    {
        if (_panelRoot != null)
            _panelRoot.SetActive(false);

        if (_panelOpen && playSe && !_skipCancelSe && GManager.instance != null)
            GManager.instance.PlayCancelSE();

        _panelOpen = false;
    }

    void OnDestroy()
    {
        ContinuousController.OnBattleChatReceived -= OnChatReceived;
        if (_inputField != null)
            _inputField.onSubmit.RemoveListener(OnInputSubmit);
        if (_sendButton != null)
            _sendButton.onClick.RemoveListener(OnClickSend);
        if (_toggleButton != null)
            _toggleButton.onClick.RemoveListener(OnClickToggleButton);
    }

    void OnChatReceived(string senderName, string text, int actorNumber)
    {
        string safeText = DataBase.ReplaceToASCII(text ?? string.Empty);
        bool isMine = PhotonNetwork.LocalPlayer != null
            && actorNumber == PhotonNetwork.LocalPlayer.ActorNumber;

        _messages.Add(new ChatMessage
        {
            Text = safeText,
            IsMine = isMine,
        });

        while (_messages.Count > MaxStoredMessages || GetCombinedLength() > MaxDisplayCharacters)
        {
            if (_messages.Count == 0)
                break;
            _messages.RemoveAt(0);
        }

        if (_panelOpen)
        {
            RefreshLogText();
            StartCoroutine(ScrollToBottomNextFrame());
        }
        else
        {
            SetUnread(_unreadCount + 1);
        }
    }

    void OnClickSend()
    {
        TrySendCurrentInput();
    }

    void OnInputSubmit(string _)
    {
        TrySendCurrentInput();
    }

    void TrySendCurrentInput()
    {
        if (_inputField == null || ContinuousController.instance == null)
            return;

        string text = _inputField.text;
        _inputField.text = string.Empty;
        ContinuousController.instance.SendBattleChat(text);
        _inputField.ActivateInputField();
    }

    void RefreshLogText()
    {
        if (_logText == null)
            return;

        var sb = new StringBuilder(256);
        for (int i = 0; i < _messages.Count; i++)
        {
            ChatMessage msg = _messages[i];
            string nameColor = msg.IsMine ? YouNameColor : EnemyNameColor;
            string displayName = msg.IsMine ? "You" : "Opponent";
            string align = msg.IsMine ? "left" : "right";

            sb.Append("<align=").Append(align).Append(">")
                .Append("<color=").Append(nameColor).Append("><b>")
                .Append(displayName)
                .Append(":</b></color> ")
                .Append(EscapeRichText(msg.Text))
                .Append("</align>\n");
        }

        _logText.text = sb.ToString();
    }

    static string EscapeRichText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;
        return text.Replace("<", " ").Replace(">", " ");
    }

    int GetCombinedLength()
    {
        int length = 0;
        for (int i = 0; i < _messages.Count; i++)
            length += _messages[i].Text.Length + 12;
        return length;
    }

    void SetUnread(int count)
    {
        _unreadCount = Mathf.Max(0, count);
        if (_unreadBadge != null)
            _unreadBadge.SetActive(_unreadCount > 0);
        if (_unreadBadgeText != null)
            _unreadBadgeText.text = _unreadCount > 9 ? "9+" : _unreadCount.ToString();
    }

    IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;
        if (_scroll != null)
            _scroll.verticalNormalizedPosition = 0f;
    }

    void EnsureUi()
    {
        if (_built && _panelRoot != null && _toggleButton != null && _logText != null)
            return;

        // Drop broken bubble panel from previous build
        if (_panelRoot != null && _logText == null)
        {
            Destroy(_panelRoot);
            _panelRoot = null;
            _scroll = null;
            _inputField = null;
            _sendButton = null;
        }

        if (_panelRoot == null || _logText == null || _scroll == null || _inputField == null || _sendButton == null)
            BuildDefaultUi();

        WireHandlers();
        _built = true;
    }

    void WireHandlers()
    {
        if (_sendButton != null)
        {
            _sendButton.onClick.RemoveListener(OnClickSend);
            _sendButton.onClick.AddListener(OnClickSend);
        }

        if (_inputField != null)
        {
            _inputField.characterLimit = ContinuousController.BattleChatMaxLength;
            _inputField.lineType = TMP_InputField.LineType.SingleLine;
            _inputField.onSubmit.RemoveListener(OnInputSubmit);
            _inputField.onSubmit.AddListener(OnInputSubmit);
        }

        if (_toggleButton != null)
        {
            _toggleButton.onClick.RemoveListener(OnClickToggleButton);
            _toggleButton.onClick.AddListener(OnClickToggleButton);
        }
    }

    void BuildDefaultUi()
    {
        // Remove leftover bubble UI from previous builds
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name == "ChatPanel" || child.name == "ChatButton" || child.name == "ChatToggleButton")
                Destroy(child.gameObject);
        }
        _panelRoot = null;
        _logText = null;
        _scroll = null;
        _inputField = null;
        _sendButton = null;
        _toggleButton = null;
        _unreadBadge = null;
        _unreadBadgeText = null;

        Transform canvasTransform = transform;
        if (GManager.instance != null && GManager.instance.canvas != null)
            canvasTransform = GManager.instance.canvas.transform;

        if (transform.parent != canvasTransform)
            transform.SetParent(canvasTransform, false);

        RectTransform rootRt = GetComponent<RectTransform>();
        if (rootRt == null)
            rootRt = gameObject.AddComponent<RectTransform>();
        StretchFull(rootRt);
        rootRt.anchorMin = new Vector2(0f, 0f);
        rootRt.anchorMax = new Vector2(1f, 1f);
        rootRt.offsetMin = Vector2.zero;
        rootRt.offsetMax = Vector2.zero;

        BuildToggleButton();
        BuildChatPanel();

        if (_panelRoot != null)
            _panelRoot.SetActive(false);
    }

    void BuildToggleButton()
    {
        if (_toggleButton != null)
            return;

        Sprite circleSprite = TryGetHudCircleSprite();

        GameObject toggleGo = CreateUiObject("ChatButton", transform);
        RectTransform toggleRt = toggleGo.GetComponent<RectTransform>();
        toggleRt.anchorMin = toggleRt.anchorMax = new Vector2(0.5f, 0.5f);
        toggleRt.anchoredPosition = new Vector2(-654.2f, 478.3f);
        toggleRt.sizeDelta = new Vector2(100f, 100f);
        toggleRt.localScale = new Vector3(0.75f, 0.75f, 1f);

        GameObject bgGo = CreateUiObject("background", toggleGo.transform);
        RectTransform bgRt = bgGo.GetComponent<RectTransform>();
        bgRt.anchorMin = bgRt.anchorMax = new Vector2(0.5f, 0.5f);
        bgRt.anchoredPosition = Vector2.zero;
        bgRt.sizeDelta = new Vector2(190f, 190f);
        Image bgImage = bgGo.AddComponent<Image>();
        bgImage.raycastTarget = false;
        bgImage.color = new Color(0f, 0.045201976f, 0.3882353f, 1f);
        if (circleSprite != null)
            bgImage.sprite = circleSprite;

        GameObject iconGo = CreateUiObject("ChatIconButton", bgGo.transform);
        RectTransform iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = iconRt.anchorMax = new Vector2(0.5f, 0.5f);
        iconRt.anchoredPosition = Vector2.zero;
        iconRt.sizeDelta = new Vector2(190f, 190f);
        Image iconImage = iconGo.AddComponent<Image>();
        iconImage.color = new Color(1f, 1f, 1f, 0.08f);
        if (circleSprite != null)
            iconImage.sprite = circleSprite;

        _toggleButton = iconGo.AddComponent<Button>();
        _toggleButton.targetGraphic = iconImage;
        _toggleButton.transition = Selectable.Transition.ColorTint;

        TMP_Text toggleLabel = CreateTmpText(iconGo.transform, "Label", "C", 72f);
        toggleLabel.fontStyle = FontStyles.Bold;
        StretchFull(toggleLabel.rectTransform);

        GameObject badgeGo = CreateUiObject("UnreadBadge", toggleGo.transform);
        RectTransform badgeRt = badgeGo.GetComponent<RectTransform>();
        badgeRt.anchorMin = badgeRt.anchorMax = new Vector2(1f, 1f);
        badgeRt.pivot = new Vector2(0.5f, 0.5f);
        badgeRt.anchoredPosition = new Vector2(18f, 18f);
        badgeRt.sizeDelta = new Vector2(48f, 48f);
        Image badgeImage = badgeGo.AddComponent<Image>();
        badgeImage.color = new Color(0.85f, 0.15f, 0.15f, 1f);
        if (circleSprite != null)
            badgeImage.sprite = circleSprite;
        _unreadBadge = badgeGo;
        _unreadBadgeText = CreateTmpText(badgeGo.transform, "Count", "0", 22f);
        StretchFull(_unreadBadgeText.rectTransform);
        badgeGo.SetActive(false);
    }

    void BuildChatPanel()
    {
        if (_panelRoot != null)
            return;

        GameObject panelGo = CreateUiObject("ChatPanel", transform);
        RectTransform panelRt = panelGo.GetComponent<RectTransform>();
        panelRt.anchorMin = panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = new Vector2(-400f, 20f);
        panelRt.sizeDelta = new Vector2(540f, 680f);
        Image panelBg = panelGo.AddComponent<Image>();
        panelBg.color = PanelBg;
        _panelRoot = panelGo;

        // Header
        GameObject headerGo = CreateUiObject("Header", panelGo.transform);
        RectTransform headerRt = headerGo.GetComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0f, 1f);
        headerRt.anchorMax = new Vector2(1f, 1f);
        headerRt.pivot = new Vector2(0.5f, 1f);
        headerRt.anchoredPosition = Vector2.zero;
        headerRt.sizeDelta = new Vector2(0f, 56f);
        Image headerImage = headerGo.AddComponent<Image>();
        headerImage.color = HeaderBg;

        TMP_Text title = CreateTmpText(headerGo.transform, "Title", "Chat", 26f);
        title.alignment = TextAlignmentOptions.Left;
        title.fontStyle = FontStyles.Bold;
        RectTransform titleRt = title.rectTransform;
        titleRt.anchorMin = new Vector2(0f, 0f);
        titleRt.anchorMax = new Vector2(1f, 1f);
        titleRt.offsetMin = new Vector2(18f, 0f);
        titleRt.offsetMax = new Vector2(-70f, 0f);

        GameObject closeGo = CreateUiObject("CloseButton", headerGo.transform);
        RectTransform closeRt = closeGo.GetComponent<RectTransform>();
        closeRt.anchorMin = closeRt.anchorMax = new Vector2(1f, 0.5f);
        closeRt.pivot = new Vector2(1f, 0.5f);
        closeRt.anchoredPosition = new Vector2(-10f, 0f);
        closeRt.sizeDelta = new Vector2(44f, 36f);
        Image closeImage = closeGo.AddComponent<Image>();
        closeImage.color = new Color(1f, 1f, 1f, 0.08f);
        Button closeButton = closeGo.AddComponent<Button>();
        closeButton.targetGraphic = closeImage;
        closeButton.onClick.AddListener(() => OffChat());
        TMP_Text closeLabel = CreateTmpText(closeGo.transform, "Label", "X", 22f);
        StretchFull(closeLabel.rectTransform);

        // Message thread (plain text, no bubbles)
        GameObject scrollGo = CreateUiObject("Scroll", panelGo.transform);
        RectTransform scrollRt = scrollGo.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0f, 0f);
        scrollRt.anchorMax = new Vector2(1f, 1f);
        scrollRt.offsetMin = new Vector2(0f, 64f);
        scrollRt.offsetMax = new Vector2(0f, -56f);
        Image scrollImage = scrollGo.AddComponent<Image>();
        scrollImage.color = ThreadBg;
        scrollGo.AddComponent<RectMask2D>();

        GameObject viewportGo = CreateUiObject("Viewport", scrollGo.transform);
        StretchFull(viewportGo.GetComponent<RectTransform>());
        viewportGo.AddComponent<RectMask2D>();

        GameObject contentGo = CreateUiObject("Content", viewportGo.transform);
        RectTransform contentRt = contentGo.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 0f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.offsetMin = Vector2.zero;
        contentRt.offsetMax = Vector2.zero;

        ContentSizeFitter contentFitter = contentGo.AddComponent<ContentSizeFitter>();
        contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _logText = CreateTmpText(contentGo.transform, "LogText", string.Empty, 22f);
        _logText.alignment = TextAlignmentOptions.TopLeft;
        _logText.enableWordWrapping = true;
        _logText.richText = true;
        _logText.color = Color.white;
        RectTransform logRt = _logText.rectTransform;
        logRt.anchorMin = new Vector2(0f, 1f);
        logRt.anchorMax = new Vector2(1f, 1f);
        logRt.pivot = new Vector2(0.5f, 1f);
        logRt.anchoredPosition = Vector2.zero;
        logRt.sizeDelta = new Vector2(0f, 0f);
        logRt.offsetMin = new Vector2(14f, logRt.offsetMin.y);
        logRt.offsetMax = new Vector2(-14f, logRt.offsetMax.y);
        ContentSizeFitter logFitter = _logText.gameObject.AddComponent<ContentSizeFitter>();
        logFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        logFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _scroll = scrollGo.AddComponent<ScrollRect>();
        _scroll.viewport = viewportGo.GetComponent<RectTransform>();
        _scroll.content = contentRt;
        _scroll.horizontal = false;
        _scroll.vertical = true;
        _scroll.movementType = ScrollRect.MovementType.Clamped;
        _scroll.scrollSensitivity = 30f;

        // Input bar
        GameObject inputBar = CreateUiObject("InputBar", panelGo.transform);
        RectTransform inputBarRt = inputBar.GetComponent<RectTransform>();
        inputBarRt.anchorMin = new Vector2(0f, 0f);
        inputBarRt.anchorMax = new Vector2(1f, 0f);
        inputBarRt.pivot = new Vector2(0.5f, 0f);
        inputBarRt.anchoredPosition = Vector2.zero;
        inputBarRt.sizeDelta = new Vector2(0f, 64f);
        Image inputBarImage = inputBar.AddComponent<Image>();
        inputBarImage.color = HeaderBg;

        GameObject inputGo = CreateUiObject("InputField", inputBar.transform);
        RectTransform inputRt = inputGo.GetComponent<RectTransform>();
        inputRt.anchorMin = new Vector2(0f, 0.5f);
        inputRt.anchorMax = new Vector2(1f, 0.5f);
        inputRt.pivot = new Vector2(0.5f, 0.5f);
        inputRt.anchoredPosition = new Vector2(-42f, 0f);
        inputRt.sizeDelta = new Vector2(-100f, 40f);
        Image inputBg = inputGo.AddComponent<Image>();
        inputBg.color = InputBg;

        TMP_Text placeholder = CreateTmpText(inputGo.transform, "Placeholder", "Message...", 20f);
        placeholder.fontStyle = FontStyles.Italic;
        placeholder.color = new Color(1f, 1f, 1f, 0.45f);
        placeholder.alignment = TextAlignmentOptions.Left;
        StretchFull(placeholder.rectTransform, 14f, 4f, 14f, 4f);

        TMP_Text inputText = CreateTmpText(inputGo.transform, "Text", string.Empty, 20f);
        inputText.alignment = TextAlignmentOptions.Left;
        StretchFull(inputText.rectTransform, 14f, 4f, 14f, 4f);

        _inputField = inputGo.AddComponent<TMP_InputField>();
        _inputField.textViewport = inputRt;
        _inputField.textComponent = inputText;
        _inputField.placeholder = placeholder;
        _inputField.fontAsset = inputText.font;

        GameObject sendGo = CreateUiObject("SendButton", inputBar.transform);
        RectTransform sendRt = sendGo.GetComponent<RectTransform>();
        sendRt.anchorMin = sendRt.anchorMax = new Vector2(1f, 0.5f);
        sendRt.pivot = new Vector2(1f, 0.5f);
        sendRt.anchoredPosition = new Vector2(-10f, 0f);
        sendRt.sizeDelta = new Vector2(70f, 40f);
        Image sendImage = sendGo.AddComponent<Image>();
        sendImage.color = SendBg;
        _sendButton = sendGo.AddComponent<Button>();
        _sendButton.targetGraphic = sendImage;
        TMP_Text sendLabel = CreateTmpText(sendGo.transform, "Label", "Send", 18f);
        StretchFull(sendLabel.rectTransform);
    }

    static Sprite TryGetHudCircleSprite()
    {
        GameObject logButton = GameObject.Find("LogButton");
        if (logButton == null)
            return null;

        Transform bg = logButton.transform.Find("background");
        if (bg == null)
            return null;

        Image bgImage = bg.GetComponent<Image>();
        return bgImage != null ? bgImage.sprite : null;
    }

    static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        return go;
    }

    static TMP_Text CreateTmpText(Transform parent, string name, string text, float fontSize)
    {
        GameObject go = CreateUiObject(name, parent);
        TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;
        return tmp;
    }

    static void StretchFull(RectTransform rt, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }
}
