using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// Keeps a PvP battle alive across a phone lock / background: reconnects with
/// ReconnectAndRejoin and holds the remaining player until PlayerTtl expires.
/// Same-process pause only — a killed app still forfeits after the grace period.
///
/// Desync guard: freeze early via pause flag + heartbeat so both clients stop
/// near the same moment instead of waiting for Photon's ~10s timeout.
/// </summary>
public class BattleReconnectService : MonoBehaviourPunCallbacks
{
    public const int PlayerTtlMs = 90000;
    public const int MinEmptyRoomTtlMs = 90000;
    public const float KeepAliveInBackgroundSeconds = 90f;
    /// <summary>
    /// Max time the remaining player waits after the opponent goes silent.
    /// A force-closed app can stay "active" via KeepAlive while heartbeat is stale,
    /// which used to freeze the survivor on "Waiting for opponent" until PlayerTtl.
    /// </summary>
    public const float HoldForfeitSeconds = 20f;

    const string PausePropKey = "BattlePaused";
    const string HeartbeatPropKey = "BattleHb";
    const int HeartbeatIntervalMs = 1000;
    /// <summary>Must be > interval + lag; too low causes false freezes.</summary>
    const int HeartbeatStaleMs = 3500;

    const int ReconnectAttempts = 5;
    const float ReconnectAttemptSeconds = 8f;
    const float ReconnectRetryDelaySeconds = 1f;

    public static BattleReconnectService Instance { get; private set; }

    public bool IsReconnecting { get; private set; }
    public bool IsHoldingForOpponent { get; private set; }
    public bool IntentionalDisconnect { get; private set; }

