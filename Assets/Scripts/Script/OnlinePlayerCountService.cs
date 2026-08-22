using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// Tracks Photon's regional CCU (AppStats) and ranked players in visible lobby rooms.
/// While title/home is open, keeps one Master+Lobby connection for live updates (no reconnect polling).
/// Ranked count = sum of PlayerCount in rooms with Mode=ranked (queue / open ranked rooms).
/// In-match ranked rooms that hide themselves (IsVisible=false) drop out of this count.
/// </summary>
public class OnlinePlayerCountService : MonoBehaviour, IOnEventCallback, ILobbyCallbacks
{
    public static OnlinePlayerCountService Instance { get; private set; }

    public int LastCount { get; private set; }
    public bool HasReceivedAppStats { get; private set; }

    public int LastRankedCount { get; private set; }
    public bool HasReceivedRankedCount { get; private set; }

    /// <summary>
    /// When true, matchmaking owns the Photon socket — menu presence must not disconnect.
    /// </summary>
    public bool MatchmakingOwnsConnection { get; private set; }

    public event Action Changed;

    bool _menuPresenceDesired;
    bool _presenceConnecting;
    bool _pausedByApp;
    Coroutine _presenceCoroutine;
    readonly Dictionary<string, RoomInfo> _lobbyRooms = new Dictionary<string, RoomInfo>();

    /// <summary>Snapshot of visible lobby rooms (for friend-invite scanning).</summary>
    public IReadOnlyDictionary<string, RoomInfo> LobbyRooms => _lobbyRooms;

    public static OnlinePlayerCountService EnsureExists()
    {
        if (Instance != null)
        {
            return Instance;
        }

        var go = new GameObject("OnlinePlayerCountService");
        return go.AddComponent<OnlinePlayerCountService>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            if (_menuPresenceDesired && !MatchmakingOwnsConnection)
            {
                _pausedByApp = true;
                StopPresenceCoroutine();
                StartCoroutine(DisconnectIfPresenceOwnedCoroutine());
            }
        }
        else if (_pausedByApp)
        {
            _pausedByApp = false;
            if (_menuPresenceDesired && !MatchmakingOwnsConnection)
            {
                EnsureMenuPresenceRunning();
            }
        }
    }

    public void SetMatchmakingOwnsConnection(bool owns)
    {
        MatchmakingOwnsConnection = owns;
    }

    /// <summary>
    /// Keep a silent Master+Lobby session while title/home is visible.
    /// </summary>
    public void SetMenuPresenceEnabled(bool enabled)
    {
        _menuPresenceDesired = enabled;

        if (!enabled)
        {
            StopPresenceCoroutine();
            if (!MatchmakingOwnsConnection)
            {
                StartCoroutine(DisconnectIfPresenceOwnedCoroutine());
            }

            return;
        }

        if (_pausedByApp)
        {
            return;
        }

        EnsureMenuPresenceRunning();
    }

    void EnsureMenuPresenceRunning()
    {
        if (MatchmakingOwnsConnection)
        {
            if (PhotonNetwork.InLobby)
            {
                RecalculateRankedCount();
            }

            Changed?.Invoke();
            return;
        }

        if (PhotonNetwork.InLobby && PhotonNetwork.IsConnectedAndReady)
        {
            RecalculateRankedCount();
            Changed?.Invoke();
            return;
        }

        if (_presenceConnecting)
        {
            return;
        }

        StopPresenceCoroutine();
        _presenceCoroutine = StartCoroutine(MenuPresenceCoroutine());
    }

    void StopPresenceCoroutine()
    {
        if (_presenceCoroutine != null)
        {
            StopCoroutine(_presenceCoroutine);
            _presenceCoroutine = null;
        }

        _presenceConnecting = false;
    }

    IEnumerator MenuPresenceCoroutine()
    {
        _presenceConnecting = true;

        try
        {
            if (ContinuousController.instance == null)
            {
                yield break;
            }

            if (!PhotonNetwork.IsConnectedAndReady)
            {
                yield return ContinuousController.instance.StartCoroutine(
                    PhotonUtility.ConnectToMasterServerCoroutine(matchmakingOwnsConnection: false));
            }

            if (!_menuPresenceDesired || MatchmakingOwnsConnection)
            {
                yield break;
            }

            if (!PhotonNetwork.IsConnectedAndReady)
            {
                PhotonUtility.RetryStatus = null;
                yield break;
            }

            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
                yield return new WaitUntil(() =>
                    PhotonNetwork.InLobby || MatchmakingOwnsConnection || !_menuPresenceDesired);
            }

            PhotonUtility.RetryStatus = null;

            if (PhotonNetwork.InLobby && !HasReceivedRankedCount)
            {
                // Empty lobby may not fire room-list updates immediately; show 0 until first update.
                LastRankedCount = 0;
                HasReceivedRankedCount = true;
                Changed?.Invoke();
            }
            else
            {
                Changed?.Invoke();
            }
        }
        finally
        {
            _presenceConnecting = false;
            _presenceCoroutine = null;
        }
    }

    IEnumerator DisconnectIfPresenceOwnedCoroutine()
    {
        if (MatchmakingOwnsConnection || ContinuousController.instance == null)
        {
            yield break;
        }

        if (!PhotonNetwork.IsConnected)
        {
            yield break;
        }

        yield return ContinuousController.instance.StartCoroutine(PhotonUtility.DisconnectCoroutine());
        PhotonUtility.RetryStatus = null;
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent == null || photonEvent.Code != EventCode.AppStats)
        {
            return;
        }

        LastCount = PhotonNetwork.CountOfPlayers;
        HasReceivedAppStats = true;
        Changed?.Invoke();
    }

    public void OnJoinedLobby() { }

    public void OnLeftLobby()
    {
        _lobbyRooms.Clear();
    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (roomList == null)
        {
            return;
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            if (info == null || string.IsNullOrEmpty(info.Name))
            {
                continue;
            }

            if (info.RemovedFromList)
            {
                _lobbyRooms.Remove(info.Name);
            }
            else
            {
                _lobbyRooms[info.Name] = info;
            }
        }

        RecalculateRankedCount();
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics) { }

    void RecalculateRankedCount()
    {
        int ranked = 0;
        foreach (var pair in _lobbyRooms)
        {
            RoomInfo room = pair.Value;
            if (room == null || room.RemovedFromList)
            {
                continue;
            }

            Hashtable props = room.CustomProperties;
            if (props == null)
            {
                continue;
            }

            if (!props.TryGetValue(RankedKeys.ModeProperty, out object modeObj))
            {
                continue;
            }

            string mode = modeObj as string;
            if (mode != RankedKeys.ModeRanked)
            {
                continue;
            }

            ranked += room.PlayerCount;
        }

        LastRankedCount = ranked;
        HasReceivedRankedCount = true;
        Changed?.Invoke();
    }

    public string FormatDisplayString()
    {
        if (HasReceivedAppStats)
        {
            return LocalizeUtility.GetLocalizedString(
                EngMessage: $"Online: {LastCount}",
                JpnMessage: $"オンライン: {LastCount}");
        }

        return LocalizeUtility.GetLocalizedString(
            EngMessage: "Online: —",
            JpnMessage: "オンライン: —");
    }

    public string FormatRankedDisplayString()
    {
        if (HasReceivedRankedCount)
        {
            return LocalizeUtility.GetLocalizedString(
                EngMessage: $"Ranked: {LastRankedCount}",
                JpnMessage: $"ランク: {LastRankedCount}");
        }

        return LocalizeUtility.GetLocalizedString(
            EngMessage: "Ranked: —",
            JpnMessage: "ランク: —");
    }
}
