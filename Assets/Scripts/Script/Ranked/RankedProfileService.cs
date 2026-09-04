using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Loads and caches the local player's ranked profile (MMR / wins / losses / tier).
/// </summary>
public class RankedProfileService
{
    const string PrefMmr = "RankedOffline_MMR";
    const string PrefWins = "RankedOffline_Wins";
    const string PrefLosses = "RankedOffline_Losses";

    public RankedProfile Cached { get; private set; } = new RankedProfile();

    public IEnumerator Refresh(PlayFabAuthService auth, Action<RankedProfile, string> onComplete = null)
    {
        if (auth == null || !auth.IsLoggedIn)
        {
            onComplete?.Invoke(null, "Not logged in");
            yield break;
        }

        if (auth.IsOfflineMode || !PlayFabConfig.Current.HasTitleId)
        {
            Cached = LoadOfflineProfile(auth.PlayFabId);
            onComplete?.Invoke(Cached, null);
            yield break;
        }

        bool done = false;
        string error = null;
        Dictionary<string, int> stats = null;

        yield return PlayFabClientApi.GetPlayerStatistics(
            PlayFabConfig.Current.titleId,
            (result, s) =>
            {
                if (!result.success)
                {
                    error = result.errorMessage;
                }
                else
                {
                    stats = s;
                }

                done = true;
            });

        while (!done) yield return null;

        if (error != null)
        {
            // Fall back to defaults if stats missing (first login before CloudScript init)
            Cached = new RankedProfile
            {
                playFabId = auth.PlayFabId,
                mmr = RankedRating.DefaultMmr,
                wins = 0,
                losses = 0,
                isOffline = false,
            };
            onComplete?.Invoke(Cached, null);
            yield break;
        }

        int mmr = RankedRating.DefaultMmr;
        int wins = 0;
        int losses = 0;
        if (stats != null)
        {
            if (stats.TryGetValue(RankedKeys.StatMmr, out int m)) mmr = m;
            if (stats.TryGetValue(RankedKeys.StatWins, out int w)) wins = w;
            if (stats.TryGetValue(RankedKeys.StatLosses, out int l)) losses = l;
        }

        // First-time players: ensure MMR exists client-side; CloudScript Begin will seed server stats.
        if (stats == null || !stats.ContainsKey(RankedKeys.StatMmr))
        {
            mmr = RankedRating.DefaultMmr;
        }

        Cached = new RankedProfile
        {
            playFabId = auth.PlayFabId,
            mmr = mmr,
            wins = wins,
            losses = losses,
            isOffline = false,
        };

        onComplete?.Invoke(Cached, null);
    }

    public void ApplyLocalResult(bool won, int newMmr, int wins, int losses, PlayFabAuthService auth)
    {
        if (Cached == null)
        {
            Cached = new RankedProfile();
        }

        Cached.mmr = newMmr;
        Cached.wins = wins;
        Cached.losses = losses;
        Cached.playFabId = auth?.PlayFabId ?? Cached.playFabId;
        Cached.isOffline = auth == null || auth.IsOfflineMode;

        if (Cached.isOffline)
        {
            SaveOfflineProfile(Cached);
        }
    }

    public RankedProfile LoadOfflineProfile(string playFabId)
    {
        return new RankedProfile
        {
            playFabId = playFabId,
            mmr = PlayerPrefs.GetInt(PrefMmr, RankedRating.DefaultMmr),
            wins = PlayerPrefs.GetInt(PrefWins, 0),
            losses = PlayerPrefs.GetInt(PrefLosses, 0),
            isOffline = true,
        };
    }

    void SaveOfflineProfile(RankedProfile profile)
    {
        PlayerPrefs.SetInt(PrefMmr, profile.mmr);
        PlayerPrefs.SetInt(PrefWins, profile.wins);
        PlayerPrefs.SetInt(PrefLosses, profile.losses);
        PlayerPrefs.Save();
    }
}
