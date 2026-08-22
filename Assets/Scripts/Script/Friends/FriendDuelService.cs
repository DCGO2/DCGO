using System;
using System.Collections;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// FindFriends presence, challenge room create/join, and invite popup while on home lobby.
/// </summary>
public class FriendDuelService : MonoBehaviourPunCallbacks
{
    public class PresenceInfo
    {
        public bool isOnline;
        public bool isInRoom;
        public string roomName;
    }

    readonly Dictionary<string, PresenceInfo> _presence =
        new Dictionary<string, PresenceInfo>(StringComparer.OrdinalIgnoreCase);

    readonly Dictionary<string, RoomInfo> _lobbyRoomCache =
        new Dictionary<string, RoomInfo>(StringComparer.OrdinalIgnoreCase);

    readonly HashSet<string> _declinedRooms =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    string _activeSeriesRoom;

    Coroutine _findFriendsLoop;
    Coroutine _inviteWatchLoop;
    Coroutine _inviteTimeout;
    bool _listeningInvites;
    bool _pendingInviteShown;
    string _pendingInviteRoom;
    bool _createFailed;
    GameObject _inviteOverlay;

    public void NotifySeriesRoom(string roomName)
    {
        if (string.IsNullOrEmpty(roomName))
        {
            return;
        }

        _activeSeriesRoom = roomName;
        SetInviteListening(false);
        DestroyInviteOverlay();
        _pendingInviteShown = false;
    }

    public void RememberEndedSeriesRoom(string roomName)
    {
        if (!string.IsNullOrEmpty(roomName))
        {
            _declinedRooms.Add(roomName);
        }

        _activeSeriesRoom = null;
        DestroyInviteOverlay();
        _pendingInviteShown = false;
    }

    public void DestroyInviteOverlayPublic()
    {
        DestroyInviteOverlay();
        _pendingInviteShown = false;
    }

    public event Action PresenceChanged;
    public event Action InviteHandled;

    public bool IsChallenging { get; private set; }

    public PresenceInfo GetPresence(string playFabId)
    {
        if (string.IsNullOrEmpty(playFabId))
        {
            return null;
        }

        return _presence.TryGetValue(playFabId, out var info) ? info : null;
    }

    public bool CanChallenge(string playFabId)
    {
        var info = GetPresence(playFabId);
        return info != null && info.isOnline && !info.isInRoom;
    }

    public void SetInviteListening(bool enabled)
    {
        _listeningInvites = enabled;
        if (!enabled)
        {
            _pendingInviteShown = false;
            _pendingInviteRoom = null;
            DestroyInviteOverlay();
            StopInviteWatch();
            StopPresencePolling();
            return;
        }

        if (PhotonNetwork.InRoom ||
            (ContinuousController.instance != null && ContinuousController.instance.isFriendDuel))
        {
            return;
        }

        StartPresencePolling();
        StartInviteWatch();
        ScanForPendingInvite();
    }

    public void StartPresencePolling()
    {
        StopPresencePolling();
        _findFriendsLoop = StartCoroutine(FindFriendsLoop());
    }

    public void StopPresencePolling()
    {
        if (_findFriendsLoop != null)
        {
            StopCoroutine(_findFriendsLoop);
            _findFriendsLoop = null;
        }
    }

    void StartInviteWatch()
    {
        StopInviteWatch();
        _inviteWatchLoop = StartCoroutine(InviteWatchLoop());
    }

    void StopInviteWatch()
    {
        if (_inviteWatchLoop != null)
        {
            StopCoroutine(_inviteWatchLoop);
            _inviteWatchLoop = null;
        }
    }

    IEnumerator FindFriendsLoop()
    {
        while (true)
        {
            if (PhotonNetwork.InRoom || PhotonNetwork.Server != ServerConnection.MasterServer)
            {
                yield return new WaitForSecondsRealtime(1f);
                continue;
            }

            RequestFindFriends();
            yield return new WaitForSecondsRealtime(FriendKeys.FindFriendsPollSeconds);
        }
    }

