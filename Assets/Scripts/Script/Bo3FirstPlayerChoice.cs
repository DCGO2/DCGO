using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// After a Bo3 game, the loser picks who goes first in the next game.
/// Choice is stored on the Photon room so both clients agree before rematch.
/// </summary>
public static class Bo3FirstPlayerChoice
{
    public const float TimeoutSeconds = 25f;

    static GameObject _overlay;
    static Text _infoText;
    static GameObject _buttonsRoot;
    static string _userIdKey;
    static string _gameIndexKey;
    static int _expectedGameIndex;
    static string _localUserId;
    static string _loserUserId;
    static bool _keysArmed;
    static string _pendingFirstUserId;
    static int _pendingGameIndex = -1;

    public static string ReadChosenFirstUserId(string userIdKey, string gameIndexKey, int expectedGameIndex)
    {
        if (_pendingGameIndex == expectedGameIndex && !string.IsNullOrEmpty(_pendingFirstUserId))
        {
            return _pendingFirstUserId;
        }

        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null ||
            PhotonNetwork.CurrentRoom.CustomProperties == null)
        {
            return null;
        }

        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        if (!hash.TryGetValue(gameIndexKey, out object gObj))
        {
            return null;
        }

        int gameIndex;
        try
        {
            gameIndex = System.Convert.ToInt32(gObj);
        }
        catch
        {
            return null;
        }

        if (gameIndex != expectedGameIndex)
        {
            return null;
        }

        if (hash.TryGetValue(userIdKey, out object idObj) && idObj != null)
        {
            string id = idObj as string ?? idObj.ToString();
            if (!string.IsNullOrEmpty(id))
            {
                return id;
            }
        }

