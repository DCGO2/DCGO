using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// Ranked matchmaking lobby: skill-banded JoinRandomOrCreateRoom with forced banlist.
/// Can share UI refs with random lobby when assigned in inspector; otherwise falls back to random lobby components.
/// </summary>
public class LobbyManager_RankedMatch : MonoBehaviourPunCallbacks
{
    private static readonly int CloseHash = Animator.StringToHash("Close");
    private static readonly int OpenHash = Animator.StringToHash("Open");
    private static WaitForSeconds _waitForSeconds0_2 = new WaitForSeconds(0.2f);
    private static WaitForSeconds _waitForSeconds0_1 = new WaitForSeconds(0.1f);
    private static WaitForSeconds _waitForSeconds0_5 = new WaitForSeconds(0.5f);
    private static WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1f);

    [Header("Shared UI (optional – can reuse Random Match lobby objects)")]
    public Text MessageText;
    public Text TimeText;
    public Text RankStatusText;
    public GameObject ReturnButton;
    public DeckInfoPanel deckInfoPanel;
    public Animator anim;
    public LoadingObject loadingObject;
    public LoadingObject disconnectLoadingObject;

    bool endLoadingText;
    bool isCoroutineRunning;
    bool startJoin;
    bool DoneCompleteMatching;
    bool once1;
    bool m;
    bool n;
    int time;
    float searchSeconds;
    int currentTolerance = 100;
    Button ReturnButtonButton;
    Coroutine searchExpandCoroutine;

    // Waiting line Y (Random Match uses -193). Slightly lower when ranked so rank line has room above.
    const float RankedMessageLocalY = -238f;
    // Gap above waiting line (higher = rank moves up; avoid dragging into deck box)
    const float RankedRankAboveMessage = 95f;

    string RankedKey => RankedKeys.RoomNameKey;

    void ResolveUiFromRandomLobbyIfNeeded()
    {
        var random = Opening.instance != null ? Opening.instance.battle?.lobbyManager_RandomMatch : null;
        if (random == null) return;

        if (MessageText == null) MessageText = random.MessageText;
        if (TimeText == null) TimeText = random.TimeText;
        if (ReturnButton == null) ReturnButton = random.ReturnButton;
        if (deckInfoPanel == null) deckInfoPanel = random.deckInfoPanel;
        if (anim == null) anim = random.anim;
        if (loadingObject == null) loadingObject = random.loadingObject;
        if (disconnectLoadingObject == null) disconnectLoadingObject = random.disconnectLoadingObject;
    }

    Text _patchedTitleText;
    string _patchedTitleOriginal;

    public void SetUpLobby()
    {
        ResolveUiFromRandomLobbyIfNeeded();

        Opening.instance.OffYesNoObjects();
        Opening.instance.deck.trialDraw.Close();
        Opening.instance.deck.deckListPanel.Close();

        ContinuousController.instance.isAI = false;
        ContinuousController.instance.isRandomMatch = false;
        ContinuousController.instance.isRanked = true;
        ContinuousController.instance.useBanlist = true;

        // Enable the GO that owns this component, or random lobby UI host if we ride on it
        gameObject.SetActive(true);
        if (Opening.instance.battle.lobbyManager_RandomMatch != null &&
            Opening.instance.battle.lobbyManager_RandomMatch != this as object)
        {
            Opening.instance.battle.lobbyManager_RandomMatch.gameObject.SetActive(true);
        }

        ApplyRankedHeaderTitle();
        ContinuousController.instance.StartCoroutine(ConnectCoroutine());

        if (anim != null)
        {
            anim.SetInteger(OpenHash, 1);
            anim.SetInteger(CloseHash, 0);
        }
    }

    void ApplyRankedHeaderTitle()
    {
        // Shared Random Match panel still has a "Random Match" header Text – patch while ranked is open
        var random = Opening.instance != null ? Opening.instance.battle?.lobbyManager_RandomMatch : null;
        if (random == null) return;

        foreach (var text in random.GetComponentsInChildren<Text>(true))
        {
            if (text == null || string.IsNullOrEmpty(text.text)) continue;
            if (text.text.IndexOf("Random Match", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.text.IndexOf("ランダムマッチ", StringComparison.Ordinal) >= 0)
            {
                _patchedTitleText = text;
                _patchedTitleOriginal = text.text;
                text.text = LocalizeUtility.GetLocalizedString(
                    EngMessage: "Ranked Match",
                    JpnMessage: "ランクマッチ");
                break;
            }
        }
    }

    void RestoreRankedHeaderTitle()
    {
        if (_patchedTitleText && _patchedTitleOriginal != null)
        {
            _patchedTitleText.text = _patchedTitleOriginal;
        }

        _patchedTitleText = null;
        _patchedTitleOriginal = null;
    }

    public void OffLobby()
    {
        // Ranked often reuses Random Match panel; always hide that UI host
        var random = Opening.instance != null ? Opening.instance.battle?.lobbyManager_RandomMatch : null;
        if (random != null)
        {
            random.OffLobby();
        }

        // If LobbyManager_RankedMatch was auto-added on BattleMode's root, do NOT disable that GO
        // (would hide mode select / entire battle hub)
        if (Opening.instance != null && Opening.instance.battle != null &&
            gameObject == Opening.instance.battle.gameObject)
        {
            return;
        }

        if (gameObject != null && gameObject != (random != null ? random.gameObject : null))
        {
            gameObject.SetActive(false);
        }
    }

    public void CloseLobby()
    {
        ContinuousController.instance.StartCoroutine(CloseLobbyCoroutine());
    }

    public IEnumerator CloseLobbyCoroutine()
    {
        ResolveUiFromRandomLobbyIfNeeded();

        // Stop every local queue loop immediately (prevents re-queue after return)
        once1 = true;
        DoneCompleteMatching = true;
        startJoin = false;
        m = true;
        n = true;
        endLoadingText = true;
        isCoroutineRunning = false;

        if (searchExpandCoroutine != null)
        {
            StopCoroutine(searchExpandCoroutine);
            searchExpandCoroutine = null;
        }

        // Waiting-text loops are often hosted on ContinuousController — stop them before tearing UI down
        if (ContinuousController.instance != null &&
            ContinuousController.instance.LoadingTextCoroutine != null)
        {
            ContinuousController.instance.StopCoroutine(ContinuousController.instance.LoadingTextCoroutine);
            ContinuousController.instance.LoadingTextCoroutine = null;
        }

        // Do NOT StopAllCoroutines() — this method itself is a coroutine on this component.

        if (ReturnButton)
        {
            ReturnButton.SetActive(false);
        }

        if (TimeText)
        {
            TimeText.gameObject.SetActive(false);
        }

        if (MessageText)
        {
            MessageText.text = "";
        }

        if (RankStatusText != null)
        {
            RankStatusText.text = "";
            RankStatusText.gameObject.SetActive(false);
        }

        if (disconnectLoadingObject != null)
        {
            yield return ContinuousController.instance.StartCoroutine(disconnectLoadingObject.StartLoading("Now Loading"));
        }

        if (RankedServices.Instance != null)
        {
            yield return RankedServices.Instance.Match.CancelMatch(RankedServices.Instance.Auth);
            RankedServices.Instance.Match.ClearActiveMatch();
        }

        if (PhotonNetwork.InRoom)
        {
            PhotonUtility.LeaveRoomImmediate();
        }

        yield return new WaitWhile(() => PhotonNetwork.InRoom);

        if (PhotonNetwork.IsConnected)
        {
            yield return ContinuousController.instance.StartCoroutine(PhotonUtility.DisconnectCoroutine());
        }

        ContinuousController.instance.isRanked = false;
        ContinuousController.instance.isRandomMatch = false;
        RestoreRankedHeaderTitle();
        OffLobby();

        if (disconnectLoadingObject != null)
        {
            yield return ContinuousController.instance.StartCoroutine(disconnectLoadingObject.EndLoading());
        }
    }

    public IEnumerator Init()
    {
        n = false;
        m = false;
        once1 = false;
        endLoadingText = false;
        isCoroutineRunning = false;
        time = 0;
        searchSeconds = 0f;
        currentTolerance = 100;
        startJoin = false;
        DoneCompleteMatching = false;
        ContinuousController.instance.LoadingTextCoroutine = null;
        if (ReturnButton != null) ReturnButton.SetActive(true);
        if (MessageText != null) MessageText.text = "";
        if (TimeText != null) TimeText.gameObject.SetActive(true);
        UpdateRankStatusText();
        StartCoroutine(TimeCountUp());

        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        yield return new WaitWhile(() => PhotonNetwork.InLobby);

        if (PhotonNetwork.IsConnected)
        {
            PhotonUtility.DisconnectImmediate();
        }

        yield return new WaitWhile(() => PhotonNetwork.IsConnected);
    }

    void EnsureRankStatusTextUi()
    {
        if (MessageText == null)
        {
            return;
        }

        if (RankStatusText == null)
        {
            var go = new GameObject("RankStatusText_Runtime", typeof(RectTransform));
            go.layer = MessageText.gameObject.layer;
            go.transform.SetParent(MessageText.transform.parent, false);
            RankStatusText = go.AddComponent<Text>();
            RankStatusText.raycastTarget = false;
        }

        RankStatusText.gameObject.SetActive(true);
        LayoutRankStatusTextInLobby();
    }

    /// <summary>
    /// One short rank line above "Signing in", centered and a bit smaller than the waiting text.
    /// </summary>
    void LayoutRankStatusTextInLobby()
    {
        if (RankStatusText == null || MessageText == null)
        {
            return;
        }

        var rt = RankStatusText.GetComponent<RectTransform>();
        var msgRt = MessageText.GetComponent<RectTransform>();
        if (rt == null || msgRt == null)
        {
            return;
        }

        if (rt.parent != msgRt.parent)
        {
            rt.SetParent(msgRt.parent, false);
        }

        RankStatusText.font = MessageText.font;
        RankStatusText.material = MessageText.material;
        RankStatusText.color = MessageText.color;
        RankStatusText.fontSize = MessageText.fontSize;
        RankStatusText.fontStyle = FontStyle.Bold;
        RankStatusText.alignment = TextAnchor.MiddleCenter;
        RankStatusText.horizontalOverflow = HorizontalWrapMode.Overflow;
        RankStatusText.verticalOverflow = VerticalWrapMode.Overflow;
        RankStatusText.resizeTextForBestFit = false;
        RankStatusText.lineSpacing = 1f;
        RankStatusText.supportRichText = false;

        rt.localRotation = Quaternion.identity;
        // Slightly smaller than "Signing in" / "Waiting for opponent"
        const float sizeMul = 0.82f;
        rt.localScale = new Vector3(
            msgRt.localScale.x * sizeMul,
            msgRt.localScale.y * sizeMul,
            msgRt.localScale.z);

        rt.anchorMin = msgRt.anchorMin;
        rt.anchorMax = msgRt.anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        // Wide enough for a one-line rank string when centered
        float w = msgRt.sizeDelta.x > 10f ? msgRt.sizeDelta.x : 600f;
        float h = msgRt.sizeDelta.y > 10f ? msgRt.sizeDelta.y : 80f;
        rt.sizeDelta = new Vector2(Mathf.Max(w, 700f), h);

        Vector3 msgLp = MessageText.transform.localPosition;
        // Center on X (MessageText uses ~-180 for left-shifted short strings)
        // Keep rank just above waiting line so they don't overlap
        rt.localPosition = new Vector3(0f, msgLp.y + RankedRankAboveMessage, msgLp.z);
    }

    void SetRankedMessagePosition(float localX)
    {
        if (MessageText == null)
        {
            return;
        }

        MessageText.transform.localPosition = new Vector3(localX, RankedMessageLocalY, 0f);
        LayoutRankStatusTextInLobby();
    }

    void UpdateRankStatusText()
    {
        EnsureRankStatusTextUi();

        var ranked = RankedServices.EnsureExists();
        var profile = ranked.Profile != null ? ranked.Profile.Cached : null;
        if (profile == null)
        {
            if (RankStatusText != null)
            {
                RankStatusText.text = LocalizeUtility.GetLocalizedString(
                    EngMessage: "Rank: …",
                    JpnMessage: "ランク: …");
            }

            return;
        }

        if (RankStatusText != null)
        {
            RankStatusText.gameObject.SetActive(true);
            RankStatusText.text = profile.FormatLobbyLine();
            LayoutRankStatusTextInLobby();
        }
    }

    IEnumerator TimeCountUp()
    {
        time = 0;
        if (TimeText != null) TimeText.gameObject.SetActive(true);

        while (!DoneCompleteMatching)
        {
            string min = (time / 60).ToString();
            string sec = (time % 60).ToString();
            if (min.Length == 1) min = $"0{min}";
            if (sec.Length == 1) sec = $"0{sec}";
            if (TimeText != null) TimeText.text = $"{min}:{sec}";
            time++;
            searchSeconds = time;
            yield return _waitForSeconds1;
        }

        if (TimeText != null) TimeText.gameObject.SetActive(false);
    }

    IEnumerator ConnectCoroutine()
    {
        ResolveUiFromRandomLobbyIfNeeded();

        if (ContinuousController.instance.BattleDeckData != null && deckInfoPanel != null)
        {
            _ = deckInfoPanel.SetUpDeckInfoPanel(ContinuousController.instance.BattleDeckData);
        }

        if (ReturnButton != null) ReturnButton.SetActive(false);

        endLoadingText = true;
        yield return ContinuousController.instance.StartCoroutine(Init());
        yield return _waitForSeconds0_5;
        endLoadingText = false;

        ContinuousController.instance.LoadingTextCoroutine = ContinuousController.instance.StartCoroutine(SetWaitingText(
            LocalizeUtility.GetLocalizedString(EngMessage: "Signing in", JpnMessage: "サインイン中")));

        if (MessageText != null) SetRankedMessagePosition(-180f);

        bool authOk = false;
        string authError = null;
        yield return RankedServices.EnsureExists().BootstrapForRanked((ok, err) =>
        {
            authOk = ok;
            authError = err;
        });

        if (!authOk)
        {
            if (MessageText != null)
            {
                MessageText.text = authError ?? "Ranked login failed";
            }

            if (ReturnButton != null) ReturnButton.SetActive(true);
            yield break;
        }

        UpdateRankStatusText();

        RankedServices.Instance.Auth.ApplyPhotonAuthValues();

        ContinuousController.instance.LoadingTextCoroutine = ContinuousController.instance.StartCoroutine(SetWaitingText(
            LocalizeUtility.GetLocalizedString(EngMessage: "Connecting", JpnMessage: "接続中")));

        yield return ContinuousController.instance.StartCoroutine(PhotonUtility.ConnectToLobbyCoroutine());
        yield return ContinuousController.instance.StartCoroutine(PhotonUtility.SignUpBattleDeckData());
        yield return ContinuousController.instance.StartCoroutine(PhotonUtility.SetRankedPlayerProperties());
        yield return _waitForSeconds0_5;

        searchExpandCoroutine = StartCoroutine(SearchAndExpandCoroutine());

        yield return new WaitWhile(() => !PhotonNetwork.InRoom);
        yield return _waitForSeconds0_1;

        if (loadingObject != null)
        {
            yield return ContinuousController.instance.StartCoroutine(loadingObject.EndLoading());
        }

        if (ReturnButton != null) ReturnButton.SetActive(true);
    }

    IEnumerator SearchAndExpandCoroutine()
    {
        // Stable casual-like queue: join or create rooms tagged Mode=ranked.
        // No mid-queue leave/recreate (that was preventing two players from staying matched).
        // Retry join/create if still alone; never drop an occupied room while waiting for P2.

        float nextRetryAt = 0f;

        while (!DoneCompleteMatching)
        {
            currentTolerance = RankedRating.GetMmrTolerance(searchSeconds);

            if (PhotonNetwork.InRoom)
            {
                int count = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.PlayerCount : 0;
                if (count >= 2)
                {
                    ContinuousController.instance.LoadingTextCoroutine = ContinuousController.instance.StartCoroutine(SetWaitingText(
                        LocalizeUtility.GetLocalizedString(
                            EngMessage: "Match found",
                            JpnMessage: "マッチ成立")));
                    yield break;
                }

                ContinuousController.instance.LoadingTextCoroutine = ContinuousController.instance.StartCoroutine(SetWaitingText(
                    LocalizeUtility.GetLocalizedString(
                        EngMessage: "Waiting for opponent",
                        JpnMessage: "相手を待っています")));
            }
            else if (!startJoin && PhotonNetwork.InLobby && PhotonNetwork.IsConnectedAndReady)
            {
                if (Time.realtimeSinceStartup >= nextRetryAt)
                {
                    TryJoinOrCreateRankedRoom(currentTolerance);
                    nextRetryAt = Time.realtimeSinceStartup + 3f;
                }

                ContinuousController.instance.LoadingTextCoroutine = ContinuousController.instance.StartCoroutine(SetWaitingText(
                    LocalizeUtility.GetLocalizedString(
                        EngMessage: "Matching ranked",
                        JpnMessage: "ランクマッチング中")));
            }
            else if (!PhotonNetwork.InLobby && PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InRoom)
            {
                PhotonNetwork.JoinLobby();
            }

            yield return _waitForSeconds1;
        }
    }

    IEnumerator LeaveRoomForRematch()
    {
        startJoin = false;
        if (PhotonNetwork.InRoom)
        {
            PhotonUtility.LeaveRoomImmediate();
        }

        yield return new WaitWhile(() => PhotonNetwork.InRoom);

        if (!PhotonNetwork.InLobby && PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.JoinLobby();
            yield return new WaitWhile(() => !PhotonNetwork.InLobby);
        }
    }

    void TryJoinOrCreateRankedRoom(int tolerance)
    {
        if (!PhotonNetwork.IsConnectedAndReady || PhotonNetwork.InRoom || startJoin)
        {
            return;
        }

        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
            return;
        }

        int selfMmr = RankedServices.EnsureExists().Profile.Cached?.mmr ?? RankedRating.DefaultMmr;
        int bucket = RankedRating.GetMmrBucket(selfMmr);

        // Only filter by Mode=ranked so any two ranked players can find each other.
        // (Strict MMR buckets + leave/recreate caused permanent miss matches.)
        Hashtable expected = new Hashtable
        {
            { RankedKeys.ModeProperty, RankedKeys.ModeRanked },
        };

        RoomOptions roomOptions = BuildRoomOptions(bucket);
        string roomName = StringUtils.GeneratePassword_AlpahabetNum(8) + RankedKey;

        startJoin = true;
        Debug.Log($"[Ranked] JoinRandomOrCreateRoom Mode=ranked mmr={selfMmr} bucket={bucket} name={roomName}");

        bool started = PhotonNetwork.JoinRandomOrCreateRoom(
            expectedCustomRoomProperties: expected,
            expectedMaxPlayers: 2,
            matchingType: MatchmakingMode.FillRoom,
            typedLobby: TypedLobby.Default,
            sqlLobbyFilter: null,
            roomName: roomName,
            roomOptions: roomOptions);

        if (!started)
        {
            startJoin = false;
            Debug.LogWarning("[Ranked] JoinRandomOrCreateRoom failed to start — will retry");
        }
    }

    RoomOptions BuildRoomOptions(int mmrBucket)
    {
        var options = new RoomOptions
        {
            IsVisible = true,
            IsOpen = true,
            PublishUserId = true,
            MaxPlayers = 2,
            CustomRoomProperties = new Hashtable
            {
                { "RoomCreator", PhotonNetwork.NickName },
                { RankedKeys.ModeProperty, RankedKeys.ModeRanked },
                { RankedKeys.MmrBucketProperty, mmrBucket },
                { RankedKeys.UseBanlistProperty, true },
            },
            CustomRoomPropertiesForLobby = new[]
            {
                "RoomCreator",
                RankedKeys.ModeProperty,
                RankedKeys.MmrBucketProperty,
                RankedKeys.UseBanlistProperty,
            },
        };
        BattleReconnectService.ApplyBattleTtl(options);
        return options;
    }

    public override void OnJoinedRoom()
    {
        startJoin = false;
        if (ContinuousController.instance == null || !ContinuousController.instance.isRanked)
        {
            return;
        }

        Debug.Log($"[Ranked] OnJoinedRoom name={PhotonNetwork.CurrentRoom?.Name} players={PhotonNetwork.CurrentRoom?.PlayerCount}");

        endLoadingText = true;
        endLoadingText = false;
        ContinuousController.instance.LoadingTextCoroutine = ContinuousController.instance.StartCoroutine(SetWaitingText(
            LocalizeUtility.GetLocalizedString(EngMessage: "Waiting for opponent", JpnMessage: "相手を待っています")));

        if (MessageText != null) SetRankedMessagePosition(-148f);
        if (ReturnButton != null) ReturnButton.SetActive(true);

        // If we joined an already-full room (join after host had 1 waiting + us)
        if (PhotonNetwork.CurrentRoom != null &&
            BattleReconnectService.CountActivePlayers() >= PhotonNetwork.CurrentRoom.MaxPlayers &&
            PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(GoNextScene());
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (ContinuousController.instance == null || !ContinuousController.instance.isRanked || DoneCompleteMatching)
        {
            return;
        }

        Debug.Log($"[Ranked] OnPlayerEnteredRoom count={PhotonNetwork.CurrentRoom?.PlayerCount}");

        if (PhotonNetwork.IsMasterClient &&
            PhotonNetwork.CurrentRoom != null &&
            BattleReconnectService.CountActivePlayers() >= PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            StartCoroutine(GoNextScene());
        }
    }

    public override void OnCreatedRoom()
    {
        startJoin = false;
        Debug.Log($"[Ranked] OnCreatedRoom name={PhotonNetwork.CurrentRoom?.Name}");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // JoinRandomOrCreateRoom may not always create on this path for all Photon builds
        if (ContinuousController.instance != null && ContinuousController.instance.isRanked)
        {
            Debug.Log($"[Ranked] Join random failed: [{returnCode}] {message} — creating room");
            startJoin = false;
            if (!PhotonNetwork.InRoom && !DoneCompleteMatching)
            {
                CreateRankedRoomFallback();
            }
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (ContinuousController.instance != null && ContinuousController.instance.isRanked && !DoneCompleteMatching)
        {
            Debug.Log($"[Ranked] Join room failed: [{returnCode}] {message}");
            startJoin = false;
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        if (ContinuousController.instance != null && ContinuousController.instance.isRanked)
        {
            Debug.Log($"[Ranked] Create room failed: [{returnCode}] {message}");
            startJoin = false;
        }
    }

    void CreateRankedRoomFallback()
    {
        if (PhotonNetwork.InRoom || startJoin || DoneCompleteMatching)
        {
            return;
        }

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            return;
        }

        int selfMmr = RankedServices.EnsureExists().Profile.Cached?.mmr ?? RankedRating.DefaultMmr;
        int bucket = RankedRating.GetMmrBucket(selfMmr);
        RoomOptions roomOptions = BuildRoomOptions(bucket);
        string roomName = StringUtils.GeneratePassword_AlpahabetNum(8) + RankedKey;
        startJoin = true;
        Debug.Log($"[Ranked] CreateRoom fallback name={roomName}");
        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    IEnumerator SetWaitingText(string defaultString)
    {
        if (isCoroutineRunning || !MessageText)
        {
            yield break;
        }

        isCoroutineRunning = true;
        float waitTime = 0.18f;
        int count = 0;

        while (!endLoadingText && MessageText)
        {
            count++;
            if (count >= 4) count = 0;
            MessageText.text = defaultString;
            for (int i = 0; i < count; i++) MessageText.text += ".";
            yield return new WaitForSeconds(waitTime);
        }

        isCoroutineRunning = false;
    }

    void Start()
    {
        ResolveUiFromRandomLobbyIfNeeded();
        if (ReturnButton != null && ReturnButton.transform.childCount > 0)
        {
            ReturnButtonButton = ReturnButton.transform.GetChild(0).GetComponent<Button>();
        }
    }

        void LateUpdate()
    {
        if (ContinuousController.instance == null || !ContinuousController.instance.isRanked ||
            !gameObject.activeInHierarchy || DoneCompleteMatching)
        {
            return;
        }

        if (PhotonNetwork.InRoom)
        {
            if (BattleReconnectService.CountActivePlayers() == PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    StartCoroutine(GoNextScene());
                }

                if (ReturnButtonButton != null) ReturnButtonButton.enabled = false;
            }
            else if (ReturnButtonButton != null)
            {
                ReturnButtonButton.enabled = true;
            }
        }
        else if (ReturnButtonButton != null)
        {
            ReturnButtonButton.enabled = true;
        }

        if (DoneCompleteMatching && ReturnButton != null)
        {
            ReturnButton.SetActive(false);
        }
    }

    IEnumerator GoNextScene()
    {
        if (DoneCompleteMatching || once1) yield break;
        once1 = true;
        yield return _waitForSeconds0_1;

        string matchId = Guid.NewGuid().ToString("N");
        var roomHash = new Hashtable
        {
            { RankedKeys.MatchIdProperty, matchId },
            { RankedKeys.UseBanlistProperty, true },
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;

        // Use ContinuousController PhotonView like other battle RPCs (reliable for both clients)
        PhotonView ccView = ContinuousController.instance != null
            ? ContinuousController.instance.GetComponent<PhotonView>()
            : null;

        if (ccView != null)
        {
            ccView.RPC(nameof(ContinuousController.RankedGoToBattleScene), RpcTarget.All, matchId);
        }
        else
        {
            // Offline / missing view fallback
            BeginBattleTransition(matchId);
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        // Backup if RPC is delayed: both clients start when MatchId appears
        if (!ContinuousController.instance.isRanked || DoneCompleteMatching)
        {
            return;
        }

        if (propertiesThatChanged != null &&
            propertiesThatChanged.ContainsKey(RankedKeys.MatchIdProperty))
        {
            string matchId = propertiesThatChanged[RankedKeys.MatchIdProperty] as string;
            BeginBattleTransition(matchId);
        }
    }

    /// <summary>Called by ContinuousController RPC and room-property backup.</summary>
    public void BeginBattleTransition(string matchId)
    {
        if (DoneCompleteMatching) return;

        endLoadingText = true;
        DoneCompleteMatching = true;

        if (searchExpandCoroutine != null)
        {
            StopCoroutine(searchExpandCoroutine);
            searchExpandCoroutine = null;
        }

        if (TimeText != null) TimeText.gameObject.SetActive(false);
        if (MessageText != null)
        {
            MessageText.text = LocalizeUtility.GetLocalizedString(
                EngMessage: "Matching completed!",
                JpnMessage: "マッチングしました!");
            SetRankedMessagePosition(-390f);
        }

        if (ReturnButton != null) ReturnButton.SetActive(false);

        ContinuousController.instance.StartCoroutine(GoToBattleSceneCoroutine(matchId));
    }

    IEnumerator GoToBattleSceneCoroutine(string matchId)
    {
        ContinuousController.instance.StartCoroutine(Opening.instance.OpeningBGM.FadeOut(0.2f));

        // Resolve opponent for ranked report (non-blocking PlayFab begin)
        string oppId = null;
        int oppMmr = RankedRating.DefaultMmr;
        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (p.IsLocal) continue;
            if (p.CustomProperties.TryGetValue(RankedKeys.PlayFabIdProperty, out object idObj))
            {
                oppId = idObj as string;
            }

            if (p.CustomProperties.TryGetValue(RankedKeys.MmrProperty, out object mmrObj))
            {
                oppMmr = Convert.ToInt32(mmrObj);
            }
        }

        if (string.IsNullOrEmpty(matchId) && PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(RankedKeys.MatchIdProperty, out object mid))
        {
            matchId = mid as string;
        }

        // Stash match id immediately; PlayFab BeginMatch must never block entering battle
        var ranked = RankedServices.EnsureExists();
        ContinuousController.instance.StartCoroutine(
            BeginMatchNonBlocking(ranked, matchId, oppId ?? "unknown", oppMmr));

        yield return _waitForSeconds0_1;

        foreach (Camera camera in Opening.instance.openingCameras)
        {
            camera.gameObject.SetActive(false);
        }

        yield return _waitForSeconds0_1;
        SceneManager.LoadSceneAsync("BattleScene", LoadSceneMode.Additive);
        yield return null;
    }

    IEnumerator BeginMatchNonBlocking(RankedServices ranked, string matchId, string oppId, int oppMmr)
    {
        // Always keep local match id so results can report even if PlayFab is slow/down
        ranked.Match.EnsureActiveMatch(matchId, oppId, oppMmr);

        float timeout = 8f;
        float elapsed = 0f;
        bool done = false;

        ContinuousController.instance.StartCoroutine(
            ranked.Match.BeginMatch(
                ranked.Auth,
                ranked.Profile,
                matchId,
                oppId,
                oppMmr,
                (ok, err) =>
                {
                    done = true;
                    if (!ok) Debug.LogWarning($"[Ranked] BeginMatch failed: {err}");
                }));

        while (!done && elapsed < timeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!done)
        {
            Debug.LogWarning("[Ranked] BeginMatch timed out — battle continues; rating may use offline settle.");
        }
    }
}
