using DCGO.Networking;
using System;
using System.Collections.Generic;

public class OfflineNetworkProvider : INetworkProvider
{
    private GameNetworkEvents _gameEvents;

    #region Connection
    public bool IsConnected()
    {
        return false;
    }

    public void Disconnect(Action onDisconnected)
    {
        if (onDisconnected != null)
        {
            onDisconnected();
        }
    }
    #endregion

    #region Matchmaking
    public void StartMatchmaking(MatchmakingEvents matchmakingEvents)
    {

    }

    public void CancelMatchmaking()
    {

    }
    #endregion

    #region Private Room
    public void CreatePrivateRoom(bool useBanList, PrivateRoomEvents privateRoomEvents)
    {

    }

    public void JoinPrivateRoom(string roomCode, PrivateRoomEvents privateRoomEvents)
    {

    }

    public void LeavePrivateRoom()
    {

    }

    public void SetRoomReadyState(bool isReady)
    {

    }

    public void SetRoomSetting(string settingsKey, int value)
    {

    }
    #endregion

    #region Room
    public bool IsInRoom()
    {
        return false;
    }

    public bool IsRoomHost()
    {
        return true;
    }

    public RoomInfo GetRoomInfo()
    {
        RoomInfo roomInfo = new RoomInfo();
        roomInfo.Players = new List<NetworkPlayerInfo>();
        roomInfo.RoomSettings = new Dictionary<string, int>();

        return roomInfo;
    }

    public int GetRoomSetting(string settingsKey, int defaultValue)
    {
        return defaultValue;
    }

    public void SetDeckData(DeckData deckData)
    {

    }
    #endregion

    #region Game
    public void CreateGame(GameContext gameContext, GameNetworkEvents gameNetworkEvents)
    {
        _gameEvents = gameNetworkEvents;

        if (_gameEvents.OnGameStarted != null)
        {
            _gameEvents.OnGameStarted();
        }
    }

    public NetworkPlayerInfo GetPlayerInfo(Player player)
    {
        return new NetworkPlayerInfo();
    }

    public void SetGameSeed(long gameSeed)
    {
        if (_gameEvents.OnGameSeed != null)
        {
            _gameEvents.OnGameSeed(gameSeed);
        }
    }

    public void SendMainPhaseAction(Player player, MainPhaseAction action)
    {
        if (_gameEvents.OnMainPhaseAction != null)
        {
            _gameEvents.OnMainPhaseAction(player, action);
        }
    }

    public void SendPlayerSelection(Player player, IPlayerSelection playerSelection)
    {
        if (_gameEvents.OnPlayerSelection != null)
        {
            _gameEvents.OnPlayerSelection(player, playerSelection);
        }
    }

    public void SendSurrender(Player player)
    {
        if (_gameEvents.OnSurrender != null)
        {
            _gameEvents.OnSurrender(player);
        }
    }

    public void EndGame()
    {
        _gameEvents = new GameNetworkEvents();
    }
    #endregion
}
