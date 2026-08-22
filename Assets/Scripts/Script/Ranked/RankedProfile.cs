using System;

[Serializable]
public class RankedProfile
{
    public string playFabId;
    public int mmr = RankedRating.DefaultMmr;
    public int wins;
    public int losses;
    public bool isOffline;

    public string TierName => RankedRating.GetTierName(mmr);
    public string TierNameLocalized => RankedRating.GetTierNameLocalized(mmr);
    public int MmrBucket => RankedRating.GetMmrBucket(mmr);

    public string FormatShort()
    {
        return $"{TierNameLocalized} {mmr}";
    }

    /// <summary>Localized status with Online/Offline source for UI (home, etc.).</summary>
    public string FormatStatusLine()
    {
        string sourceEn = isOffline ? "Offline" : "Online";
        string sourceJp = isOffline ? "オフライン" : "オンライン";
        return LocalizeUtility.GetLocalizedString(
            EngMessage: $"Ranked ({sourceEn}): {FormatShort()} ({wins}W-{losses}L)",
            JpnMessage: $"ランク（{sourceJp}）: {FormatShort()} ({wins}勝{losses}敗)");
    }

    /// <summary>Shorter two-line form (legacy; prefer FormatLobbyLine in matchmaking).</summary>
    public string FormatStatusLineCompact()
    {
        return FormatLobbyLine();
    }

    /// <summary>
    /// One short line for the ranked matchmaking modal (fits Random Match panel).
    /// Example: "Silver 1056 · 4W-0L · Online"
    /// </summary>
    public string FormatLobbyLine()
    {
        string sourceEn = isOffline ? "Offline" : "Online";
        string sourceJp = isOffline ? "オフライン" : "オンライン";
        return LocalizeUtility.GetLocalizedString(
            EngMessage: $"{FormatShort()} · {wins}W-{losses}L · {sourceEn}",
            JpnMessage: $"{FormatShort()} · {wins}勝{losses}敗 · {sourceJp}");
    }
}

[Serializable]
public class RankedMatchReportResult
{
    public bool success;
    public string errorMessage;
    public bool alreadyProcessed;
    public bool pendingOpponent;
    public int mmr;
    public int mmrDelta;
    public int wins;
    public int losses;
    public string tierName;
}
