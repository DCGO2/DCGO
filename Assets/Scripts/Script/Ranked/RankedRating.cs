using System;

/// <summary>
/// Elo rating helpers and tier thresholds for ranked ladder.
/// </summary>
public static class RankedRating
{
    public const int DefaultMmr = 1000;
    public const int KFactor = 32;

    public enum Tier
    {
        Bronze = 0,
        Silver = 1,
        Gold = 2,
        Platinum = 3,
        Diamond = 4,
        Master = 5,
    }

    public static Tier GetTier(int mmr)
    {
        if (mmr >= 1800) return Tier.Master;
        if (mmr >= 1600) return Tier.Diamond;
        if (mmr >= 1400) return Tier.Platinum;
        if (mmr >= 1200) return Tier.Gold;
        if (mmr >= 1000) return Tier.Silver;
        return Tier.Bronze;
    }

    public static string GetTierName(int mmr) => GetTier(mmr).ToString();

    public static string GetTierNameLocalized(int mmr)
    {
        var tier = GetTier(mmr);
        return LocalizeUtility.GetLocalizedString(
            EngMessage: tier.ToString(),
            JpnMessage: tier switch
            {
                Tier.Bronze => "ブロンズ",
                Tier.Silver => "シルバー",
                Tier.Gold => "ゴールド",
                Tier.Platinum => "プラチナ",
                Tier.Diamond => "ダイヤモンド",
                Tier.Master => "マスター",
                _ => tier.ToString(),
            });
    }

    /// <summary>Short label for battle name plates: "Silver 1056".</summary>
    public static string FormatBesideName(int mmr)
    {
        return $"{GetTierNameLocalized(mmr)} {mmr}";
    }

    /// <summary>MMR bucket used for lobby property filters (50-point steps).</summary>
    public static int GetMmrBucket(int mmr)
    {
        return (Math.Max(0, mmr) / 50) * 50;
    }

    public static double ExpectedScore(int rating, int opponentRating)
    {
        return 1.0 / (1.0 + Math.Pow(10.0, (opponentRating - rating) / 400.0));
    }

    public static void ApplyElo(int winnerMmr, int loserMmr, out int newWinnerMmr, out int newLoserMmr, out int winnerDelta, out int loserDelta)
    {
        double expW = ExpectedScore(winnerMmr, loserMmr);
        double expL = ExpectedScore(loserMmr, winnerMmr);

        winnerDelta = (int)Math.Round(KFactor * (1.0 - expW));
        loserDelta = (int)Math.Round(KFactor * (0.0 - expL));

        newWinnerMmr = Math.Max(0, winnerMmr + winnerDelta);
        newLoserMmr = Math.Max(0, loserMmr + loserDelta);
    }

    /// <summary>Search tolerance expansion by seconds spent in queue.</summary>
    public static int GetMmrTolerance(float searchSeconds)
    {
        if (searchSeconds < 30f) return 100;
        if (searchSeconds < 60f) return 200;
        if (searchSeconds < 90f) return 400;
        return 9999;
    }
}
