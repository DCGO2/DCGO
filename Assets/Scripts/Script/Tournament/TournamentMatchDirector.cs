using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// Runs a 2-player tournament match room: Bo3 loop, first-player, routing after the series.
/// </summary>
public class TournamentMatchDirector : MonoBehaviourPunCallbacks
{
    private static readonly WaitForSeconds Wait01 = new WaitForSeconds(0.1f);
    private static readonly WaitForSeconds Wait1 = new WaitForSeconds(1f);

    public bool InMatchRoom { get; private set; }
    public bool ShouldReloadNextGame { get; private set; }
    public bool RoutingAfterSeries { get; private set; }

    int _round;
    int _matchIndex;
    bool _startingBattle;
    bool _joinFailed;
    bool _createFailed;
    bool _joinOrCreatePending;
    bool _autoAdvancingResult;
    string _pendingRoomName;
    Text _seriesOverlay;
    Coroutine _autoAdvanceFromResult;

    public void ResetDirector()
    {
        InMatchRoom = false;
        ShouldReloadNextGame = false;
        RoutingAfterSeries = false;
        _startingBattle = false;
        CancelAutoAdvanceFromResult();

        _round = 0;
        _matchIndex = 0;
        _pendingRoomName = null;
        DestroyOverlay();
    }

    public IEnumerator JoinMatchRoomCoroutine(int round, int matchIndex)
    {
        var cc = ContinuousController.instance;
        var state = cc != null ? cc.TournamentState : null;
        if (state == null)
        {
            yield break;
        }

        ResetDirector();
        _round = round;
        _matchIndex = matchIndex;
        InMatchRoom = true;

        var match = state.GetMatch(round, matchIndex);
        if (!TournamentKeys.IsReadyTwoPlayerMatch(match))
        {
            state.ResolveOpeningByes();
            cc.TournamentState = state;
            InMatchRoom = false;
            var next = state.FindActiveMatchFor(TournamentState.EnsureLocalPlayerId());
            int guard = 0;
            while (IsAssignedByeMatch(next) && guard++ < 32)
            {
                state.ResolveOpeningByes();
                next = state.FindActiveMatchFor(TournamentState.EnsureLocalPlayerId());
            }

            if (TournamentKeys.IsReadyTwoPlayerMatch(next))
            {
                yield return JoinMatchRoomCoroutine(next.round, next.matchIndex);
            }
            else
            {
                yield return JoinWaitHubCoroutine();
            }

            yield break;
        }

        string roomName = TournamentKeys.MatchRoomName(state.tourneyId, round, matchIndex, state.useBanlist);
        _pendingRoomName = roomName;

        var options = new RoomOptions
        {
            IsVisible = false,
            IsOpen = true,
            PublishUserId = true,
            MaxPlayers = 2,
            EmptyRoomTtl = 120000,
            CustomRoomProperties = new Hashtable
            {
                { TournamentKeys.ModeProperty, TournamentKeys.ModeTournament },
                { TournamentKeys.TourneyIdProperty, state.tourneyId },
                { TournamentKeys.UseBanlistProperty, state.useBanlist },
                { TournamentKeys.RoomKindProperty, TournamentKeys.RoomKindMatch },
                { TournamentKeys.RoundProperty, round },
                { TournamentKeys.MatchIndexProperty, matchIndex },
                { TournamentKeys.UserIdAProperty, match != null ? match.userIdA ?? "" : "" },
                { TournamentKeys.UserIdBProperty, match != null ? match.userIdB ?? "" : "" },
                { TournamentKeys.SeriesWinsAProperty, match != null ? match.seriesWinsA : 0 },
                { TournamentKeys.SeriesWinsBProperty, match != null ? match.seriesWinsB : 0 },
                { TournamentKeys.GameIndexProperty, match != null ? match.gameIndex : 0 },
                { "RoomCreator", PhotonNetwork.NickName },
            },
            CustomRoomPropertiesForLobby = new[]
            {
                TournamentKeys.ModeProperty,
                TournamentKeys.TourneyIdProperty,
            },
        };
        BattleReconnectService.ApplyBattleTtl(options);

        for (int attempt = 0; attempt < 3; attempt++)
        {
            if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.Name == roomName)
            {
                break;
            }

            yield return JoinOrCreateNamedRoom(roomName, options);
            if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.Name == roomName)
            {
                break;
            }

