using DCGO.Networking;
using System;
using UnityEngine;

public class OfflineNetworkProvider : INetworkProvider
{
    private GameNetworkEvents _gameEvents;

    public void Initialise()
    {

    }

    #region Matchmaking
    public void StartMatchmaking(MatchmakingEvents matchmakingEvents)
    {

    }

    public void CancelMatchmaking()
    {

    }
    #endregion

    #region Private Room
    public void CreatePrivateRoom(bool useBanList, Action<bool> onPrivateRoomJoined)
    {

    }

    public void JoinPrivateRoom(string roomCode, Action<bool> onPrivateRoomJoined)
    {

    }

    public void OnPrivateRoomEvents(PrivateRoomEvents privateRoomEvents)
    {

    }

    public void SetRoomReadyState(bool isReady)
    {

    }

    public void SetDeckData(DeckData deckData)
    {

    }

    public void SetRoomSetting(string settingsKey, int value)
    {

    }

    public int GetRoomSetting(string setting)
    {
        return -1;
    }

    public void LeavePrivateRoom(Action onPrivateRoomLeft)
    {

    }
    #endregion

    #region Room
    public bool IsRoomHost()
    {
        return true;
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

    public void SendMainPhaseAction(Player player, MainPhaseAction action)
    {
        if (_gameEvents.OnMainPhaseAction != null)
        {
            _gameEvents.OnMainPhaseAction(player, action);
        }
    }

    public void SendPlayerSelection(Player player, IPlayerSelection playerSelection)
    {
        if (_gameEvents.OnPlayerSelecton != null)
        {
            _gameEvents.OnPlayerSelecton(player, playerSelection);
        }
    }

    public void SetGameSeed(long gameSeed)
    {
        if (_gameEvents.OnGameSeed != null)
        {
            _gameEvents.OnGameSeed.Invoke(gameSeed);
        }
    }

    public NetworkPlayerInfo GetPlayerInfo(Player player)
    {
        return new NetworkPlayerInfo();
    }

    public void SendSurrender(Player player)
    {
        if (_gameEvents.OnSurrender != null)
        {
            _gameEvents.OnSurrender.Invoke(player);
        }
    }

    public void EndGame()
    {
        _gameEvents = new GameNetworkEvents();
    }
    #endregion
}
