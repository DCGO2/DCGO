/// <summary>
/// Photon property keys and room-name helpers for tournament mode.
/// </summary>
public static class TournamentKeys
{
    public const int DefaultPlayerCount = 8;
    public const int MinPlayersToStart = 2;
    public const int WinsToTakeSeries = 2;

    /// <summary>Bye placeholder in bracket slots / match sides.</summary>
    public const string ByeUserId = "__BYE__";

    /// <summary>Supported bracket capacities (powers of 2). Empty seats become byes.</summary>
    public static readonly int[] AllowedPlayerCounts = { 4, 8, 16 };

    public static bool IsBye(string userId)
    {
        return userId == ByeUserId;
    }

    /// <summary>Both seats filled with real players — safe to open a Photon match room.</summary>
    public static bool IsReadyTwoPlayerMatch(TournamentMatchSlot match)
    {
        if (match == null || match.complete)
        {
            return false;
        }

        if (string.IsNullOrEmpty(match.userIdA) || string.IsNullOrEmpty(match.userIdB))
        {
            return false;
        }

        return !IsBye(match.userIdA) && !IsBye(match.userIdB);
    }

    public static TournamentPlayerSlot CreateByeSlot(int seed)
    {
        return new TournamentPlayerSlot
        {
            userId = ByeUserId,
            nickName = "BYE",
            lockedDeckCode = null,
            lockedDeckName = "BYE",
            seed = seed,
            eliminated = false,
        };
    }

    public const string PlayerCountProperty = "TourneyPlayerCount";

    /// <summary>
    /// Active size for this client session. Host picks on create; joiners sync from room.
    /// </summary>
    public static int ActivePlayerCount
    {
        get
        {
            var cc = ContinuousController.instance;
            if (cc != null && IsAllowedPlayerCount(cc.TournamentPlayerCount))
            {
                return cc.TournamentPlayerCount;
            }

            return DefaultPlayerCount;
        }
        set
        {
            int size = NormalizePlayerCount(value);
            if (ContinuousController.instance != null)
            {
                ContinuousController.instance.TournamentPlayerCount = size;
            }
        }
    }

    public static bool IsAllowedPlayerCount(int count)
    {
        for (int i = 0; i < AllowedPlayerCounts.Length; i++)
        {
            if (AllowedPlayerCounts[i] == count)
            {
                return true;
            }
        }

        return false;
    }

    public static int NormalizePlayerCount(int count)
    {
        return IsAllowedPlayerCount(count) ? count : DefaultPlayerCount;
    }

    /// <summary>Last round index (0-based). 4→1, 8→2, 16→3.</summary>
    public static int FinalRound => FinalRoundFor(ActivePlayerCount);

    public static int FinalRoundFor(int playerCount)
    {
        playerCount = NormalizePlayerCount(playerCount);
        int round = 0;
        int size = playerCount;
        while (size > 2)
        {
            size /= 2;
            round++;
        }

        return round;
    }

    public static int MatchesInRound(int round) => MatchesInRoundFor(ActivePlayerCount, round);

    public static int MatchesInRoundFor(int playerCount, int round)
    {
        playerCount = NormalizePlayerCount(playerCount);
        int matches = playerCount / 2;
        for (int i = 0; i < round; i++)
        {
            matches /= 2;
        }

        return matches < 1 ? 1 : matches;
    }

    public static string RoundDisplayName(int round) => RoundDisplayNameFor(ActivePlayerCount, round);

    public static string RoundDisplayNameFor(int playerCount, int round)
    {
        playerCount = NormalizePlayerCount(playerCount);
        int final = FinalRoundFor(playerCount);
        if (round >= final)
        {
            return "Finals";
        }

        // From the end: Finals-1 = Semifinals, Finals-2 = Quarterfinals, else Round of N
        int fromEnd = final - round;
        if (fromEnd == 1)
        {
            return "Semifinals";
        }

        if (fromEnd == 2)
        {
            return "Quarterfinals";
        }

        int playersInRound = playerCount;
        for (int i = 0; i < round; i++)
        {
            playersInRound /= 2;
        }

        return $"Round of {playersInRound}";
    }

    public const string ModeProperty = "Mode";
    public const string ModeTournament = "tournament";

    public const string UseBanlistProperty = "UseBanlist";
    public const string TourneyIdProperty = "TourneyId";
    public const string StateProperty = "TourneyState";
    public const string StartedProperty = "TourneyStarted";
    public const string RoomKindProperty = "TourneyRoomKind";

    public const string RoomKindLobby = "lobby";
    public const string RoomKindMatch = "match";
    public const string RoomKindWaitHub = "waithub";

    public const string RoundProperty = "TourneyRound";
    public const string MatchIndexProperty = "TourneyMatchIndex";
    public const string UserIdAProperty = "TourneyUserIdA";
    public const string UserIdBProperty = "TourneyUserIdB";
    public const string SeriesWinsAProperty = "TourneySeriesWinsA";
    public const string SeriesWinsBProperty = "TourneySeriesWinsB";
    public const string GameIndexProperty = "TourneyGameIndex";
    public const string LastLoserProperty = "TourneyLastLoser";
    /// <summary>User id the loser chose to go first in the next Bo3 game.</summary>
    public const string NextFirstUserIdProperty = "TourneyNextFirstUserId";
    /// <summary>Game index the next-first choice applies to (avoids reusing game 2's pick for game 3).</summary>
    public const string NextFirstGameIndexProperty = "TourneyNextFirstGameIndex";
    /// <summary>Local player is on the post-game result screen (used to auto-start the next Bo3 game together).</summary>
    public const string OnResultProperty = "TourneyOnResult";

    public const string PlayerIdProperty = "TourneyPlayerId";
    public const string LockedDeckProperty = "TourneyLockedDeck";
    public const string ReadyPropertyPrefix = "TourneyReady";

    public const string LobbyInfix = "-T-";
    public const string WaitHubInfix = "-T-W-";

    public static string LobbyRoomName(string tourneyId, bool useBanlist = false)
    {
        // Banlist lives in room properties — do not put it in the lobby name.
        // Friends only share the 5-digit ID; mismatched local banlist toggles used to cause 32758.
        return tourneyId + LobbyInfix + "Lobby";
    }

    /// <summary>Lobby names to try when joining (current + legacy banlist-suffixed names).</summary>
    public static string[] LobbyRoomNameJoinCandidates(string tourneyId, bool preferBanlist)
    {
        return new[]
        {
            LobbyRoomName(tourneyId),
            tourneyId + LobbyInfix + preferBanlist,
            tourneyId + LobbyInfix + !preferBanlist,
        };
    }

    public static string MatchRoomName(string tourneyId, int round, int matchIndex, bool useBanlist)
    {
        return $"{tourneyId}-T-R{round}-M{matchIndex}-{useBanlist}";
    }

    public static string WaitHubRoomName(string tourneyId, bool useBanlist = false)
    {
        // Banlist lives in room properties — one hub per tournament id.
        return tourneyId + WaitHubInfix + "Hub";
    }

    public static string ReadyKey(string roomName)
    {
        return ReadyPropertyPrefix + roomName;
    }

    public static string DisplayRoomId(string roomName)
    {
        if (string.IsNullOrEmpty(roomName))
        {
            return "";
        }

        return roomName.Length >= 5 ? roomName.Substring(0, 5) : roomName;
    }

    public static bool IsTournamentRoomName(string roomName)
    {
        return !string.IsNullOrEmpty(roomName) && roomName.Contains("-T-");
    }
}
