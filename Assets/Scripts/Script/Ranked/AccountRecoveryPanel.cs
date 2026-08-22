using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Runtime Account panel: show / copy private recovery code, recover after phone wipe.
/// Not the public friend code (PlayFabId).
/// </summary>
public class AccountRecoveryPanel : MonoBehaviour
{
    static AccountRecoveryPanel _instance;

    GameObject _root;
    Text _codeText;
    Text _showHideLabel;
    Text _statusText;
    InputField _recoverField;
    YesNoObject _confirmWindow;
    bool _open;
    bool _busy;
    bool _codeVisible;

    public static AccountRecoveryPanel EnsureExists()
    {
        if (_instance != null)
        {
            return _instance;
        }

        var go = new GameObject("AccountRecoveryPanelHost");
        _instance = go.AddComponent<AccountRecoveryPanel>();
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
        if (_root != null)
        {
            Destroy(_root);
            _root = null;
            _codeText = null;
            _showHideLabel = null;
            _statusText = null;
            _recoverField = null;
        }

        EnsureUi();
        _open = true;
        _busy = false;
        _codeVisible = false;
        _root.SetActive(true);
        StartCoroutine(BootstrapCoroutine());
    }

    public void Hide()
    {
        _open = false;
        if (_root != null)
        {
            _root.SetActive(false);
        }
    }

    IEnumerator BootstrapCoroutine()
    {
        SetStatus(LocalizeUtility.GetLocalizedString(
            EngMessage: "Loading...",
            JpnMessage: "読み込み中..."));

        var ranked = RankedServices.EnsureExists();
        yield return ranked.Auth.EnsureLoggedIn();

        bool ok = false;
        string err = null;
        yield return ranked.Auth.EnsureRecoveryCodeAttached((success, error) =>
        {
            ok = success;
            err = error;
        });

        RefreshCodeDisplay();
        if (!ok)
        {
            SetStatus(err ?? LocalizeUtility.GetLocalizedString(
                EngMessage: "Could not load recovery code.",
                JpnMessage: "リカバリーコードを取得できませんでした。"));
        }
        else
        {
            SetStatus("");
        }
    }

    void RefreshCodeDisplay()
    {
        var ranked = RankedServices.Instance;
        string code = ranked?.Auth?.RecoveryCodeDisplay;
        if (_codeText == null)
        {
            return;
        }

        if (ranked != null && ranked.Auth != null && ranked.Auth.IsOfflineMode)
        {
            _codeText.text = LocalizeUtility.GetLocalizedString(
                EngMessage: "Recovery code unavailable (offline).",
                JpnMessage: "リカバリーコードはオフラインでは利用できません。");
            UpdateShowHideLabel();
            return;
        }

        if (string.IsNullOrEmpty(code))
        {
            _codeText.text = LocalizeUtility.GetLocalizedString(
                EngMessage: "Recovery code: —",
                JpnMessage: "リカバリーコード: —");
            UpdateShowHideLabel();
            return;
        }

        string shown = _codeVisible ? code : MaskRecoveryCode(code);
        _codeText.text = LocalizeUtility.GetLocalizedString(
            EngMessage: $"Your recovery code: {shown}",
            JpnMessage: $"リカバリーコード: {shown}");
        UpdateShowHideLabel();
    }

    static string MaskRecoveryCode(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return "—";
        }