        return null;
    }

    public static void WriteChosenFirstUserId(string userIdKey, string gameIndexKey, int gameIndex, string firstUserId)
    {
        if (!PhotonNetwork.InRoom || string.IsNullOrEmpty(firstUserId))
        {
            return;
        }

        _pendingFirstUserId = firstUserId;
        _pendingGameIndex = gameIndex;

        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
        {
            { userIdKey, firstUserId },
            { gameIndexKey, gameIndex },
        });
    }

    public static int ActorNumberForUserId(string userId)
    {
        if (string.IsNullOrEmpty(userId) || PhotonNetwork.PlayerList == null)
        {
            return -1;
        }

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            var player = PhotonNetwork.PlayerList[i];
            if (player == null)
            {
                continue;
            }

            string tournamentId = TournamentState.ReadPlayerId(player);
            if (!string.IsNullOrEmpty(tournamentId) &&
                string.Equals(tournamentId, userId, System.StringComparison.OrdinalIgnoreCase))
            {
                return player.ActorNumber;
            }

            string friendId = FriendDuelDirector.ReadPlayerId(player);
            if (!string.IsNullOrEmpty(friendId) &&
                string.Equals(friendId, userId, System.StringComparison.OrdinalIgnoreCase))
            {
                return player.ActorNumber;
            }
        }

        return -1;
    }

    public static IEnumerator WaitForChoice(
        string userIdKey,
        string gameIndexKey,
        int expectedGameIndex,
        string localUserId,
        string loserUserId)
    {
        if (string.IsNullOrEmpty(loserUserId) ||
            string.IsNullOrEmpty(localUserId) ||
            expectedGameIndex < 1)
        {
            yield break;
        }

        _userIdKey = userIdKey;
        _gameIndexKey = gameIndexKey;
        _expectedGameIndex = expectedGameIndex;
        _localUserId = localUserId;
        _loserUserId = loserUserId;
        _keysArmed = true;
        if (_pendingGameIndex != expectedGameIndex)
        {
            _pendingFirstUserId = null;
            _pendingGameIndex = -1;
        }

        string existing = ReadChosenFirstUserId(userIdKey, gameIndexKey, expectedGameIndex);
        if (!string.IsNullOrEmpty(existing))
        {
            _keysArmed = false;
            yield break;
        }

        bool localIsLoser = string.Equals(localUserId, loserUserId, System.StringComparison.OrdinalIgnoreCase);
        ShowOverlay(localIsLoser);

        float waited = 0f;
        while (waited < TimeoutSeconds)
        {
            if (!PhotonNetwork.InRoom)
            {
                Hide();
                _keysArmed = false;
                yield break;
            }

            string chosen = ReadChosenFirstUserId(userIdKey, gameIndexKey, expectedGameIndex);
            if (!string.IsNullOrEmpty(chosen))
            {
                Hide();
                _keysArmed = false;
                yield break;
            }

            UpdateCountdown(localIsLoser, TimeoutSeconds - waited);
            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        if (string.IsNullOrEmpty(ReadChosenFirstUserId(userIdKey, gameIndexKey, expectedGameIndex)))
        {
            WriteChosenFirstUserId(userIdKey, gameIndexKey, expectedGameIndex, loserUserId);
            Debug.Log($"[Bo3] First-player choice timed out — default loser first {loserUserId}");
        }

        Hide();
        _keysArmed = false;
    }

    public static void Hide()
    {
        _keysArmed = false;
        DestroyOverlay();
    }

    static void DestroyOverlay()
    {
        if (_overlay != null)
        {
            Object.Destroy(_overlay);
            _overlay = null;
            _infoText = null;
            _buttonsRoot = null;
        }
    }

    static void ShowOverlay(bool localIsLoser)
    {
        DestroyOverlay();

        EnsureEventSystem();

        Font font = ResolveFont();
        if (font == null)
        {
            Debug.LogWarning("[Bo3] No UI font — cannot show first-player choice");
            return;
        }

        _overlay = new GameObject("Bo3FirstPlayerChoiceOverlay");
        Object.DontDestroyOnLoad(_overlay);

        var rootRt = _overlay.AddComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.offsetMin = Vector2.zero;
        rootRt.offsetMax = Vector2.zero;

        var canvas = _overlay.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32760;
        canvas.overrideSorting = true;
        _overlay.AddComponent<GraphicRaycaster>();

        var dim = _overlay.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.65f);
        dim.raycastTarget = true;

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(_overlay.transform, false);
        var panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(740f, localIsLoser ? 340f : 220f);
        panel.GetComponent<Image>().color = new Color(0.1f, 0.12f, 0.2f, 0.98f);

        var infoGo = new GameObject("Info", typeof(RectTransform));
        infoGo.transform.SetParent(panel.transform, false);
        _infoText = infoGo.AddComponent<Text>();
        _infoText.font = font;
        _infoText.fontSize = 26;
        _infoText.alignment = TextAnchor.MiddleCenter;
        _infoText.color = Color.white;
        _infoText.raycastTarget = false;
        _infoText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _infoText.verticalOverflow = VerticalWrapMode.Overflow;
        var infoRt = _infoText.GetComponent<RectTransform>();
        infoRt.anchorMin = new Vector2(0.06f, localIsLoser ? 0.42f : 0.12f);
        infoRt.anchorMax = new Vector2(0.94f, 0.92f);
        infoRt.offsetMin = Vector2.zero;
        infoRt.offsetMax = Vector2.zero;

        if (localIsLoser)
        {
            _buttonsRoot = new GameObject("Buttons", typeof(RectTransform));
            _buttonsRoot.transform.SetParent(panel.transform, false);
            var buttonsRt = _buttonsRoot.GetComponent<RectTransform>();
            buttonsRt.anchorMin = Vector2.zero;
            buttonsRt.anchorMax = Vector2.one;
            buttonsRt.offsetMin = Vector2.zero;
            buttonsRt.offsetMax = Vector2.zero;
            CreateButton(_buttonsRoot.transform, font,
                LocalizeUtility.GetLocalizedString(EngMessage: "I go first", JpnMessage: "自分が先攻"),
                new Vector2(-150f, -90f),
                () => OnLocalChoice(localGoesFirst: true));
            CreateButton(_buttonsRoot.transform, font,
                LocalizeUtility.GetLocalizedString(EngMessage: "Opponent goes first", JpnMessage: "相手が先攻"),
                new Vector2(150f, -90f),
                () => OnLocalChoice(localGoesFirst: false));
        }

        UpdateCountdown(localIsLoser, TimeoutSeconds);
        Opening.instance?.PlayDecisionSE();
    }

    static void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        var go = new GameObject("Bo3ChoiceEventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
        Object.DontDestroyOnLoad(go);
    }

    static void OnLocalChoice(bool localGoesFirst)
    {
        if (!_keysArmed || string.IsNullOrEmpty(_localUserId))
        {
            Debug.LogWarning($"[Bo3] First-player click ignored armed={_keysArmed} local={_localUserId}");
            return;
        }

        string firstId = localGoesFirst ? _localUserId : OpponentIdFromRoom(_localUserId);
        if (string.IsNullOrEmpty(firstId))
        {
            firstId = localGoesFirst ? _localUserId : _loserUserId;
        }

        WriteChosenFirstUserId(_userIdKey, _gameIndexKey, _expectedGameIndex, firstId);
        if (_buttonsRoot != null)
        {
            _buttonsRoot.SetActive(false);
        }

        if (_infoText != null)
        {
            _infoText.text = LocalizeUtility.GetLocalizedString(
                EngMessage: localGoesFirst ? "You go first next game." : "Opponent goes first next game.",
                JpnMessage: localGoesFirst ? "次のゲームはあなたが先攻です。" : "次のゲームは相手が先攻です。");
        }

        Debug.Log($"[Bo3] Loser chose first={firstId} localGoesFirst={localGoesFirst}");
    }

    static string OpponentIdFromRoom(string localUserId)
    {
        if (PhotonNetwork.PlayerList == null)
        {
            return null;
        }

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            var player = PhotonNetwork.PlayerList[i];
            if (player == null || player.IsLocal)
            {
                continue;
            }

            string id = TournamentState.ReadPlayerId(player);
            if (string.IsNullOrEmpty(id))
            {
                id = FriendDuelDirector.ReadPlayerId(player);
            }

            if (!string.IsNullOrEmpty(id) &&
                !string.Equals(id, localUserId, System.StringComparison.OrdinalIgnoreCase))
            {
                return id;
            }
        }

        return null;
    }

    static void UpdateCountdown(bool localIsLoser, float remaining)
    {
        if (_infoText == null)
        {
            return;
        }

        int seconds = Mathf.Max(0, Mathf.CeilToInt(remaining));
        if (localIsLoser)
        {
            _infoText.text = LocalizeUtility.GetLocalizedString(
                EngMessage: $"You lost this game.\nWho goes first next?\n({seconds}s — default: you)",
                JpnMessage: $"このゲームに負けました。\n次のゲームの先攻を選んでください。\n（{seconds}秒・未選択時はあなたが先攻）");
        }
        else
        {
            _infoText.text = LocalizeUtility.GetLocalizedString(
                EngMessage: $"Waiting for the loser to choose who goes first...\n({seconds}s)",
                JpnMessage: $"敗者が次のゲームの先攻を選んでいます…\n（{seconds}秒）");
        }
    }

    static void CreateButton(Transform parent, Font font, string label, Vector2 pos, UnityAction onClick)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(260f, 56f);
        go.GetComponent<Image>().color = new Color(0.25f, 0.45f, 0.85f, 1f);
        go.GetComponent<Button>().onClick.AddListener(onClick);

        var tGo = new GameObject("Label", typeof(RectTransform));
        tGo.transform.SetParent(go.transform, false);
        var text = tGo.AddComponent<Text>();
        text.font = font;
        text.fontSize = 22;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = label;
        text.raycastTarget = false;
        var trt = text.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
    }

    static Font ResolveFont()
    {
        if (Opening.instance != null && Opening.instance.VerText != null && Opening.instance.VerText.font != null)
        {
            return Opening.instance.VerText.font;
        }

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font != null)
        {
            return font;
        }

        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font != null)
        {
            return font;
        }

        return Font.CreateDynamicFontFromOSFont("Arial", 24);
    }
}
