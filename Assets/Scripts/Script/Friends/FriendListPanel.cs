using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Runtime-built Friends panel on the Opening canvas.
/// </summary>
public class FriendListPanel : MonoBehaviour
{
    static FriendListPanel _instance;

    GameObject _root;
    Text _ownCodeText;
    Text _statusText;
    InputField _addCodeField;
    Transform _listContent;
    readonly List<GameObject> _rows = new List<GameObject>();
    YesNoObject _formatWindow;
    bool _open;

    public static FriendListPanel EnsureExists()
    {
        if (_instance != null)
        {
            return _instance;
        }

        var go = new GameObject("FriendListPanelHost");
        _instance = go.AddComponent<FriendListPanel>();
        DontDestroyOnLoad(go);
        return _instance;
    }

    public static void ShowFromHome()
    {
        EnsureExists().Show();
    }

    public static void HideIfOpen()
    {
        if (_instance != null && _instance._open)
        {
            _instance.Hide();
        }
    }

    public void Show()
    {
        // Rebuild UI so layout tweaks apply after script recompile / Play restart mid-session.
        if (_root != null)
        {
            Destroy(_root);
            _root = null;
            _ownCodeText = null;
            _statusText = null;
            _addCodeField = null;
            _listContent = null;
            _rows.Clear();
        }

        EnsureUi();
        _open = true;
        _root.SetActive(true);

        var svc = FriendServices.EnsureExists();
        svc.List.Changed -= RefreshList;
        svc.List.Changed += RefreshList;
        svc.Duel.PresenceChanged -= RefreshList;
        svc.Duel.PresenceChanged += RefreshList;
        svc.Duel.SetInviteListening(true);
        svc.Duel.StartPresencePolling();

        StartCoroutine(BootstrapCoroutine());
    }

    public void Hide()
    {
        _open = false;
        if (_root != null)
        {
            _root.SetActive(false);
        }

        var svc = FriendServices.Instance;
        if (svc != null)
        {
            svc.List.Changed -= RefreshList;
            svc.Duel.PresenceChanged -= RefreshList;
            // Do not stop presence / invite watch — Home keeps listening for challenges.
        }
    }

    IEnumerator BootstrapCoroutine()
    {
        SetStatus("Loading...");
        var list = FriendServices.EnsureExists().List;
        yield return list.EnsureLoggedIn();
        yield return list.RefreshFromPlayFab();
        UpdateOwnCode();
        RefreshList();
        FriendServices.EnsureExists().Duel.RequestFindFriends();
        SetStatus("");
    }

    void UpdateOwnCode()
    {
        string id = FriendListService.LocalPlayFabId() ?? "—";
        if (_ownCodeText != null)
        {
            _ownCodeText.text = LocalizeUtility.GetLocalizedString(
                EngMessage: $"Your friend code: {id}",
                JpnMessage: $"フレンドコード: {id}");
        }
    }

    void SetStatus(string msg)
    {
        if (_statusText != null)
        {
            _statusText.text = msg ?? "";
        }
    }

    void EnsureUi()
    {
        if (_root != null)
        {
            return;
        }

        Canvas canvas = null;
        if (Opening.instance != null && Opening.instance.canvasRect != null)
        {
            canvas = Opening.instance.canvasRect.GetComponentInParent<Canvas>();
        }

        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }

        Font font = Opening.instance != null && Opening.instance.VerText != null
            ? Opening.instance.VerText.font
            : Resources.GetBuiltinResource<Font>("Arial.ttf");

        _root = new GameObject("FriendListPanel", typeof(RectTransform), typeof(Image));
        _root.transform.SetParent(canvas.transform, false);
        var rootRt = _root.GetComponent<RectTransform>();
        rootRt.anchorMin = new Vector2(0.5f, 0.5f);
        rootRt.anchorMax = new Vector2(0.5f, 0.5f);
        rootRt.pivot = new Vector2(0.5f, 0.5f);
        rootRt.sizeDelta = new Vector2(980f, 760f);
        rootRt.anchoredPosition = Vector2.zero;
        var bg = _root.GetComponent<Image>();
        bg.color = new Color(0.08f, 0.1f, 0.16f, 0.96f);