    IEnumerator InviteWatchLoop()
    {
        var wait = new WaitForSecondsRealtime(1.5f);
        while (_listeningInvites)
        {
            MergeOnlinePlayerCountRooms();
            ScanForPendingInvite();
            yield return wait;
        }
    }

    void MergeOnlinePlayerCountRooms()
    {
        var opc = OnlinePlayerCountService.Instance;
        if (opc?.LobbyRooms == null)
        {
            return;
        }

        foreach (var kv in opc.LobbyRooms)
        {
            if (kv.Value == null || string.IsNullOrEmpty(kv.Key))
            {
                continue;
            }

            if (kv.Value.RemovedFromList)
            {
                _lobbyRoomCache.Remove(kv.Key);
            }
            else
            {
                _lobbyRoomCache[kv.Key] = kv.Value;
            }
        }
    }

    public void RequestFindFriends()
    {
        var friends = FriendServices.EnsureExists().List;
        string[] ids = friends.FriendUserIds();
        if (ids == null || ids.Length == 0)
        {
            return;
        }

        if (!PhotonNetwork.IsConnectedAndReady ||
            PhotonNetwork.InRoom ||
            PhotonNetwork.Server != ServerConnection.MasterServer)
        {
            return;
        }

        PhotonNetwork.FindFriends(ids);
    }

    public override void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        if (friendList == null)
        {
            return;
        }

        foreach (var f in friendList)
        {
            if (f == null || string.IsNullOrEmpty(f.UserId))
            {
                continue;
            }

            _presence[f.UserId] = new PresenceInfo
            {
                isOnline = f.IsOnline,
                isInRoom = f.IsInRoom,
                roomName = f.Room,
            };
        }

        PresenceChanged?.Invoke();

        if (_listeningInvites)
        {
            ScanForPendingInvite();
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (roomList == null)
        {
            return;
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            var room = roomList[i];
            if (room == null || string.IsNullOrEmpty(room.Name))
            {
                continue;
            }

            if (room.RemovedFromList)
            {
                _lobbyRoomCache.Remove(room.Name);
            }
            else
            {
                _lobbyRoomCache[room.Name] = room;
            }
        }

        if (_listeningInvites)
        {
            ScanForPendingInvite();
        }
    }

