using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCGO.Networking
{
    public enum GameState
    {
        Menu,
        Matchmaking,
        EnteringPrivateRoom,
        PrivateRoom,
        InMatchmakingGame,
        InPrivateRoomGame,
        BotMatch
    }

    public struct GameNetworkEvents
    {
        public Action OnGameStarted;
        public Action<long> OnGameSeed;
        public Action<Player, IPlayerSelection> OnPlayerSelecton;
        public Action<Player, MainPhaseAction> OnMainPhaseAction;
        public Action<Player> OnSurrender;
    }

    public struct NetworkPlayerInfo
    {
        public int GameID;
        public string Name;
        public bool IsReady;
        public DeckData DeckData;
        public bool WaitingForGameStart;
    }

    public struct RoomInfo
    {
        public string RoomCode;
        public List<NetworkPlayerInfo> Players;
        public Dictionary<string, int> RoomSettings;
    }

    public struct PrivateRoomEvents
    {
        public Action<RoomInfo> OnRoomInfoUpdated;
        public Action OnAllPlayersReady;
    }

    public struct MatchmakingEvents
    {
        public Action<bool> OnConnected;
        public Action OnMatchFound;
        public Action OnCanceled;
    }

    public interface INetworkProvider
    {
        public abstract void Initialise();

        public abstract void StartMatchmaking(MatchmakingEvents matchmakingEvents);
        public abstract void CancelMatchmaking();

        public abstract void CreatePrivateRoom(bool useBanList, Action<bool> onRoomCreated);

        public abstract bool IsRoomHost();

        public abstract void JoinPrivateRoom(string roomCode, Action<bool> onRoomJoined);

        public abstract void SetRoomReadyState(bool isReady);

        public abstract void SetDeckData(DeckData deckData);

        public abstract void SetRoomSetting(string settingsKey, int value);

        public abstract void OnPrivateRoomEvents(PrivateRoomEvents roomNetworkEvents);
        public abstract void LeavePrivateRoom(Action onPrivateRoomLeft);

        public abstract void CreateGame(GameContext context, GameNetworkEvents gameNetworkEvents);

        public abstract void SetGameSeed(long gameSeed);

        public abstract int GetRoomSetting(string setting);

        public abstract NetworkPlayerInfo GetPlayerInfo(Player player);

        public abstract void EndGame();

        abstract void SendMainPhaseAction(Player player, MainPhaseAction action);
        abstract void SendPlayerSelection(Player player, IPlayerSelection playerSelection);

        abstract void SendSurrender(Player player);
    }
}