        // Row 1 — friend code + Copy / Close with clear gaps
        _ownCodeText = CreateText(_root.transform, "OwnCode", font, 22, TextAnchor.UpperLeft,
            new Vector2(28f, -22f), new Vector2(640f, 40f));
        CreateButton(_root.transform, "Copy", font, "Copy", new Vector2(700f, -18f), new Vector2(130f, 48f),
            () =>
            {
                string id = FriendListService.LocalPlayFabId();
                if (!string.IsNullOrEmpty(id))
                {
                    GUIUtility.systemCopyBuffer = id;
                    SetStatus(LocalizeUtility.GetLocalizedString(
                        EngMessage: "Copied friend code.",
                        JpnMessage: "フレンドコードをコピーしました。"));
                }
            });
        CreateButton(_root.transform, "Close", font, "X", new Vector2(900f, -18f), new Vector2(52f, 48f), Hide);

        // Row 2 — paste code + Add
        _addCodeField = CreateInputField(_root.transform, "AddCode", font,
            new Vector2(28f, -86f), new Vector2(680f, 48f));
        CreateButton(_root.transform, "Add", font,
            LocalizeUtility.GetLocalizedString(EngMessage: "Add", JpnMessage: "追加"),
            new Vector2(740f, -86f), new Vector2(212f, 48f), OnClickAddCode);

        // Row 3 — last opponent on its own line so it is not jammed against Add
        CreateButton(_root.transform, "AddLast", font,
            LocalizeUtility.GetLocalizedString(EngMessage: "Add last opponent", JpnMessage: "直前の相手を追加"),
            new Vector2(28f, -150f), new Vector2(280f, 48f), OnClickAddLast);

        _statusText = CreateText(_root.transform, "Status", font, 18, TextAnchor.UpperLeft,
            new Vector2(320f, -158f), new Vector2(620f, 36f));

