using DCGO.Networking;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using PhotonPlayer = Photon.Realtime.Player;
using PhotonRoomInfo = Photon.Realtime.RoomInfo;

public class PunNetworkProvider : MonoBehaviourPunCallbacks, INetworkProvider
{
    const string RandomMatchKey = "RandomMatchKey";
    const string RoomCreatorKey = "RoomCreator";
    const string UseBanListKey = "UseBanlist";
    const string RoomSettingPrefix = "RoomSetting-";
    const string ReadyKey = "IsReady";
    const string WaitingForGameStartKey = "WaitingForGameStart";
    const string DeckDataKey = "BattleDeckData";
    const int MaxConnectRetries = 5;

    private MatchmakingEvents _matchmakingEvents;
    private PrivateRoomEvents _privateRoomEvents;
    private GameNetworkEvents _gameEvents;

    private Coroutine _matchmakingCoroutine = null;
    private Coroutine _enteringPrivateRoomCoroutine = null;

    private bool _inPrivateRoom = false;
    private bool _inGame = false;
    private bool _gameStarted = false;

    private List<PhotonRoomInfo> _rooms = new List<PhotonRoomInfo>();
    private bool _receivedRoomList = false;
    private int _roomState = 0;

    private Player[] _players = null;
    private List<NetworkPlayerInfo> _playerInfos = new List<NetworkPlayerInfo>();

    private string _connectedRegion = null;
    private string _retryStatus = null;

    void Awake()
    {
        DCGONetwork.Provider = this;
    }

