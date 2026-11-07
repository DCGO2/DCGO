using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

#region Payload
[Serializable]
public class GameStatsPayload
{
    public int schemaVersion;
    public string matchId;
    public string reporterSide;
    public string matchType;
    public string gameVersion;
    public string startedAt;
    public string endedAt;
    public GameStatsResult result;
    public GameStatsDeck player1;
    public GameStatsDeck player2;
    public string log;
}

[Serializable]
public class GameStatsResult
{
    //JsonUtility cannot write null, so "" means no side
    public string winnerSide;
    public string endReason;
    public string effectName;
    public string wentFirstSide;
    public int turnCount;
}

[Serializable]
public class GameStatsDeck
{
    public string deckCode;
    public List<string> mainDeck;
    public List<string> eggDeck;
}
#endregion

//Sends the result of random matches to the stats collector
public static class GameStatsReporter
{
    //The dcgo-stats Worker that collects the reports. Empty disables reporting.
    public const string Endpoint = "https://dcgo-stats.gentle-night-d366.workers.dev/v1/games";

    const int SchemaVersion = 1;
    const int MaxAttempts = 3;
    const int TimeoutSeconds = 15;

    static bool SharingEnabled => ContinuousController.instance != null && ContinuousController.instance.shareRandomMatchmakingGameplayData;

    #region Snapshot at game start
    //The opponent can leave the room before the game ends, so everything is taken at game start
    static bool _hasSnapshot = false;
    static bool _hasReported = false;
    static bool _endedByReconnect = false;

    static GameContext _gameContext;
    static string _roomName;
    static long _seed;
    static string _reporterSide;
    static string _localName;
    static string _startedAt;
    static GameStatsDeck _player1Deck;
    static GameStatsDeck _player2Deck;

    public static void OnGameStart(GameContext gameContext)
    {
        _hasSnapshot = false;
        _hasReported = false;
        _endedByReconnect = false;
        _gameContext = null;

        try
        {
            if (!SharingEnabled || !ContinuousController.instance.isRandomMatch || ContinuousController.instance.isAI || GManager.instance.IsAI)
            {
                return;
            }

            if (!PhotonNetwork.InRoom)
            {
                return;
            }

            Photon.Realtime.Player masterPlayer = null;
            Photon.Realtime.Player nonMasterPlayer = null;

            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
            {
                if (player.ActorNumber == PhotonNetwork.CurrentRoom.MasterClientId)
                {
                    masterPlayer = player;
                }

                else
                {
                    nonMasterPlayer = player;
                }
            }

            _player1Deck = CreateDeck(masterPlayer);
            _player2Deck = CreateDeck(nonMasterPlayer);

            if (_player1Deck == null || _player2Deck == null)
            {
                Debug.LogWarning("[GameStats] Deck data is missing, this game will not be reported");
                return;
            }

            _gameContext = gameContext;
            _roomName = PhotonNetwork.CurrentRoom.Name;
            _seed = ContinuousController.instance.RandomSeed;
            _reporterSide = gameContext.PlayerFromID(0) == GManager.instance.You ? GameStatsUtility.Player1Side : GameStatsUtility.Player2Side;
            //The log text goes through ReplaceToASCII, so the name has to as well
            _localName = DataBase.ReplaceToASCII(GManager.instance.You.PlayerName);
            _startedAt = GameStatsUtility.FormatUtc(DateTime.UtcNow);
            _hasSnapshot = true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[GameStats] Failed to snapshot the game: {ex.Message}");
        }
    }

    static GameStatsDeck CreateDeck(Photon.Realtime.Player player)
    {
        if (player == null)
        {
            return null;
        }

        if (!player.CustomProperties.TryGetValue(ContinuousController.DeckDataPropertyKey, out object value))
        {
            return null;
        }

        string deckCode = value as string;

        if (string.IsNullOrEmpty(deckCode))
        {
            return null;
        }

        DeckData deckData = new DeckData(deckCode);

        return new GameStatsDeck()
        {
            deckCode = GameStatsUtility.StripDeckName(deckCode),
            mainDeck = SortedCardIDs(deckData.DeckCards()),
            eggDeck = SortedCardIDs(deckData.DigitamaDeckCards()),
        };
    }

    //One entry per copy, sorted so both clients send the same list
    static List<string> SortedCardIDs(List<CEntity_Base> cards)
    {
        List<string> cardIDs = new List<string>();

        foreach (CEntity_Base card in cards)
        {
            cardIDs.Add(card.CardID);
        }

        cardIDs.Sort(string.CompareOrdinal);

        return cardIDs;
    }
    #endregion

