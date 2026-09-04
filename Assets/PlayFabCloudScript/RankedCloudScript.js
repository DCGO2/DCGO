// Deploy this to PlayFab Title → Automation → Cloud Script (set as active revision).
// Classic player statistics: RankedMMR, RankedWins, RankedLosses
// NOTE: v1 settles on the FIRST valid report so UpdatePlayerStatistics always runs
// even if the opponent never reports (disconnect / client quit). Second agreeing
// report is idempotent; conflicting second report is rejected without reversing.

handlers.BeginRankedMatch = function (args, context) {
    var matchId = args && args.matchId;
    var opponentId = args && args.opponentPlayFabId;
    if (!matchId || !opponentId) {
        return { error: true, errorMessage: "matchId and opponentPlayFabId required" };
    }

    var selfId = currentPlayerId;
    ensurePlayerStats(selfId);

    var key = "ranked_match_" + matchId;
    var existing = server.GetTitleInternalData({ Keys: [key] });
    var dataMap = (existing && existing.Data) ? existing.Data : {};
    if (dataMap[key]) {
        try {
            var parsed = JSON.parse(dataMap[key]);
            if (parsed.status === "settled") {
                var sSettled = getStats(selfId);
                return { success: true, alreadySettled: true, mmr: sSettled.mmr, wins: sSettled.wins, losses: sSettled.losses, tier: tierName(sSettled.mmr) };
            }
            if (parsed.playerA !== selfId && parsed.playerB !== selfId) {
                if (!parsed.playerB || parsed.playerB === "unknown") {
                    parsed.playerB = selfId;
                }
            }
            server.SetTitleInternalData({ Key: key, Value: JSON.stringify(parsed) });
            var stats = getStats(selfId);
            return { success: true, mmr: stats.mmr, wins: stats.wins, losses: stats.losses, tier: tierName(stats.mmr) };
        } catch (e) {
            // recreate below
        }
    }

    var record = {
        playerA: selfId,
        playerB: opponentId,
        status: "active",
        reports: {},
        createdAt: Date.now()
    };
    server.SetTitleInternalData({ Key: key, Value: JSON.stringify(record) });

    var s = getStats(selfId);
    return { success: true, mmr: s.mmr, wins: s.wins, losses: s.losses, tier: tierName(s.mmr) };
};