        var chars = code.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] != '-')
            {
                chars[i] = '*';
            }
        }

        return new string(chars);
    }

    void UpdateShowHideLabel()
    {
        if (_showHideLabel == null)
        {
            return;
        }

        _showHideLabel.text = _codeVisible
            ? LocalizeUtility.GetLocalizedString(EngMessage: "Hide", JpnMessage: "隠す")
            : LocalizeUtility.GetLocalizedString(EngMessage: "Show", JpnMessage: "表示");
    }

    void OnClickShowHide()
    {
        _codeVisible = !_codeVisible;
        RefreshCodeDisplay();
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

        _root = new GameObject("AccountRecoveryPanel", typeof(RectTransform), typeof(Image));
        _root.transform.SetParent(canvas.transform, false);
        var rootRt = _root.GetComponent<RectTransform>();
        rootRt.anchorMin = new Vector2(0.5f, 0.5f);
        rootRt.anchorMax = new Vector2(0.5f, 0.5f);
        rootRt.pivot = new Vector2(0.5f, 0.5f);
        rootRt.sizeDelta = new Vector2(920f, 520f);
        rootRt.anchoredPosition = Vector2.zero;
        _root.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.16f, 0.96f);

        var title = CreateText(_root.transform, "Title", font, 28, TextAnchor.UpperLeft,
            new Vector2(28f, -22f), new Vector2(700f, 40f));
        title.text = LocalizeUtility.GetLocalizedString(
            EngMessage: "Account",
            JpnMessage: "アカウント");
        title.fontStyle = FontStyle.Bold;

        CreateButton(_root.transform, "Close", font, "X", new Vector2(840f, -18f), new Vector2(52f, 48f), Hide);

        _codeText = CreateText(_root.transform, "Code", font, 22, TextAnchor.UpperLeft,
            new Vector2(28f, -80f), new Vector2(480f, 40f));

        var showHideGo = CreateButton(_root.transform, "ShowHide", font,
            LocalizeUtility.GetLocalizedString(EngMessage: "Show", JpnMessage: "表示"),
            new Vector2(520f, -76f), new Vector2(150f, 48f), OnClickShowHide);
        _showHideLabel = showHideGo.GetComponentInChildren<Text>();

        CreateButton(_root.transform, "Copy", font,
            LocalizeUtility.GetLocalizedString(EngMessage: "Copy", JpnMessage: "コピー"),
            new Vector2(690f, -76f), new Vector2(150f, 48f), OnClickCopy);

        var warn = CreateText(_root.transform, "Warn", font, 18, TextAnchor.UpperLeft,
            new Vector2(28f, -140f), new Vector2(860f, 70f));
        warn.text = LocalizeUtility.GetLocalizedString(
            EngMessage: "Write this down before wiping your phone. Keep it private — it is NOT your friend code.",
            JpnMessage: "端末を初期化する前に控えてください。他人に教えないでください（フレンドコードとは別です）。");
        warn.horizontalOverflow = HorizontalWrapMode.Wrap;
        warn.verticalOverflow = VerticalWrapMode.Overflow;

        var recoverLabel = CreateText(_root.transform, "RecoverLabel", font, 22, TextAnchor.UpperLeft,
            new Vector2(28f, -230f), new Vector2(860f, 36f));
        recoverLabel.text = LocalizeUtility.GetLocalizedString(
            EngMessage: "Recover account after reinstall",
            JpnMessage: "再インストール後にアカウントを復元");
        recoverLabel.fontStyle = FontStyle.Bold;

        _recoverField = CreateInputField(_root.transform, "RecoverCode", font,
            new Vector2(28f, -280f), new Vector2(620f, 48f),
            LocalizeUtility.GetLocalizedString(
                EngMessage: "Paste recovery code",
                JpnMessage: "リカバリーコードを入力"));

        CreateButton(_root.transform, "Recover", font,
            LocalizeUtility.GetLocalizedString(EngMessage: "Recover", JpnMessage: "復元"),
            new Vector2(680f, -280f), new Vector2(200f, 48f), OnClickRecover);

        _statusText = CreateText(_root.transform, "Status", font, 18, TextAnchor.UpperLeft,
            new Vector2(28f, -350f), new Vector2(860f, 80f));
        _statusText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _statusText.verticalOverflow = VerticalWrapMode.Overflow;

        _root.SetActive(false);
    }

    void OnClickCopy()
    {
        var ranked = RankedServices.Instance;
        string code = ranked?.Auth?.RecoveryCodeDisplay;
        if (string.IsNullOrEmpty(code))
        {
            SetStatus(LocalizeUtility.GetLocalizedString(
                EngMessage: "No recovery code yet.",
                JpnMessage: "リカバリーコードがまだありません。"));
            return;
        }

        GUIUtility.systemCopyBuffer = code;
        SetStatus(LocalizeUtility.GetLocalizedString(
            EngMessage: "Copied recovery code.",
            JpnMessage: "リカバリーコードをコピーしました。"));
    }

    void OnClickRecover()
    {
        if (_busy)
        {
            return;
        }

        string code = _recoverField != null ? _recoverField.text : null;
        if (string.IsNullOrWhiteSpace(code))
        {
            SetStatus(LocalizeUtility.GetLocalizedString(
                EngMessage: "Enter your recovery code.",
                JpnMessage: "リカバリーコードを入力してください。"));
            return;
        }

        var window = ResolveConfirmWindow();
        if (window == null)
        {
            StartCoroutine(RecoverCoroutine(code));
            return;
        }

        window.CloseOnButtonClicked = true;
        window.SetUpYesNoObject(
            new List<UnityAction>
            {
                () => StartCoroutine(RecoverCoroutine(code)),
            },
            new List<string>
            {
                LocalizeUtility.GetLocalizedString(EngMessage: "Recover", JpnMessage: "復元"),
            },
            LocalizeUtility.GetLocalizedString(
                EngMessage: "This replaces the account currently on this device. Continue?",
                JpnMessage: "この端末の現在のアカウントが置き換わります。続行しますか？"),
            true);
    }

    IEnumerator RecoverCoroutine(string code)
    {
        if (_busy)
        {
            yield break;
        }

        _busy = true;
        SetStatus(LocalizeUtility.GetLocalizedString(
            EngMessage: "Recovering...",
            JpnMessage: "復元中..."));

        var ranked = RankedServices.EnsureExists();
        bool ok = false;
        string err = null;
        yield return ranked.Auth.RecoverWithCode(code, (success, error) =>
        {
            ok = success;
            err = error;
        });

        if (!ok)
        {
            SetStatus(err ?? LocalizeUtility.GetLocalizedString(
                EngMessage: "Recovery failed.",
                JpnMessage: "復元に失敗しました。"));
            _busy = false;
            yield break;
        }

        // Refresh ranked profile + friends under the recovered identity
        yield return ranked.BootstrapForRanked();
        var friends = FriendServices.EnsureExists();
        yield return friends.List.RefreshFromPlayFab();
        friends.Duel.SetInviteListening(true);

        // Re-sync home UI (name + rank)
        if (Opening.instance != null && Opening.instance.home != null &&
            Opening.instance.home.playerInfo != null)
        {
            Opening.instance.home.playerInfo.SetPlayerInfo();
        }

        // Photon UserId mismatch fix (same as HomeMode friends bootstrap)
        OnlinePlayerCountService.EnsureExists().SetMenuPresenceEnabled(false);
        if (Photon.Pun.PhotonNetwork.IsConnected)
        {
            yield return ContinuousController.instance.StartCoroutine(PhotonUtility.DisconnectCoroutine());
        }

        ranked.Auth.ApplyPhotonAuthValues();
        OnlinePlayerCountService.EnsureExists().SetMenuPresenceEnabled(true);

        RefreshCodeDisplay();
        if (_recoverField != null)
        {
            _recoverField.text = "";
        }

        SetStatus(LocalizeUtility.GetLocalizedString(
            EngMessage: "Account recovered. Ranked progress and friends restored.",
            JpnMessage: "アカウントを復元しました。ランクとフレンドが戻ります。"));
        _busy = false;
    }

    YesNoObject ResolveConfirmWindow()
    {
        if (_confirmWindow != null)
        {
            return _confirmWindow;
        }

        if (Opening.instance == null)
        {
            return null;
        }

        var yesNos = Opening.instance.GetComponentsInChildren<YesNoObject>(true);
        if (yesNos != null && yesNos.Length > 0)
        {
            _confirmWindow = yesNos[0];
        }

        return _confirmWindow;
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
        Vector2 anchoredPos, Vector2 sizeDelta, string placeholderText)
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
        ph.text = placeholderText ?? "";
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
