using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
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
    int _lastHeartbeatSent;
    Coroutine _reconnectCoroutine;
    Coroutine _overlayCoroutine;

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
        if (!IsInBattle() || IsHoldingForOpponent)
        {
            return;
        }

        IsHoldingForOpponent = true;
        SyncFreezeAndOverlay();
    }

    public void ReleaseBattleHold()
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

        if (IsHoldingForOpponent && (!IsInBattle() || !ShouldHoldForOpponent()))
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
    }

    bool ShouldHoldForOpponent()
    {
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
        }
        else
        {
            HideOverlay();
            UnfreezeBattle();
        }
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
        var loading = GManager.instance != null ? GManager.instance.LoadingObject : null;
        if (loading == null)
        {
            return;
        }

        if (loading.anim != null)
        {
            loading.anim.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        _overlayOwned = true;
        if (_overlayCoroutine != null)
        {
            StopCoroutine(_overlayCoroutine);
            _overlayCoroutine = null;
        }

        _overlayCoroutine = StartCoroutine(loading.StartLoading(message));
    }

    void HideOverlay()
    {
        if (!_overlayOwned)
        {
            return;
        }

        _overlayOwned = false;
        var loading = GManager.instance != null ? GManager.instance.LoadingObject : null;
        if (loading == null)
        {
            return;
        }

        if (_overlayCoroutine != null)
        {
            StopCoroutine(_overlayCoroutine);
            _overlayCoroutine = null;
        }

        _overlayCoroutine = StartCoroutine(EndOverlay(loading));
    }

    IEnumerator EndOverlay(LoadingObject loading)
    {
        if (loading != null)
        {
            yield return loading.EndLoading();
        }

        _overlayCoroutine = null;
    }
}
