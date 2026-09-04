using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates ranked match sessions and reports results to PlayFab CloudScript (or offline Elo).
/// </summary>
public class RankedMatchService
{
    readonly Dictionary<string, OfflineMatchState> _offlineMatches = new Dictionary<string, OfflineMatchState>();

    public string ActiveMatchId { get; private set; }
    public string OpponentPlayFabId { get; private set; }
    public int OpponentMmr { get; private set; }

    class OfflineMatchState
    {
        public string playerA;
        public string playerB;
        public int mmrA;
        public int mmrB;
        public string winnerId;
        public string loserId;
        public bool settled;
        public RankedMatchReportResult resultA;
        public RankedMatchReportResult resultB;
    }

    public void EnsureActiveMatch(string matchId, string opponentPlayFabId, int opponentMmr)
    {
        ActiveMatchId = matchId;
        OpponentPlayFabId = opponentPlayFabId;
        OpponentMmr = opponentMmr;
    }

    public IEnumerator BeginMatch(
        PlayFabAuthService auth,
        RankedProfileService profileService,
        string matchId,
        string opponentPlayFabId,
        int opponentMmr,
        Action<bool, string> onComplete = null)
    {
        ActiveMatchId = matchId;
        OpponentPlayFabId = opponentPlayFabId;
        OpponentMmr = opponentMmr;

        if (auth == null || !auth.IsLoggedIn)
        {
            onComplete?.Invoke(false, "Not logged in");
            yield break;
        }

        if (auth.IsOfflineMode || !PlayFabConfig.Current.HasTitleId)
        {
            string selfId = auth.PlayFabId;
            int selfMmr = profileService.Cached?.mmr ?? RankedRating.DefaultMmr;
            _offlineMatches[matchId] = new OfflineMatchState
            {
                playerA = selfId,
                playerB = opponentPlayFabId,
                mmrA = selfMmr,
                mmrB = opponentMmr > 0 ? opponentMmr : RankedRating.DefaultMmr,
            };
            onComplete?.Invoke(true, null);
            yield break;
        }

        bool done = false;
        bool ok = false;
        string error = null;

        var param = new Dictionary<string, object>
        {
            { "matchId", matchId },
            { "opponentPlayFabId", opponentPlayFabId },
            { "selfMmr", profileService.Cached?.mmr ?? RankedRating.DefaultMmr },
            { "opponentMmr", opponentMmr },
        };

        yield return PlayFabClientApi.ExecuteCloudScript(
            PlayFabConfig.Current.titleId,
            RankedKeys.CloudBeginMatch,
            param,
            (result, fn) =>
            {
                ok = result.success && (fn == null || !PlayFabClientApi.GetBool(fn, "error", false));
                if (!ok)
                {
                    error = result.errorMessage;
                    if (fn != null)
                    {
                        string fe = PlayFabClientApi.GetString(fn, "errorMessage");
                        if (!string.IsNullOrEmpty(fe)) error = fe;
                    }
                }
                else if (fn != null && PlayFabClientApi.GetInt(fn, "mmr", -1) >= 0)
                {
                    // Server may return seeded stats
                    int mmr = PlayFabClientApi.GetInt(fn, "mmr", RankedRating.DefaultMmr);
                    int wins = PlayFabClientApi.GetInt(fn, "wins", profileService.Cached?.wins ?? 0);
                    int losses = PlayFabClientApi.GetInt(fn, "losses", profileService.Cached?.losses ?? 0);
                    profileService.ApplyLocalResult(false, mmr, wins, losses, auth);
                }

                done = true;
            });

        while (!done) yield return null;
        onComplete?.Invoke(ok, error);
    }

    public IEnumerator ReportResult(
        PlayFabAuthService auth,
        RankedProfileService profileService,
        bool localWon,
        bool surrendered,
        bool disconnect,
        Action<RankedMatchReportResult> onComplete = null)
    {
        var report = new RankedMatchReportResult();

        if (string.IsNullOrEmpty(ActiveMatchId) || auth == null || !auth.IsLoggedIn)
        {
            report.success = false;
            report.errorMessage = "No active ranked match";
            onComplete?.Invoke(report);
            yield break;
        }

        string selfId = auth.PlayFabId;
        string winnerId = localWon ? selfId : OpponentPlayFabId;
        string loserId = localWon ? OpponentPlayFabId : selfId;

        // Disconnect: local client still connected reports opponent left.
        if (disconnect)
        {
            winnerId = selfId;
            loserId = OpponentPlayFabId;
            localWon = true;
        }

        if (auth.IsOfflineMode || !PlayFabConfig.Current.HasTitleId)
        {
            Debug.LogWarning("[Ranked] ReportResult using offline path — PlayFab statistics will NOT update.");
            report = SettleOffline(auth, profileService, ActiveMatchId, winnerId, loserId, localWon);
            onComplete?.Invoke(report);
            yield break;
        }

        // Don't report with both sides missing real ids
        if (string.IsNullOrEmpty(winnerId) || string.IsNullOrEmpty(loserId))
        {
            report.success = false;
            report.errorMessage = "Missing winner/loser PlayFabId";
            onComplete?.Invoke(report);
            yield break;
        }

        bool done = false;
        var param = new Dictionary<string, object>
        {
            { "matchId", ActiveMatchId },
            { "winnerPlayFabId", winnerId },
            { "loserPlayFabId", loserId },
            { "surrendered", surrendered },
            { "disconnect", disconnect },
        };

        Debug.Log($"[Ranked] ExecuteCloudScript ReportRankedMatch args={MiniJson.Serialize(param)}");

        yield return PlayFabClientApi.ExecuteCloudScript(
            PlayFabConfig.Current.titleId,
            RankedKeys.CloudReportMatch,
            param,
            (result, fn) =>
            {
                if (!result.success)
                {
                    report.success = false;
                    report.errorMessage = result.errorMessage;
                    done = true;
                    return;
                }

                if (fn == null)
                {
                    report.success = false;
                    report.errorMessage = "Empty CloudScript result (is ReportRankedMatch deployed?)";
                    done = true;
                    return;
                }

                if (PlayFabClientApi.GetBool(fn, "error", false))
                {
                    report.success = false;
                    report.errorMessage = PlayFabClientApi.GetString(fn, "errorMessage") ?? "Report failed";
                    done = true;
                    return;
                }

                report.success = true;
                report.alreadyProcessed = PlayFabClientApi.GetBool(fn, "alreadyProcessed");
                report.pendingOpponent = PlayFabClientApi.GetBool(fn, "pendingOpponent");
                report.mmr = PlayFabClientApi.GetInt(fn, "mmr", profileService.Cached?.mmr ?? RankedRating.DefaultMmr);
                report.mmrDelta = PlayFabClientApi.GetInt(fn, "mmrDelta");
                report.wins = PlayFabClientApi.GetInt(fn, "wins", profileService.Cached?.wins ?? 0);
                report.losses = PlayFabClientApi.GetInt(fn, "losses", profileService.Cached?.losses ?? 0);
                report.tierName = PlayFabClientApi.GetString(fn, "tier") ?? RankedRating.GetTierName(report.mmr);

                if (!report.pendingOpponent)
                {
                    profileService.ApplyLocalResult(localWon, report.mmr, report.wins, report.losses, auth);
                }

                done = true;
            });

        while (!done) yield return null;
        onComplete?.Invoke(report);
    }

