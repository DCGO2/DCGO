using System.Collections;
using System.Linq;
using System.Text;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Handles dropped connections during an online (PvP) match.
//
// Phase 1 = same-session reconnect: the Unity client keeps running and its game state stays intact,
// we only have to get the Photon connection back and make sure both clients still agree on the state.
//
//  * The client that lost its connection retries PhotonNetwork.ReconnectAndRejoin() for up to
//    ReconnectWindowSeconds. If it does not get back in time it loses the match.
//  * The other client pauses behind an overlay and waits for the player to return. If the player does
//    not return within ReconnectWindowSeconds (or Photon removes them from the room) it wins the match.
//  * After a rejoin both clients exchange a checksum of the visible game state. If they disagree
//    (an RPC was lost during the outage) or the check cannot be completed, the match ends as a
//    no-contest (EndGame with no winner) instead of continuing out of sync.
//
// Requires RoomOptions.PlayerTtl = PlayerTtlMilliseconds on every room that hosts a match, otherwise
// Photon removes a disconnected player immediately and nothing can be recovered.
//
// This component lives on the GManager GameObject (same PhotonView as TurnStateMachine).
public class ReconnectManager : MonoBehaviourPunCallbacks
{
    // How long a disconnected player has to come back. Keep in sync with the room's PlayerTtl.
    public const float ReconnectWindowSeconds = 60f;
    public const int PlayerTtlMilliseconds = 60000;

    const float AttemptTimeoutSeconds = 6f;
    const float AttemptRetryDelaySeconds = 1f;
    const float ResyncTimeoutSeconds = 8f;
    const float ResyncSendIntervalSeconds = 1f;

    enum State
    {
        Idle,
        LocalReconnecting,   // this client lost its connection and is trying to get back
        WaitingForOpponent,  // the opponent is inactive; this client is paused waiting for them
    }

    public static ReconnectManager Instance { get; private set; }

    // True while a drop is being handled. GManager.CheckDisconnect must not end the match in the meantime.
    public static bool IsHandlingDisconnect => Instance != null && Instance._state != State.Idle;

    State _state = State.Idle;
    Coroutine _routine;

    string _roomName;
    bool _joinedRoom;
    bool _joinRoomFailed;
    bool _rejoinByNameWhenOnMaster;

    bool _hasRemoteHash;
    int _remoteHash;
    string _remoteSummary = "";
    bool _resyncOk;

    GameObject _overlayRoot;
    Text _overlayText;
    Button _leaveButton;

    #region Unity
    void Awake()
    {
        Instance = this;

        // This component is added at runtime next to TurnStateMachine; make sure Photon sees our RPC.
        if (photonView != null)
        {
            photonView.RefreshRpcMonoBehaviourCache();
        }
    }

    void Update()
    {
        // Remember the room so we can rejoin by name if ReconnectAndRejoin() is not possible.
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null)
        {
            _roomName = PhotonNetwork.CurrentRoom.Name;
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        HideOverlay();
    }

    void OnDestroy()
    {
        HideOverlay();

        if (Instance == this)
        {
            Instance = null;
        }
    }
    #endregion

    #region Helpers
    static bool MatchInProgress
    {
        get
        {
            GManager gm = GManager.instance;

            return gm != null
                && !gm.IsAI
                && gm.turnStateMachine != null
                && gm.turnStateMachine.gameContext != null
                && !gm.turnStateMachine.endGame;
        }
    }

    static bool OpponentPresent()
    {
        return PhotonNetwork.PlayerListOthers.Length > 0;
    }

    static bool OpponentActive()
    {
        return PhotonNetwork.PlayerListOthers.Any(p => !p.IsInactive);
    }

