using System;
using System.Collections.Generic;

namespace DCGO.Networking
{
    public struct MatchmakingEvents
    {
        public Action<bool> OnConnected;
        public Action OnMatchFound;
        public Action OnCanceled;
        public Action OnDisconnected;
    }

    public struct PrivateRoomEvents
    {
        public Action<bool> OnJoined;
        public Action<RoomInfo> OnRoomInfoUpdated;
        public Action OnAllPlayersReady;
        public Action OnLeft;
        public Action OnDisconnected;
    }

    public struct GameNetworkEvents
    {
        public Action OnGameStarted;
        public Action<long> OnGameSeed;
        public Action<Player, IPlayerSelection> OnPlayerSelection;
        public Action<Player, MainPhaseAction> OnMainPhaseAction;
        public Action<Player> OnSurrender;

        //Player whose connection dropped, and how many seconds they have to come back
        public Action<Player, float> OnConnectionLost;
        public Action OnVerifyingGameState;
        public Action OnConnectionRestored;

        //Player who didn't come back, or null when the match should end with no result
        public Action<Player> OnDisconnected;

        //Compared between clients after a reconnect so a desynced match isn't continued
        public Func<int> GetStateHash;
    }

    public struct NetworkPlayerInfo
    {
        public int GameID;
        public string Name;
        public bool IsReady;
        public DeckData DeckData;
    }

    public struct RoomInfo
    {
        public string RoomCode;
        public List<NetworkPlayerInfo> Players;
        public Dictionary<string, int> RoomSettings;
    }

    public interface INetworkProvider
    {
        bool IsConnected();
        void Disconnect(Action onDisconnected);

        void StartMatchmaking(MatchmakingEvents matchmakingEvents);
        void CancelMatchmaking();

        void CreatePrivateRoom(bool useBanList, PrivateRoomEvents privateRoomEvents);
        void JoinPrivateRoom(string roomCode, PrivateRoomEvents privateRoomEvents);
        void LeavePrivateRoom();
        void SetRoomReadyState(bool isReady);
        void SetRoomSetting(string settingsKey, int value);

        bool IsInRoom();
        bool IsRoomHost();
        RoomInfo GetRoomInfo();
        int GetRoomSetting(string settingsKey, int defaultValue);
        void SetDeckData(DeckData deckData);

        void CreateGame(GameContext gameContext, GameNetworkEvents gameNetworkEvents);
        NetworkPlayerInfo GetPlayerInfo(Player player);
        void SetGameSeed(long gameSeed);
        void SendMainPhaseAction(Player player, MainPhaseAction action);
        void SendPlayerSelection(Player player, IPlayerSelection playerSelection);
        void SendSurrender(Player player);
        void GiveUpReconnect();
        void EndGame();
    }
}