    #region Report at game end
    //Called by ReconnectManager right before it ends the game
    public static void OnReconnectEnd()
    {
        _endedByReconnect = true;
    }

    public static void OnGameEnd(TurnStateMachine turnStateMachine, Player winner, bool surrendered, string effectName)
    {
        try
        {
            //The snapshot has to be from this game, and each game is reported once
            if (!_hasSnapshot || _hasReported || _gameContext != turnStateMachine.gameContext)
            {
                return;
            }

            _hasReported = true;

            if (string.IsNullOrEmpty(Endpoint) || Application.isEditor || !SharingEnabled)
            {
                return;
            }

            GameContext gameContext = _gameContext;

            bool bothPlayersInRoom = PhotonNetwork.IsConnected && PhotonNetwork.PlayerList.Length == 2;

            string localLabel = GameStatsUtility.SideLabel(_reporterSide);
            string opponentLabel = GameStatsUtility.SideLabel(GameStatsUtility.OtherSide(_reporterSide));

            //Everything is read now, as the battle scene can be unloaded while sending
            GameStatsPayload payload = new GameStatsPayload()
            {
                schemaVersion = SchemaVersion,
                matchId = GameStatsUtility.MatchId(_roomName, _seed),
                reporterSide = _reporterSide,
                matchType = "random",
                gameVersion = ContinuousController.instance.GameVerString,
                startedAt = _startedAt,
                endedAt = GameStatsUtility.FormatUtc(DateTime.UtcNow),
                result = new GameStatsResult()
                {
                    winnerSide = SideOf(gameContext, winner),
                    endReason = GameStatsUtility.EndReason(surrendered, effectName, winner != null, bothPlayersInRoom, _endedByReconnect),
                    effectName = effectName ?? "",
                    wentFirstSide = SideOf(gameContext, gameContext.FirstPlayer),
                    turnCount = turnStateMachine.TurnCount,
                },
                player1 = _player1Deck,
                player2 = _player2Deck,
                log = GameStatsUtility.AnonymizeLog(GManager.instance.playLog.FullLogText, _localName, localLabel, opponentLabel),
            };

            string json = JsonUtility.ToJson(payload);

            _ = SendReport(json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[GameStats] Failed to create the report: {ex.Message}");
        }
    }

    static string SideOf(GameContext gameContext, Player player)
    {
        if (player == null)
        {
            return "";
        }

        return player == gameContext.PlayerFromID(0) ? GameStatsUtility.Player1Side : GameStatsUtility.Player2Side;
    }

    static async Task SendReport(string json)
    {
        byte[] body = Encoding.UTF8.GetBytes(json);
        int currentAttempt = 0;

        while (currentAttempt < MaxAttempts)
        {
            if (!SharingEnabled) return;

            bool canRetry = true;
            string retryAfter = null;

            try
            {
                currentAttempt++;
                Debug.Log($"[GameStats] Sending report, attempt {currentAttempt} of {MaxAttempts}");

                using (UnityWebRequest request = new UnityWebRequest(Endpoint, UnityWebRequest.kHttpVerbPOST))
                {
                    request.uploadHandler = new UploadHandlerRaw(body);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");
                    request.timeout = TimeoutSeconds;

                    UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                    while (!operation.isDone)
                    {
                        if (!SharingEnabled)
                        {
                            request.Abort();
                            return;
                        }

                        await Task.Yield();
                    }

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log("[GameStats] Report sent");
                        return;
                    }

                    //Only connection errors, rate limits and server errors are worth retrying
                    long responseCode = request.responseCode;
                    canRetry = request.result == UnityWebRequest.Result.ConnectionError || responseCode == 429 || responseCode >= 500;
                    retryAfter = request.GetResponseHeader("Retry-After");

                    Debug.LogWarning($"[GameStats] Attempt {currentAttempt} failed: {request.error} ({responseCode})");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GameStats] Attempt {currentAttempt} failed: {ex.Message}");
            }

            if (!canRetry)
            {
                return;
            }

            if (currentAttempt < MaxAttempts)
            {
                //Waits 2s after 1st fail, 4s after 2nd fail, or as long as the collector asks
                int delay = GameStatsUtility.RetryDelayMilliseconds(retryAfter, currentAttempt * 2000);
                await Task.Delay(delay);
            }
        }

        Debug.Log($"[GameStats] All {MaxAttempts} attempts failed, the report was dropped");
    }
    #endregion
}
