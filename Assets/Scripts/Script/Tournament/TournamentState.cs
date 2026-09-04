using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[Serializable]
public class TournamentPlayerSlot
{
    public string userId;
    public string nickName;
    public string lockedDeckCode;
    public string lockedDeckName;
    public int seed;
    public bool eliminated;
}

[Serializable]
public class TournamentMatchSlot
{
    public int round;
    public int matchIndex;
    public string userIdA;
    public string userIdB;
    public int seriesWinsA;
    public int seriesWinsB;
    public int gameIndex;
    public string winnerUserId;
    public string lastGameLoserUserId;
    public bool complete;
}

[Serializable]
public class TournamentState
{
    public string tourneyId;
    public bool useBanlist;
    public bool started;
    public bool finished;
    public int bracketSeed;
    public string championUserId;
    public int playerCount;
    public TournamentPlayerSlot[] players;
    public TournamentMatchSlot[] matches;

    public int ResolvedPlayerCount => TournamentKeys.NormalizePlayerCount(
        playerCount > 0 ? playerCount : TournamentKeys.ActivePlayerCount);

    public static TournamentState CreateNew(string tourneyId, bool useBanlist, int playerCount = 0)
    {
        int size = TournamentKeys.NormalizePlayerCount(
            playerCount > 0 ? playerCount : TournamentKeys.ActivePlayerCount);
        return new TournamentState
        {
            tourneyId = tourneyId,
            useBanlist = useBanlist,
            started = false,
            finished = false,
            playerCount = size,
            players = new TournamentPlayerSlot[size],
            matches = BuildEmptyMatches(size),
        };
    }

    public static TournamentMatchSlot[] BuildEmptyMatches(int playerCount = 0)
    {
        int size = TournamentKeys.NormalizePlayerCount(
            playerCount > 0 ? playerCount : TournamentKeys.ActivePlayerCount);
        var list = new System.Collections.Generic.List<TournamentMatchSlot>();
        int finalRound = TournamentKeys.FinalRoundFor(size);
        for (int round = 0; round <= finalRound; round++)
        {
            int matchCount = TournamentKeys.MatchesInRoundFor(size, round);
            for (int m = 0; m < matchCount; m++)
            {
                list.Add(NewMatch(round, m));
            }
        }

        return list.ToArray();
    }

    static TournamentMatchSlot NewMatch(int round, int matchIndex)
    {
        return new TournamentMatchSlot
        {
            round = round,
            matchIndex = matchIndex,
        };
    }

