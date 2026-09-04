using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Bo1 / Bo3 series for friend direct duels. Rematch stays in the same Photon room.
/// </summary>
public class FriendDuelDirector : MonoBehaviour
{
    static readonly WaitForSeconds Wait01 = new WaitForSeconds(0.1f);

    public bool ShouldReloadNextGame { get; private set; }
    public bool SeriesComplete { get; private set; }

    public int SeriesWinsA { get; private set; }
    public int SeriesWinsB { get; private set; }
    public int GameIndex { get; private set; }
    public int WinsToTake { get; private set; } = 1;
    public string UserIdA { get; private set; }
    public string UserIdB { get; private set; }
    public string LastLoserUserId { get; private set; }
    public string WinnerUserId { get; private set; }

    Text _seriesOverlay;
    Coroutine _autoAdvanceFromResult;
    bool _autoAdvancingResult;
    bool _startingBattle;
    bool _startedBattleOk;

    public void ResetDirector()
    {
        CancelAutoAdvanceFromResult();
        ShouldReloadNextGame = false;
        SeriesComplete = false;
        SeriesWinsA = 0;
        SeriesWinsB = 0;
        GameIndex = 0;
        WinsToTake = 1;
        UserIdA = null;
        UserIdB = null;
        LastLoserUserId = null;
        WinnerUserId = null;
        DestroyOverlay();
    }

    public void BeginSeriesFromRoom()
    {
        SyncFromRoom();
        var cc = ContinuousController.instance;
        if (cc != null && cc.FriendWinsToTake > WinsToTake)
        {
            WinsToTake = cc.FriendWinsToTake;
        }

        if (WinsToTake < 1)
        {
            WinsToTake = 1;
        }

        if (cc != null)
        {
            cc.isFriendDuel = true;
            cc.FriendWinsToTake = WinsToTake;
        }

        EnsureSides();
        PublishRoomProps();
        AttachSeriesOverlayWhenReady();
        FriendServices.Instance?.Duel?.NotifySeriesRoom(PhotonNetwork.CurrentRoom?.Name);
    }

    public void SyncFromRoom()
    {
        if (!PhotonNetwork.InRoom)
        {
            return;
        }

        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        if (hash.TryGetValue(FriendKeys.WinsToTakeProperty, out object w))
        {
            WinsToTake = System.Convert.ToInt32(w);
        }

        if (hash.TryGetValue(FriendKeys.SeriesWinsAProperty, out object a))
        {
            SeriesWinsA = System.Convert.ToInt32(a);
        }

        if (hash.TryGetValue(FriendKeys.SeriesWinsBProperty, out object b))
        {
            SeriesWinsB = System.Convert.ToInt32(b);
        }

        if (hash.TryGetValue(FriendKeys.GameIndexProperty, out object g))
        {
            GameIndex = System.Convert.ToInt32(g);
        }

        if (hash.TryGetValue(FriendKeys.UserIdAProperty, out object ua) && ua is string sa)
        {
            UserIdA = sa;
        }

        if (hash.TryGetValue(FriendKeys.UserIdBProperty, out object ub) && ub is string sb)
        {
            UserIdB = sb;
        }

        if (hash.TryGetValue(FriendKeys.LastLoserProperty, out object loser) && loser is string ls)
        {
            LastLoserUserId = ls;
        }
    }

    void EnsureSides()
    {
        string localId = FriendListService.LocalPlayFabId() ?? PhotonNetwork.LocalPlayer?.UserId;
        if (string.IsNullOrEmpty(UserIdA) && !string.IsNullOrEmpty(localId) && PhotonNetwork.IsMasterClient)
        {
            UserIdA = localId;
        }

        if (string.IsNullOrEmpty(UserIdB) && PhotonNetwork.PlayerList != null)
        {
            foreach (var p in PhotonNetwork.PlayerList)
            {
                if (p == null || p.IsLocal)
                {
                    continue;
                }

                string id = ReadPlayerId(p);
                if (!string.IsNullOrEmpty(id) && id != UserIdA)
                {
                    UserIdB = id;
                    break;
                }
            }
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
            player.CustomProperties.TryGetValue(RankedKeys.PlayFabIdProperty, out object idObj) &&
            idObj is string id &&
            !string.IsNullOrEmpty(id))
        {
            return id;
        }

        return null;
    }