    // Only drops that look like a lost connection are worth retrying. Intentional disconnects,
    // kicks, authentication problems and so on keep the old behaviour (match ends).
    static bool IsRecoverable(Photon.Realtime.DisconnectCause cause)
    {
        switch (cause)
        {
            case Photon.Realtime.DisconnectCause.ClientTimeout:
            case Photon.Realtime.DisconnectCause.ServerTimeout:
            case Photon.Realtime.DisconnectCause.Exception:
            case Photon.Realtime.DisconnectCause.ExceptionOnConnect:
            case Photon.Realtime.DisconnectCause.DnsExceptionOnConnect:
            case Photon.Realtime.DisconnectCause.DisconnectByServerReasonUnknown:
                return true;

            default:
                return false;
        }
    }

    void StopRoutine()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }
    }

    void ResetFlags()
    {
        _joinedRoom = false;
        _joinRoomFailed = false;
        _rejoinByNameWhenOnMaster = false;
        _hasRemoteHash = false;
        _remoteHash = 0;
        _remoteSummary = "";
        _resyncOk = false;
    }

    static string Localized(string eng, string jpn)
    {
        return LocalizeUtility.GetLocalizedString(EngMessage: eng, JpnMessage: jpn);
    }
    #endregion

    #region Photon callbacks
    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        if (!MatchInProgress)
        {
            return;
        }

        if (_state == State.LocalReconnecting)
        {
            // A failed reconnect attempt also ends up here. The reconnect loop handles it.
            return;
        }

        if (!IsRecoverable(cause))
        {
            Debug.Log($"[Reconnect] Disconnected ({cause}); not recoverable, match ends as before.");
            return;
        }

        Debug.LogWarning($"[Reconnect] Connection lost ({cause}). Trying to rejoin for {ReconnectWindowSeconds}s.");

        StopRoutine();
        ResetFlags();
        _state = State.LocalReconnecting;
        _routine = StartCoroutine(LocalReconnectRoutine());
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (!MatchInProgress)
        {
            return;
        }

        if (_state == State.Idle && otherPlayer.IsInactive)
        {
            // The opponent lost their connection (Photon keeps their slot for PlayerTtl).
            Debug.LogWarning("[Reconnect] Opponent became inactive. Pausing and waiting.");

            ResetFlags();
            _state = State.WaitingForOpponent;
            _routine = StartCoroutine(WaitForOpponentRoutine());
        }

        // If the opponent was removed for good (left on purpose, or their PlayerTtl ran out) while we are
        // waiting, WaitForOpponentRoutine notices it through OpponentPresent(). If we are idle,
        // GManager.CheckDisconnect ends the match as before.
    }

    public override void OnJoinedRoom()
    {
        if (_state == State.LocalReconnecting)
        {
            _joinedRoom = true;
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (_state == State.LocalReconnecting)
        {
            // Most likely the room no longer exists. Retrying will not help.
            Debug.LogWarning($"[Reconnect] Rejoin failed: [{returnCode}] {message}");
            _joinRoomFailed = true;
        }
    }

    public override void OnConnectedToMaster()
    {
        // Fallback path: ReconnectToMaster() succeeded, now rejoin the room by name.
        if (_state == State.LocalReconnecting && _rejoinByNameWhenOnMaster)
        {
            _rejoinByNameWhenOnMaster = false;

            if (!string.IsNullOrEmpty(_roomName))
            {
                PhotonNetwork.RejoinRoom(_roomName);
            }
        }
    }
    #endregion

    #region Local client lost its connection
    IEnumerator LocalReconnectRoutine()
    {
        ShowOverlay();

        float deadline = Time.unscaledTime + ReconnectWindowSeconds;
        bool rejoined = false;

        while (Time.unscaledTime < deadline && MatchInProgress && !_joinRoomFailed)
        {
            SetCountdownText(
                Localized("Connection lost. Reconnecting...", "接続が切れました。再接続中..."),
                deadline);

            _joinedRoom = false;

            if (TryStartReconnect())
            {
                float attemptEnd = Mathf.Min(deadline, Time.unscaledTime + AttemptTimeoutSeconds);

                while (Time.unscaledTime < attemptEnd && !_joinedRoom && !_joinRoomFailed)
                {
                    SetCountdownText(
                        Localized("Connection lost. Reconnecting...", "接続が切れました。再接続中..."),
                        deadline);
                    yield return null;
                }

                if (_joinedRoom)
                {
                    rejoined = true;
                    break;
                }

                // Attempt timed out. Make sure the client is fully disconnected before trying again.
                if (PhotonNetwork.IsConnected)
                {
                    PhotonNetwork.Disconnect();
                }
            }

            float waitEnd = Time.unscaledTime + AttemptRetryDelaySeconds;

            while (Time.unscaledTime < waitEnd)
            {
                yield return null;
            }
        }

        if (!MatchInProgress)
        {
            // Match already ended by someone else.
            FinishWithoutResult();
            yield break;
        }

        if (!rejoined)
        {
            Debug.LogWarning("[Reconnect] Could not rejoin in time. Match is lost.");
            AbandonConnection();
            EndMatch(GManager.instance.Opponent);
            yield break;
        }

        Debug.Log("[Reconnect] Rejoined the room.");

        if (!OpponentPresent())
        {
            // Opponent is gone for good (they left or their slot expired).
            EndMatch(null);
            yield break;
        }

        if (!OpponentActive())
        {
            // The opponent dropped as well. Switch to waiting for them.
            _state = State.WaitingForOpponent;
            _routine = StartCoroutine(WaitForOpponentRoutine());
            yield break;
        }

        yield return StartCoroutine(ResyncRoutine());

        FinishResync();
    }

    bool TryStartReconnect()
    {
        Photon.Realtime.LoadBalancingClient client = PhotonNetwork.NetworkingClient;

        // Can only start from the Disconnected state; wait if the previous attempt is still winding down.
        if (client.LoadBalancingPeer.PeerState != PeerStateValue.Disconnected)
        {
            return false;
        }

        if (PhotonNetwork.ReconnectAndRejoin())
        {
            return true;
        }

        // No known game server to return to: go through the master server and rejoin by room name.
        if (!string.IsNullOrEmpty(_roomName) && client.ReconnectToMaster())
        {
            _rejoinByNameWhenOnMaster = true;
            return true;
        }

        return false;
    }

    // Drops the Photon connection so a half-open reconnect cannot silently put this client back into
    // the room after the match has been decided.
    void AbandonConnection()
    {
        if (PhotonNetwork.NetworkClientState != Photon.Realtime.ClientState.Disconnected
            && PhotonNetwork.NetworkClientState != Photon.Realtime.ClientState.PeerCreated)
        {
            PhotonNetwork.Disconnect();
        }
    }
    #endregion

    #region Opponent lost their connection
    IEnumerator WaitForOpponentRoutine()
    {
        ShowOverlay();

        float deadline = Time.unscaledTime + ReconnectWindowSeconds;
        bool opponentBack = false;

        while (Time.unscaledTime < deadline && MatchInProgress)
        {
            if (!PhotonNetwork.InRoom)
            {
                // We lost our own connection while waiting. The local reconnect flow takes over.
                yield break;
            }

            if (!OpponentPresent())
            {
                // Photon removed the opponent (their PlayerTtl ran out or they left on purpose).
                break;
            }

            if (OpponentActive())
            {
                opponentBack = true;
                break;
            }

            SetCountdownText(
                Localized("Opponent disconnected. Waiting for them to return...", "対戦相手の接続が切れました。復帰を待っています..."),
                deadline);

            yield return null;
        }

        if (!MatchInProgress)
        {
            FinishWithoutResult();
            yield break;
        }

        if (!opponentBack)
        {
            Debug.Log("[Reconnect] Opponent did not return. You win.");
            EndMatch(GManager.instance.You);
            yield break;
        }

        Debug.Log("[Reconnect] Opponent is back.");

        yield return StartCoroutine(ResyncRoutine());

        FinishResync();
    }
    #endregion

    #region State check after a rejoin
    IEnumerator ResyncRoutine()
    {
        _resyncOk = false;

        // Both sides are connected again; the check takes a few seconds and decides the outcome itself.
        SetLeaveButtonVisible(false);

        SetText(Localized("Reconnected. Verifying game state...", "再接続しました。ゲーム状態を確認中..."));

        float end = Time.unscaledTime + ResyncTimeoutSeconds;
        float nextSend = 0f;

        while (Time.unscaledTime < end && MatchInProgress)
        {
            if (!PhotonNetwork.InRoom || !OpponentActive())
            {
                yield break;
            }

            if (Time.unscaledTime >= nextSend)
            {
                SendStateCheck();
                nextSend = Time.unscaledTime + ResyncSendIntervalSeconds;
            }

            if (_hasRemoteHash)
            {
                int localHash = ComputeStateHash(out string localSummary);

                if (_remoteHash == localHash)
                {
                    // Tell the other side one last time so it can finish too.
                    SendStateCheck();
                    _resyncOk = true;
                    yield break;
                }
            }

            yield return null;
        }

        if (!_resyncOk)
        {
            ComputeStateHash(out string finalSummary);
            Debug.LogError($"[Reconnect] State check failed.\n  local : {finalSummary}\n  remote: {_remoteSummary}");
        }
    }

    void FinishResync()
    {
        if (!MatchInProgress)
        {
            FinishWithoutResult();
            return;
        }

        if (_resyncOk)
        {
            Debug.Log("[Reconnect] State check passed. Resuming match.");
            FinishWithoutResult();
            return;
        }

        // The two clients disagree, or the check could not be completed: do not keep playing out of sync.
        EndMatch(null);
    }

    void SendStateCheck()
    {
        if (!PhotonNetwork.InRoom || photonView == null)
        {
            return;
        }

        int hash = ComputeStateHash(out string summary);
        photonView.RPC("ReceiveStateCheck", Photon.Pun.RpcTarget.Others, hash, summary);
    }

    [PunRPC]
    public void ReceiveStateCheck(int hash, string summary)
    {
        _remoteHash = hash;
        _remoteSummary = summary;
        _hasRemoteHash = true;
    }

    // Checksum of the state both clients must agree on. Deliberately uses only things that are identical
    // on both sides (absolute player IDs, never "You"/"Opponent") and avoids anything randomized per process.
    static int ComputeStateHash(out string summary)
    {
        StringBuilder sb = new StringBuilder();
        int hash = 17;

        void Add(string name, int value)
        {
            unchecked
            {
                hash = hash * 31 + value;
            }

            sb.Append(name).Append('=').Append(value).Append(' ');
        }

        TurnStateMachine tsm = GManager.instance.turnStateMachine;
        GameContext ctx = tsm.gameContext;

        Add("turn", tsm.TurnCount);
        Add("phase", (int)ctx.TurnPhase);
        Add("turnPlayer", ctx.TurnPlayer != null ? ctx.TurnPlayer.PlayerID : -1);
        Add("memory", ctx.Memory);

        for (int id = 0; id <= 1; id++)
        {
            Player p = ctx.PlayerFromID(id);

            if (p == null)
            {
                Add($"p{id}", -1);
                continue;
            }

            Add($"p{id}.hand", p.HandCards.Count);
            Add($"p{id}.library", p.LibraryCards.Count);
            Add($"p{id}.security", p.SecurityCards.Count);
            Add($"p{id}.trash", p.TrashCards.Count);
            Add($"p{id}.digitama", p.DigitamaLibraryCards.Count);
            Add($"p{id}.field", p.GetFieldPermanents().Count);
        }

        summary = sb.ToString();
        return hash;
    }
    #endregion

    #region Ending the match
    void EndMatch(Player winner, bool surrendered = false)
    {
        StopRoutine();
        HideOverlay();
        _state = State.Idle;
        ResetFlags();

        if (GManager.instance != null
            && GManager.instance.turnStateMachine != null
            && !GManager.instance.turnStateMachine.endGame)
        {
            GManager.instance.turnStateMachine.EndGame(winner, surrendered);
        }
    }

    // "Leave match" on the overlay: the player does not want to wait out the reconnect window.
    void OnClickLeaveButton()
    {
        if (!MatchInProgress)
        {
            FinishWithoutResult();
            return;
        }

        switch (_state)
        {
            case State.LocalReconnecting:
                // We are the one who dropped. Giving up is a surrender: the same loss as running out the timer, just sooner.
                // The opponent cannot be told (we are offline); they win when our PlayerTtl runs out.
                Debug.Log("[Reconnect] Player chose to leave instead of reconnecting. Match is lost.");
                StopRoutine();
                AbandonConnection();
                EndMatch(GManager.instance.Opponent, true);
                break;

            case State.WaitingForOpponent:
                // The opponent dropped. Leaving early gives the same result as letting the timer run out.
                Debug.Log("[Reconnect] Player chose not to wait for the opponent. You win.");
                EndMatch(GManager.instance.You);
                break;
        }
    }

    void FinishWithoutResult()
    {
        _routine = null;
        HideOverlay();
        _state = State.Idle;
        ResetFlags();
    }
    #endregion

    #region Overlay
    // A full-screen blocker built at runtime (no scene or prefab changes needed). It also swallows clicks,
    // so nobody can act (and send RPCs into the void) while the other side is away. The only thing that
    // can be clicked is the "Leave match" button, which is hidden while the post-rejoin state check runs.
    void ShowOverlay()
    {
        if (_overlayRoot == null)
        {
            _overlayRoot = new GameObject("ReconnectOverlay");

            Canvas canvas = _overlayRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;

            CanvasScaler scaler = _overlayRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            _overlayRoot.AddComponent<GraphicRaycaster>();

            GameObject blocker = new GameObject("Blocker");
            blocker.transform.SetParent(_overlayRoot.transform, false);
            Image image = blocker.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.65f);
            image.raycastTarget = true;
            Stretch(blocker.GetComponent<RectTransform>());

            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(_overlayRoot.transform, false);
            _overlayText = textObject.AddComponent<Text>();
            _overlayText.font = Font.CreateDynamicFontFromOSFont("Arial", 40);
            _overlayText.fontSize = 40;
            _overlayText.alignment = TextAnchor.MiddleCenter;
            _overlayText.color = new Color(1f, 0.67f, 0f, 1f);
            _overlayText.raycastTarget = false;
            Stretch(textObject.GetComponent<RectTransform>());

            _leaveButton = CreateLeaveButton(_overlayRoot.transform);
        }

        _overlayRoot.SetActive(true);
        SetLeaveButtonVisible(true);

        // Drop keyboard/controller focus so Enter/Space cannot trigger a hidden button.
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    Button CreateLeaveButton(Transform parent)
    {
        GameObject buttonObject = new GameObject("LeaveButton");
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(360f, 80f);
        rect.anchoredPosition = new Vector2(0f, -180f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.55f, 0.1f, 0.1f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() =>
        {
            // One click only; the match result screen takes over from here.
            button.interactable = false;
            OnClickLeaveButton();
        });

        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(buttonObject.transform, false);
        Text label = labelObject.AddComponent<Text>();
        label.font = Font.CreateDynamicFontFromOSFont("Arial", 32);
        label.fontSize = 32;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.raycastTarget = false;
        label.text = Localized("Leave match", "対戦から退出");
        Stretch(labelObject.GetComponent<RectTransform>());

        return button;
    }

    void SetLeaveButtonVisible(bool visible)
    {
        if (_leaveButton != null)
        {
            _leaveButton.gameObject.SetActive(visible);
            _leaveButton.interactable = visible;
        }
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    void HideOverlay()
    {
        if (_overlayRoot != null)
        {
            Destroy(_overlayRoot);
            _overlayRoot = null;
            _overlayText = null;
            _leaveButton = null;
        }
    }

    void SetText(string text)
    {
        if (_overlayText != null)
        {
            _overlayText.text = text;
        }
    }

    void SetCountdownText(string message, float deadline)
    {
        int seconds = Mathf.Max(0, Mathf.CeilToInt(deadline - Time.unscaledTime));
        SetText($"{message}\n{seconds}s");
    }
    #endregion
}