    bool _allowReconnect;
    bool _skipInitialPause = true;
    bool _timeScaleHeld;
    bool _overlayOwned;
    bool _localPausedFlag;
    float _savedTimeScale = 1f;
    float _holdStartedUnscaled = -1f;
    bool _holdExpired;
    int _lastHeartbeatSent;
    Coroutine _reconnectCoroutine;
    GameObject _waitOverlay;
    Text _waitText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ApplyPhotonBackgroundSettings();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        ApplyPhotonBackgroundSettings();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        ReleaseBattleHold();
    }

    public static BattleReconnectService EnsureExists()
    {
        if (Instance != null)
        {
            return Instance;
        }

        var go = new GameObject("BattleReconnectService");
        return go.AddComponent<BattleReconnectService>();
    }

    public static void ApplyBattleTtl(RoomOptions options)
    {
        if (options == null)
        {
            return;
        }

        options.PlayerTtl = PlayerTtlMs;
        if (options.EmptyRoomTtl < MinEmptyRoomTtlMs)
        {
            options.EmptyRoomTtl = MinEmptyRoomTtlMs;
        }
    }

    public static void ApplyPhotonBackgroundSettings()
    {
        PhotonNetwork.MinimalTimeScaleToDispatchInFixedUpdate = 0f;
        PhotonNetwork.KeepAliveInBackground = KeepAliveInBackgroundSeconds;
    }

    public static int CountActivePlayers()
    {
        if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.Players == null)
        {
            return 0;
        }

        int n = 0;
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (player != null && !player.IsInactive)
            {
                n++;
            }
        }

        return n;
    }

    public static bool HasInactiveOpponent()
    {
        if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.Players == null)
        {
            return false;
        }

        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (player != null && !player.IsLocal && player.IsInactive)
            {
                return true;
            }
        }

        return false;
    }

    public void MarkIntentionalDisconnect()
    {
        IntentionalDisconnect = true;
        _allowReconnect = false;
        StopReconnectCoroutine();
        IsReconnecting = false;
        ClearLocalPauseFlag();
        ReleaseBattleHold();
    }

    public void ClearIntentionalDisconnect()
    {
        IntentionalDisconnect = false;
    }

    public void NotifyLeftRoomIntentionally()
    {
        _allowReconnect = false;
        StopReconnectCoroutine();
        IsReconnecting = false;
        ClearLocalPauseFlag();
        ReleaseBattleHold();
    }

    public void EnsureHoldForOpponent()
    {
        if (!IsInBattle() || _holdExpired)
        {
            return;
        }

        if (!IsHoldingForOpponent)
        {
            _holdStartedUnscaled = Time.unscaledTime;
            IsHoldingForOpponent = true;
        }

        SyncFreezeAndOverlay();
    }

    public bool HasHoldExpired()
    {
        if (_holdExpired)
        {
            return true;
        }

        if (!IsHoldingForOpponent || _holdStartedUnscaled < 0f)
        {
            return false;
        }

        if (Time.unscaledTime - _holdStartedUnscaled < HoldForfeitSeconds)
        {
            return false;
        }

        _holdExpired = true;
        Debug.LogWarning("[BattleReconnect] Opponent hold timed out — remaining player wins");
        return true;
    }

    public void ReleaseBattleHold()
    {
        IsHoldingForOpponent = false;
        _holdStartedUnscaled = -1f;
        if (!IsReconnecting)
        {
            HideOverlay();
            UnfreezeBattle();
        }
        else
        {
            SyncFreezeAndOverlay();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        bool skip = IntentionalDisconnect || ConnectionHandler.AppQuits;
        IntentionalDisconnect = false;

        Debug.LogWarning($"[BattleReconnect] OnDisconnected cause={cause} skip={skip} allow={_allowReconnect}");

        if (skip)
        {
            return;
        }

        if (!_allowReconnect || !ShouldAttemptReconnect(cause))
        {
            return;
        }

        StartReconnect();
    }

    public override void OnJoinedRoom()
    {
        _allowReconnect = true;
        ApplyPhotonBackgroundSettings();

        if (IsReconnecting)
        {
            Debug.Log($"[BattleReconnect] Rejoined room={PhotonNetwork.CurrentRoom?.Name}");
            IsReconnecting = false;
            ClearLocalPauseFlag();
            PublishHeartbeat(force: true);
        }

        if (IsHoldingForOpponent && !ShouldHoldForOpponent())
        {
            IsHoldingForOpponent = false;
        }

        SyncFreezeAndOverlay();
    }

    public override void OnLeftRoom()
    {
        if (IntentionalDisconnect || !_allowReconnect)
        {
            IsHoldingForOpponent = false;
            IsReconnecting = false;
            HideOverlay();
            UnfreezeBattle();
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (!IsInBattle())
        {
            return;
        }

        if (otherPlayer != null && otherPlayer.IsInactive)
        {
            EnsureHoldForOpponent();
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (!IsHoldingForOpponent)
        {
            return;
        }

        if (!ShouldHoldForOpponent())
        {
            IsHoldingForOpponent = false;
            SyncFreezeAndOverlay();
        }
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
    {
        if (!IsInBattle() || targetPlayer == null || targetPlayer.IsLocal)
        {
            return;
        }

        if (changedProps != null &&
            (changedProps.ContainsKey(PausePropKey) || changedProps.ContainsKey(HeartbeatPropKey)))
        {
            RefreshOpponentHold();
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[BattleReconnect] OnJoinRoomFailed [{returnCode}] {message}");
    }

    void OnApplicationPause(bool pause)
    {
        if (_skipInitialPause)
        {
            _skipInitialPause = false;
            if (pause)
            {
                return;
            }
        }

        HandleAppPauseOrFocus(pause);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (_skipInitialPause)
        {
            return;
        }

        HandleAppPauseOrFocus(!hasFocus);
    }

    void HandleAppPauseOrFocus(bool paused)
    {
        if (!IsInBattle() || IntentionalDisconnect)
        {
            return;
        }

        if (paused)
        {
            // Tell the opponent to freeze immediately — before Photon times out (~10s).
            SetLocalPauseFlag(true);
            return;
        }

        SetLocalPauseFlag(false);
        PublishHeartbeat(force: true);
        TryStartReconnectAfterResume();
    }

    void Update()
    {
        if (IsInBattle() && PhotonNetwork.InRoom && !IntentionalDisconnect)
        {
            MaybePublishHeartbeat();
            RefreshOpponentHold();
        }

        if (IsHoldingForOpponent && (!IsInBattle() || !ShouldHoldForOpponent() || HasHoldExpired()))
        {
            IsHoldingForOpponent = false;
            if (!IsReconnecting)
            {
                HideOverlay();
                UnfreezeBattle();
            }
            else
            {
                SyncFreezeAndOverlay();
            }

            return;
        }

        UpdateHoldCountdown();

        if (!IsHoldingForOpponent && !IsReconnecting)
        {
            return;
        }

        if (!IsInBattle() && !IsReconnecting)
        {
            HideOverlay();
            UnfreezeBattle();
        }
    }

    void RefreshOpponentHold()
    {
        if (!IsInBattle() || IsReconnecting)
        {
            return;
        }

        if (ShouldHoldForOpponent())
        {
            EnsureHoldForOpponent();
        }
        else if (IsHoldingForOpponent)
        {
            IsHoldingForOpponent = false;
            SyncFreezeAndOverlay();
        }
        else if (_holdExpired && OpponentLooksConnected())
        {
            _holdExpired = false;
        }
    }

    bool ShouldHoldForOpponent()
    {
        if (_holdExpired)
        {
            return false;
        }

        if (HasInactiveOpponent())
        {
            return true;
        }

        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom?.Players == null)
        {
            return false;
        }

        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (player == null || player.IsLocal || player.IsInactive)
            {
                continue;
            }

            if (IsPlayerMarkedPaused(player) || IsHeartbeatStale(player))
            {
                return true;
            }
        }

        return false;
    }

    bool OpponentLooksConnected()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom?.Players == null)
        {
            return false;
        }

        if (HasInactiveOpponent() || CountActivePlayers() < 2)
        {
            return false;
        }

        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (player == null || player.IsLocal)
            {
                continue;
            }

            if (IsPlayerMarkedPaused(player) || IsHeartbeatStale(player))
            {
                return false;
            }
        }

        return true;
    }

    static bool IsPlayerMarkedPaused(Photon.Realtime.Player player)
    {
        if (player?.CustomProperties == null)
        {
            return false;
        }

        return player.CustomProperties.TryGetValue(PausePropKey, out object paused) &&
               paused is bool b &&
               b;
    }

    static bool IsHeartbeatStale(Photon.Realtime.Player player)
    {
        if (player?.CustomProperties == null)
        {
            return false;
        }

        if (!player.CustomProperties.TryGetValue(HeartbeatPropKey, out object hbObj))
        {
            return false;
        }

        int hb;
        try
        {
            hb = System.Convert.ToInt32(hbObj);
        }
        catch
        {
            return false;
        }

        int age = PhotonNetwork.ServerTimestamp - hb;
        return age > HeartbeatStaleMs;
    }

    void MaybePublishHeartbeat()
    {
        if (!PhotonNetwork.InRoom || !PhotonNetwork.IsConnectedAndReady)
        {
            return;
        }

        int now = PhotonNetwork.ServerTimestamp;
        if (_lastHeartbeatSent != 0 &&
            unchecked(now - _lastHeartbeatSent) < HeartbeatIntervalMs)
        {
            return;
        }

        PublishHeartbeat(force: false);
    }

    void PublishHeartbeat(bool force)
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null)
        {
            return;
        }

        int now = PhotonNetwork.ServerTimestamp;
        if (!force &&
            _lastHeartbeatSent != 0 &&
            unchecked(now - _lastHeartbeatSent) < HeartbeatIntervalMs)
        {
            return;
        }

        _lastHeartbeatSent = now;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
        {
            { HeartbeatPropKey, now },
        });
    }

    void SetLocalPauseFlag(bool paused)
    {
        _localPausedFlag = paused;
        if (!PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null)
        {
            return;
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
        {
            { PausePropKey, paused },
            { HeartbeatPropKey, PhotonNetwork.ServerTimestamp },
        });
        _lastHeartbeatSent = PhotonNetwork.ServerTimestamp;
    }

    void ClearLocalPauseFlag()
    {
        if (!_localPausedFlag &&
            (PhotonNetwork.LocalPlayer == null ||
             !IsPlayerMarkedPaused(PhotonNetwork.LocalPlayer)))
        {
            _localPausedFlag = false;
            return;
        }

        _localPausedFlag = false;
        if (!PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null)
        {
            return;
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
        {
            { PausePropKey, false },
            { HeartbeatPropKey, PhotonNetwork.ServerTimestamp },
        });
        _lastHeartbeatSent = PhotonNetwork.ServerTimestamp;
    }

    void TryStartReconnectAfterResume()
    {
        if (IntentionalDisconnect || ConnectionHandler.AppQuits || !_allowReconnect)
        {
            return;
        }

        if (PhotonNetwork.InRoom && PhotonNetwork.IsConnected)
        {
            return;
        }

        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            StartReconnect();
        }
    }

    void StartReconnect()
    {
        if (IntentionalDisconnect || ConnectionHandler.AppQuits || !_allowReconnect)
        {
            return;
        }

        if (_reconnectCoroutine != null)
        {
            return;
        }

        _reconnectCoroutine = StartCoroutine(ReconnectCoroutine());
    }

    void StopReconnectCoroutine()
    {
        if (_reconnectCoroutine != null)
        {
            StopCoroutine(_reconnectCoroutine);
            _reconnectCoroutine = null;
        }
    }

    IEnumerator ReconnectCoroutine()
    {
        IsReconnecting = true;
        SyncFreezeAndOverlay();

        try
        {
            for (int attempt = 0; attempt < ReconnectAttempts; attempt++)
            {
                if (IntentionalDisconnect || ConnectionHandler.AppQuits || !_allowReconnect)
                {
                    yield break;
                }

                if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
                {
                    yield break;
                }

                ApplyRankedAuthIfNeeded();
                ApplyPhotonBackgroundSettings();

                if (PhotonNetwork.IsConnected && !PhotonNetwork.InRoom)
                {
                    PhotonNetwork.Disconnect();
                    float waitDisconnect = 0f;
                    while (PhotonNetwork.IsConnected && waitDisconnect < 5f && !IntentionalDisconnect)
                    {
                        waitDisconnect += Time.unscaledDeltaTime;
                        yield return null;
                    }
                }

                if (IntentionalDisconnect || ConnectionHandler.AppQuits)
                {
                    yield break;
                }

                bool started = PhotonNetwork.ReconnectAndRejoin();
                Debug.Log($"[BattleReconnect] ReconnectAndRejoin attempt={attempt + 1} started={started}");

                if (started)
                {
                    float elapsed = 0f;
                    while (elapsed < ReconnectAttemptSeconds && !PhotonNetwork.InRoom && !IntentionalDisconnect)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        yield return null;
                    }

                    if (PhotonNetwork.InRoom)
                    {
                        yield break;
                    }
                }

                yield return new WaitForSecondsRealtime(ReconnectRetryDelaySeconds);
            }
        }
        finally
        {
            IsReconnecting = false;
            _reconnectCoroutine = null;
            SyncFreezeAndOverlay();
        }
    }

    static void ApplyRankedAuthIfNeeded()
    {
        if (ContinuousController.instance != null &&
            ContinuousController.instance.isRanked &&
            RankedServices.Instance != null)
        {
            RankedServices.Instance.Auth.ApplyPhotonAuthValues();
        }
    }

    static bool ShouldAttemptReconnect(DisconnectCause cause)
    {
        switch (cause)
        {
            case DisconnectCause.None:
            case DisconnectCause.ClientTimeout:
            case DisconnectCause.ServerTimeout:
            case DisconnectCause.DisconnectByServerReasonUnknown:
            case DisconnectCause.Exception:
                return true;
            default:
                return false;
        }
    }

    static bool IsInBattle()
    {
        return GManager.instance != null &&
               !GManager.instance.IsAI &&
               GManager.instance.turnStateMachine != null &&
               !GManager.instance.turnStateMachine.endGame;
    }

    void SyncFreezeAndOverlay()
    {
        bool hold = IsInBattle() && (IsReconnecting || IsHoldingForOpponent);
        if (hold)
        {
            FreezeBattle();
            string message = IsReconnecting
                ? LocalizeUtility.GetLocalizedString(EngMessage: "Reconnecting", JpnMessage: "再接続中")
                : LocalizeUtility.GetLocalizedString(EngMessage: "Waiting for opponent", JpnMessage: "相手を待っています");
            ShowOverlay(message);
            UpdateHoldCountdown();
        }
        else
        {
            HideOverlay();
            UnfreezeBattle();
        }
    }

    void UpdateHoldCountdown()
    {
        if (_waitText == null)
        {
            return;
        }

        if (IsReconnecting)
        {
            _waitText.text = LocalizeUtility.GetLocalizedString(
                EngMessage: "Reconnecting...",
                JpnMessage: "再接続中…");
            return;
        }

        if (!IsHoldingForOpponent || _holdStartedUnscaled < 0f)
        {
            return;
        }

        int remain = Mathf.Max(0, Mathf.CeilToInt(HoldForfeitSeconds - (Time.unscaledTime - _holdStartedUnscaled)));
        _waitText.text = LocalizeUtility.GetLocalizedString(
            EngMessage: $"Opponent disconnected\nWaiting for them to return ({remain}s)",
            JpnMessage: $"相手が切断しました\n復帰を待っています（{remain}秒）");
    }

    void FreezeBattle()
    {
        if (_timeScaleHeld)
        {
            return;
        }

        _savedTimeScale = Time.timeScale;
        if (_savedTimeScale <= 0f)
        {
            _savedTimeScale = 1f;
        }

        Time.timeScale = 0f;
        _timeScaleHeld = true;
    }

    void UnfreezeBattle()
    {
        if (!_timeScaleHeld)
        {
            return;
        }

        Time.timeScale = _savedTimeScale;
        _timeScaleHeld = false;
    }

    void ShowOverlay(string message)
    {
        _overlayOwned = true;
        EnsureWaitOverlay();
        if (_waitText != null && !string.IsNullOrEmpty(message))
        {
            _waitText.text = message;
        }

        UpdateHoldCountdown();
    }

    void HideOverlay()
    {
        _overlayOwned = false;
        if (_waitOverlay != null)
        {
            Destroy(_waitOverlay);
            _waitOverlay = null;
            _waitText = null;
        }
    }

    void EnsureWaitOverlay()
    {
        if (_waitOverlay != null)
        {
            return;
        }

        if (EventSystem.current == null)
        {
            var es = new GameObject("BattleReconnectEventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            DontDestroyOnLoad(es);
        }

        Font font = ResolveOverlayFont();
        _waitOverlay = new GameObject("BattleReconnectOverlay");
        DontDestroyOnLoad(_waitOverlay);

        var rootRt = _waitOverlay.AddComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.offsetMin = Vector2.zero;
        rootRt.offsetMax = Vector2.zero;

        var canvas = _waitOverlay.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32750;
        canvas.overrideSorting = true;
        _waitOverlay.AddComponent<GraphicRaycaster>();

        var dim = _waitOverlay.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.72f);
        dim.raycastTarget = true;

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(_waitOverlay.transform, false);
        var panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(720f, 240f);
        panel.GetComponent<Image>().color = new Color(0.1f, 0.12f, 0.2f, 0.98f);

        var infoGo = new GameObject("Info", typeof(RectTransform));
        infoGo.transform.SetParent(panel.transform, false);
        _waitText = infoGo.AddComponent<Text>();
        _waitText.font = font;
        _waitText.fontSize = 30;
        _waitText.alignment = TextAnchor.MiddleCenter;
        _waitText.color = Color.white;
        _waitText.raycastTarget = false;
        _waitText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _waitText.verticalOverflow = VerticalWrapMode.Overflow;
        var infoRt = _waitText.GetComponent<RectTransform>();
        infoRt.anchorMin = new Vector2(0.06f, 0.08f);
        infoRt.anchorMax = new Vector2(0.94f, 0.92f);
        infoRt.offsetMin = Vector2.zero;
        infoRt.offsetMax = Vector2.zero;
    }

    static Font ResolveOverlayFont()
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

        return Font.CreateDynamicFontFromOSFont("Arial", 28);
    }
}