        var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollGo.transform.SetParent(_root.transform, false);
        var scrollRt = scrollGo.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0f, 0f);
        scrollRt.anchorMax = new Vector2(1f, 1f);
        scrollRt.offsetMin = new Vector2(24f, 24f);
        scrollRt.offsetMax = new Vector2(-24f, -220f);
        scrollGo.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.1f, 0.9f);

        var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentGo.transform.SetParent(scrollGo.transform, false);
        var contentRt = contentGo.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = new Vector2(0f, 0f);
        var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.spacing = 6f;
        vlg.padding = new RectOffset(8, 8, 8, 8);
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        var fitter = contentGo.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var scroll = scrollGo.GetComponent<ScrollRect>();
        scroll.content = contentRt;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.viewport = scrollRt;
        _listContent = contentGo.transform;

        _root.SetActive(false);
    }

    void RefreshList()
    {
        if (_listContent == null)
        {
            return;
        }

        foreach (var row in _rows)
        {
            if (row != null)
            {
                Destroy(row);
            }
        }

        _rows.Clear();

        Font font = Opening.instance != null && Opening.instance.VerText != null
            ? Opening.instance.VerText.font
            : Resources.GetBuiltinResource<Font>("Arial.ttf");

        var friends = FriendServices.EnsureExists().List.Friends;
        var duel = FriendServices.EnsureExists().Duel;

        if (friends.Count == 0)
        {
            var empty = CreateText(_listContent, "Empty", font, 20, TextAnchor.MiddleCenter,
                Vector2.zero, new Vector2(800f, 40f));
            empty.text = LocalizeUtility.GetLocalizedString(
                EngMessage: "No friends yet. Add a friend code or last opponent.",
                JpnMessage: "フレンドがいません。コードか直前の相手を追加してください。");
            var le = empty.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 48f;
            _rows.Add(empty.gameObject);
            return;
        }

        for (int i = 0; i < friends.Count; i++)
        {
            var friend = friends[i];
            _rows.Add(CreateFriendRow(friend, duel, font));
        }
    }

    GameObject CreateFriendRow(FriendEntry friend, FriendDuelService duel, Font font)
    {
        var row = new GameObject("FriendRow_" + friend.playFabId, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        row.transform.SetParent(_listContent, false);
        row.GetComponent<Image>().color = new Color(0.14f, 0.16f, 0.22f, 1f);
        row.GetComponent<LayoutElement>().preferredHeight = 64f;

        string presenceLabel = LocalizeUtility.GetLocalizedString(
            EngMessage: "Offline", JpnMessage: "オフライン");
        Color presenceColor = new Color(0.7f, 0.7f, 0.7f);
        bool canDuel = false;
        var presence = duel.GetPresence(friend.playFabId);
        if (presence != null && presence.isOnline)
        {
            if (presence.isInRoom)
            {
                presenceLabel = LocalizeUtility.GetLocalizedString(
                    EngMessage: "In match", JpnMessage: "対戦中");
                presenceColor = new Color(1f, 0.75f, 0.3f);
            }
            else
            {
                presenceLabel = LocalizeUtility.GetLocalizedString(
                    EngMessage: "Online", JpnMessage: "オンライン");
                presenceColor = new Color(0.35f, 0.95f, 0.45f);
                canDuel = true;
            }
        }

        string name = string.IsNullOrEmpty(friend.displayName) ? friend.playFabId : friend.displayName;
        var nameText = CreateText(row.transform, "Name", font, 22, TextAnchor.MiddleLeft,
            new Vector2(16f, 0f), new Vector2(320f, 50f));
        nameText.text = name;
        var nameRt = nameText.GetComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0f, 0.5f);
        nameRt.anchorMax = new Vector2(0f, 0.5f);
        nameRt.pivot = new Vector2(0f, 0.5f);

        var status = CreateText(row.transform, "Presence", font, 18, TextAnchor.MiddleLeft,
            new Vector2(350f, 0f), new Vector2(160f, 50f));
        status.text = presenceLabel;
        status.color = presenceColor;
        var statusRt = status.GetComponent<RectTransform>();
        statusRt.anchorMin = new Vector2(0f, 0.5f);
        statusRt.anchorMax = new Vector2(0f, 0.5f);
        statusRt.pivot = new Vector2(0f, 0.5f);

        string capturedId = friend.playFabId;
        string capturedName = name;
        var duelBtn = CreateButton(row.transform, "Duel", font,
            LocalizeUtility.GetLocalizedString(EngMessage: "Duel", JpnMessage: "デュエル"),
            new Vector2(520f, -8f), new Vector2(150f, 44f),
            () => OnClickDuel(capturedId, capturedName));
        duelBtn.GetComponent<Button>().interactable = canDuel;

        CreateButton(row.transform, "Remove", font,
            LocalizeUtility.GetLocalizedString(EngMessage: "Remove", JpnMessage: "削除"),
            new Vector2(700f, -8f), new Vector2(150f, 44f),
            () => StartCoroutine(RemoveFriendCoroutine(capturedId)));

        return row;
    }

    void OnClickAddCode()
    {
        string code = _addCodeField != null ? _addCodeField.text : null;
        StartCoroutine(AddCodeCoroutine(code));
    }

    IEnumerator AddCodeCoroutine(string code)
    {
        SetStatus("Adding...");
        bool ok = false;
        string err = null;
        yield return FriendServices.EnsureExists().List.AddFriendById(code, null, (success, error) =>
        {
            ok = success;
            err = error;
        });
        SetStatus(ok
            ? LocalizeUtility.GetLocalizedString(EngMessage: "Friend added.", JpnMessage: "追加しました。")
            : (err ?? "Failed"));
        if (ok && _addCodeField != null)
        {
            _addCodeField.text = "";
        }

        FriendServices.EnsureExists().Duel.RequestFindFriends();
        RefreshList();
    }

    void OnClickAddLast()
    {
        StartCoroutine(AddLastCoroutine());
    }

    IEnumerator AddLastCoroutine()
    {
        SetStatus("Adding...");
        bool ok = false;
        string err = null;
        yield return FriendServices.EnsureExists().List.AddLastOpponent((success, error) =>
        {
            ok = success;
            err = error;
        });
        SetStatus(ok
            ? LocalizeUtility.GetLocalizedString(EngMessage: "Friend added.", JpnMessage: "追加しました。")
            : (err ?? "Failed"));
        FriendServices.EnsureExists().Duel.RequestFindFriends();
        RefreshList();
    }

    IEnumerator RemoveFriendCoroutine(string id)
    {
        yield return FriendServices.EnsureExists().List.RemoveFriend(id);
        RefreshList();
    }

    void OnClickDuel(string playFabId, string displayName)
    {
        if (!FriendServices.EnsureExists().Duel.CanChallenge(playFabId))
        {
            SetStatus(LocalizeUtility.GetLocalizedString(
                EngMessage: "Friend is not available.",
                JpnMessage: "フレンドは挑戦できません。"));
            return;
        }

        ShowFormatPicker(playFabId, displayName);
    }

    void ShowFormatPicker(string playFabId, string displayName)
    {
        var window = ResolveFormatWindow();
        if (window == null)
        {
            // Fallback: Bo1 directly
            FriendServices.EnsureExists().Duel.ChallengeFriend(playFabId, displayName, 1);
            Hide();
            return;
        }

        window.CloseOnButtonClicked = true;
        window.SetUpYesNoObject(
            new List<UnityAction>
            {
                () =>
                {
                    Hide();
                    FriendServices.EnsureExists().Duel.ChallengeFriend(playFabId, displayName, 1);
                },
                () =>
                {
                    Hide();
                    FriendServices.EnsureExists().Duel.ChallengeFriend(playFabId, displayName, 2);
                },
            },
            new List<string>
            {
                LocalizeUtility.GetLocalizedString(EngMessage: "Best of 1", JpnMessage: "1本勝負"),
                LocalizeUtility.GetLocalizedString(EngMessage: "Best of 3", JpnMessage: "3本先取"),
            },
            LocalizeUtility.GetLocalizedString(
                EngMessage: $"Challenge {displayName}",
                JpnMessage: $"{displayName}に挑戦"),
            true);
    }

    YesNoObject ResolveFormatWindow()
    {
        if (_formatWindow != null)
        {
            return _formatWindow;
        }

        if (Opening.instance == null)
        {
            return null;
        }

        var yesNos = Opening.instance.GetComponentsInChildren<YesNoObject>(true);
        if (yesNos != null && yesNos.Length > 0)
        {
            _formatWindow = yesNos[0];
        }

        return _formatWindow;
    }

    static Text CreateText(Transform parent, string name, Font font, int size, TextAnchor anchor,
        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.alignment = anchor;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        var rt = text.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        return text;
    }

    static GameObject CreateButton(Transform parent, string name, Font font, string label,
        Vector2 anchoredPos, Vector2 sizeDelta, UnityAction onClick)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        go.GetComponent<Image>().color = new Color(0.25f, 0.4f, 0.7f, 1f);
        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(onClick);

        var textGo = new GameObject("Label", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var text = textGo.AddComponent<Text>();
        text.font = font;
        text.fontSize = 18;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = label;
        text.raycastTarget = false;
        var trt = text.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        return go;
    }

    static InputField CreateInputField(Transform parent, string name, Font font,
        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        go.GetComponent<Image>().color = new Color(0.2f, 0.22f, 0.28f, 1f);

        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var text = textGo.AddComponent<Text>();
        text.font = font;
        text.fontSize = 20;
        text.color = Color.white;
        text.supportRichText = false;
        var trt = text.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(10f, 4f);
        trt.offsetMax = new Vector2(-10f, -4f);

        var placeholderGo = new GameObject("Placeholder", typeof(RectTransform));
        placeholderGo.transform.SetParent(go.transform, false);
        var ph = placeholderGo.AddComponent<Text>();
        ph.font = font;
        ph.fontSize = 18;
        ph.fontStyle = FontStyle.Italic;
        ph.color = new Color(1f, 1f, 1f, 0.4f);
        ph.text = LocalizeUtility.GetLocalizedString(
            EngMessage: "Paste friend code",
            JpnMessage: "フレンドコードを入力");
        var prt = ph.GetComponent<RectTransform>();
        prt.anchorMin = Vector2.zero;
        prt.anchorMax = Vector2.one;
        prt.offsetMin = new Vector2(10f, 4f);
        prt.offsetMax = new Vector2(-10f, -4f);

        var input = go.GetComponent<InputField>();
        input.textComponent = text;
        input.placeholder = ph;
        return input;
    }
}