handlers.ReportRankedMatch = function (args, context) {
    try {
        var matchId = args && args.matchId;
        var winnerId = args && args.winnerPlayFabId;
        var loserId = args && args.loserPlayFabId;
        if (!matchId || !winnerId || !loserId) {
            return { error: true, errorMessage: "matchId, winnerPlayFabId, loserPlayFabId required" };
        }

        var selfId = currentPlayerId;

        // Resolve "unknown" opponent ids to the other participant when possible
        if (winnerId === "unknown" && selfId === loserId) {
            // we reported loss but winner unknown — cannot settle cleanly
            return { error: true, errorMessage: "winnerPlayFabId is unknown" };
        }
        if (loserId === "unknown" && selfId === winnerId) {
            // forfeit win with unknown leaver — still need a synthetic loser for stats
            // use a placeholder so winner gets Elo; skip loser Update if unknown
        }

        // Reporter must be winner or loser (or loser/winner unknown edge)
        if (selfId !== winnerId && selfId !== loserId && loserId !== "unknown" && winnerId !== "unknown") {
            return { error: true, errorMessage: "Reporter is not a match participant" };
        }

        var key = "ranked_match_" + matchId;
        var existing = server.GetTitleInternalData({ Keys: [key] });
        var raw = existing && existing.Data ? existing.Data[key] : null;
        if (!raw) {
            var auto = {
                playerA: selfId,
                playerB: (selfId === winnerId) ? loserId : winnerId,
                status: "active",
                reports: {},
                createdAt: Date.now()
            };
            server.SetTitleInternalData({ Key: key, Value: JSON.stringify(auto) });
            raw = JSON.stringify(auto);
        }

        var match = JSON.parse(raw);
        if (match.status === "settled" && match.result) {
            var selfStatsDone = getStats(selfId);
            var deltaDone = 0;
            if (match.result.deltas && match.result.deltas[selfId] != null) {
                deltaDone = match.result.deltas[selfId];
            }
            return {
                success: true,
                alreadyProcessed: true,
                pendingOpponent: false,
                mmr: selfStatsDone.mmr,
                mmrDelta: deltaDone,
                wins: selfStatsDone.wins,
                losses: selfStatsDone.losses,
                tier: tierName(selfStatsDone.mmr)
            };
        }

        match.reports = match.reports || {};
        match.reports[selfId] = {
            winnerPlayFabId: winnerId,
            loserPlayFabId: loserId,
            surrendered: !!(args && args.surrendered),
            disconnect: !!(args && args.disconnect),
            at: Date.now()
        };

        // If a second reporter disagrees with first stored report before settle, conflict
        var reportIds = Object.keys(match.reports);
        if (reportIds.length >= 2) {
            var r0 = match.reports[reportIds[0]];
            var r1 = match.reports[reportIds[1]];
            if (r0.winnerPlayFabId !== r1.winnerPlayFabId || r0.loserPlayFabId !== r1.loserPlayFabId) {
                // Prefer non-unknown verdict if one side has real IDs
                if (r0.winnerPlayFabId === "unknown" || r0.loserPlayFabId === "unknown") {
                    winnerId = r1.winnerPlayFabId;
                    loserId = r1.loserPlayFabId;
                } else if (r1.winnerPlayFabId === "unknown" || r1.loserPlayFabId === "unknown") {
                    winnerId = r0.winnerPlayFabId;
                    loserId = r0.loserPlayFabId;
                } else {
                    match.status = "conflict";
                    server.SetTitleInternalData({ Key: key, Value: JSON.stringify(match) });
                    return { error: true, errorMessage: "Conflicting match reports" };
                }
            }
        }

        // Settle immediately on first valid report (apply Elo + UpdatePlayerStatistics)
        ensurePlayerStats(winnerId === "unknown" ? null : winnerId);
        ensurePlayerStats(loserId === "unknown" ? null : loserId);

        var w = winnerId !== "unknown" ? getStats(winnerId) : { mmr: 1000, wins: 0, losses: 0, mmrMissing: false };
        var l = loserId !== "unknown" ? getStats(loserId) : { mmr: 1000, wins: 0, losses: 0, mmrMissing: false };
        var elo = applyElo(w.mmr, l.mmr);

        if (winnerId !== "unknown") {
            server.UpdatePlayerStatistics({
                PlayFabId: winnerId,
                Statistics: [
                    { StatisticName: "RankedMMR", Value: elo.newWinner },
                    { StatisticName: "RankedWins", Value: w.wins + 1 }
                ]
            });
            log.info({ message: "Updated winner stats", PlayFabId: winnerId, mmr: elo.newWinner });
        }

        if (loserId !== "unknown") {
            server.UpdatePlayerStatistics({
                PlayFabId: loserId,
                Statistics: [
                    { StatisticName: "RankedMMR", Value: elo.newLoser },
                    { StatisticName: "RankedLosses", Value: l.losses + 1 }
                ]
            });
            log.info({ message: "Updated loser stats", PlayFabId: loserId, mmr: elo.newLoser });
        }

        match.status = "settled";
        match.result = {
            winnerPlayFabId: winnerId,
            loserPlayFabId: loserId,
            deltas: {}
        };
        if (winnerId !== "unknown") match.result.deltas[winnerId] = elo.winnerDelta;
        if (loserId !== "unknown") match.result.deltas[loserId] = elo.loserDelta;
        server.SetTitleInternalData({ Key: key, Value: JSON.stringify(match) });

        var after = getStats(selfId);
        var selfDelta = 0;
        if (match.result.deltas && match.result.deltas[selfId] != null) {
            selfDelta = match.result.deltas[selfId];
        }

        return {
            success: true,
            alreadyProcessed: false,
            pendingOpponent: false,
            mmr: after.mmr,
            mmrDelta: selfDelta,
            wins: after.wins,
            losses: after.losses,
            tier: tierName(after.mmr)
        };
    } catch (e) {
        return { error: true, errorMessage: "ReportRankedMatch exception: " + e };
    }
};

