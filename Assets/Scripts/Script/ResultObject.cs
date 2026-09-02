using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;

public class ResultObject : MonoBehaviour
{
    [SerializeField] Image WinImage;
    [SerializeField] Image LoseImage;
    [SerializeField] Text ResultText;
    public Button ReturnToResultButton;

    public void Init()
    {
        this.gameObject.SetActive(false);
    }
    // === DCGO-CUSTOM:ranked begin ===
    IEnumerator ReportRankedOutcome(bool localWon, bool disconnect, bool surrendered)
    {
        var ranked = RankedServices.EnsureExists();

        // Re-login if session was lost mid-match
        if (ranked.Auth == null || !ranked.Auth.IsLoggedIn ||
            (!ranked.Auth.IsOfflineMode && !PlayFabClientApi.IsLoggedIn))
        {
            bool ok = false;
            yield return ranked.BootstrapForRanked((success, err) =>
            {
                ok = success;
                if (!success) Debug.LogWarning($"[Ranked] Re-login before report failed: {err}");
            });
            if (!ok)
            {
                AppendRankedText("\nRank update failed: not signed in to PlayFab");
                yield break;
            }
        }

        if (disconnect)
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.PlayerList.Length < 2)
            {
                localWon = true;
            }
            else if (!PhotonNetwork.IsConnected)
            {
                localWon = false;
            }
            else
            {
                AppendRankedText("\nRank unchanged");
                yield break;
            }
        }

        // Resolve opponent id late (props may arrive mid-match)
        if (string.IsNullOrEmpty(ranked.Match.OpponentPlayFabId) ||
            ranked.Match.OpponentPlayFabId == "unknown")
        {
            foreach (var p in PhotonNetwork.PlayerList)
            {
                if (p.IsLocal) continue;
                if (p.CustomProperties.TryGetValue(RankedKeys.PlayFabIdProperty, out object idObj) &&
                    idObj is string id && !string.IsNullOrEmpty(id))
                {
                    int mmr = RankedRating.DefaultMmr;
                    if (p.CustomProperties.TryGetValue(RankedKeys.MmrProperty, out object mmrObj))
                    {
                        mmr = Convert.ToInt32(mmrObj);
                    }

                    ranked.Match.EnsureActiveMatch(
                        ranked.Match.ActiveMatchId ?? Guid.NewGuid().ToString("N"),
                        id,
                        mmr);
                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(ranked.Match.ActiveMatchId))
        {
            string matchId = null;
            if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null &&
                PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(RankedKeys.MatchIdProperty, out object mid))
            {
                matchId = mid as string;
            }

            if (string.IsNullOrEmpty(matchId))
            {
                matchId = Guid.NewGuid().ToString("N");
            }

            ranked.Match.EnsureActiveMatch(
                matchId,
                ranked.Match.OpponentPlayFabId ?? "unknown",
                ranked.Match.OpponentMmr);
        }

        Debug.Log(
            $"[Ranked] Reporting matchId={ranked.Match.ActiveMatchId} " +
            $"self={ranked.Auth.PlayFabId} opp={ranked.Match.OpponentPlayFabId} " +
            $"won={localWon} offline={ranked.Auth.IsOfflineMode}");

        RankedMatchReportResult report = null;
        yield return ranked.Match.ReportResult(
            ranked.Auth,
            ranked.Profile,
            localWon,
            surrendered,
            disconnect,
            r => report = r);

        if (report == null || !report.success)
        {
            AppendRankedText($"\nRank update failed: {report?.errorMessage ?? "unknown"}");
            Debug.LogWarning($"[Ranked] Report failed: {report?.errorMessage}");
            yield break;
        }

        if (report.pendingOpponent)
        {
            AppendRankedText("\nWaiting for opponent rank confirmation...");
            yield break;
        }

        string delta = report.mmrDelta >= 0 ? $"+{report.mmrDelta}" : report.mmrDelta.ToString();
        string tier = string.IsNullOrEmpty(report.tierName)
            ? RankedRating.GetTierName(report.mmr)
            : report.tierName;
        AppendRankedText($"\n{tier} {report.mmr} ({delta} MMR)");
        if (ranked.Auth != null && ranked.Auth.IsOfflineMode)
        {
            AppendRankedText("\nOffline rank");
        }

        Debug.Log($"[Ranked] Stats updated: {tier} {report.mmr} ({delta}) W{report.wins}/L{report.losses} offline={ranked.Auth?.IsOfflineMode}");
    }

    void AppendRankedText(string extra)
    {
        // Report runs on RankedServices and may finish after BattleScene unload destroyed this UI
        if (!this || !ResultText)
        {
            return;
        }

        if (string.IsNullOrEmpty(ResultText.text))
        {
            ResultText.text = extra.TrimStart('\n');
        }
        else
        {
            ResultText.text += extra;
        }
    }
    // === DCGO-CUSTOM:ranked end ===
    // === DCGO-CUSTOM:tournament begin ===
    static void RelabelTournamentReturnButton(bool nextGame)
    {
        if (GManager.instance == null || GManager.instance.resultObject == null)
        {
            return;
        }

        var buttons = GManager.instance.resultObject.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null)
            {
                continue;
            }

            var label = buttons[i].GetComponentInChildren<Text>(true);
            string current = label != null ? label.text ?? "" : "";
            bool isReturn = buttons[i].name.IndexOf("Return", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            buttons[i].name.IndexOf("Title", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            current.IndexOf("title", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            current.IndexOf("タイトル", StringComparison.OrdinalIgnoreCase) >= 0;
            if (!isReturn)
            {
                continue;
            }

            buttons[i].interactable = false;

            if (label == null)
            {
                continue;
            }

            label.text = nextGame
                ? LocalizeUtility.GetLocalizedString(
                    EngMessage: "Next game starting...",
                    JpnMessage: "次のゲームを開始します…")
                : LocalizeUtility.GetLocalizedString(
                    EngMessage: "Returning...",
                    JpnMessage: "戻っています…");
        }
    }
    // === DCGO-CUSTOM:tournament end ===

    public void ShowResult(Player Winner, bool Surrendered, string effectName = "")
    {
        this.gameObject.SetActive(true);

        // === DCGO-CUSTOM:ranked begin ===
        bool isRanked = ContinuousController.instance != null && ContinuousController.instance.isRanked;
        // === DCGO-CUSTOM:tournament begin ===
        bool isTournament = ContinuousController.instance != null && ContinuousController.instance.isTournament;
        // === DCGO-CUSTOM:tournament end ===
        // === DCGO-CUSTOM:ranked end ===
        bool skipRankedReport = false;

        string log = "";

        log += "\nEnd Game";

        if (Winner != null)
        {
            log += $"\nWinner:{Winner.PlayerName}";
        }

        ResultText.text = "";

        if (String.IsNullOrEmpty(effectName))
        {
            log += $"\nEffect:{effectName}";
            ResultText.text = effectName;
        }            

        if (Winner == GManager.instance.You)
        {
            ContinuousController.instance.PlaySE(GManager.instance.WinSE);

            WinImage.gameObject.SetActive(true);
            LoseImage.gameObject.SetActive(false);

            if (!GManager.instance.IsAI)
            {
                // === DCGO-CUSTOM:friends begin ===
                bool isFriendDuel = ContinuousController.instance != null && ContinuousController.instance.isFriendDuel;
                if (!isRanked && !isTournament && !isFriendDuel)
                {
                    ContinuousController.instance.WinCount++;
                    ContinuousController.instance.SaveWinCount();
                }
                // === DCGO-CUSTOM:friends end ===

                if (Surrendered)
                    ResultText.text = "The opponent has surrendered.";

                if (String.IsNullOrEmpty(effectName))
                    ResultText.text = effectName;
            }
        }

        else if (Winner != null)
        {
            ContinuousController.instance.PlaySE(GManager.instance.LoseSE);

            WinImage.gameObject.SetActive(false);
            LoseImage.gameObject.SetActive(true);

            if (Winner != null)
            {
                if (Surrendered)
                    ResultText.text = "You have surrendered.";
            }
        }
        else
        {
            WinImage.gameObject.SetActive(false);
            LoseImage.gameObject.SetActive(true);

            bool isDisconnected = true;

            if (PhotonNetwork.IsConnected)
            {
                if (PhotonNetwork.PlayerList.Length == 2)
                {
                    isDisconnected = false;
                }

                if (GManager.instance.IsAI)
                {
                    isDisconnected = false;
                }

                WinImage.gameObject.SetActive(true);
                LoseImage.gameObject.SetActive(false);
            }

            if (isDisconnected)
            {
                log += $"\nDisconnected";

                ResultText.text = "Disconnected.";
            }

            else
            {
                log += $"\nDraw";

                ResultText.text = "Draw.";
                skipRankedReport = true;
            }
        }

        // === DCGO-CUSTOM:ranked begin ===
        if (isRanked && !GManager.instance.IsAI && !skipRankedReport)
        {
            // Capture battle-scene state now — ReportRankedOutcome outlives BattleScene unload
            bool localWon = Winner == GManager.instance.You;
            bool disconnect = Winner == null;
            var ranked = RankedServices.EnsureExists();
            ranked.StartCoroutine(ReportRankedOutcome(localWon, disconnect, Surrendered));
        }
        // === DCGO-CUSTOM:ranked end ===
        // === DCGO-CUSTOM:tournament begin ===
        if (isTournament && !GManager.instance.IsAI)
        {
            bool? localWon = null;
            if (Winner == GManager.instance.You)
            {
                localWon = true;
            }
            else if (Winner != null)
            {
                localWon = false;
            }

            bool disconnect = Winner == null && !skipRankedReport;
            bool draw = Winner == null && skipRankedReport;
            var match = TournamentServices.EnsureExists().Match;
            match.NotifyGameEnded(localWon, disconnect, draw);

            string seriesLine = match.FormatSeriesStatusLine();
            if (!string.IsNullOrEmpty(seriesLine))
            {
                if (string.IsNullOrEmpty(ResultText.text))
                {
                    ResultText.text = seriesLine;
                }
                else
                {
                    ResultText.text += "\n" + seriesLine;
                }
            }

            TournamentServices.EnsureExists().Match.BeginAutoAdvanceFromResult();
            RelabelTournamentReturnButton(match.ShouldReloadNextGame);
        }
        // === DCGO-CUSTOM:tournament end ===

        // === DCGO-CUSTOM:friends begin ===
        if (!GManager.instance.IsAI)
        {
            RememberLastOpponentFromRoom();
        }

        bool friendDuel = ContinuousController.instance != null && ContinuousController.instance.isFriendDuel;
        if (friendDuel && !GManager.instance.IsAI)
        {
            bool? localWon = null;
            if (Winner == GManager.instance.You)
            {
                localWon = true;
            }
            else if (Winner != null)
            {
                localWon = false;
            }

            bool disconnect = Winner == null;
            var director = FriendServices.EnsureExists().Director;
            director.NotifyGameEnded(localWon, disconnect, Winner == null && !disconnect);

            string seriesLine = director.FormatSeriesStatusLine();
            if (!string.IsNullOrEmpty(seriesLine))
            {
                if (string.IsNullOrEmpty(ResultText.text))
                {
                    ResultText.text = seriesLine;
                }
                else
                {
                    ResultText.text += "\n" + seriesLine;
                }
            }

            director.BeginAutoAdvanceFromResult();
            RelabelTournamentReturnButton(director.ShouldReloadNextGame);
        }
        // === DCGO-CUSTOM:friends end ===

        PlayLog.OnAddLog?.Invoke(log);
    }

    // === DCGO-CUSTOM:friends begin ===
    static void RememberLastOpponentFromRoom()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.PlayerList == null)
        {
            return;
        }

        string localId = FriendListService.LocalPlayFabId();
        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (p == null || p.IsLocal)
            {
                continue;
            }

            string id = FriendDuelDirector.ReadPlayerId(p);
            string name = p.NickName;
            if (p.CustomProperties != null &&
                p.CustomProperties.TryGetValue(ContinuousController.PlayerNameKey, out object nObj) &&
                nObj is string ns &&
                !string.IsNullOrEmpty(ns))
            {
                name = ns;
            }

            if (string.IsNullOrEmpty(id) ||
                (!string.IsNullOrEmpty(localId) &&
                 string.Equals(id, localId, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            FriendServices.EnsureExists().List.RememberLastOpponent(id, name);
            break;
        }
    }
    // === DCGO-CUSTOM:friends end ===

    #region Temporarily display/hide Result Screen
    public void OnClickReturnToResultsdButton()
    {
        this.gameObject.SetActive(true);
        ReturnToResultButton.gameObject.SetActive(false);
    }

    public void OnClickCheckFieldResultsButton()
    {
        this.gameObject.SetActive(false);
        ReturnToResultButton.gameObject.SetActive(true);
        ReturnToResultButton.onClick.RemoveAllListeners();
        ReturnToResultButton.onClick.AddListener(() => OnClickReturnToResultsdButton());
    }
    #endregion
}