    public IEnumerator CancelMatch(PlayFabAuthService auth, Action onComplete = null)
    {
        string matchId = ActiveMatchId;
        ActiveMatchId = null;
        OpponentPlayFabId = null;
        OpponentMmr = 0;

        if (string.IsNullOrEmpty(matchId) || auth == null || !auth.IsLoggedIn)
        {
            onComplete?.Invoke();
            yield break;
        }

        if (auth.IsOfflineMode || !PlayFabConfig.Current.HasTitleId)
        {
            _offlineMatches.Remove(matchId);
            onComplete?.Invoke();
            yield break;
        }

        bool done = false;
        yield return PlayFabClientApi.ExecuteCloudScript(
            PlayFabConfig.Current.titleId,
            RankedKeys.CloudCancelMatch,
            new Dictionary<string, object> { { "matchId", matchId } },
            (_, __) => { done = true; });
        while (!done) yield return null;
        onComplete?.Invoke();
    }

    RankedMatchReportResult SettleOffline(
        PlayFabAuthService auth,
        RankedProfileService profileService,
        string matchId,
        string winnerId,
        string loserId,
        bool localWon)
    {
        var report = new RankedMatchReportResult();
        int selfMmr = profileService.Cached?.mmr ?? RankedRating.DefaultMmr;
        int oppMmr = OpponentMmr > 0 ? OpponentMmr : RankedRating.DefaultMmr;

        // Dual-report agreement: wait for both reports if we have state; otherwise apply immediately for local UX.
        if (!_offlineMatches.TryGetValue(matchId, out var state))
        {
            state = new OfflineMatchState
            {
                playerA = auth.PlayFabId,
                playerB = OpponentPlayFabId,
                mmrA = selfMmr,
                mmrB = oppMmr,
            };
            _offlineMatches[matchId] = state;
        }

        if (state.settled)
        {
            report.success = true;
            report.alreadyProcessed = true;
            report.mmr = profileService.Cached?.mmr ?? selfMmr;
            report.wins = profileService.Cached?.wins ?? 0;
            report.losses = profileService.Cached?.losses ?? 0;
            report.tierName = RankedRating.GetTierName(report.mmr);
            return report;
        }

        // Offline v1: trust first consistent report on each client and apply local Elo
        // (each client updates only own profile; mutual agreement is enforced when PlayFab is live).
        if (!string.IsNullOrEmpty(state.winnerId) && state.winnerId != winnerId)
        {
            report.success = false;
            report.errorMessage = "Conflicting match reports";
            return report;
        }

        state.winnerId = winnerId;
        state.loserId = loserId;

        int winnerMmr = string.Equals(winnerId, auth.PlayFabId, StringComparison.Ordinal) ? selfMmr : oppMmr;
        int loserMmr = string.Equals(loserId, auth.PlayFabId, StringComparison.Ordinal) ? selfMmr : oppMmr;
        RankedRating.ApplyElo(winnerMmr, loserMmr, out int newW, out int newL, out int dW, out int dL);

        int newSelf = localWon ? newW : newL;
        int delta = localWon ? dW : dL;
        int wins = (profileService.Cached?.wins ?? 0) + (localWon ? 1 : 0);
        int losses = (profileService.Cached?.losses ?? 0) + (localWon ? 0 : 1);

        profileService.ApplyLocalResult(localWon, newSelf, wins, losses, auth);
        state.settled = true;

        report.success = true;
        report.mmr = newSelf;
        report.mmrDelta = delta;
        report.wins = wins;
        report.losses = losses;
        report.tierName = RankedRating.GetTierName(newSelf);
        return report;
    }

    public void ClearActiveMatch()
    {
        ActiveMatchId = null;
        OpponentPlayFabId = null;
        OpponentMmr = 0;
    }
}