handlers.CancelRankedMatch = function (args, context) {
    var matchId = args && args.matchId;
    if (!matchId) {
        return { success: true };
    }
    var key = "ranked_match_" + matchId;
    var existing = server.GetTitleInternalData({ Keys: [key] });
    var raw = existing && existing.Data ? existing.Data[key] : null;
    if (!raw) {
        return { success: true };
    }
    try {
        var match = JSON.parse(raw);
        if (match.status === "active") {
            match.status = "cancelled";
            server.SetTitleInternalData({ Key: key, Value: JSON.stringify(match) });
        }
    } catch (e) { }
    return { success: true };
};

handlers.GetRankedProfile = function (args, context) {
    ensurePlayerStats(currentPlayerId);
    var s = getStats(currentPlayerId);
    return { mmr: s.mmr, wins: s.wins, losses: s.losses, tier: tierName(s.mmr) };
};

// ---- helpers ----
function ensurePlayerStats(playFabId) {
    if (!playFabId || playFabId === "unknown") {
        return;
    }
    var s = getStats(playFabId);
    if (s.mmrMissing) {
        server.UpdatePlayerStatistics({
            PlayFabId: playFabId,
            Statistics: [
                { StatisticName: "RankedMMR", Value: 1000 },
                { StatisticName: "RankedWins", Value: 0 },
                { StatisticName: "RankedLosses", Value: 0 }
            ]
        });
    }
}

function getStats(playFabId) {
    if (!playFabId || playFabId === "unknown") {
        return { mmr: 1000, wins: 0, losses: 0, mmrMissing: true };
    }
    var res = server.GetPlayerStatistics({
        PlayFabId: playFabId,
        StatisticNames: ["RankedMMR", "RankedWins", "RankedLosses"]
    });
    var mmr = 1000;
    var wins = 0;
    var losses = 0;
    var mmrMissing = true;
    if (res && res.Statistics) {
        for (var i = 0; i < res.Statistics.length; i++) {
            var st = res.Statistics[i];
            if (st.StatisticName === "RankedMMR") { mmr = st.Value; mmrMissing = false; }
            if (st.StatisticName === "RankedWins") wins = st.Value;
            if (st.StatisticName === "RankedLosses") losses = st.Value;
        }
    }
    return { mmr: mmr, wins: wins, losses: losses, mmrMissing: mmrMissing };
}

function expectedScore(rating, opponent) {
    return 1.0 / (1.0 + Math.pow(10.0, (opponent - rating) / 400.0));
}

function applyElo(winnerMmr, loserMmr) {
    var K = 32;
    var expW = expectedScore(winnerMmr, loserMmr);
    var expL = expectedScore(loserMmr, winnerMmr);
    var winnerDelta = Math.round(K * (1.0 - expW));
    var loserDelta = Math.round(K * (0.0 - expL));
    return {
        newWinner: Math.max(0, winnerMmr + winnerDelta),
        newLoser: Math.max(0, loserMmr + loserDelta),
        winnerDelta: winnerDelta,
        loserDelta: loserDelta
    };
}

function tierName(mmr) {
    if (mmr >= 1800) return "Master";
    if (mmr >= 1600) return "Diamond";
    if (mmr >= 1400) return "Platinum";
    if (mmr >= 1200) return "Gold";
    if (mmr >= 1000) return "Silver";
    return "Bronze";
}