            yield return Wait01;
        }

        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom.Name != roomName)
        {
            Debug.LogWarning($"[Tournament] Failed to join match room {roomName} (now in {PhotonNetwork.CurrentRoom?.Name}) — falling back to wait hub");
            InMatchRoom = false;
            yield return JoinWaitHubCoroutine();
            yield break;
        }

        SyncMatchPropsFromRoom();
        EnsureLockedDeckProperty();
        Opening.instance?.battle?.tournamentLobbyManager?.ShowMatchWaiting(_round, _matchIndex);

        float wait = 0f;
        const float assignedOpponentTimeout = 45f * 60f;
        int maxActiveSeen = BattleReconnectService.CountActivePlayers();
        while (PhotonNetwork.InRoom && BattleReconnectService.CountActivePlayers() < 2)
        {
            int active = BattleReconnectService.CountActivePlayers();
            if (active > maxActiveSeen)
            {
                maxActiveSeen = active;
            }

            var waitingMatch = state.GetMatch(_round, _matchIndex);
            bool opponentSeatEmpty = waitingMatch != null &&
                (string.IsNullOrEmpty(waitingMatch.userIdA) || string.IsNullOrEmpty(waitingMatch.userIdB));
            // Bye winners sit in the next match room until the feeder series finishes.
            // Never forfeit that wait — a Bo3 can last far longer than a few minutes.
            if (BattleReconnectService.HasInactiveOpponent())
            {
                wait += Time.unscaledDeltaTime;
                yield return null;
                continue;
            }

            if (!opponentSeatEmpty && maxActiveSeen >= 2)
            {
                break;
            }

            if (!opponentSeatEmpty && wait >= assignedOpponentTimeout)
            {
                break;
            }

            wait += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!PhotonNetwork.InRoom)
        {
            InMatchRoom = false;
            yield break;
        }

        if (BattleReconnectService.CountActivePlayers() < 2)
        {
            InMatchRoom = false;
            yield return JoinWaitHubCoroutine();
            yield break;
        }

        yield return StartBattleCoroutine(isRematch: match != null && match.gameIndex > 0);
    }

    public IEnumerator JoinWaitHubCoroutine()
    {
        InMatchRoom = false;
        ShouldReloadNextGame = false;
        var cc = ContinuousController.instance;
        var state = cc != null ? cc.TournamentState : null;
        if (state == null)
        {
            yield break;
        }

        string roomName = TournamentKeys.WaitHubRoomName(state.tourneyId, state.useBanlist);
        _pendingRoomName = roomName;

        var options = new RoomOptions
        {
            IsVisible = false,
            IsOpen = true,
            PublishUserId = true,
            MaxPlayers = (byte)TournamentKeys.NormalizePlayerCount(
                state.ResolvedPlayerCount),
            EmptyRoomTtl = 300000,
            CustomRoomProperties = new Hashtable
            {
                { TournamentKeys.ModeProperty, TournamentKeys.ModeTournament },
                { TournamentKeys.TourneyIdProperty, state.tourneyId },
                { TournamentKeys.UseBanlistProperty, state.useBanlist },
                { TournamentKeys.RoomKindProperty, TournamentKeys.RoomKindWaitHub },
                { TournamentKeys.PlayerCountProperty, state.ResolvedPlayerCount },
                { TournamentKeys.StateProperty, state.ToRoomJson() },
                { TournamentKeys.StartedProperty, true },
                { "RoomCreator", PhotonNetwork.NickName },
            },
            CustomRoomPropertiesForLobby = new[]
            {
                TournamentKeys.ModeProperty,
                TournamentKeys.TourneyIdProperty,
            },
        };
        BattleReconnectService.ApplyBattleTtl(options);

        yield return JoinOrCreateNamedRoom(roomName, options);
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[Tournament] Failed to join wait hub");
            Opening.instance?.battle?.tournamentLobbyManager?.ShowWaitHub();
            yield break;
        }

        var lobby = Opening.instance?.battle?.tournamentLobbyManager;
        lobby?.ShowWaitHub();
        // Publish first so the bye / waiting player merges the filled final.
        // Do not dispatch from here: EndBattle may still be tearing down, and
        // starting the next match inside that coroutine soft-locks the winner.
        PublishStateToCurrentRoom();
    }

    public void NotifyGameEnded(bool? localWon, bool disconnect, bool draw)
    {
        ShouldReloadNextGame = false;

        var cc = ContinuousController.instance;
        var state = cc != null ? cc.TournamentState : null;
        if (state == null)
        {
            Debug.LogWarning("[Tournament] NotifyGameEnded: missing TournamentState");
            return;
        }

        SyncMatchPropsFromRoom();

        var match = state.GetMatch(_round, _matchIndex);
        if (match == null || match.complete)
        {
            match = state.FindActiveMatchFor(TournamentState.EnsureLocalPlayerId());
        }

        if (match != null)
        {
            _round = match.round;
            _matchIndex = match.matchIndex;
        }

        string localId = TournamentState.EnsureLocalPlayerId();
        string winnerId = null;

        if (PhotonNetwork.InRoom && BattleReconnectService.CountActivePlayers() < 2)
        {
            // Empty match: surrender / disconnect must not start another ghost game.
            ShouldReloadNextGame = false;
            if (match != null && !match.complete && localWon == true)
            {
                state.CompleteMatch(match, localId);
                cc.TournamentState = state;
            }

            Debug.LogWarning("[Tournament] Game ended with fewer than 2 players — no rematch");
            return;
        }

        if (draw)
        {
            // Replay the same game index; series score unchanged.
            ShouldReloadNextGame = match != null && !match.complete;
            Debug.Log($"[Tournament] Draw → rematch game={match?.gameIndex} shouldReload={ShouldReloadNextGame}");
            return;
        }

        if (localWon == true)
        {
            winnerId = localId;
        }
        else if (localWon == false)
        {
            winnerId = OpponentUserId(state, localId, match);
        }
        else if (disconnect)
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && BattleReconnectService.CountActivePlayers() < 2)
            {
                winnerId = localId;
            }
            else if (!PhotonNetwork.IsConnected)
            {
                winnerId = OpponentUserId(state, localId, match);
            }
        }

        if (match == null)
        {
            Debug.LogWarning("[Tournament] NotifyGameEnded: no active match");
            return;
        }

        if (string.IsNullOrEmpty(winnerId))
        {
            // Prefer staying in the series over wrongly exiting to hub.
            ShouldReloadNextGame = !match.complete;
            Debug.LogWarning($"[Tournament] NotifyGameEnded: unresolved winner, shouldReload={ShouldReloadNextGame}");
            return;
        }

        // Avoid double-counting if room props already reflect this finished game.
        int expectedGameIndex = match.gameIndex;
        bool alreadyApplied =
            PhotonNetwork.InRoom &&
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(TournamentKeys.GameIndexProperty, out object gObj) &&
            System.Convert.ToInt32(gObj) > expectedGameIndex;

        bool wouldComplete =
            (match.userIdA == winnerId && match.seriesWinsA + 1 >= TournamentKeys.WinsToTakeSeries) ||
            (match.userIdB == winnerId && match.seriesWinsB + 1 >= TournamentKeys.WinsToTakeSeries);

        if (!alreadyApplied)
        {
            state.ApplyGameResult(_round, _matchIndex, winnerId, wouldComplete);
            cc.TournamentState = state;
        }
        else
        {
            SyncMatchPropsFromRoom();
            if (PhotonNetwork.InRoom &&
                PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(TournamentKeys.StateProperty, out object jsonObj) &&
                jsonObj is string json)
            {
                var incoming = TournamentState.FromJson(json);
                if (incoming != null)
                {
                    state.MergeFrom(incoming);
                    cc.TournamentState = state;
                }
            }

            match = state.GetMatch(_round, _matchIndex);
            wouldComplete = match != null && match.complete;
        }

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
        {
            var updated = state.GetMatch(_round, _matchIndex);
            if (updated != null)
            {
                var hash = PhotonNetwork.CurrentRoom.CustomProperties;
                hash[TournamentKeys.SeriesWinsAProperty] = updated.seriesWinsA;
                hash[TournamentKeys.SeriesWinsBProperty] = updated.seriesWinsB;
                hash[TournamentKeys.GameIndexProperty] = updated.gameIndex;
                hash[TournamentKeys.LastLoserProperty] = updated.lastGameLoserUserId ?? "";
                hash[TournamentKeys.StateProperty] = state.ToRoomJson();
                PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
            }
        }

        ShouldReloadNextGame = !wouldComplete && !state.finished;
        Debug.Log(
            $"[Tournament] Game result winner={winnerId} score={match.seriesWinsA}-{match.seriesWinsB} " +
            $"complete={wouldComplete} nextGame={ShouldReloadNextGame}");

        if (wouldComplete && winnerId == localId)
        {
            cc.WinCount++;
            cc.SaveWinCount();
        }
    }

    /// <summary>
    /// After both players see the result, leave automatically so Bo3 game 2/3
    /// starts without pressing Return to Title.
    /// </summary>
    public void BeginAutoAdvanceFromResult()
    {
        SetLocalOnResult(true);
        RelabelResultReturnButton();

        if (_autoAdvanceFromResult != null)
        {
            StopCoroutine(_autoAdvanceFromResult);
        }

        _autoAdvanceFromResult = StartCoroutine(AutoAdvanceFromResultCoroutine());
    }

    public void CancelAutoAdvanceFromResult()
    {
        if (_autoAdvanceFromResult != null)
        {
            StopCoroutine(_autoAdvanceFromResult);
            _autoAdvanceFromResult = null;
        }

        _autoAdvancingResult = false;
    }

    IEnumerator AutoAdvanceFromResultCoroutine()
    {
        _autoAdvancingResult = true;

        float shown = 0f;
        const float minShowSeconds = 2f;
        while (shown < minShowSeconds)
        {
            shown += Time.unscaledDeltaTime;
            yield return null;
        }

        if (ShouldReloadNextGame && PhotonNetwork.InRoom && BattleReconnectService.CountActivePlayers() >= 2)
        {
            float waitBoth = 0f;
            const float waitBothTimeout = 8f;
            while (waitBoth < waitBothTimeout && !AllPlayersOnResult())
            {
                if (!PhotonNetwork.InRoom || BattleReconnectService.CountActivePlayers() < 2)
                {
                    break;
                }

                waitBoth += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        _autoAdvanceFromResult = null;
        _autoAdvancingResult = false;

        if (ContinuousController.instance == null)
        {
            yield break;
        }

        Debug.Log($"[Tournament] Auto-advance from result rematch={ShouldReloadNextGame}");
        if (GManager.instance != null)
        {
            GManager.instance.ReturnToTitle();
        }
        else
        {
            ContinuousController.instance.EndBattle();
        }
    }

    static void SetLocalOnResult(bool onResult)
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            return;
        }

        var hash = PhotonNetwork.LocalPlayer.CustomProperties ?? new Hashtable();
        hash[TournamentKeys.OnResultProperty] = onResult;
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    static bool AllPlayersOnResult()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.PlayerList == null || PhotonNetwork.PlayerList.Length < 2)
        {
            return false;
        }

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            var p = PhotonNetwork.PlayerList[i];
            if (p == null)
            {
                return false;
            }

            if (!p.CustomProperties.TryGetValue(TournamentKeys.OnResultProperty, out object value) ||
                !(value is bool onResult) ||
                !onResult)
            {
                return false;
            }
        }

        return true;
    }

    static void RelabelResultReturnButton()
    {
        var result = GManager.instance != null ? GManager.instance.resultObject : null;
        if (result == null)
        {
            return;
        }

        var director = TournamentServices.EnsureExists().Match;
        bool rematch = director != null && director.ShouldReloadNextGame;
        string label = rematch ? "Next game starting..." : "Returning...";

        var buttons = result.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null)
            {
                continue;
            }

            bool isReturn = buttons[i].name.IndexOf("Return", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                            buttons[i].name.IndexOf("Title", System.StringComparison.OrdinalIgnoreCase) >= 0;
            if (!isReturn)
            {
                continue;
            }

            buttons[i].interactable = false;

            var texts = buttons[i].GetComponentsInChildren<Text>(true);
            for (int t = 0; t < texts.Length; t++)
            {
                if (texts[t] != null)
                {
                    texts[t].text = label;
                }
            }
        }
    }

    public string FormatSeriesStatusLine()
    {
        var state = ContinuousController.instance != null ? ContinuousController.instance.TournamentState : null;
        var match = state != null ? state.GetMatch(_round, _matchIndex) : null;
        if (match == null)
        {
            return null;
        }

        string localId = TournamentState.EnsureLocalPlayerId();
        int you = state.IsPlayerA(match, localId) ? match.seriesWinsA : match.seriesWinsB;
        int opp = state.IsPlayerA(match, localId) ? match.seriesWinsB : match.seriesWinsA;
        if (ShouldReloadNextGame)
        {
            return $"Series {you}-{opp} — next game starting...";
        }

        if (match.complete)
        {
            bool localWonSeries = match.winnerUserId == localId;
            return localWonSeries
                ? $"Series won {you}-{opp}"
                : $"Series lost {you}-{opp}";
        }

        return $"Series {you}-{opp}";
    }

    public IEnumerator StartNextGameCoroutine()
    {
        ShouldReloadNextGame = false;
        InMatchRoom = true;

        if (!InConfiguredMatchRoom())
        {
            Debug.LogWarning("[Tournament] Rematch is not in a match room — joining it now");
            yield return JoinMatchRoomCoroutine(_round, _matchIndex);
            yield break;
        }

        float wait = 0f;
        float reconnectWait = BattleReconnectService.PlayerTtlMs / 1000f;
        while (PhotonNetwork.InRoom && BattleReconnectService.CountActivePlayers() < 2 && wait < reconnectWait)
        {
            wait += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!PhotonNetwork.InRoom || BattleReconnectService.CountActivePlayers() < 2)
        {
            // Opponent never arrived — do not award a phantom series. Meet in the hub.
            yield return JoinWaitHubCoroutine();
            yield break;
        }

        yield return StartBattleCoroutine(isRematch: true);
    }

    public IEnumerator RouteAfterSeriesCoroutine()
    {
        RoutingAfterSeries = true;
        ShouldReloadNextGame = false;
        InMatchRoom = false;
        DestroyOverlay();

        var cc = ContinuousController.instance;
        var state = cc != null ? cc.TournamentState : null;
        if (state != null)
        {
            state.ResolveOpeningByes();
            cc.TournamentState = state;
        }

        // Always reconvene in the wait hub so the bye / waiting player receives
        // the updated bracket. Going straight to the next match room left them behind.
        // Leave RoutingAfterSeries true until EndBattle finishes EndLoading so
        // Update cannot start the final during teardown.
        yield return JoinWaitHubCoroutine();
    }

    public void EndRoutingAfterSeries()
    {
        RoutingAfterSeries = false;
    }

    IEnumerator StartBattleCoroutine(bool isRematch)
    {
        if (_startingBattle)
        {
            yield break;
        }

        _startingBattle = true;

        yield return ClearLeftoverBattleSceneCoroutine();

        if (GManager.instance != null || ContinuousController.IsBattleSceneLoaded())
        {
            Debug.LogWarning("[Tournament] Leftover battle scene blocked the next match — returning to wait hub");
            _startingBattle = false;
            yield return AbortStartBattleToWaitHubCoroutine();
            yield break;
        }

        if (!InConfiguredMatchRoom() ||
            !PhotonNetwork.InRoom ||
            BattleReconnectService.CountActivePlayers() < 2)
        {
            Debug.LogWarning($"[Tournament] Refusing to start battle in '{PhotonNetwork.CurrentRoom?.Name}' players={BattleReconnectService.CountActivePlayers()}");
            _startingBattle = false;
            yield break;
        }

        InMatchRoom = true;
        if (ContinuousController.instance != null)
        {
            ContinuousController.instance.CanSetRandom = false;
            ContinuousController.instance.DoneSetRandom = false;
        }

        Debug.Log($"[Tournament] Start battle rematch={isRematch} room={PhotonNetwork.CurrentRoom?.Name} players={PhotonNetwork.CurrentRoom.PlayerCount}");

        try
        {
            Opening.instance?.battle?.tournamentLobbyManager?.HideLobbyUi();

            if (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
            {
                ApplyFirstPlayerProperty(isRematch);
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
            }

            var unloadLoading = Opening.instance != null ? Opening.instance.LoadingObject_Unload : null;
            if (unloadLoading != null)
            {
                yield return ContinuousController.instance.StartCoroutine(
                    unloadLoading.StartLoading("Now Loading"));
            }

            var playerProp = PhotonNetwork.LocalPlayer.CustomProperties ?? new Hashtable();
            playerProp["isBattle"] = true;
            playerProp[TournamentKeys.OnResultProperty] = false;
            EnsureLockedDeckOnHash(playerProp);
            string locked = TournamentState.ReadDeckCode(PhotonNetwork.LocalPlayer);
            if (string.IsNullOrEmpty(locked))
            {
                string localId = TournamentState.EnsureLocalPlayerId();
                locked = ContinuousController.instance.TournamentState != null
                    ? ContinuousController.instance.TournamentState.LockedDeckCode(localId)
                    : null;
            }

            if (!string.IsNullOrEmpty(locked))
            {
                playerProp[ContinuousController.DeckDataPropertyKey] = locked;
                playerProp[TournamentKeys.LockedDeckProperty] = locked;
                if (ContinuousController.instance.BattleDeckData == null ||
                    ContinuousController.instance.BattleDeckData.GetThisDeckCode() != locked)
                {
                    ContinuousController.instance.BattleDeckData = new DeckData(locked);
                }
            }

            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProp);
            yield return Wait01;

            foreach (Camera camera in Opening.instance.openingCameras)
            {
                camera.gameObject.SetActive(false);
            }

            ContinuousController.instance.StartCoroutine(Opening.instance.OpeningBGM.FadeOut(0.5f));
            yield return Wait1;

            ContinuousController.CleanStalePhotonViews();
            PhotonNetwork.IsMessageQueueRunning = true;

            yield return ClearLeftoverBattleSceneCoroutine();
            if (ContinuousController.IsBattleSceneLoaded() || GManager.instance != null)
            {
                Debug.LogWarning("[Tournament] Battle load aborted — leftover scene after cameras off");
                RestoreOpeningCameras();
                if (unloadLoading != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(unloadLoading.EndLoading());
                }

                yield return AbortStartBattleToWaitHubCoroutine();
                yield break;
            }

            var load = SceneManager.LoadSceneAsync(ContinuousController.BattleSceneName, LoadSceneMode.Additive);
            while (load != null && !load.isDone)
            {
                yield return null;
            }

            float waitGm = 0f;
            while (GManager.instance == null && waitGm < 20f)
            {
                waitGm += Time.deltaTime;
                yield return null;
            }

            if (unloadLoading != null)
            {
                yield return ContinuousController.instance.StartCoroutine(unloadLoading.EndLoading());
            }

            StartCoroutine(AttachSeriesOverlayWhenReady());
        }
        finally
        {
            _startingBattle = false;
        }
    }

    IEnumerator ClearLeftoverBattleSceneCoroutine()
    {
        for (int guard = 0; guard < 4 && ContinuousController.IsBattleSceneLoaded(); guard++)
        {
            var unload = SceneManager.UnloadSceneAsync(ContinuousController.BattleSceneName);
            if (unload == null)
            {
                break;
            }

            yield return unload;
        }

        yield return null;
        yield return null;
        ContinuousController.CleanStalePhotonViews();

        float waitGone = 0f;
        while ((ContinuousController.IsBattleSceneLoaded() || GManager.instance != null) && waitGone < 2f)
        {
            waitGone += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    IEnumerator AbortStartBattleToWaitHubCoroutine()
    {
        RestoreOpeningCameras();
        InMatchRoom = false;
        _startingBattle = false;
        yield return JoinWaitHubCoroutine();
    }

    static void RestoreOpeningCameras()
    {
        if (Opening.instance == null)
        {
            return;
        }

        Opening.instance.openingObject.SetActive(true);
        if (Opening.instance.openingCameras == null)
        {
            return;
        }

        foreach (Camera camera in Opening.instance.openingCameras)
        {
            if (camera != null)
            {
                camera.gameObject.SetActive(true);
            }
        }
    }

    IEnumerator AttachSeriesOverlayWhenReady()
    {
        float t = 0f;
        while (GManager.instance == null && t < 15f)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (GManager.instance == null || GManager.instance.canvas == null)
        {
            yield break;
        }

        DestroyOverlay();
        var go = new GameObject("TournamentSeriesOverlay");
        go.transform.SetParent(GManager.instance.canvas.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -18f);
        rt.sizeDelta = new Vector2(900f, 48f);

        _seriesOverlay = go.AddComponent<Text>();
        _seriesOverlay.font = ResolveFont();
        _seriesOverlay.fontSize = 22;
        _seriesOverlay.alignment = TextAnchor.UpperCenter;
        _seriesOverlay.color = Color.white;
        _seriesOverlay.raycastTarget = false;
        RefreshSeriesOverlay();
    }

    void RefreshSeriesOverlay()
    {
        if (_seriesOverlay == null)
        {
            return;
        }

        var state = ContinuousController.instance != null ? ContinuousController.instance.TournamentState : null;
        var match = state != null ? state.GetMatch(_round, _matchIndex) : null;
        if (match == null)
        {
            _seriesOverlay.text = "Tournament";
            return;
        }

        string localId = TournamentState.EnsureLocalPlayerId();
        int you = state.IsPlayerA(match, localId) ? match.seriesWinsA : match.seriesWinsB;
        int opp = state.IsPlayerA(match, localId) ? match.seriesWinsB : match.seriesWinsA;
        int gameNumber = match.gameIndex + 1;
        _seriesOverlay.text = $"Game {gameNumber}/3  —  You {you}-{opp}";
    }

    void ApplyFirstPlayerProperty(bool isRematch)
    {
        var state = ContinuousController.instance.TournamentState;
        var match = state != null ? state.GetMatch(_round, _matchIndex) : null;
        int firstPlayerId = -1;

        string loserId = match != null ? match.lastGameLoserUserId : null;
        if (string.IsNullOrEmpty(loserId) &&
            PhotonNetwork.InRoom &&
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(TournamentKeys.LastLoserProperty, out object loserObj) &&
            loserObj is string roomLoser)
        {
            loserId = roomLoser;
        }

        if (isRematch && !string.IsNullOrEmpty(loserId))
        {
            foreach (var p in PhotonNetwork.PlayerList)
            {
                if (TournamentState.ReadPlayerId(p) == loserId)
                {
                    firstPlayerId = p.ActorNumber;
                    break;
                }
            }
        }

        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        hash[DataBase.FirstPlayerKey] = firstPlayerId;
        if (!string.IsNullOrEmpty(loserId))
        {
            hash[TournamentKeys.LastLoserProperty] = loserId;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        Debug.Log($"[Tournament] FirstPlayer actor={firstPlayerId} rematch={isRematch} loser={loserId}");
    }

    void AwardSeriesForfeitIfAlone()
    {
        var cc = ContinuousController.instance;
        var state = cc != null ? cc.TournamentState : null;
        if (state == null)
        {
            return;
        }

        var match = state.GetMatch(_round, _matchIndex);
        if (match == null || match.complete)
        {
            return;
        }

        string localId = TournamentState.EnsureLocalPlayerId();
        state.CompleteMatch(match, localId);
        cc.TournamentState = state;
        cc.WinCount++;
        cc.SaveWinCount();
        ShouldReloadNextGame = false;
    }

    void SyncMatchPropsFromRoom()
    {
        if (!PhotonNetwork.InRoom)
        {
            return;
        }

        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        if (hash.TryGetValue(TournamentKeys.RoundProperty, out object roundObj))
        {
            _round = System.Convert.ToInt32(roundObj);
        }

        if (hash.TryGetValue(TournamentKeys.MatchIndexProperty, out object matchObj))
        {
            _matchIndex = System.Convert.ToInt32(matchObj);
        }

        var state = ContinuousController.instance.TournamentState;
        var match = state != null ? state.GetMatch(_round, _matchIndex) : null;
        if (match == null)
        {
            return;
        }

        if (hash.TryGetValue(TournamentKeys.SeriesWinsAProperty, out object a))
        {
            match.seriesWinsA = System.Convert.ToInt32(a);
        }

        if (hash.TryGetValue(TournamentKeys.SeriesWinsBProperty, out object b))
        {
            match.seriesWinsB = System.Convert.ToInt32(b);
        }

        if (hash.TryGetValue(TournamentKeys.GameIndexProperty, out object g))
        {
            match.gameIndex = System.Convert.ToInt32(g);
        }

        if (hash.TryGetValue(TournamentKeys.LastLoserProperty, out object loserObj) &&
            loserObj is string loser &&
            !string.IsNullOrEmpty(loser))
        {
            match.lastGameLoserUserId = loser;
        }

        if (hash.TryGetValue(TournamentKeys.StateProperty, out object jsonObj) && jsonObj is string json)
        {
            var incoming = TournamentState.FromJson(json);
            if (incoming != null)
            {
                state.MergeFrom(incoming);
            }
        }
    }

    void PublishStateToCurrentRoom()
    {
        var state = ContinuousController.instance != null ? ContinuousController.instance.TournamentState : null;
        if (!PhotonNetwork.InRoom || state == null)
        {
            return;
        }

        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        hash[TournamentKeys.StateProperty] = state.ToRoomJson();
        hash[TournamentKeys.StartedProperty] = true;
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
    }

    void EnsureLockedDeckProperty()
    {
        var hash = PhotonNetwork.LocalPlayer.CustomProperties ?? new Hashtable();
        EnsureLockedDeckOnHash(hash);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    static void EnsureLockedDeckOnHash(Hashtable hash)
    {
        string localId = TournamentState.EnsureLocalPlayerId();
        var state = ContinuousController.instance != null ? ContinuousController.instance.TournamentState : null;
        string code = state != null ? state.LockedDeckCode(localId) : null;
        if (string.IsNullOrEmpty(code) && ContinuousController.instance.BattleDeckData != null)
        {
            code = ContinuousController.instance.BattleDeckData.GetThisDeckCode();
        }

        if (!string.IsNullOrEmpty(code))
        {
            hash[TournamentKeys.LockedDeckProperty] = code;
            hash[ContinuousController.DeckDataPropertyKey] = code;
        }

        hash[TournamentKeys.PlayerIdProperty] = localId;
    }

    static string OpponentUserId(TournamentState state, string localId, TournamentMatchSlot match = null)
    {
        match ??= state.FindActiveMatchFor(localId) ?? state.GetMatch(0, 0);
        if (match == null)
        {
            return null;
        }

        string other = match.userIdA == localId ? match.userIdB : match.userIdA;
        return TournamentKeys.IsBye(other) ? null : other;
    }

    /// <summary>Both sides filled and at least one is BYE — must auto-resolve, never Photon-battle.</summary>
    static bool IsAssignedByeMatch(TournamentMatchSlot match)
    {
        if (match == null || match.complete)
        {
            return false;
        }

        if (string.IsNullOrEmpty(match.userIdA) || string.IsNullOrEmpty(match.userIdB))
        {
            return false;
        }

        return TournamentKeys.IsBye(match.userIdA) || TournamentKeys.IsBye(match.userIdB);
    }

    IEnumerator JoinOrCreateNamedRoom(string roomName, RoomOptions options)
    {
        _joinFailed = false;
        _createFailed = false;

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom(false);
            yield return new WaitWhile(() => PhotonNetwork.InRoom);
        }

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            yield return ContinuousController.instance.StartCoroutine(PhotonUtility.ConnectToMasterServerCoroutine());
        }

        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);

        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        yield return new WaitUntil(() => PhotonNetwork.InLobby && PhotonNetwork.IsConnectedAndReady);

        _pendingRoomName = roomName;
        _joinOrCreatePending = true;
        _joinFailed = false;
        _createFailed = false;
        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);

        float t = 0f;
        // JoinOrCreate fires OnJoinRoomFailed(32758) when the room does not exist yet, then creates it.
        // Do not abort on join-failed while that create is still in flight.
        while (!PhotonNetwork.InRoom && !_createFailed && t < 15f)
        {
            t += Time.deltaTime;
            yield return null;
        }

        _joinOrCreatePending = false;
        if (PhotonNetwork.InRoom)
        {
            yield break;
        }

        _joinFailed = false;
        _createFailed = false;
        PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);

        t = 0f;
        while (!PhotonNetwork.InRoom && !_createFailed && t < 10f)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (!PhotonNetwork.InRoom)
        {
            _joinFailed = false;
            PhotonNetwork.JoinRoom(roomName);
            t = 0f;
            while (!PhotonNetwork.InRoom && !_joinFailed && t < 10f)
            {
                t += Time.deltaTime;
                yield return null;
            }
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (_joinOrCreatePending && (returnCode == 32758 || returnCode == ErrorCode.GameDoesNotExist))
        {
            return;
        }

        _joinFailed = true;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        _createFailed = true;
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (!InMatchRoom)
        {
            return;
        }

        SyncMatchPropsFromRoom();
        RefreshSeriesOverlay();
    }

    /// <summary>True only inside a room created as a tournament match room (never the lobby / wait hub).</summary>
    static bool InConfiguredMatchRoom()
    {
        if (!PhotonNetwork.InRoom)
        {
            return false;
        }

        var props = PhotonNetwork.CurrentRoom.CustomProperties;
        return props != null &&
               props.TryGetValue(TournamentKeys.RoomKindProperty, out object kind) &&
               kind is string kindStr &&
               kindStr == TournamentKeys.RoomKindMatch;
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        // Inactive disconnects are held by BattleReconnectService until PlayerTtl.
        // A full leave (TTL expired / LeaveRoom(false)) is a forfeit via GManager.CheckDisconnect.
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (!InMatchRoom || _startingBattle || GManager.instance != null ||
            ContinuousController.IsBattleSceneLoaded() || !InConfiguredMatchRoom())
        {
            return;
        }

        if (PhotonNetwork.InRoom && BattleReconnectService.CountActivePlayers() >= 2)
        {
            StartCoroutine(StartBattleCoroutine(isRematch: false));
        }
    }

    void DestroyOverlay()
    {
        if (_seriesOverlay != null)
        {
            Destroy(_seriesOverlay.gameObject);
            _seriesOverlay = null;
        }
    }

    static Font ResolveFont()
    {
        var rm = Opening.instance != null ? Opening.instance.battle?.roomManager : null;
        if (rm != null && rm.RoomIDText != null && rm.RoomIDText.font != null)
        {
            return rm.RoomIDText.font;
        }

        var arial = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (arial != null)
        {
            return arial;
        }

        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    void OnDestroy()
    {
        DestroyOverlay();
    }
}