    void PublishRoomProps()
    {
        if (!PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom)
        {
            return;
        }

        var hash = PhotonNetwork.CurrentRoom.CustomProperties ?? new Hashtable();
        hash[FriendKeys.SeriesWinsAProperty] = SeriesWinsA;
        hash[FriendKeys.SeriesWinsBProperty] = SeriesWinsB;
        hash[FriendKeys.GameIndexProperty] = GameIndex;
        hash[FriendKeys.WinsToTakeProperty] = WinsToTake;
        hash[FriendKeys.UserIdAProperty] = UserIdA ?? "";
        hash[FriendKeys.UserIdBProperty] = UserIdB ?? "";
        hash[FriendKeys.LastLoserProperty] = LastLoserUserId ?? "";
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
    }

    public void NotifyGameEnded(bool? localWon, bool disconnect, bool draw)
    {
        SyncFromRoom();
        EnsureSides();

        var cc = ContinuousController.instance;
        if (cc != null && cc.FriendWinsToTake > WinsToTake)
        {
            WinsToTake = cc.FriendWinsToTake;
        }

        if (WinsToTake < 1)
        {
            WinsToTake = 1;
        }

        string localId = FriendListService.LocalPlayFabId() ?? PhotonNetwork.LocalPlayer?.UserId;
        ShouldReloadNextGame = false;
        FriendServices.Instance?.Duel?.SetInviteListening(false);

        if (PhotonNetwork.InRoom && BattleReconnectService.CountActivePlayers() < 2)
        {
            if (localWon == true && !SeriesComplete)
            {
                CompleteSeries(localId);
            }

            ShouldReloadNextGame = false;
            return;
        }

        if (draw)
        {
            ShouldReloadNextGame = !SeriesComplete && WinsToTake > 1;
            return;
        }

        string winnerId = null;
        if (localWon == true)
        {
            winnerId = localId;
        }
        else if (localWon == false)
        {
            winnerId = OpponentId(localId);
        }
        else if (disconnect)
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && BattleReconnectService.CountActivePlayers() < 2)
            {
                winnerId = localId;
            }
            else if (!PhotonNetwork.IsConnected)
            {
                winnerId = OpponentId(localId);
            }
        }

        if (string.IsNullOrEmpty(winnerId))
        {
            ShouldReloadNextGame = !SeriesComplete && WinsToTake > 1;
            return;
        }

        int roomGameIndex = GameIndex;
        if (PhotonNetwork.InRoom &&
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(FriendKeys.GameIndexProperty, out object gObj))
        {
            roomGameIndex = System.Convert.ToInt32(gObj);
        }

        bool alreadyApplied = roomGameIndex > GameIndex;
        if (!alreadyApplied && PhotonNetwork.IsMasterClient)
        {
            ApplyGameResult(winnerId);
        }
        else
        {
            SyncFromRoom();
        }

        ShouldReloadNextGame = !SeriesComplete && WinsToTake > 1 &&
            SeriesWinsA < WinsToTake && SeriesWinsB < WinsToTake;
        RefreshSeriesOverlay();

        if (SeriesComplete && WinnerUserId == localId)
        {
            ContinuousController.instance.WinCount++;
            ContinuousController.instance.SaveWinCount();
        }
    }

    void ApplyGameResult(string winnerId)
    {
        bool winnerIsA = string.Equals(winnerId, UserIdA, System.StringComparison.OrdinalIgnoreCase);
        if (winnerIsA)
        {
            SeriesWinsA++;
            LastLoserUserId = UserIdB;
        }
        else
        {
            SeriesWinsB++;
            LastLoserUserId = UserIdA;
        }

        GameIndex++;

        if (WinsToTake > 1 &&
            (SeriesWinsA >= WinsToTake || SeriesWinsB >= WinsToTake))
        {
            CompleteSeries(winnerId);
        }
        else if (WinsToTake <= 1)
        {
            CompleteSeries(winnerId);
        }

        PublishRoomProps();
    }

    void CompleteSeries(string winnerId)
    {
        SeriesComplete = true;
        WinnerUserId = winnerId;
        ShouldReloadNextGame = false;
        PublishRoomProps();
    }

    string OpponentId(string localId)
    {
        if (localId == UserIdA)
        {
            return UserIdB;
        }

        if (localId == UserIdB)
        {
            return UserIdA;
        }

        return UserIdB ?? UserIdA;
    }

    public string FormatSeriesStatusLine()
    {
        string localId = FriendListService.LocalPlayFabId() ?? PhotonNetwork.LocalPlayer?.UserId;
        int you = localId == UserIdA ? SeriesWinsA : SeriesWinsB;
        int opp = localId == UserIdA ? SeriesWinsB : SeriesWinsA;

        if (ShouldReloadNextGame)
        {
            return $"Series {you}-{opp} — next game starting...";
        }

        if (SeriesComplete)
        {
            bool localWon = WinnerUserId == localId;
            return localWon
                ? $"Series won {you}-{opp}"
                : $"Series lost {you}-{opp}";
        }

        return $"Series {you}-{opp}";
    }

    public void BeginAutoAdvanceFromResult()
    {
        SetLocalOnResult(true);
        CancelAutoAdvanceFromResult();
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
        Bo3FirstPlayerChoice.Hide();
    }

    IEnumerator AutoAdvanceFromResultCoroutine()
    {
        _autoAdvancingResult = true;
        float start = Time.unscaledTime;

        if (ShouldReloadNextGame)
        {
            yield return WaitForLoserFirstPlayerChoice();
        }

        float shown = Time.unscaledTime - start;
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

        Bo3FirstPlayerChoice.Hide();
        _autoAdvanceFromResult = null;
        _autoAdvancingResult = false;

        // Re-read room props after both clients have been on the result screen.
        SyncFromRoom();
        var cc = ContinuousController.instance;
        if (cc != null && cc.FriendWinsToTake > WinsToTake)
        {
            WinsToTake = cc.FriendWinsToTake;
        }

        ShouldReloadNextGame = !SeriesComplete && WinsToTake > 1 &&
            SeriesWinsA < WinsToTake && SeriesWinsB < WinsToTake;

        if (cc == null)
        {
            yield break;
        }

        Debug.Log($"[Friends] Auto-advance rematch={ShouldReloadNextGame} score={SeriesWinsA}-{SeriesWinsB} winsToTake={WinsToTake}");

        if (GManager.instance != null)
        {
            GManager.instance.ReturnToTitle();
        }
        else
        {
            ContinuousController.instance.EndBattle();
        }
    }

    IEnumerator WaitForLoserFirstPlayerChoice()
    {
        SyncFromRoom();
        string localId = FriendListService.LocalPlayFabId() ?? PhotonNetwork.LocalPlayer?.UserId;
        string loserId = LastLoserUserId;
        float waitedLoser = 0f;
        while (string.IsNullOrEmpty(loserId) && waitedLoser < 4f)
        {
            if (PhotonNetwork.InRoom &&
                PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(FriendKeys.LastLoserProperty, out object loserObj) &&
                loserObj is string roomLoser &&
                !string.IsNullOrEmpty(roomLoser))
            {
                loserId = roomLoser;
                LastLoserUserId = roomLoser;
                break;
            }

            waitedLoser += Time.unscaledDeltaTime;
            yield return null;
        }

        if (string.IsNullOrEmpty(loserId))
        {
            yield break;
        }

        yield return Bo3FirstPlayerChoice.WaitForChoice(
            FriendKeys.NextFirstUserIdProperty,
            FriendKeys.NextFirstGameIndexProperty,
            GameIndex,
            localId,
            loserId);
    }

    static void SetLocalOnResult(bool onResult)
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            return;
        }

        var hash = PhotonNetwork.LocalPlayer.CustomProperties ?? new Hashtable();
        hash[FriendKeys.OnResultProperty] = onResult;
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

            if (!p.CustomProperties.TryGetValue(FriendKeys.OnResultProperty, out object v) || !(v is bool b) || !b)
            {
                return false;
            }
        }

        return true;
    }

    public IEnumerator StartNextGameCoroutine()
    {
        ShouldReloadNextGame = false;
        FriendServices.Instance?.Duel?.SetInviteListening(false);
        FriendServices.Instance?.Duel?.DestroyInviteOverlayPublic();
        Opening.instance?.battle?.roomManager?.Off();

        if (Opening.instance != null)
        {
            Opening.instance.openingObject.SetActive(false);
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
            _startingBattle = false;
            yield return EndSeriesToHomeCoroutine();
            yield break;
        }

        yield return StartBattleCoroutine(isRematch: true);
        if (!_startedBattleOk)
        {
            yield return EndSeriesToHomeCoroutine();
        }
    }

    public IEnumerator EndSeriesToHomeCoroutine()
    {
        CancelAutoAdvanceFromResult();
        DestroyOverlay();
        FriendServices.Instance?.Duel?.RememberEndedSeriesRoom(PhotonNetwork.CurrentRoom?.Name);
        FriendServices.Instance?.Duel?.SetInviteListening(false);
        FriendServices.Instance?.Duel?.DestroyInviteOverlayPublic();

        Opening.instance?.battle?.roomManager?.Off();
        RestoreOpeningCameras();

        // Must fully leave (not become inactive) so the opponent is not left waiting
        // on a dead Room Match lobby where CountActivePlayers never reaches MaxPlayers.
        PhotonUtility.LeaveRoomImmediate();
        float leftWait = 0f;
        while (PhotonNetwork.InRoom && leftWait < 8f)
        {
            leftWait += Time.unscaledDeltaTime;
            yield return null;
        }

        if (PhotonNetwork.InRoom)
        {
            PhotonUtility.DisconnectImmediate();
            float discWait = 0f;
            while (PhotonNetwork.IsConnected && discWait < 8f)
            {
                discWait += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        if (ContinuousController.instance != null)
        {
            ContinuousController.instance.ClearFriendDuel();
        }

        OnlinePlayerCountService.EnsureExists().SetMatchmakingOwnsConnection(false);

        if (Opening.instance != null)
        {
            Opening.instance.openingObject.SetActive(true);
            RestoreOpeningCameras();
            if (Opening.instance.home != null)
            {
                yield return Opening.instance.home.SetUpHomeMode_DisconnectCoroutine();
            }
        }
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

    IEnumerator StartBattleCoroutine(bool isRematch)
    {
        if (_startingBattle)
        {
            yield break;
        }

        _startingBattle = true;
        _startedBattleOk = false;

        if (ContinuousController.IsBattleSceneLoaded())
        {
            for (int guard = 0; guard < 4 && ContinuousController.IsBattleSceneLoaded(); guard++)
            {
                var unload = SceneManager.UnloadSceneAsync("BattleScene");
                if (unload != null)
                {
                    yield return unload;
                }
            }

            yield return null;
            yield return null;
        }

        if (!PhotonNetwork.InRoom || BattleReconnectService.CountActivePlayers() < 2)
        {
            _startingBattle = false;
            yield break;
        }

        if (ContinuousController.instance != null)
        {
            ContinuousController.instance.CanSetRandom = false;
            ContinuousController.instance.DoneSetRandom = false;
            ContinuousController.instance.isFriendDuel = true;
        }

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
        {
            ApplyFirstPlayerProperty(isRematch);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }

        var unloadLoading = Opening.instance != null ? Opening.instance.LoadingObject_Unload : null;
        if (unloadLoading != null && !unloadLoading.gameObject.activeSelf)
        {
            yield return ContinuousController.instance.StartCoroutine(
                unloadLoading.StartLoading("Now Loading"));
        }

        var playerProp = PhotonNetwork.LocalPlayer.CustomProperties ?? new Hashtable();
        playerProp["isBattle"] = true;
        playerProp[FriendKeys.OnResultProperty] = false;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProp);
        yield return Wait01;

        if (Opening.instance != null)
        {
            Opening.instance.openingObject.SetActive(false);
            foreach (Camera camera in Opening.instance.openingCameras)
            {
                camera.gameObject.SetActive(false);
            }

            ContinuousController.instance.StartCoroutine(Opening.instance.OpeningBGM.FadeOut(0.5f));
        }

        var load = SceneManager.LoadSceneAsync("BattleScene", LoadSceneMode.Additive);
        while (load != null && !load.isDone)
        {
            yield return null;
        }

        float waitGm = 0f;
        while (GManager.instance == null && waitGm < 20f)
        {
            waitGm += Time.unscaledDeltaTime;
            yield return null;
        }

        if (unloadLoading != null)
        {
            float waitCover = 0f;
            while (waitCover < 3f)
            {
                var loading = GManager.instance != null ? GManager.instance.LoadingObject : null;
                if (loading != null && loading.gameObject.activeSelf)
                {
                    break;
                }

                waitCover += Time.unscaledDeltaTime;
                yield return null;
            }

            unloadLoading.Off();
            if (unloadLoading.transform.parent != null)
            {
                unloadLoading.transform.parent.gameObject.SetActive(false);
            }
        }

        _startedBattleOk = true;
        _startingBattle = false;
        AttachSeriesOverlayWhenReady();
    }

    public void ApplyFirstPlayerProperty(bool isRematch)
    {
        if (!PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom)
        {
            return;
        }

        SyncFromRoom();
        int firstPlayerId = -1;
        string loserId = LastLoserUserId;

        string firstUserId = Bo3FirstPlayerChoice.ReadChosenFirstUserId(
            FriendKeys.NextFirstUserIdProperty,
            FriendKeys.NextFirstGameIndexProperty,
            GameIndex);
        if (string.IsNullOrEmpty(firstUserId))
        {
            firstUserId = loserId;
        }

        if (isRematch && !string.IsNullOrEmpty(firstUserId))
        {
            firstPlayerId = Bo3FirstPlayerChoice.ActorNumberForUserId(firstUserId);
        }

        var hash = PhotonNetwork.CurrentRoom.CustomProperties ?? new Hashtable();
        hash[DataBase.FirstPlayerKey] = firstPlayerId;
        if (!string.IsNullOrEmpty(loserId))
        {
            hash[FriendKeys.LastLoserProperty] = loserId;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
    }

    void AttachSeriesOverlayWhenReady()
    {
        if (WinsToTake <= 1)
        {
            return;
        }

        CancelInvoke(nameof(TryAttachOverlay));
        InvokeRepeating(nameof(TryAttachOverlay), 0.5f, 0.5f);
    }

    void TryAttachOverlay()
    {
        if (GManager.instance == null || WinsToTake <= 1)
        {
            return;
        }

        if (_seriesOverlay != null)
        {
            RefreshSeriesOverlay();
            return;
        }

        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            return;
        }

        var go = new GameObject("FriendSeriesOverlay", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -18f);
        rt.sizeDelta = new Vector2(900f, 48f);

        _seriesOverlay = go.AddComponent<Text>();
        _seriesOverlay.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (_seriesOverlay.font == null && Opening.instance != null && Opening.instance.VerText != null)
        {
            _seriesOverlay.font = Opening.instance.VerText.font;
        }

        _seriesOverlay.fontSize = 22;
        _seriesOverlay.alignment = TextAnchor.UpperCenter;
        _seriesOverlay.color = Color.white;
        _seriesOverlay.raycastTarget = false;
        RefreshSeriesOverlay();
        CancelInvoke(nameof(TryAttachOverlay));
    }

    void RefreshSeriesOverlay()
    {
        if (_seriesOverlay == null)
        {
            return;
        }

        string localId = FriendListService.LocalPlayFabId() ?? PhotonNetwork.LocalPlayer?.UserId;
        int you = localId == UserIdA ? SeriesWinsA : SeriesWinsB;
        int opp = localId == UserIdA ? SeriesWinsB : SeriesWinsA;
        int gameNumber = GameIndex + 1;
        int maxGames = WinsToTake * 2 - 1;
        _seriesOverlay.text = $"Game {gameNumber}/{maxGames}  —  You {you}-{opp}";
    }

    void DestroyOverlay()
    {
        CancelInvoke(nameof(TryAttachOverlay));
        if (_seriesOverlay != null)
        {
            Destroy(_seriesOverlay.gameObject);
            _seriesOverlay = null;
        }
    }

    void OnDestroy()
    {
        DestroyOverlay();
    }
}
