using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

// Pure game stats logic (no UnityEngine references) so it can be unit-tested
// without a running Unity instance.
public static class GameStatsUtility
{
    #region Sides and labels
    public const string Player1Side = "player1";
    public const string Player2Side = "player2";

    public const string Player1Label = "Player1";
    public const string Player2Label = "Player2";

    //Player.PlayerName returns this for the other player in PvP
    public const string OpponentName = "Opponent";

    //Used when the local player is also named "Opponent", as the two sides cannot be told apart
    public const string UnknownLabel = "Unknown";

    public static string OtherSide(string side)
    {
        return side == Player1Side ? Player2Side : Player1Side;
    }

    public static string SideLabel(string side)
    {
        return side == Player1Side ? Player1Label : Player2Label;
    }
    #endregion

    #region Match ID
    //Both clients share the room name and the seed, so they derive the same ID
    public static string MatchId(string roomName, long seed)
    {
        string source = $"{roomName}:{seed.ToString(CultureInfo.InvariantCulture)}";

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(source));

            StringBuilder sb = new StringBuilder(hash.Length * 2);

            foreach (byte b in hash)
            {
                sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }
    }
    #endregion

    #region End reason
    public static string EndReason(bool surrendered, string effectName, bool hasWinner, bool bothPlayersInRoom, bool endedByReconnect = false)
    {
        //Every ending decided by ReconnectManager comes from a lost connection, even when it names a winner
        if (endedByReconnect)
        {
            return "disconnect";
        }

        if (surrendered)
        {
            return "surrender";
        }

        if (!string.IsNullOrEmpty(effectName))
        {
            return "effect";
        }

        if (hasWinner)
        {
            return "normal";
        }

        //Same check as ResultObject uses to tell a draw from a disconnect
        return bothPlayersInRoom ? "draw" : "disconnect";
    }
    #endregion

    #region Retry delay
    //Longest wait between attempts, so a report is not held for too long
    public const int MaxRetryDelayMilliseconds = 60000;

    //Waits as long as the collector asks with Retry-After (in seconds), otherwise uses the fallback
    public static int RetryDelayMilliseconds(string retryAfter, int fallbackMilliseconds)
    {
        int delay = fallbackMilliseconds;

        if (int.TryParse(retryAfter, NumberStyles.None, CultureInfo.InvariantCulture, out int seconds) && seconds > 0)
        {
            delay = seconds >= MaxRetryDelayMilliseconds / 1000 ? MaxRetryDelayMilliseconds : seconds * 1000;
        }

        return Math.Min(Math.Max(delay, 0), MaxRetryDelayMilliseconds);
    }
    #endregion

    #region Deck code
    //The deck code starts with the deck name, which the player writes, so it is removed
    public static string StripDeckName(string deckCode)
    {
        if (string.IsNullOrEmpty(deckCode))
        {
            return "";
        }

        int nameEnd = deckCode.IndexOf(',');

        return nameEnd < 0 ? deckCode : deckCode.Substring(nameEnd);
    }
    #endregion

    #region Timestamp
    public static string FormatUtc(DateTime time)
    {
        return time.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
    }
    #endregion

    #region Log anonymization
    //Prefix of the log line written by ResultObject
    const string WinnerPrefix = "Winner:";

    //"Add {count} cards to security:{name}" written by CardController
    static readonly Regex SecurityLineRegex = new Regex(@"^(Add \d+ cards? to security:)(.*)$", RegexOptions.CultureInvariant);

    //Player names only appear in the log as a whole line, or after a known prefix.
    //Only those spots are replaced, so a short name inside card text is left alone.
    public static string AnonymizeLog(string log, string localName, string localLabel, string opponentLabel)
    {
        if (string.IsNullOrEmpty(log))
        {
            return "";
        }

        if (string.IsNullOrWhiteSpace(localName))
        {
            localName = null;
        }

        //A name with a line break cannot be matched by line, so replace it as a whole
        else if (localName.IndexOf('\n') >= 0 || localName.IndexOf('\r') >= 0)
        {
            log = log.Replace(localName, localName == OpponentName ? UnknownLabel : localLabel);
            localName = null;
        }

        string[] lines = log.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string lineEnd = "";

            if (line.EndsWith("\r", StringComparison.Ordinal))
            {
                line = line.Substring(0, line.Length - 1);
                lineEnd = "\r";
            }

            lines[i] = AnonymizeLine(line, localName, localLabel, opponentLabel) + lineEnd;
        }

        return string.Join("\n", lines);
    }

    static string AnonymizeLine(string line, string localName, string localLabel, string opponentLabel)
    {
        string label = NameLabel(line, localName, localLabel, opponentLabel);

        if (label != null)
        {
            return label;
        }

        if (line.StartsWith(WinnerPrefix, StringComparison.Ordinal))
        {
            label = NameLabel(line.Substring(WinnerPrefix.Length), localName, localLabel, opponentLabel);

            if (label != null)
            {
                return WinnerPrefix + label;
            }
        }

        Match match = SecurityLineRegex.Match(line);

        if (match.Success)
        {
            label = NameLabel(match.Groups[2].Value, localName, localLabel, opponentLabel);

            if (label != null)
            {
                return match.Groups[1].Value + label;
            }
        }

        return line;
    }

    //Label for an exact player name, or null when the text is not a player name
    static string NameLabel(string text, string localName, string localLabel, string opponentLabel)
    {
        if (localName != null && text == localName)
        {
            return localName == OpponentName ? UnknownLabel : localLabel;
        }

        if (text == OpponentName)
        {
            return opponentLabel;
        }

        return null;
    }
    #endregion
}
