/// <summary>
/// Photon property keys and match constants for ranked mode.
/// </summary>
public static class RankedKeys
{
    public const string RoomNameKey = "rankedMatchRoom";
    public const string ModeProperty = "Mode";
    public const string ModeRanked = "ranked";
    public const string MmrBucketProperty = "MmrBucket";
    public const string MmrProperty = "RankedMMR";
    public const string PlayFabIdProperty = "PlayFabId";
    public const string MatchIdProperty = "RankedMatchId";
    public const string UseBanlistProperty = "UseBanlist";

    public const string StatMmr = "RankedMMR";
    public const string StatWins = "RankedWins";
    public const string StatLosses = "RankedLosses";

    public const string CloudBeginMatch = "BeginRankedMatch";
    public const string CloudReportMatch = "ReportRankedMatch";
    public const string CloudCancelMatch = "CancelRankedMatch";
    public const string CloudGetProfile = "GetRankedProfile";
}