    void Update()
    {
        if (_inGame)
        {
            if (_gameStarted)
            {
                GameUpdate();
            }
        }
        else if (_inPrivateRoom)
        {
            PrivateRoomUpdate();
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1))
        {
            _showPhotonDebug = !_showPhotonDebug;
        }
#endif
    }

    #region Connection
    public bool IsConnected()
    {
        return PhotonNetwork.IsConnected;
    }

    public void Disconnect(Action onDisconnected)
    {
        StopAllCoroutines();

        _matchmakingCoroutine = null;
        _enteringPrivateRoomCoroutine = null;
        _inPrivateRoom = false;
        _inGame = false;
        _gameStarted = false;

        _matchmakingEvents = new MatchmakingEvents();
        _privateRoomEvents = new PrivateRoomEvents();
        _gameEvents = new GameNetworkEvents();

        StartCoroutine(DisconnectCoroutine(onDisconnected));
    }

    IEnumerator DisconnectCoroutine(Action onDisconnected)
    {
        yield return DisconnectCoroutine();

        if (onDisconnected != null)
        {
            onDisconnected();
        }
    }

    IEnumerator DisconnectCoroutine()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }

        yield return new WaitWhile(() => PhotonNetwork.IsConnected);

        SetLocalPlayerProperty(ReadyKey, false);
        SetLocalPlayerProperty(WaitingForGameStartKey, false);
    }

    IEnumerator ConnectCoroutine()
    {
        string region = ContinuousController.instance.serverRegion;

        if (PhotonNetwork.IsConnected && _connectedRegion != region)
        {
            yield return DisconnectCoroutine();
        }

        float retryDelay = 3f;

        for (int attempt = 0; attempt <= MaxConnectRetries && !PhotonNetwork.IsConnectedAndReady; attempt++)
        {
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.NetworkingClient.AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
                PhotonNetwork.NickName = ContinuousController.instance.PlayerName;
                PhotonNetwork.GameVersion = ContinuousController.instance.GameVerString;
                PhotonNetwork.ConnectToRegion(region);
                _connectedRegion = region;
            }

            yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady || PhotonNetwork.NetworkingClient.State == Photon.Realtime.ClientState.Disconnected);

            if (PhotonNetwork.IsConnectedAndReady)
            {
                break;
            }

            Photon.Realtime.DisconnectCause cause = PhotonNetwork.NetworkingClient.DisconnectedCause;
            Debug.LogWarning($"[Photon] Connection failed: {cause} (attempt {attempt + 1}/{MaxConnectRetries + 1})");

            if (cause != Photon.Realtime.DisconnectCause.MaxCcuReached || attempt == MaxConnectRetries)
            {
                break;
            }

            _retryStatus = LocalizeUtility.GetLocalizedString(
                EngMessage: $"Server full. Retrying... ({attempt + 1}/{MaxConnectRetries})",
                JpnMessage: $"サーバーが満員です。再接続中... ({attempt + 1}/{MaxConnectRetries})"
            );

            yield return new WaitForSeconds(retryDelay);
            retryDelay += 2f;
        }

        if (PhotonNetwork.IsConnectedAndReady)
        {
            _retryStatus = null;
        }
        else
        {
            _retryStatus = LocalizeUtility.GetLocalizedString(
                EngMessage: "Connection failed. Please try again later.",
                JpnMessage: "接続に失敗しました。後でもう一度お試しください。"
            );
            Debug.LogError("[Photon] Connection failed permanently");
        }
    }

    IEnumerator ConnectToLobbyCoroutine()
    {
        yield return ConnectCoroutine();

        if (PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        yield return new WaitUntil(() => PhotonNetwork.InLobby || !PhotonNetwork.IsConnected);
    }
    #endregion

    #region Matchmaking
    public void StartMatchmaking(MatchmakingEvents matchmakingEvents)
    {
        if (_matchmakingCoroutine != null)
        {
            return;
        }

        _matchmakingEvents = matchmakingEvents;
        _matchmakingCoroutine = StartCoroutine(MatchmakingCoroutine());
    }

    public void CancelMatchmaking()
    {
        if (_inGame)
        {
            return;
        }

        if (_matchmakingCoroutine != null)
        {
            StopCoroutine(_matchmakingCoroutine);
            _matchmakingCoroutine = null;
        }

        StartCoroutine(CancelMatchmakingCoroutine());
    }

    IEnumerator MatchmakingCoroutine()
    {
        yield return DisconnectCoroutine();

        _rooms.Clear();
        _receivedRoomList = false;

        yield return ConnectToLobbyCoroutine();

        bool connected = PhotonNetwork.InLobby;

        if (_matchmakingEvents.OnConnected != null)
        {
            _matchmakingEvents.OnConnected(connected);
        }

        if (!connected)
        {
            _matchmakingCoroutine = null;
            yield break;
        }

        yield return new WaitUntil(() => _receivedRoomList || !PhotonNetwork.IsConnected);

        if (PhotonNetwork.IsConnected)
        {
            CreateOrJoinRandomMatch();
        }

        yield return new WaitUntil(() => IsRoomFull() || !PhotonNetwork.IsConnected);

        _matchmakingCoroutine = null;

        if (!PhotonNetwork.IsConnected)
        {
            if (_matchmakingEvents.OnDisconnected != null)
            {
                _matchmakingEvents.OnDisconnected();
            }

            yield break;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }

        if (_matchmakingEvents.OnMatchFound != null)
        {
            _matchmakingEvents.OnMatchFound();
        }
    }

    IEnumerator CancelMatchmakingCoroutine()
    {
        yield return DisconnectCoroutine();

        MatchmakingEvents matchmakingEvents = _matchmakingEvents;
        _matchmakingEvents = new MatchmakingEvents();

        if (matchmakingEvents.OnCanceled != null)
        {
            matchmakingEvents.OnCanceled();
        }
    }

    void CreateOrJoinRandomMatch()
    {
        List<PhotonRoomInfo> matchmakingRooms = GetMatchmakingRooms();

        if (matchmakingRooms.Count == 0)
        {
            CreateRoom(true, true);
        }
        else
        {
            PhotonNetwork.JoinRoom(matchmakingRooms[UnityEngine.Random.Range(0, matchmakingRooms.Count)].Name);
        }
    }

    IEnumerator RetryRandomMatchCoroutine()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.IsConnected);

        if (_matchmakingCoroutine != null && PhotonNetwork.IsConnectedAndReady)
        {
            CreateRoom(true, true);
        }
    }

    List<PhotonRoomInfo> GetMatchmakingRooms()
    {
        List<PhotonRoomInfo> matchmakingRooms = new List<PhotonRoomInfo>();

        foreach (PhotonRoomInfo room in _rooms)
        {
            if (room.PlayerCount != 0 && room.MaxPlayers != 0 && room.CustomProperties[RoomCreatorKey] != null && room.Name.Contains(RandomMatchKey))
            {
                matchmakingRooms.Add(room);
            }
        }

        return matchmakingRooms;
    }

    bool IsRoomFull()
    {
        return PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers;
    }
    #endregion

    #region Private Room
    public void CreatePrivateRoom(bool useBanList, PrivateRoomEvents privateRoomEvents)
    {
        if (_enteringPrivateRoomCoroutine != null)
        {
            return;
        }

        _privateRoomEvents = privateRoomEvents;
        _enteringPrivateRoomCoroutine = StartCoroutine(CreatePrivateRoomCoroutine(useBanList));
    }

    public void JoinPrivateRoom(string roomCode, PrivateRoomEvents privateRoomEvents)
    {
        if (_enteringPrivateRoomCoroutine != null)
        {
            return;
        }

        _privateRoomEvents = privateRoomEvents;
        _enteringPrivateRoomCoroutine = StartCoroutine(JoinPrivateRoomCoroutine(roomCode));
    }

    IEnumerator CreatePrivateRoomCoroutine(bool useBanList)
    {
        yield return DisconnectCoroutine();
        yield return ConnectToLobbyCoroutine();

        if (PhotonNetwork.InLobby)
        {
            CreateRoom(false, useBanList);
        }

        yield return WaitForPrivateRoomCoroutine();
    }

    IEnumerator JoinPrivateRoomCoroutine(string roomCode)
    {
        yield return ConnectToLobbyCoroutine();

        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinRoom(roomCode);
        }

        yield return WaitForPrivateRoomCoroutine();
    }

    IEnumerator WaitForPrivateRoomCoroutine()
    {
        yield return new WaitUntil(() => PhotonNetwork.InRoom || !PhotonNetwork.IsConnected);

        OnPrivateRoomEntered(PhotonNetwork.InRoom);
    }

    void OnPrivateRoomEntered(bool success)
    {
        _enteringPrivateRoomCoroutine = null;
        _inPrivateRoom = success;
        _roomState = 0;

        if (_privateRoomEvents.OnJoined != null)
        {
            _privateRoomEvents.OnJoined(success);
        }
    }

    public void LeavePrivateRoom()
    {
        if (_enteringPrivateRoomCoroutine != null)
        {
            StopCoroutine(_enteringPrivateRoomCoroutine);
            _enteringPrivateRoomCoroutine = null;
        }

        _inPrivateRoom = false;

        StartCoroutine(LeavePrivateRoomCoroutine());
    }

    IEnumerator LeavePrivateRoomCoroutine()
    {
        yield return DisconnectCoroutine();

        PrivateRoomEvents privateRoomEvents = _privateRoomEvents;
        _privateRoomEvents = new PrivateRoomEvents();

        if (privateRoomEvents.OnLeft != null)
        {
            privateRoomEvents.OnLeft();
        }
    }

    public void SetRoomReadyState(bool isReady)
    {
        SetLocalPlayerProperty(ReadyKey, isReady);
    }

    public void SetRoomSetting(string settingsKey, int value)
    {
        if (!PhotonNetwork.InRoom)
        {
            return;
        }

        Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        roomProperties[RoomSettingPrefix + settingsKey] = value;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
    }

    void PrivateRoomUpdate()
    {
        if (!PhotonNetwork.InRoom)
        {
            _inPrivateRoom = false;

            if (_privateRoomEvents.OnDisconnected != null)
            {
                _privateRoomEvents.OnDisconnected();
            }

            return;
        }

        RoomInfo roomInfo = GetRoomInfo();
        int roomState = GetRoomState(roomInfo);

        if (roomState != _roomState)
        {
            _roomState = roomState;

            if (_privateRoomEvents.OnRoomInfoUpdated != null)
            {
                _privateRoomEvents.OnRoomInfoUpdated(roomInfo);
            }
        }

        bool allPlayersReady = roomInfo.Players.Count == PhotonNetwork.CurrentRoom.MaxPlayers;

        foreach (NetworkPlayerInfo playerInfo in roomInfo.Players)
        {
            allPlayersReady = allPlayersReady && playerInfo.IsReady;
        }

        if (allPlayersReady && _privateRoomEvents.OnAllPlayersReady != null)
        {
            _privateRoomEvents.OnAllPlayersReady();
        }
    }

    int GetRoomState(RoomInfo roomInfo)
    {
        HashCode roomState = new HashCode();
        roomState.Add(roomInfo.RoomCode);
        roomState.Add(roomInfo.Players.Count);

        foreach (NetworkPlayerInfo playerInfo in roomInfo.Players)
        {
            roomState.Add(playerInfo.Name);
            roomState.Add(playerInfo.IsReady);
        }

        foreach (KeyValuePair<string, int> setting in roomInfo.RoomSettings)
        {
            roomState.Add(setting.Key);
            roomState.Add(setting.Value);
        }

        return roomState.ToHashCode();
    }
    #endregion

    #region Room
    public bool IsInRoom()
    {
        return PhotonNetwork.InRoom;
    }

    public bool IsRoomHost()
    {
        return PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient;
    }

    public RoomInfo GetRoomInfo()
    {
        RoomInfo roomInfo = new RoomInfo();
        roomInfo.Players = new List<NetworkPlayerInfo>();
        roomInfo.RoomSettings = new Dictionary<string, int>();

        if (!PhotonNetwork.InRoom)
        {
            return roomInfo;
        }

        roomInfo.RoomCode = PhotonNetwork.CurrentRoom.Name[..5];

        foreach (PhotonPlayer player in PhotonNetwork.PlayerList)
        {
            roomInfo.Players.Add(CollectPlayerInfo(player));
        }

        foreach (var property in PhotonNetwork.CurrentRoom.CustomProperties)
        {
            string key = property.Key.ToString();

            if (key.StartsWith(RoomSettingPrefix) && property.Value is int value)
            {
                roomInfo.RoomSettings[key.Substring(RoomSettingPrefix.Length)] = value;
            }
        }

        return roomInfo;
    }

    public int GetRoomSetting(string settingsKey, int defaultValue)
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(RoomSettingPrefix + settingsKey, out object value) && value is int setting)
        {
            return setting;
        }

        return defaultValue;
    }

    public void SetDeckData(DeckData deckData)
    {
        SetLocalPlayerProperty(DeckDataKey, deckData != null ? deckData.GetThisDeckCode() : null);
    }

    NetworkPlayerInfo CollectPlayerInfo(PhotonPlayer player)
    {
        NetworkPlayerInfo playerInfo = new NetworkPlayerInfo();
        playerInfo.Name = player.NickName;
        playerInfo.GameID = player.IsMasterClient ? 0 : 1;
        playerInfo.IsReady = GetPlayerProperty(player, ReadyKey);

        if (player.CustomProperties.TryGetValue(DeckDataKey, out object value) && value is string deckCode)
        {
            playerInfo.DeckData = new DeckData(deckCode);
        }

        return playerInfo;
    }

    bool GetPlayerProperty(PhotonPlayer player, string key)
    {
        return player.CustomProperties.TryGetValue(key, out object value) && value is bool flag && flag;
    }

    void SetLocalPlayerProperty(string key, object value)
    {
        Hashtable playerProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        playerProperties[key] = value;

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        }
    }

    void CreateRoom(bool isRandomMatch, bool useBanList)
    {
        Photon.Realtime.RoomOptions roomOptions = new Photon.Realtime.RoomOptions
        {
            IsVisible = true,
            IsOpen = true,
            PublishUserId = true,
            MaxPlayers = 2,
            CustomRoomProperties = new Hashtable()
            {
                { RoomCreatorKey, PhotonNetwork.NickName },
                { UseBanListKey, useBanList }
            },
            CustomRoomPropertiesForLobby = new string[] { RoomCreatorKey, UseBanListKey }
        };

        string roomName = RandomString.Create_Num(isRandomMatch ? 8 : 5);
        roomName += isRandomMatch ? RandomMatchKey : "-" + useBanList;

        PhotonNetwork.CreateRoom(roomName, roomOptions, null);
    }

    public override void OnRoomListUpdate(List<PhotonRoomInfo> roomList)
    {
        foreach (PhotonRoomInfo room in roomList)
        {
            _rooms.RemoveAll(r => r.Name == room.Name);

            if (!room.RemovedFromList)
            {
                _rooms.Add(room);
            }
        }

        _receivedRoomList = true;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        OnEnterRoomFailed(returnCode, message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        OnEnterRoomFailed(returnCode, message);
    }

    void OnEnterRoomFailed(short returnCode, string message)
    {
        if (_enteringPrivateRoomCoroutine != null)
        {
            Debug.Log($"[Private Room] Entering room failed: [{returnCode}] {message}");
            StopCoroutine(_enteringPrivateRoomCoroutine);
            OnPrivateRoomEntered(false);
        }
        else if (_matchmakingCoroutine != null)
        {
            Debug.Log($"[Random Match] Entering room failed: [{returnCode}] {message}, retrying...");
            StartCoroutine(RetryRandomMatchCoroutine());
        }
    }
    #endregion

    #region Game
    public void CreateGame(GameContext gameContext, GameNetworkEvents gameNetworkEvents)
    {
        _inGame = true;
        _gameStarted = false;
        _gameEvents = gameNetworkEvents;
        _players = gameContext.Players.ToArray();
        _playerInfos = GetRoomInfo().Players;

        if (IsRoomHost())
        {
            StartCoroutine(WaitForPlayersCoroutine());
        }

        SetLocalPlayerProperty(WaitingForGameStartKey, true);
    }

    IEnumerator WaitForPlayersCoroutine()
    {
        yield return new WaitUntil(() =>
        {
            foreach (PhotonPlayer player in PhotonNetwork.PlayerList)
            {
                if (!GetPlayerProperty(player, WaitingForGameStartKey))
                {
                    return false;
                }
            }

            return true;
        });

        photonView.RPC("StartGame_Internal", RpcTarget.All);
    }

    [PunRPC]
    void StartGame_Internal()
    {
        SetLocalPlayerProperty(WaitingForGameStartKey, false);
        SetLocalPlayerProperty(ReadyKey, false);

        _gameStarted = true;

        if (_gameEvents.OnGameStarted != null)
        {
            _gameEvents.OnGameStarted();
        }
    }

    public NetworkPlayerInfo GetPlayerInfo(Player player)
    {
        foreach (NetworkPlayerInfo playerInfo in _playerInfos)
        {
            if (playerInfo.GameID == player.PlayerID)
            {
                return playerInfo;
            }
        }

        return new NetworkPlayerInfo();
    }

    Player GetPlayerFromID(int playerID)
    {
        if (_players == null)
        {
            return null;
        }

        foreach (Player player in _players)
        {
            if (player.PlayerID == playerID)
            {
                return player;
            }
        }

        return null;
    }

    public void SetGameSeed(long gameSeed)
    {
        photonView.RPC("SetGameSeed_Internal", RpcTarget.All, gameSeed);
    }

    [PunRPC]
    void SetGameSeed_Internal(long gameSeed)
    {
        if (_gameEvents.OnGameSeed != null)
        {
            _gameEvents.OnGameSeed(gameSeed);
        }
    }

    public void SendMainPhaseAction(Player player, MainPhaseAction action)
    {
        photonView.RPC("QueueMainPhaseAction_Internal", RpcTarget.All, player.PlayerID, GamePacketFactory.GetId(action.GetType()), action.Serialize());
    }

    [PunRPC]
    void QueueMainPhaseAction_Internal(int playerID, byte packetId, byte[] bytes)
    {
        Player player = GetPlayerFromID(playerID);

        if (player != null && _gameEvents.OnMainPhaseAction != null)
        {
            _gameEvents.OnMainPhaseAction(player, GamePacketFactory.Create(packetId, bytes) as MainPhaseAction);
        }
    }

    public void SendPlayerSelection(Player player, IPlayerSelection playerSelection)
    {
        photonView.RPC("QueuePlayerSelection_Internal", RpcTarget.All, player.PlayerID, GamePacketFactory.GetId(playerSelection.GetType()), playerSelection.Serialize());
    }

    [PunRPC]
    void QueuePlayerSelection_Internal(int playerID, byte packetId, byte[] bytes)
    {
        Player player = GetPlayerFromID(playerID);

        if (player != null && _gameEvents.OnPlayerSelection != null)
        {
            _gameEvents.OnPlayerSelection(player, GamePacketFactory.Create(packetId, bytes) as IPlayerSelection);
        }
    }

    public void SendSurrender(Player player)
    {
        photonView.RPC("SendSurrender_Internal", RpcTarget.All, player.PlayerID);
    }

    [PunRPC]
    void SendSurrender_Internal(int playerID)
    {
        if (_gameEvents.OnSurrender != null)
        {
            _gameEvents.OnSurrender(GetPlayerFromID(playerID));
        }
    }

    void GameUpdate()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            _gameStarted = false;

            if (_gameEvents.OnDisconnected != null)
            {
                _gameEvents.OnDisconnected();
            }
        }
    }

    public void EndGame()
    {
        _inGame = false;
        _gameStarted = false;
        _gameEvents = new GameNetworkEvents();
        _players = null;
        _playerInfos = new List<NetworkPlayerInfo>();

        if (!_inPrivateRoom)
        {
            StartCoroutine(DisconnectCoroutine());
        }
    }
    #endregion