    void ScanForPendingInvite()
    {
        if (!_listeningInvites || _pendingInviteShown || PhotonNetwork.InRoom)
        {
            return;
        }

        var cc = ContinuousController.instance;
        if (cc != null && cc.isFriendDuel)
        {
            return;
        }

        if (ContinuousController.IsBattleSceneLoaded())
        {
            return;
        }

        string localId = FriendListService.LocalPlayFabId();

        // Primary path: FindFriends reports a friend sitting in an fd- room (works even
        // when lobby custom properties never arrive).
        foreach (var kv in _presence)
        {
            var info = kv.Value;
            if (info == null || !info.isInRoom || !FriendKeys.IsFriendDuelRoomName(info.roomName))
            {
                continue;
            }

            if (ShouldIgnoreInviteRoom(info.roomName))
            {
                continue;
            }

            string friendName = kv.Key;
            var list = FriendServices.EnsureExists().List;
            foreach (var f in list.Friends)
            {
                if (string.Equals(f.playFabId, kv.Key, StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrEmpty(f.displayName))
                {
                    friendName = f.displayName;
                    break;
                }
            }

            Debug.Log($"[Friends] Invite detected via FindFriends room={info.roomName} from={kv.Key}");
            ShowInvitePopup(info.roomName, friendName, 0);
            return;
        }

        if (string.IsNullOrEmpty(localId))
        {
            return;
        }

        foreach (var kv in _lobbyRoomCache)
        {
            var room = kv.Value;
            if (!IsInviteForLocalPlayer(room, localId))
            {
                continue;
            }

            if (ShouldIgnoreInviteRoom(room.Name))
            {
                continue;
            }

            Debug.Log($"[Friends] Invite detected room={room.Name} from lobby cache");
            ShowInvitePopup(room);
            return;
        }
    }

    bool ShouldIgnoreInviteRoom(string roomName)
    {
        if (string.IsNullOrEmpty(roomName))
        {
            return true;
        }

        if (_declinedRooms.Contains(roomName))
        {
            return true;
        }

        if (!string.IsNullOrEmpty(_activeSeriesRoom) &&
            string.Equals(_activeSeriesRoom, roomName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    static bool IsInviteForLocalPlayer(RoomInfo room, string localId)
    {
        if (room == null || !room.IsOpen || room.RemovedFromList || room.PlayerCount <= 0 || room.PlayerCount >= room.MaxPlayers)
        {
            return false;
        }

        var props = room.CustomProperties;
        if (props == null)
        {
            return false;
        }

        string mode = ReadPropString(props, FriendKeys.ModeProperty);
        if (!string.Equals(mode, FriendKeys.ModeFriend, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string target = ReadPropString(props, FriendKeys.TargetUserIdProperty);
        if (string.IsNullOrEmpty(target))
        {
            return false;
        }

        return string.Equals(target, localId, StringComparison.OrdinalIgnoreCase);
    }

    static string ReadPropString(Hashtable props, string key)
    {
        if (props == null || !props.TryGetValue(key, out object v) || v == null)
        {
            return null;
        }

        return v.ToString();
    }

    void ShowInvitePopup(RoomInfo room)
    {
        string challengerName = ReadPropString(room.CustomProperties, FriendKeys.ChallengerNameProperty);
        int winsToTake = 1;
        string winsRaw = ReadPropString(room.CustomProperties, FriendKeys.WinsToTakeProperty);
        if (!string.IsNullOrEmpty(winsRaw) && int.TryParse(winsRaw, out int parsed))
        {
            winsToTake = parsed;
        }

        ShowInvitePopup(room.Name, string.IsNullOrEmpty(challengerName) ? "Friend" : challengerName, winsToTake);
    }

    void ShowInvitePopup(string roomName, string challengerName, int winsToTake)
    {
        _pendingInviteShown = true;
        _pendingInviteRoom = roomName;

        FriendListPanel.HideIfOpen();

        if (string.IsNullOrEmpty(challengerName))
        {
            challengerName = "Friend";
        }

        string info = winsToTake >= 2
            ? LocalizeUtility.GetLocalizedString(
                EngMessage: $"{challengerName} challenges you to a Best of 3 duel.",
                JpnMessage: $"{challengerName}からBest of 3のデュエル挑戦です。")
            : LocalizeUtility.GetLocalizedString(
                EngMessage: $"{challengerName} challenges you to a duel.",
                JpnMessage: $"{challengerName}からデュエル挑戦です。");

        // Always use a dedicated overlay canvas. Opening YesNoObjects live under Battle
        // (inactive on Home), so SetUpYesNoObject looks like a success but nothing shows.
        Debug.Log($"[Friends] Showing invite overlay room={roomName}");
        ShowInviteOverlay(roomName, winsToTake, info);
        Opening.instance?.PlayDecisionSE();
    }

    void ShowInviteOverlay(string roomName, int winsToTake, string info)
    {
        DestroyInviteOverlay();

        Font font = ResolveOverlayFont();
        if (font == null)
        {
            Debug.LogError("[Friends] No UI font — cannot draw invite overlay");
            _pendingInviteShown = false;
            return;
        }

        _inviteOverlay = new GameObject("FriendInviteOverlay");
        DontDestroyOnLoad(_inviteOverlay);

        var canvas = _inviteOverlay.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32760;
        canvas.overrideSorting = true;
        _inviteOverlay.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        var dim = _inviteOverlay.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.65f);
        dim.raycastTarget = true;

        var rootRt = _inviteOverlay.GetComponent<RectTransform>();
        if (rootRt == null)
        {
            rootRt = _inviteOverlay.AddComponent<RectTransform>();
        }

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(_inviteOverlay.transform, false);
        var panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(720f, 320f);
        panel.GetComponent<Image>().color = new Color(0.1f, 0.12f, 0.2f, 0.98f);

        var infoGo = new GameObject("Info", typeof(RectTransform));
        infoGo.transform.SetParent(panel.transform, false);
        var infoText = infoGo.AddComponent<Text>();
        infoText.font = font;
        infoText.fontSize = 28;
        infoText.alignment = TextAnchor.MiddleCenter;
        infoText.color = Color.white;
        infoText.text = info;
        infoText.horizontalOverflow = HorizontalWrapMode.Wrap;
        infoText.verticalOverflow = VerticalWrapMode.Overflow;
        var infoRt = infoText.GetComponent<RectTransform>();
        infoRt.anchorMin = new Vector2(0.06f, 0.42f);
        infoRt.anchorMax = new Vector2(0.94f, 0.92f);
        infoRt.offsetMin = Vector2.zero;
        infoRt.offsetMax = Vector2.zero;

        CreateOverlayButton(panel.transform, font,
            LocalizeUtility.GetLocalizedString(EngMessage: "Accept", JpnMessage: "受ける"),
            new Vector2(-140f, -90f),
            () =>
            {
                DestroyInviteOverlay();
                StartCoroutine(AcceptInviteCoroutine(roomName, winsToTake));
            });
        CreateOverlayButton(panel.transform, font,
            LocalizeUtility.GetLocalizedString(EngMessage: "Decline", JpnMessage: "断る"),
            new Vector2(140f, -90f),
            () => DeclineInvite(roomName));
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

        return Font.CreateDynamicFontFromOSFont("Arial", 24);
    }

    static void CreateOverlayButton(Transform parent, Font font, string label, Vector2 pos, UnityAction onClick)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(180f, 52f);
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
        var trt = text.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
    }

    void DestroyInviteOverlay()
    {
        if (_inviteOverlay != null)
        {
            Destroy(_inviteOverlay);
            _inviteOverlay = null;
        }
    }

    void DeclineInvite(string roomName)
    {
        DestroyInviteOverlay();
        if (!string.IsNullOrEmpty(roomName))
        {
            _declinedRooms.Add(roomName);
        }

        _pendingInviteShown = false;
        _pendingInviteRoom = null;
        InviteHandled?.Invoke();
    }

    IEnumerator AcceptInviteCoroutine(string roomName, int winsToTake)
    {
        DestroyInviteOverlay();
        _pendingInviteShown = false;
        InviteHandled?.Invoke();

        if (string.IsNullOrEmpty(roomName))
        {
            yield break;
        }

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            yield return ContinuousController.instance.StartCoroutine(
                PhotonUtility.ConnectToMasterServerCoroutine(matchmakingOwnsConnection: true));
        }

        OnlinePlayerCountService.EnsureExists().SetMatchmakingOwnsConnection(true);

        SetInviteListening(false);
        StopPresencePolling();

        if (Opening.instance != null)
        {
            Opening.instance.OffModeButtons();
            Opening.instance.home?.OffHome();
            FriendListPanel.HideIfOpen();
        }

        ContinuousController.instance.isAI = false;
        ContinuousController.instance.isRandomMatch = false;
        ContinuousController.instance.isRanked = false;
        ContinuousController.instance.isTournament = false;
        ContinuousController.instance.isFriendDuel = true;
        ContinuousController.instance.FriendWinsToTake = winsToTake;

        // JoinRoom works from Master or Lobby — do not require InLobby.
        PhotonNetwork.JoinRoom(roomName);
        float wait = 0f;
        while (!PhotonNetwork.InRoom && PhotonNetwork.IsConnected && wait < 15f)
        {
            wait += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning($"[Friends] Failed to join invite room '{roomName}'");
            ContinuousController.instance.ClearFriendDuel();
            OnlinePlayerCountService.EnsureExists().SetMatchmakingOwnsConnection(false);
            Opening.instance?.home?.SetUpHome();
            yield break;
        }

        string winsRaw = ReadPropString(PhotonNetwork.CurrentRoom.CustomProperties, FriendKeys.WinsToTakeProperty);
        if (!string.IsNullOrEmpty(winsRaw) && int.TryParse(winsRaw, out int fromRoom) && fromRoom > 0)
        {
            winsToTake = fromRoom;
        }

        if (winsToTake < 1)
        {
            winsToTake = 1;
        }

        ContinuousController.instance.FriendWinsToTake = winsToTake;

        RememberChallengerFromRoom();
        FriendServices.EnsureExists().Director.ResetDirector();
        FriendServices.EnsureExists().Director.BeginSeriesFromRoom();

        if (Opening.instance?.battle?.roomManager != null)
        {
            Opening.instance.battle.roomManager.SetUpRoom();
        }
    }

    void RememberChallengerFromRoom()
    {
        if (!PhotonNetwork.InRoom)
        {
            return;
        }

        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        string id = ReadPropString(hash, FriendKeys.ChallengerUserIdProperty);
        string name = ReadPropString(hash, FriendKeys.ChallengerNameProperty);

        if (!string.IsNullOrEmpty(id))
        {
            var list = FriendServices.EnsureExists().List;
            if (!list.Contains(id))
            {
                FriendServices.EnsureExists().StartCoroutine(list.AddFriendById(id, name, null));
            }
        }
    }

    public void ChallengeFriend(string targetPlayFabId, string targetDisplayName, int winsToTake)
    {
        if (IsChallenging)
        {
            return;
        }

        StartCoroutine(ChallengeFriendCoroutine(targetPlayFabId, targetDisplayName, winsToTake));
    }

    IEnumerator ChallengeFriendCoroutine(string targetPlayFabId, string targetDisplayName, int winsToTake)
    {
        IsChallenging = true;
        _createFailed = false;

        targetPlayFabId = targetPlayFabId?.Trim();
        if (string.IsNullOrEmpty(targetPlayFabId))
        {
            IsChallenging = false;
            yield break;
        }

        yield return FriendServices.EnsureExists().List.EnsureLoggedIn();

        if (RankedServices.Instance != null)
        {
            yield return ContinuousController.instance.StartCoroutine(
                PhotonUtility.SetRankedPlayerProperties());
        }

        OnlinePlayerCountService.EnsureExists().SetMatchmakingOwnsConnection(true);

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            yield return ContinuousController.instance.StartCoroutine(
                PhotonUtility.ConnectToMasterServerCoroutine(matchmakingOwnsConnection: true));
        }

        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
            float lobbyWait = 0f;
            while (!PhotonNetwork.InLobby && PhotonNetwork.IsConnected && lobbyWait < 10f)
            {
                lobbyWait += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        if (!PhotonNetwork.InLobby)
        {
            Debug.LogWarning("[Friends] Not in lobby — cannot create challenge room");
            IsChallenging = false;
            OnlinePlayerCountService.EnsureExists().SetMatchmakingOwnsConnection(false);
            Opening.instance?.home?.SetUpHome();
            yield break;
        }

        string localId = FriendListService.LocalPlayFabId();
        string localName = ContinuousController.instance.PlayerName;

        ContinuousController.instance.isAI = false;
        ContinuousController.instance.isRandomMatch = false;
        ContinuousController.instance.isRanked = false;
        ContinuousController.instance.isTournament = false;
        ContinuousController.instance.isFriendDuel = true;
        ContinuousController.instance.FriendWinsToTake = winsToTake;

        SetInviteListening(false);
        StopPresencePolling();
        FriendListPanel.HideIfOpen();
        Opening.instance?.OffModeButtons();
        Opening.instance?.home?.OffHome();

        // Stable unique name (not shown as a 5-digit Room Match code).
        string roomName = FriendKeys.RoomNamePrefix + Guid.NewGuid().ToString("N").Substring(0, 12);

        var roomOptions = new RoomOptions
        {
            IsVisible = true,
            IsOpen = true,
            PublishUserId = true,
            MaxPlayers = 2,
            CustomRoomProperties = new Hashtable
            {
                { FriendKeys.ModeProperty, FriendKeys.ModeFriend },
                { FriendKeys.TargetUserIdProperty, targetPlayFabId },
                { FriendKeys.ChallengerUserIdProperty, localId ?? "" },
                { FriendKeys.ChallengerNameProperty, localName ?? "Player" },
                { FriendKeys.WinsToTakeProperty, winsToTake },
                { FriendKeys.UseBanlistProperty, ContinuousController.instance.useBanlist },
                { FriendKeys.SeriesWinsAProperty, 0 },
                { FriendKeys.SeriesWinsBProperty, 0 },
                { FriendKeys.GameIndexProperty, 0 },
                { FriendKeys.UserIdAProperty, localId ?? "" },
                { FriendKeys.UserIdBProperty, targetPlayFabId },
                { "RoomCreator", PhotonNetwork.NickName },
            },
            CustomRoomPropertiesForLobby = FriendKeys.LobbyProperties,
        };
        BattleReconnectService.ApplyBattleTtl(roomOptions);

        Debug.Log(
            $"[Friends] Creating challenge room={roomName} target={targetPlayFabId} " +
            $"local={localId} winsToTake={winsToTake}");

        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);

        float waited = 0f;
        while (!PhotonNetwork.InRoom && !_createFailed && waited < 15f)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!PhotonNetwork.InRoom)
        {
            CancelChallenge();
            Opening.instance?.home?.SetUpHome();
            yield break;
        }

        FriendServices.EnsureExists().Director.ResetDirector();
        FriendServices.EnsureExists().Director.BeginSeriesFromRoom();

        if (Opening.instance?.battle?.roomManager != null)
        {
            Opening.instance.battle.roomManager.SetUpRoom();
        }

        if (_inviteTimeout != null)
        {
            StopCoroutine(_inviteTimeout);
        }

        _inviteTimeout = StartCoroutine(InviteTimeoutCoroutine(targetDisplayName));
        IsChallenging = false;
    }

    IEnumerator InviteTimeoutCoroutine(string targetDisplayName)
    {
        float t = 0f;
        while (t < FriendKeys.InviteTimeoutSeconds)
        {
            if (!PhotonNetwork.InRoom)
            {
                yield break;
            }

            if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount >= 2)
            {
                yield break;
            }

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        Debug.Log($"[Friends] Invite timed out for {targetDisplayName}");
        CancelChallenge();
        if (Opening.instance?.battle?.roomManager != null)
        {
            Opening.instance.battle.roomManager.Off();
        }

        Opening.instance?.home?.SetUpHome();
        FriendListPanel.ShowFromHome();
    }

    public void CancelChallenge()
    {
        if (_inviteTimeout != null)
        {
            StopCoroutine(_inviteTimeout);
            _inviteTimeout = null;
        }

        IsChallenging = false;
        ContinuousController.instance?.ClearFriendDuel();

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        OnlinePlayerCountService.EnsureExists().SetMatchmakingOwnsConnection(false);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        _createFailed = true;
        Debug.LogWarning($"[Friends] CreateRoom failed: {returnCode} {message}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[Friends] JoinRoom failed: {returnCode} {message}");
    }

    public override void OnJoinedRoom()
    {
        StopPresencePolling();
        StopInviteWatch();

        if (_inviteTimeout != null && PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            StopCoroutine(_inviteTimeout);
            _inviteTimeout = null;
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (_inviteTimeout != null && PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            StopCoroutine(_inviteTimeout);
            _inviteTimeout = null;
        }
    }

    void OnDestroy()
    {
        DestroyInviteOverlay();
    }
}