    public static TournamentState FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonUtility.FromJson<TournamentState>(json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Tournament] Failed to parse state: {e.Message}");
            return null;
        }
    }

    public string ToRoomJson()
    {
        var copy = Clone();
        if (copy.players != null)
        {
            for (int i = 0; i < copy.players.Length; i++)
            {
                if (copy.players[i] != null)
                {
                    copy.players[i].lockedDeckCode = null;
                }
            }
        }

        return JsonUtility.ToJson(copy);
    }

    public TournamentState Clone()
    {
        return FromJson(JsonUtility.ToJson(this));
    }

    public void SeedFromLobby(List<TournamentPlayerSlot> lobbyPlayers, int seed)
    {
        int size = ResolvedPlayerCount;
        playerCount = size;
        bracketSeed = seed;

        if (lobbyPlayers == null || lobbyPlayers.Count < TournamentKeys.MinPlayersToStart)
        {
            throw new InvalidOperationException("Tournament needs at least 2 players.");
        }

        if (lobbyPlayers.Count > size)
        {
            throw new InvalidOperationException($"Too many players for a {size}-player bracket.");
        }

        var ordered = new List<TournamentPlayerSlot>();
        foreach (var p in lobbyPlayers)
        {
            if (p == null || TournamentKeys.IsBye(p.userId))
            {
                continue;
            }

            ordered.Add(p);
        }

        ordered.Sort((a, b) => string.CompareOrdinal(a.userId, b.userId));

        var rng = new System.Random(seed);
        for (int i = ordered.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            var tmp = ordered[i];
            ordered[i] = ordered[j];
            ordered[j] = tmp;
        }

        // Place byes preferentially as odd-index opponents so first-round
        // matches are usually Player vs BYE (at most one bye per match when possible).
        var slots = new TournamentPlayerSlot[size];
        int byeCount = size - ordered.Count;
        int byePlaced = 0;
        int firstRoundMatches = size / 2;

        for (int m = 0; m < firstRoundMatches && byePlaced < byeCount; m++)
        {
            slots[m * 2 + 1] = TournamentKeys.CreateByeSlot(m * 2 + 1);
            byePlaced++;
        }

        for (int m = 0; m < firstRoundMatches && byePlaced < byeCount; m++)
        {
            if (slots[m * 2] == null)
            {
                slots[m * 2] = TournamentKeys.CreateByeSlot(m * 2);
                byePlaced++;
            }
        }

        int playerIndex = 0;
        for (int i = 0; i < size; i++)
        {
            if (slots[i] != null)
            {
                continue;
            }

            var real = ordered[playerIndex++];
            real.seed = i;
            real.eliminated = false;
            slots[i] = real;
        }

        players = slots;
        matches = BuildEmptyMatches(size);
        for (int i = 0; i < firstRoundMatches; i++)
        {
            matches[i].userIdA = players[i * 2].userId;
            matches[i].userIdB = players[i * 2 + 1].userId;
        }

        started = true;
        finished = false;
        championUserId = null;

        ResolveOpeningByes();
    }

    /// <summary>
    /// Auto-complete any match that has BYE vs player (or BYE vs BYE) so winners
    /// advance without entering a Photon match room.
    /// </summary>
    public void ResolveOpeningByes()
    {
        if (matches == null)
        {
            return;
        }

        bool progressed;
        int guard = 0;
        do
        {
            progressed = false;
            guard++;
            for (int i = 0; i < matches.Length; i++)
            {
                var match = matches[i];
                if (match == null || match.complete)
                {
                    continue;
                }

                // Both sides must be assigned (player or bye) before resolving.
                if (string.IsNullOrEmpty(match.userIdA) || string.IsNullOrEmpty(match.userIdB))
                {
                    continue;
                }

                bool aBye = TournamentKeys.IsBye(match.userIdA);
                bool bBye = TournamentKeys.IsBye(match.userIdB);

                if (!aBye && !bBye)
                {
                    continue;
                }

                string winner;
                if (!aBye && bBye)
                {
                    winner = match.userIdA;
                }
                else if (aBye && !bBye)
                {
                    winner = match.userIdB;
                }
                else
                {
                    winner = TournamentKeys.ByeUserId;
                }

                CompleteMatch(match, winner);
                match.seriesWinsA = TournamentKeys.IsBye(match.userIdA) ? 0 : (winner == match.userIdA ? TournamentKeys.WinsToTakeSeries : 0);
                match.seriesWinsB = TournamentKeys.IsBye(match.userIdB) ? 0 : (winner == match.userIdB ? TournamentKeys.WinsToTakeSeries : 0);
                match.gameIndex = TournamentKeys.WinsToTakeSeries;
                progressed = true;
            }
        }
        while (progressed && guard < 64);
    }

    public TournamentMatchSlot GetMatch(int round, int matchIndex)
    {
        if (matches == null)
        {
            return null;
        }

        for (int i = 0; i < matches.Length; i++)
        {
            if (matches[i] != null && matches[i].round == round && matches[i].matchIndex == matchIndex)
            {
                return matches[i];
            }
        }

        return null;
    }

    public TournamentMatchSlot FindActiveMatchFor(string userId)
    {
        if (matches == null || string.IsNullOrEmpty(userId))
        {
            return null;
        }

        int size = ResolvedPlayerCount;
        int finalRound = TournamentKeys.FinalRoundFor(size);
        for (int round = 0; round <= finalRound; round++)
        {
            int matchCount = TournamentKeys.MatchesInRoundFor(size, round);
            for (int m = 0; m < matchCount; m++)
            {
                var match = GetMatch(round, m);
                if (match == null || match.complete)
                {
                    continue;
                }

                if (match.userIdA == userId || match.userIdB == userId)
                {
                    return match;
                }
            }
        }

        return null;
    }

    public TournamentPlayerSlot GetPlayer(string userId)
    {
        if (players == null || string.IsNullOrEmpty(userId))
        {
            return null;
        }

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && players[i].userId == userId)
            {
                return players[i];
            }
        }

        return null;
    }

    public string DisplayName(string userId)
    {
        if (TournamentKeys.IsBye(userId))
        {
            return "BYE";
        }

        var player = GetPlayer(userId);
        if (player == null)
        {
            return "?";
        }

        return string.IsNullOrEmpty(player.nickName) ? player.userId : player.nickName;
    }

    public string LockedDeckCode(string userId)
    {
        var player = GetPlayer(userId);
        return player != null ? player.lockedDeckCode : null;
    }

    public bool IsPlayerA(TournamentMatchSlot match, string userId)
    {
        return match != null && match.userIdA == userId;
    }

    public void ApplyLocalDeckSnapshot(Dictionary<string, TournamentPlayerSlot> snapshot)
    {
        if (players == null || snapshot == null)
        {
            return;
        }

        for (int i = 0; i < players.Length; i++)
        {
            var slot = players[i];
            if (slot == null || string.IsNullOrEmpty(slot.userId))
            {
                continue;
            }

            if (!snapshot.TryGetValue(slot.userId, out var src) || src == null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(src.lockedDeckCode))
            {
                slot.lockedDeckCode = src.lockedDeckCode;
            }

            if (!string.IsNullOrEmpty(src.lockedDeckName))
            {
                slot.lockedDeckName = src.lockedDeckName;
            }

            if (!string.IsNullOrEmpty(src.nickName))
            {
                slot.nickName = src.nickName;
            }
        }
    }

    public void MergeFrom(TournamentState other)
    {
        if (other == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(other.tourneyId))
        {
            tourneyId = other.tourneyId;
        }

        useBanlist = other.useBanlist;
        started = started || other.started;
        finished = finished || other.finished;
        if (other.bracketSeed != 0)
        {
            bracketSeed = other.bracketSeed;
        }

        if (!string.IsNullOrEmpty(other.championUserId))
        {
            championUserId = other.championUserId;
        }

        if (other.playerCount > 0)
        {
            playerCount = TournamentKeys.NormalizePlayerCount(other.playerCount);
        }

        if (other.players != null)
        {
            if (players == null || players.Length != other.players.Length)
            {
                players = new TournamentPlayerSlot[other.players.Length];
            }

            for (int i = 0; i < other.players.Length; i++)
            {
                var incoming = other.players[i];
                if (incoming == null)
                {
                    continue;
                }

                if (players[i] == null)
                {
                    players[i] = incoming;
                    continue;
                }

                players[i].userId = incoming.userId;
                players[i].nickName = string.IsNullOrEmpty(incoming.nickName) ? players[i].nickName : incoming.nickName;
                players[i].lockedDeckName = string.IsNullOrEmpty(incoming.lockedDeckName)
                    ? players[i].lockedDeckName
                    : incoming.lockedDeckName;
                if (!string.IsNullOrEmpty(incoming.lockedDeckCode))
                {
                    players[i].lockedDeckCode = incoming.lockedDeckCode;
                }

                players[i].seed = incoming.seed;
                players[i].eliminated = players[i].eliminated || incoming.eliminated;
            }
        }

        if (other.matches != null)
        {
            if (matches == null || matches.Length != other.matches.Length)
            {
                matches = other.matches;
            }
            else
            {
                for (int i = 0; i < other.matches.Length; i++)
                {
                    MergeMatch(matches[i], other.matches[i]);
                }
            }
        }
    }

    static void MergeMatch(TournamentMatchSlot dest, TournamentMatchSlot src)
    {
        if (dest == null || src == null)
        {
            return;
        }

        dest.round = src.round;
        dest.matchIndex = src.matchIndex;
        if (!string.IsNullOrEmpty(src.userIdA)) dest.userIdA = src.userIdA;
        if (!string.IsNullOrEmpty(src.userIdB)) dest.userIdB = src.userIdB;
        dest.seriesWinsA = Math.Max(dest.seriesWinsA, src.seriesWinsA);
        dest.seriesWinsB = Math.Max(dest.seriesWinsB, src.seriesWinsB);
        dest.gameIndex = Math.Max(dest.gameIndex, src.gameIndex);
        if (!string.IsNullOrEmpty(src.winnerUserId)) dest.winnerUserId = src.winnerUserId;
        if (!string.IsNullOrEmpty(src.lastGameLoserUserId)) dest.lastGameLoserUserId = src.lastGameLoserUserId;
        dest.complete = dest.complete || src.complete;
    }

    public void ApplyGameResult(int round, int matchIndex, string winnerUserId, bool seriesComplete)
    {
        var match = GetMatch(round, matchIndex);
        if (match == null || string.IsNullOrEmpty(winnerUserId))
        {
            return;
        }

        bool winnerIsA = match.userIdA == winnerUserId;
        if (winnerIsA)
        {
            match.seriesWinsA++;
            match.lastGameLoserUserId = match.userIdB;
        }
        else
        {
            match.seriesWinsB++;
            match.lastGameLoserUserId = match.userIdA;
        }

        match.gameIndex++;

        if (seriesComplete ||
            match.seriesWinsA >= TournamentKeys.WinsToTakeSeries ||
            match.seriesWinsB >= TournamentKeys.WinsToTakeSeries)
        {
            CompleteMatch(match, winnerUserId);
            // Winner may land on a bracket side that is already BYE (sparse brackets).
            ResolveOpeningByes();
        }
    }

    public void CompleteMatch(TournamentMatchSlot match, string winnerUserId)
    {
        if (match == null)
        {
            return;
        }

        match.complete = true;
        match.winnerUserId = winnerUserId;
        string loserId = match.userIdA == winnerUserId ? match.userIdB : match.userIdA;
        if (!TournamentKeys.IsBye(loserId))
        {
            var loser = GetPlayer(loserId);
            if (loser != null)
            {
                loser.eliminated = true;
            }
        }

        if (match.round >= TournamentKeys.FinalRoundFor(ResolvedPlayerCount))
        {
            finished = true;
            championUserId = TournamentKeys.IsBye(winnerUserId) ? null : winnerUserId;
            return;
        }

        int nextRound = match.round + 1;
        int nextMatchIndex = match.matchIndex / 2;
        var next = GetMatch(nextRound, nextMatchIndex);
        if (next == null)
        {
            return;
        }

        bool goesToA = (match.matchIndex % 2) == 0;
        if (goesToA)
        {
            next.userIdA = winnerUserId;
        }
        else
        {
            next.userIdB = winnerUserId;
        }
    }

    public string FormatBracket()
    {
        var sb = new System.Text.StringBuilder();
        int size = ResolvedPlayerCount;
        int finalRound = TournamentKeys.FinalRoundFor(size);
        for (int round = 0; round <= finalRound; round++)
        {
            AppendRound(
                sb,
                round,
                TournamentKeys.MatchesInRoundFor(size, round),
                TournamentKeys.RoundDisplayNameFor(size, round));
        }

        if (!string.IsNullOrEmpty(championUserId))
        {
            sb.AppendLine();
            sb.Append("Champion: ").Append(DisplayName(championUserId));
        }

        return sb.ToString();
    }

    void AppendRound(System.Text.StringBuilder sb, int round, int count, string title)
    {
        sb.AppendLine(title);
        for (int i = 0; i < count; i++)
        {
            var match = GetMatch(round, i);
            if (match == null)
            {
                continue;
            }

            string a = string.IsNullOrEmpty(match.userIdA) ? "TBD" : DisplayName(match.userIdA);
            string b = string.IsNullOrEmpty(match.userIdB) ? "TBD" : DisplayName(match.userIdB);
            bool byeMatch =
                (!string.IsNullOrEmpty(match.userIdA) && TournamentKeys.IsBye(match.userIdA)) ||
                (!string.IsNullOrEmpty(match.userIdB) && TournamentKeys.IsBye(match.userIdB));
            sb.Append("  ").Append(a).Append(" vs ").Append(b);
            if (byeMatch && match.complete)
            {
                sb.Append("  (bye)");
            }
            else if (match.seriesWinsA > 0 || match.seriesWinsB > 0 || match.complete)
            {
                sb.Append("  ").Append(match.seriesWinsA).Append('-').Append(match.seriesWinsB);
            }

            if (match.complete && !string.IsNullOrEmpty(match.winnerUserId) && !TournamentKeys.IsBye(match.winnerUserId))
            {
                sb.Append("  (").Append(DisplayName(match.winnerUserId)).Append(')');
            }

            sb.AppendLine();
        }
    }

    public static string ReadPlayerId(Photon.Realtime.Player player)
    {
        if (player == null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(player.UserId))
        {
            return player.UserId;
        }

        if (player.CustomProperties != null &&
            player.CustomProperties.TryGetValue(TournamentKeys.PlayerIdProperty, out object value) &&
            value is string id && !string.IsNullOrEmpty(id))
        {
            return id;
        }

        return null;
    }

    public static string EnsureLocalPlayerId()
    {
        var cc = ContinuousController.instance;
        if (cc != null && !string.IsNullOrEmpty(cc.TournamentPlayerId))
        {
            return cc.TournamentPlayerId;
        }

        string id = ReadPlayerId(PhotonNetwork.LocalPlayer);
        if (string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString("N");
            var hash = PhotonNetwork.LocalPlayer.CustomProperties ?? new Hashtable();
            hash[TournamentKeys.PlayerIdProperty] = id;
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }

        if (cc != null)
        {
            cc.TournamentPlayerId = id;
        }

        return id;
    }

    public static string ReadDeckCode(Photon.Realtime.Player player)
    {
        if (player == null || player.CustomProperties == null)
        {
            return null;
        }

        if (player.CustomProperties.TryGetValue(TournamentKeys.LockedDeckProperty, out object locked) &&
            locked is string lockedCode && !string.IsNullOrEmpty(lockedCode))
        {
            return lockedCode;
        }

        if (player.CustomProperties.TryGetValue(ContinuousController.DeckDataPropertyKey, out object value) &&
            value is string code && !string.IsNullOrEmpty(code))
        {
            return code;
        }

        return null;
    }

    public static string ReadNickName(Photon.Realtime.Player player)
    {
        if (player == null)
        {
            return "Player";
        }

        if (player.CustomProperties != null &&
            player.CustomProperties.TryGetValue(ContinuousController.PlayerNameKey, out object value) &&
            value is string name && !string.IsNullOrEmpty(name))
        {
            return name;
        }

        return string.IsNullOrEmpty(player.NickName) ? "Player" : player.NickName;
    }
}
