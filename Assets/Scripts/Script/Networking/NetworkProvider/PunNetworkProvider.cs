using DCGO.Networking;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PunNetworkProvider : MonoBehaviourPunCallbacks, INetworkProvider
{
    Player[] _players = null;

    private MatchmakingEvents _matchmakingEvents;
    private GameNetworkEvents _gameEvents;
    private PrivateRoomEvents _privateRoomEvents;
    private Action<bool> _onPrivateRoomJoined = null;
    private Action _onPrivateRoomLeft = null;

    private GameState _gameState = GameState.Menu;

    private DCGO.Networking.RoomInfo _roomInfo;

    const string RandomMatchKey = "RandomMatchKey";
    const string ReadyKey = "IsReady";
    const string WaitingForGameStartKey = "WaitingForGameStart";

    List<Photon.Realtime.RoomInfo> _rooms = new List<Photon.Realtime.RoomInfo>();
    List<Photon.Realtime.RoomInfo> _matchmakingRooms = null;

    Coroutine _matchmakingCoroutine = null;
    Coroutine _enteringPrivateRoomCoroutine = null;

    public void Awake()
    {
        DCGONetwork.Provider = this;
    }

    public void Initialise()
    {
        _gameState = GameState.Menu;
    }

    #region Matchmaking

    public void StartMatchmaking(MatchmakingEvents MatchmakingEvents)
    {
        if (_gameState != GameState.Menu)
        {
            return;
        }

        _matchmakingEvents = MatchmakingEvents;
        _gameState = GameState.Matchmaking;
        _matchmakingCoroutine = StartCoroutine(MatchmakingCoroutine());
    }

    public void CancelMatchmaking()
    {
        if (_gameState != GameState.Matchmaking)
        {
            return;
        }

        StartCoroutine(CancelMatchmakingCoroutine());
    }

    IEnumerator MatchmakingCoroutine()
    {
        _matchmakingRooms = null;

        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        yield return new WaitWhile(() => PhotonNetwork.InLobby);

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }

        yield return new WaitWhile(() => PhotonNetwork.IsConnected);

        yield return ContinuousController.instance.StartCoroutine(PhotonUtility.ConnectToLobbyCoroutine());

        if (_matchmakingEvents.OnConnected != null)
        {
            _matchmakingEvents.OnConnected(true);
        }

        yield return new WaitWhile(() => _matchmakingRooms == null);

        CreateOrJoinRandomMatch();

        yield return new WaitWhile(() => !PhotonNetwork.InRoom);

        yield return new WaitWhile(() => PhotonNetwork.CurrentRoom.PlayerCount != PhotonNetwork.CurrentRoom.MaxPlayers);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }

        if (_matchmakingEvents.OnMatchFound != null)
        {
            _matchmakingEvents.OnMatchFound();
        }
    }

    void CreateOrJoinRandomMatch()
    {
        //If there is no room, make room.
        if (_matchmakingRooms == null || _matchmakingRooms.Count == 0)
        {
            StartCoroutine(CreateRoomCoroutine(true, true));
        }

        //If there's room, I'll go in.
        else
        {
            PhotonNetwork.JoinRoom(_matchmakingRooms[UnityEngine.Random.Range(0, _matchmakingRooms.Count)].Name);
        }
    }

    IEnumerator CancelMatchmakingCoroutine()
    {
        StopCoroutine(_matchmakingCoroutine);

        SetWaitingForGameStartProperty(PhotonNetwork.LocalPlayer, false);
        SetReadyProperty(PhotonNetwork.LocalPlayer, false);

        yield return PhotonUtility.DisconnectCoroutine();

        _gameState = GameState.Menu;

        if (_matchmakingEvents.OnCanceled != null)
        {
            _matchmakingEvents.OnCanceled();
        } 
    }    
    #endregion

    #region Private Room
    public void CreatePrivateRoom(bool useBanList, Action<bool> onPrivateRoomJoined) 
    {
        if (_gameState != GameState.Menu)
        {
            return;
        }

        _onPrivateRoomJoined = onPrivateRoomJoined;
        _gameState = GameState.EnteringPrivateRoom;
        _enteringPrivateRoomCoroutine = StartCoroutine(CreatePrivateRoomCoroutine(useBanList));

    }
    public IEnumerator CreatePrivateRoomCoroutine(bool useBanList)
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        yield return new WaitWhile(() => PhotonNetwork.InLobby);

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }

        yield return new WaitWhile(() => PhotonNetwork.IsConnected);

        yield return ContinuousController.instance.StartCoroutine(PhotonUtility.ConnectToLobbyCoroutine());

        yield return StartCoroutine(CreateRoomCoroutine(false, useBanList));

        yield return new WaitWhile(() => !PhotonNetwork.InRoom);

        PrivateRoomUpdate();

        _gameState = GameState.PrivateRoom;

        if (_onPrivateRoomJoined != null)
        {
            _onPrivateRoomJoined(true);
        }
    }

    public void JoinPrivateRoom(string roomCode, Action<bool> onPrivateRoomJoined)
    {
        if (_gameState != GameState.Menu)
        {
            return;
        }

        _onPrivateRoomJoined = onPrivateRoomJoined;
        _gameState = GameState.EnteringPrivateRoom;
        _enteringPrivateRoomCoroutine = StartCoroutine(JoinRoomCoroutine(roomCode));
    }

    IEnumerator JoinRoomCoroutine(string roomCode)
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            yield return ContinuousController.instance.StartCoroutine(PhotonUtility.ConnectToMasterServerCoroutine());
        }

        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);

        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        yield return new WaitUntil(() => PhotonNetwork.InLobby && PhotonNetwork.IsConnectedAndReady);

        PhotonNetwork.JoinRoom(roomCode);

        yield return new WaitWhile(() => !PhotonNetwork.InRoom);

        _gameState = GameState.PrivateRoom;

        if (_onPrivateRoomJoined != null)
        {
            _onPrivateRoomJoined(true);
        }
    }

    public void OnPrivateRoomEvents(PrivateRoomEvents privateRoomEvents)
    {
        _privateRoomEvents = privateRoomEvents;
        PrivateRoomUpdate(true);
    }

    public void SetRoomReadyState(bool isReady)
    {
        SetReadyProperty(PhotonNetwork.LocalPlayer, isReady);
    }

    void SetReadyProperty(Photon.Realtime.Player player, bool ready)
    {
        Hashtable PlayerProp = player.CustomProperties;

        if (PlayerProp.TryGetValue(ReadyKey, out object _))
        {
            PlayerProp[ReadyKey] = ready;
        }

        else
        {
            PlayerProp.Add(ReadyKey, ready);
        }

        player.SetCustomProperties(PlayerProp);
    }

    void SetWaitingForGameStartProperty(Photon.Realtime.Player player, bool waitingForGameStart)
    {
        Hashtable PlayerProp = player.CustomProperties;

        if (PlayerProp.TryGetValue(WaitingForGameStartKey, out object _))
        {
            PlayerProp[WaitingForGameStartKey] = waitingForGameStart;
        }

        else
        {
            PlayerProp.Add(WaitingForGameStartKey, waitingForGameStart);
        }

        player.SetCustomProperties(PlayerProp);
    }

    public void SetDeckData(DeckData deckData)
    {
        PhotonUtility.SetDeckData(deckData);
    }

    public void SetRoomSetting(string settingsKey, int value)
    {
        Hashtable RoomProp = PhotonNetwork.CurrentRoom.CustomProperties;

        if (RoomProp.TryGetValue("RoomSetting-" + settingsKey, out object _))
        {
            RoomProp["RoomSetting-" + settingsKey] = value;
        }

        else
        {
            RoomProp.Add("RoomSetting-" + settingsKey, value);
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(RoomProp);
    }

    public int GetRoomSetting(string setting)
    {
        Hashtable RoomProp = PhotonNetwork.CurrentRoom.CustomProperties;

        if (RoomProp.TryGetValue("RoomSetting-" + setting, out object _))
        {
            if (RoomProp["RoomSetting-" + setting] is int settingAsInt)
            {
                return settingAsInt;
            }
        }

        return 0;
    }

    void PrivateRoomUpdate(bool forceSend = false)
    {
        int GetState(DCGO.Networking.RoomInfo roomInfo)
        {
            HashCode result = new HashCode();
            result.Add(roomInfo.Players != null ? roomInfo.Players.Count : 0);

            int ReadyCount = 0;
            if (roomInfo.Players != null)
            {
                foreach (var player in roomInfo.Players)
                {
                    ReadyCount += player.IsReady ? 1 : 0;
                }
            }
            result.Add(ReadyCount);

            int settingsTotalValue = 0;
            if (roomInfo.RoomSettings != null)
            {
                foreach (var setting in roomInfo.RoomSettings)
                {
                    settingsTotalValue += setting.Value;
                }
            }
            result.Add(settingsTotalValue);

            return result.ToHashCode();
        }

        int oldState = GetState(_roomInfo);

        string RoomName = PhotonNetwork.CurrentRoom.Name;
        _roomInfo.RoomCode = RoomName[..5];

        _roomInfo.Players = new List<NetworkPlayerInfo>();
        bool allPlayersReady = PhotonNetwork.PlayerList.Length == PhotonNetwork.CurrentRoom.MaxPlayers;

        foreach (var player in PhotonNetwork.PlayerList)
        {
            NetworkPlayerInfo playerInfo = CollectPlayerInfo(player);
            allPlayersReady = allPlayersReady && playerInfo.IsReady;
            _roomInfo.Players.Add(playerInfo);
        }

        _roomInfo.RoomSettings = new Dictionary<string, int>();
        Hashtable RoomProp = PhotonNetwork.CurrentRoom.CustomProperties;

        foreach (var Prop in RoomProp)
        {
            string key = Prop.Key.ToString();
            if (key.Contains("RoomSetting-") && Prop.Value is int value)
            {
                _roomInfo.RoomSettings.Add(key.Split("-")[1], value);
            }
        }

        if (_privateRoomEvents.OnRoomInfoUpdated != null && (forceSend || oldState != GetState(_roomInfo)))
        {
            _privateRoomEvents.OnRoomInfoUpdated.Invoke(_roomInfo);
        }

        if (allPlayersReady && _privateRoomEvents.OnAllPlayersReady != null)
        {
            _privateRoomEvents.OnAllPlayersReady.Invoke();
        }
    }

    NetworkPlayerInfo CollectPlayerInfo(Photon.Realtime.Player player)
    {
        NetworkPlayerInfo playerInfo = new NetworkPlayerInfo();
        playerInfo.Name = player.NickName;

        playerInfo.GameID = player.IsMasterClient ? 0 : 1;

        if (player.CustomProperties.TryGetValue(ReadyKey, out object readyValue))
        {
            playerInfo.IsReady = (bool)readyValue;
        }

        if (player.CustomProperties.TryGetValue(WaitingForGameStartKey, out object waitingForGameStart))
        {
            playerInfo.WaitingForGameStart = (bool)waitingForGameStart;
        }

        if (player.CustomProperties.TryGetValue(ContinuousController.DeckDataPropertyKey, out object deckValue))
        {
            playerInfo.DeckData = new DeckData((string)deckValue);
        }

        return playerInfo;
    }

    public void LeavePrivateRoom(Action onPrivateRoomLeft) 
    {
        _onPrivateRoomLeft = onPrivateRoomLeft;
        StartCoroutine(LeaveRoomCoroutine());
    }

    IEnumerator LeaveRoomCoroutine()
    {
        yield return PhotonUtility.DisconnectCoroutine();

        _gameState = GameState.Menu;

        SetWaitingForGameStartProperty(PhotonNetwork.LocalPlayer, false);
        SetReadyProperty(PhotonNetwork.LocalPlayer, false);

        if (_onPrivateRoomLeft != null)
        {
            _onPrivateRoomLeft.Invoke();
        }
    }
    #endregion

    #region Room

    public bool IsRoomHost()
    {
        return PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient;
    }

    public override void OnRoomListUpdate(List<Photon.Realtime.RoomInfo> roomList)
    {
        foreach (Photon.Realtime.RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                _rooms.RemoveAll(r => r.Name == info.Name);
            }
            else if (!_rooms.Any(r => r.Name == info.Name))
            {
                _rooms.Add(info);
            }
            else
            {
                int index = _rooms.FindIndex(r => r.Name == info.Name);
                _rooms[index] = info;
            }
        }

        GetRandomMatcingRoom(_rooms);
    }

    public void GetRandomMatcingRoom(List<Photon.Realtime.RoomInfo> roomInfo)
    {
        _matchmakingRooms = new List<Photon.Realtime.RoomInfo>();

        if (roomInfo == null || roomInfo.Count == 0)
        {
            return;
        }

        for (int i = 0; i < roomInfo.Count; i++)
        {
            int p = roomInfo[i].PlayerCount;
            string n = roomInfo[i].Name;
            int m = roomInfo[i].MaxPlayers;
            object c = roomInfo[i].CustomProperties["RoomCreator"];

            if (p != 0 && m != 0 && c != null)
            {
                if (n.Contains(RandomMatchKey))
                {
                    _matchmakingRooms.Add(roomInfo[i]);
                }
            }
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        if (this.gameObject.activeSelf && _gameState == GameState.EnteringPrivateRoom)
        {
            StopCoroutine(_enteringPrivateRoomCoroutine);
            if (_onPrivateRoomJoined != null)
            {
                _onPrivateRoomJoined(false);
            }
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (_gameState == GameState.EnteringPrivateRoom)
        {
            Debug.Log($"[Private Room] Join room failed: [{returnCode}] {message}");
            StopCoroutine(_enteringPrivateRoomCoroutine);
            if (_onPrivateRoomJoined != null)
            {
                _onPrivateRoomJoined(false);
            }
        }
        else if (_gameState == GameState.Matchmaking)
        {
            Debug.Log($"[Random Match] Join room failed: [{returnCode}] {message}, retrying...");
            _matchmakingRooms = null;

            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }

            CreateOrJoinRandomMatch();
        }
    }

    public IEnumerator CreateRoomCoroutine(bool isRandomMatch, bool useBanList)
    {
        yield return new WaitWhile(() => !PhotonNetwork.IsConnectedAndReady);
        yield return new WaitWhile(() => !PhotonNetwork.InLobby);

        //Setting up the room to be created
        RoomOptions roomOptions = new RoomOptions
        {
            IsVisible = true,   //Make the room visible in the lobby.
            IsOpen = true,      //Allow other players to enter the room
            PublishUserId = true,

            MaxPlayers = 2,

            //To display room creator in room custom properties, store creator's name
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
            {
                { "RoomCreator",PhotonNetwork.NickName },
                { "UseBanlist", useBanList }

            },

            //Display custom property information in the lobby
            CustomRoomPropertiesForLobby = new string[]
            {
                "RoomCreator",
                "UseBanlist"
            }
        };

        string RoomName = RandomString.Create_Num(isRandomMatch ? 8 : 5);

        if (isRandomMatch)
        {
            RoomName += RandomMatchKey;
        }
        else
        {
            RoomName += "-" + useBanList;
        }

        //Room Creation
        PhotonNetwork.CreateRoom(RoomName, roomOptions, null);

        while (!PhotonNetwork.InRoom)
        {
            yield return null;
        }
    }
    #endregion

    #region Game
    public void CreateGame(GameContext gameContext, GameNetworkEvents gameNetworkEvents)
    {
        if (_gameState == GameState.Matchmaking)
        {
            _gameState = GameState.InMatchmakingGame;
        }
        else if (_gameState == GameState.PrivateRoom)
        {
            _gameState = GameState.InPrivateRoomGame;
        }

        _roomInfo.Players.Clear();
        foreach (var player in PhotonNetwork.PlayerList)
        {
            NetworkPlayerInfo playerInfo = CollectPlayerInfo(player);
            _roomInfo.Players.Add(playerInfo);
        }

        _gameEvents = gameNetworkEvents;
        _players = gameContext.Players.ToArray();

        if (IsRoomHost())
        {
            StartCoroutine(WaitingToStartGameCoroutine());
        }

        SetWaitingForGameStartProperty(PhotonNetwork.LocalPlayer, true);
    }

    public IEnumerator WaitingToStartGameCoroutine()
    {
        yield return new WaitUntil(() => 
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                NetworkPlayerInfo playerInfo = CollectPlayerInfo(player);
                if (!playerInfo.WaitingForGameStart)
                {
                    return false;
                }
            }

            return true;
        });

        photonView.RPC("StartGame", RpcTarget.All);
    }

    [PunRPC]
    public void StartGame()
    {
        SetWaitingForGameStartProperty(PhotonNetwork.LocalPlayer, false);
        SetReadyProperty(PhotonNetwork.LocalPlayer, false);

        if (_gameEvents.OnGameStarted != null)
        {
            _gameEvents.OnGameStarted();
        }
    }

    private Player GetPlayerFromID(int playerID)
    {
        foreach (var player in _players)
        {
            if (player.PlayerID == playerID)
            {
                return player;
            }
        }

        return null;
    }

    public void SendMainPhaseAction(Player player, MainPhaseAction action)
    {
        photonView.RPC("QueueMainPhaseAction_Internal", RpcTarget.All, player.PlayerID, GamePacketFactory.GetId(action.GetType()), action.Serialize());
    }

    [PunRPC]
    void QueueMainPhaseAction_Internal(int playerID, byte packetId, byte[] bytes)
    {
        Player player = GetPlayerFromID(playerID);

        if (player == null)
        {
            return;
        }

        MainPhaseAction action = GamePacketFactory.Create(packetId, bytes) as MainPhaseAction;
        _gameEvents.OnMainPhaseAction(player, action);
    }

    public void SendPlayerSelection(Player player, IPlayerSelection playerSelection)
    {
        photonView.RPC("QueuePlayerSelection_Internal", RpcTarget.All, player.PlayerID, GamePacketFactory.GetId(playerSelection.GetType()), playerSelection.Serialize());
    }

    [PunRPC]
    void QueuePlayerSelection_Internal(int playerID, byte packetId, byte[] bytes)
    {
        Player player = GetPlayerFromID(playerID);

        if (player == null)
        {
            return;
        }

        IPlayerSelection playerSelection = GamePacketFactory.Create(packetId, bytes) as IPlayerSelection;
        _gameEvents.OnPlayerSelecton(player, playerSelection);
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
            _gameEvents.OnGameSeed.Invoke(gameSeed);
        }
    }

    public NetworkPlayerInfo GetPlayerInfo(Player player)
    {
        foreach (var roomPlayer in _roomInfo.Players)
        {
            if (roomPlayer.GameID == player.PlayerID)
            {
                return roomPlayer;
            }
        }

        return new NetworkPlayerInfo();
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
            _gameEvents.OnSurrender.Invoke(GetPlayerFromID(playerID));
        }
    }

    public void EndGame()
    {
        
    }
    #endregion

    private void Update()
    {
        if (_gameState == GameState.PrivateRoom && PhotonNetwork.InRoom)
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

#if UNITY_EDITOR
    #region Photon Debug HUD
    bool _showPhotonDebug = false;
    Photon.Realtime.IConnectionCallbacks _connectionCallbacks;
    Photon.Realtime.IMatchmakingCallbacks _matchmakingCallbacks;
    Photon.Realtime.ILobbyCallbacks _lobbyCallbacks;

    public override void OnEnable()
    {
        base.OnEnable();
         
        var listener = new PhotonDebugListener();
        _connectionCallbacks = listener;
        _matchmakingCallbacks = listener;
        _lobbyCallbacks = listener;
        PhotonNetwork.AddCallbackTarget(listener);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        if (_connectionCallbacks != null)
            PhotonNetwork.RemoveCallbackTarget(_connectionCallbacks);
    }

    class PhotonDebugListener :
        Photon.Realtime.IConnectionCallbacks,
        Photon.Realtime.IMatchmakingCallbacks,
        Photon.Realtime.ILobbyCallbacks
    {
        public void OnDisconnected(Photon.Realtime.DisconnectCause cause)
        {
            Debug.LogWarning($"[Photon Debug] Disconnected: {cause}");
        }

        public void OnConnected() { }
        public void OnConnectedToMaster()
        {
            Debug.Log("[Photon Debug] Connected to Master");
        }
        public void OnRegionListReceived(Photon.Realtime.RegionHandler handler) { }
        public void OnCustomAuthenticationResponse(Dictionary<string, object> data) { }
        public void OnCustomAuthenticationFailed(string debugMessage)
        {
            Debug.LogError($"[Photon Debug] Auth failed: {debugMessage}");
        }

        public void OnJoinedLobby()
        {
            Debug.Log("[Photon Debug] Joined Lobby");
        }
        public void OnLeftLobby()
        {
            Debug.Log("[Photon Debug] Left Lobby");
        }
        public void OnLobbyStatisticsUpdate(List<Photon.Realtime.TypedLobbyInfo> lobbyStatistics) { }
        public void OnRoomListUpdate(List<Photon.Realtime.RoomInfo> roomList) { }

        public void OnJoinedRoom()
        {
            Debug.Log($"[Photon Debug] Joined Room: {PhotonNetwork.CurrentRoom?.Name}");
        }
        public void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"[Photon Debug] Join Room FAILED: [{returnCode}] {message}");
        }
        public void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.LogError($"[Photon Debug] Join Random FAILED: [{returnCode}] {message}");
        }
        public void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"[Photon Debug] Create Room FAILED: [{returnCode}] {message}");
        }
        public void OnCreatedRoom()
        {
            Debug.Log("[Photon Debug] Created Room");
        }
        public void OnLeftRoom()
        {
            Debug.Log("[Photon Debug] Left Room");
        }
        public void OnFriendListUpdate(List<Photon.Realtime.FriendInfo> friendList) { }
    }

    void OnGUI()
    {
        if (!string.IsNullOrEmpty(PhotonUtility.RetryStatus))
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
            GUI.Label(new Rect(x, y, boxW, boxH), $"<color=#FFAA00>{PhotonUtility.RetryStatus}</color>", retryStyle);
        }

        if (!_showPhotonDebug) return;

        string status;

        if (!PhotonNetwork.IsConnected)
        {
            status = "<color=#FF4444>[Photon] Disconnected</color>";
        }
        else if (PhotonNetwork.InRoom)
        {
            string roomName = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.Name : "?";
            int players = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.PlayerCount : 0;
            status = $"<color=#44FF44>[Photon] Connected | Region: {PhotonNetwork.CloudRegion} | Room: {roomName} ({players} players)</color>";
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

    #region Manage connections to Photon
    public class PhotonUtility
    {
        public static string RetryStatus { get; set; } = null;

        #region Disconnected from Photon
        public static IEnumerator DisconnectCoroutine()
        {
            #region Exit Room
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }

            yield return new WaitWhile(() => PhotonNetwork.InRoom);
            #endregion

            #region Exit from the lobby
            if (PhotonNetwork.InLobby)
            {
                PhotonNetwork.LeaveLobby();
            }

            yield return new WaitWhile(() => PhotonNetwork.InLobby);
            #endregion

            #region Disconnected from Photon
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Disconnect();
            }

            yield return new WaitWhile(() => PhotonNetwork.IsConnected);
            #endregion
        }
        #endregion

        #region Connect to Photon server
        public static IEnumerator ConnectToMasterServerCoroutine()
        {
            int maxRetries = 5;
            float retryDelay = 3f;

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                if (!PhotonNetwork.IsConnected || ContinuousController.instance.LastConnectServerRegion != ContinuousController.instance.serverRegion)
                {
                    if (PhotonNetwork.IsConnected)
                    {
                        yield return ContinuousController.instance.StartCoroutine(DisconnectCoroutine());

                        yield return new WaitWhile(() => PhotonNetwork.IsConnected);
                    }

                    PhotonNetwork.NetworkingClient.AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
                    PhotonNetwork.ConnectToRegion(ContinuousController.instance.serverRegion);
                    PhotonNetwork.NickName = ContinuousController.instance.PlayerName;
                    PhotonNetwork.GameVersion = ContinuousController.instance.GameVerString;
                    ContinuousController.instance.LastConnectServerRegion = ContinuousController.instance.serverRegion;
                }

                yield return new WaitUntil(() =>
                    PhotonNetwork.IsConnectedAndReady ||
                    PhotonNetwork.NetworkingClient.State == Photon.Realtime.ClientState.Disconnected);

                if (PhotonNetwork.IsConnectedAndReady)
                {
                    RetryStatus = null;
                    yield break;
                }

                var cause = PhotonNetwork.NetworkingClient.DisconnectedCause;
                Debug.LogWarning($"[Photon] Connection failed: {cause} (attempt {attempt + 1}/{maxRetries + 1})");

                if (cause == Photon.Realtime.DisconnectCause.MaxCcuReached && attempt < maxRetries)
                {
                    RetryStatus = LocalizeUtility.GetLocalizedString(
                        EngMessage: $"Server full. Retrying... ({attempt + 1}/{maxRetries})",
                        JpnMessage: $"サーバーが満員です。再接続中... ({attempt + 1}/{maxRetries})"
                    );
                    Debug.Log($"[Photon] Server full, retrying in {retryDelay}s...");
                    yield return new WaitForSeconds(retryDelay);
                    retryDelay += 2f;
                    continue;
                }

                RetryStatus = LocalizeUtility.GetLocalizedString(
                    EngMessage: "Connection failed. Please try again later.",
                    JpnMessage: "接続に失敗しました。後でもう一度お試しください。"
                );
                Debug.LogError($"[Photon] Connection failed permanently: {cause}");
                yield break;
            }
        }
        #endregion
        #region Connect to Photon Server and Lobby
        public static IEnumerator ConnectToLobbyCoroutine()
        {
            #region Connect to Photon server
            yield return ContinuousController.instance.StartCoroutine(ConnectToMasterServerCoroutine());
            #endregion

            #region Save player name to custom properties
            yield return ContinuousController.instance.StartCoroutine(SetPlayerName());
            #endregion

            #region Save the number of wins to a custom property
            Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;

            object value;

            hash = PhotonNetwork.LocalPlayer.CustomProperties;

            if (hash.TryGetValue(ContinuousController.WinCountKey, out value))
            {
                hash[ContinuousController.WinCountKey] = ContinuousController.instance.WinCount;
            }

            else
            {
                hash.Add(ContinuousController.WinCountKey, ContinuousController.instance.WinCount);
            }

            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

            while (true)
            {
                Hashtable _hash = PhotonNetwork.LocalPlayer.CustomProperties;

                if (_hash.TryGetValue(ContinuousController.WinCountKey, out value))
                {
                    if ((int)value == ContinuousController.instance.WinCount)
                    {
                        break;
                    }

                }

                yield return null;
            }
            #endregion

            #region Connect to Lobby
            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }

            yield return new WaitWhile(() => !PhotonNetwork.InLobby);

            yield return new WaitUntil(() => PhotonNetwork.InLobby && PhotonNetwork.IsConnectedAndReady);
            #endregion
        }
        #endregion

        #region Save player name to properties
        public static IEnumerator SetPlayerName()
        {
            Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;

            object value;

            hash = PhotonNetwork.LocalPlayer.CustomProperties;

            if (hash.TryGetValue(ContinuousController.PlayerNameKey, out value))
            {
                hash[ContinuousController.PlayerNameKey] = ContinuousController.instance.PlayerName;
            }

            else
            {
                hash.Add(ContinuousController.PlayerNameKey, ContinuousController.instance.PlayerName);
            }

            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

            while (true)
            {
                Hashtable _hash = PhotonNetwork.LocalPlayer.CustomProperties;

                if (_hash.TryGetValue(ContinuousController.PlayerNameKey, out value))
                {
                    if ((string)value == ContinuousController.instance.PlayerName)
                    {
                        break;
                    }
                }

                yield return null;
            }
        }
        #endregion

        #region Save deck data to custom properties
        public static void SetDeckData(DeckData deckData)
        {
            Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;

            if (hash.TryGetValue(ContinuousController.DeckDataPropertyKey, out object value))
            {
                hash[ContinuousController.DeckDataPropertyKey] = deckData.GetThisDeckCode();
            }

            else
            {
                hash.Add(ContinuousController.DeckDataPropertyKey, deckData.GetThisDeckCode());
            }

            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
        #endregion

        #region Remove custom properties from deck data
        public static IEnumerator DeleteBattleDeckData()
        {
            Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;

            if (hash.TryGetValue(ContinuousController.DeckDataPropertyKey, out object value))
            {
                hash.Remove(ContinuousController.DeckDataPropertyKey);
            }

            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

            while (true)
            {
                Hashtable _hash = PhotonNetwork.LocalPlayer.CustomProperties;

                if (!_hash.TryGetValue(ContinuousController.DeckDataPropertyKey, out value))
                {
                    break;
                }

                yield return null;
            }
        }
        #endregion
    }
    #endregion

}