#if UNITY_EDITOR
    #region Photon Debug HUD
    bool _showPhotonDebug = false;
    PhotonDebugListener _debugListener = null;

    public override void OnEnable()
    {
        base.OnEnable();

        _debugListener = new PhotonDebugListener();
        PhotonNetwork.AddCallbackTarget(_debugListener);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        if (_debugListener != null)
        {
            PhotonNetwork.RemoveCallbackTarget(_debugListener);
            _debugListener = null;
        }
    }

    class PhotonDebugListener : Photon.Realtime.IConnectionCallbacks, Photon.Realtime.IMatchmakingCallbacks, Photon.Realtime.ILobbyCallbacks
    {
        public void OnConnected() { }
        public void OnConnectedToMaster() { Debug.Log("[Photon Debug] Connected to Master"); }
        public void OnDisconnected(Photon.Realtime.DisconnectCause cause) { Debug.LogWarning($"[Photon Debug] Disconnected: {cause}"); }
        public void OnRegionListReceived(Photon.Realtime.RegionHandler handler) { }
        public void OnCustomAuthenticationResponse(Dictionary<string, object> data) { }
        public void OnCustomAuthenticationFailed(string debugMessage) { Debug.LogError($"[Photon Debug] Auth failed: {debugMessage}"); }

        public void OnJoinedLobby() { Debug.Log("[Photon Debug] Joined Lobby"); }
        public void OnLeftLobby() { Debug.Log("[Photon Debug] Left Lobby"); }
        public void OnLobbyStatisticsUpdate(List<Photon.Realtime.TypedLobbyInfo> lobbyStatistics) { }
        public void OnRoomListUpdate(List<PhotonRoomInfo> roomList) { }

        public void OnFriendListUpdate(List<Photon.Realtime.FriendInfo> friendList) { }
        public void OnCreatedRoom() { Debug.Log("[Photon Debug] Created Room"); }
        public void OnCreateRoomFailed(short returnCode, string message) { Debug.LogError($"[Photon Debug] Create Room FAILED: [{returnCode}] {message}"); }
        public void OnJoinedRoom() { Debug.Log($"[Photon Debug] Joined Room: {PhotonNetwork.CurrentRoom?.Name}"); }
        public void OnJoinRoomFailed(short returnCode, string message) { Debug.LogError($"[Photon Debug] Join Room FAILED: [{returnCode}] {message}"); }
        public void OnJoinRandomFailed(short returnCode, string message) { Debug.LogError($"[Photon Debug] Join Random FAILED: [{returnCode}] {message}"); }
        public void OnLeftRoom() { Debug.Log("[Photon Debug] Left Room"); }
    }

    void OnGUI()
    {
        if (!string.IsNullOrEmpty(_retryStatus))
        {
            GUIStyle retryStyle = new GUIStyle(GUI.skin.label);
            retryStyle.fontSize = 20;
            retryStyle.richText = true;
            retryStyle.fontStyle = FontStyle.Bold;
            retryStyle.alignment = TextAnchor.MiddleCenter;

            float boxW = 500;
            float boxH = 40;
            float x = (Screen.width - boxW) / 2;
            float y = (Screen.height - boxH) / 2;

            GUI.Box(new Rect(x - 10, y - 10, boxW + 20, boxH + 20), "");
            GUI.Label(new Rect(x, y, boxW, boxH), $"<color=#FFAA00>{_retryStatus}</color>", retryStyle);
        }

        if (!_showPhotonDebug)
        {
            return;
        }

        string status;

        if (!PhotonNetwork.IsConnected)
        {
            status = "<color=#FF4444>[Photon] Disconnected</color>";
        }
        else if (PhotonNetwork.InRoom)
        {
            status = $"<color=#44FF44>[Photon] Connected | Region: {PhotonNetwork.CloudRegion} | Room: {PhotonNetwork.CurrentRoom.Name} ({PhotonNetwork.CurrentRoom.PlayerCount} players)</color>";
        }
        else if (PhotonNetwork.InLobby)
        {
            status = $"<color=#44FF44>[Photon] Connected | Region: {PhotonNetwork.CloudRegion} | In Lobby</color>";
        }
        else
        {
            status = $"<color=#FFAA00>[Photon] Connected | Region: {PhotonNetwork.CloudRegion} | Not in Lobby/Room</color>";
        }

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 16;
        style.richText = true;
        style.fontStyle = FontStyle.Bold;

        GUI.Label(new Rect(10, 10, 800, 30), status, style);
    }
    #endregion
#endif
}
