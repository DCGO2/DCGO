using System;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Per-player clock for online (PvP) matches.
//
//  * Both clocks go back to TurnSeconds at the start of every turn.
//  * A player's clock only runs while the game is waiting on that player's input: a main phase action
//    from the turn player, or any selection (effect targets, optional effects, counters, blocks, ...).
//  * Finishing an interaction gives time back, capped at TurnSeconds. Main phase actions (play, digivolve,
//    attack, activate) give more than effect choices.
//  * Each interaction also has its own InteractionSeconds limit, restarted whenever a new prompt opens.
//  * When either clock runs out its owner loses. The owner's client declares the timeout; the other client only
//    steps in after a grace period, so a stalled or frozen client can't hold the match forever.
//
// Lives on the GManager GameObject next to ReconnectManager. While ReconnectManager is handling a drop
// the clocks are paused.
public class TurnTimer : MonoBehaviourPunCallbacks
{
    public const float TurnSeconds = 180f;
    public const float MainPhaseActionBonusSeconds = 15f;
    public const float SelectionBonusSeconds = 5f;
    public const float InteractionSeconds = 90f;

    const float RemoteTimeoutGraceSeconds = 10f;
    const float LowTimeSeconds = 30f;
    const float LowInteractionTimeSeconds = 15f;

    static readonly Color NormalColor = Color.white;
    static readonly Color LowTimeColor = new Color(1f, 0.35f, 0.35f, 1f);

    public static TurnTimer Instance { get; private set; }

    class Clock
    {
        public float Remaining = TurnSeconds;
        public float InteractionRemaining = InteractionSeconds;
        public int SelectionWaits = 0;
        public TextMeshProUGUI Text = null;
    }

    readonly Dictionary<Player, Clock> _clocks = new Dictionary<Player, Clock>();
    Player _mainPhaseWaitPlayer = null;
    bool _timeoutSent = false;

    #region Unity
    void Awake()
    {
        Instance = this;

        // Added at runtime next to TurnStateMachine; make sure Photon sees our RPC.
        if (photonView != null)
        {
            photonView.RefreshRpcMonoBehaviourCache();
        }
    }

    void OnDestroy()
    {
        foreach (Clock clock in _clocks.Values)
        {
            if (clock.Text != null)
            {
                Destroy(clock.Text.gameObject);
            }
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Update()
    {
        TurnStateMachine turnStateMachine = GManager.instance != null ? GManager.instance.turnStateMachine : null;

        if (turnStateMachine == null || turnStateMachine.gameContext == null || turnStateMachine.endGame)
        {
            return;
        }

        bool paused = ReconnectManager.IsHandlingDisconnect;

        foreach (Player player in turnStateMachine.gameContext.Players)
        {
            if (player == null)
            {
                continue;
            }

            Clock clock = GetClock(player);
            bool running = !paused && IsWaitingOn(player, clock, turnStateMachine);

            if (running)
            {
                clock.Remaining -= Time.unscaledDeltaTime;
                clock.InteractionRemaining -= Time.unscaledDeltaTime;
            }

            UpdateText(clock, running);

            if (running && !_timeoutSent)
            {
                float timeoutAt = player.isYou ? 0f : -RemoteTimeoutGraceSeconds;

                if (clock.Remaining <= timeoutAt)
                {
                    SendTimeout(player, false);
                }
                else if (clock.InteractionRemaining <= timeoutAt)
                {
                    SendTimeout(player, true);
                }
            }
        }
    }
    #endregion

    #region Called by the game flow
    // A selection prompt for this player started.
    public static void BeginWait(Player player)
    {
        if (Instance == null || player == null)
        {
            return;
        }

        Clock clock = Instance.GetClock(player);

        if (clock.SelectionWaits == 0 && Instance._mainPhaseWaitPlayer != player)
        {
            clock.InteractionRemaining = InteractionSeconds;
        }

        clock.SelectionWaits++;
    }

    // A selection prompt for this player ended. bonusSeconds is given back when it was answered.
    public static void EndWait(Player player, float bonusSeconds)
    {
        if (Instance == null || player == null)
        {
            return;
        }

        Clock clock = Instance.GetClock(player);
        clock.SelectionWaits = Mathf.Max(0, clock.SelectionWaits - 1);
        Instance.AddTime(clock, bonusSeconds);
    }

    // The turn player can choose a main phase action. Safe to call every frame.
    public static void BeginMainPhaseWait(Player player)
    {
        if (Instance == null || player == null)
        {
            return;
        }

        if (Instance._mainPhaseWaitPlayer != player)
        {
            Instance.GetClock(player).InteractionRemaining = InteractionSeconds;
        }

        Instance._mainPhaseWaitPlayer = player;
    }

    // The turn player chose a main phase action.
    public static void EndMainPhaseWait(float bonusSeconds)
    {
        if (Instance == null)
        {
            return;
        }

        if (Instance._mainPhaseWaitPlayer != null)
        {
            Instance.AddTime(Instance.GetClock(Instance._mainPhaseWaitPlayer), bonusSeconds);
        }

        Instance._mainPhaseWaitPlayer = null;
    }

    public static void ResetForNewTurn()
    {
        if (Instance == null)
        {
            return;
        }

        foreach (Clock clock in Instance._clocks.Values)
        {
            clock.Remaining = TurnSeconds;
            clock.InteractionRemaining = InteractionSeconds;
            // No prompt is open across a turn change; clear anything left behind by a stopped coroutine.
            clock.SelectionWaits = 0;
        }

        Instance._mainPhaseWaitPlayer = null;
    }
    #endregion

    #region Helpers
    Clock GetClock(Player player)
    {
        if (!_clocks.TryGetValue(player, out Clock clock))
        {
            clock = new Clock();
            clock.Text = CreateText(player);
            _clocks[player] = clock;
        }

        return clock;
    }

    bool IsWaitingOn(Player player, Clock clock, TurnStateMachine turnStateMachine)
    {
        if (clock.SelectionWaits > 0)
        {
            return true;
        }

        return _mainPhaseWaitPlayer == player
            && turnStateMachine.gameContext.TurnPhase == GameContext.phase.Main
            && !turnStateMachine.Passed;
    }

    void AddTime(Clock clock, float seconds)
    {
        if (seconds <= 0f)
        {
            return;
        }

        clock.Remaining = Mathf.Min(TurnSeconds, clock.Remaining + seconds);
    }
    #endregion

    #region Timeout
    void SendTimeout(Player loser, bool interactionTimeout)
    {
        _timeoutSent = true;

        try
        {
            photonView.RPC("TimeOut", RpcTarget.All, loser.PlayerID, interactionTimeout);
        }
        catch (Exception e)
        {
            Debug.LogError($"[TurnTimer] Error sending timeout RPC: {e.Message}");
            TimeOut(loser.PlayerID, interactionTimeout);
        }
    }

    // interactionTimeout: the single-interaction limit ran out rather than the turn clock.
    [PunRPC]
    public void TimeOut(int loserPlayerID, bool interactionTimeout)
    {
        _timeoutSent = true;

        TurnStateMachine turnStateMachine = GManager.instance != null ? GManager.instance.turnStateMachine : null;

        if (turnStateMachine == null || turnStateMachine.endGame)
        {
            return;
        }

        Player loser = GManager.instance.GetPlayerFromID(loserPlayerID);

        if (loser == null)
        {
            return;
        }

        string reason = interactionTimeout ? "the action time limit" : "the turn time limit";
        Debug.Log($"[TurnTimer] {loser.PlayerName} ran out of time ({reason}).");
        PlayLog.OnAddLog?.Invoke($"\n{loser.PlayerName} ran out of time ({reason}).\n");

        turnStateMachine.EndGame(loser.Enemy, false);

        string resultText;

        if (interactionTimeout)
        {
            resultText = loser.isYou
                ? LocalizeUtility.GetLocalizedString(EngMessage: "You ran out of time for an action.", JpnMessage: "行動の制限時間が切れました。")
                : LocalizeUtility.GetLocalizedString(EngMessage: "The opponent ran out of time for an action.", JpnMessage: "対戦相手の行動の制限時間が切れました。");
        }
        else
        {
            resultText = loser.isYou
                ? LocalizeUtility.GetLocalizedString(EngMessage: "You ran out of time.", JpnMessage: "持ち時間が切れました。")
                : LocalizeUtility.GetLocalizedString(EngMessage: "The opponent ran out of time.", JpnMessage: "対戦相手の持ち時間が切れました。");
        }

        GManager.instance.resultObject.SetResultText(resultText);
    }
    #endregion

    #region Display
    // A copy of the player's name label, placed just below it, so it matches the battle UI styling.
    TextMeshProUGUI CreateText(Player player)
    {
        if (player.PlayerNameText == null)
        {
            return null;
        }

        TextMeshProUGUI nameText = player.PlayerNameText;
        GameObject textObject = Instantiate(nameText.gameObject, nameText.transform.parent);
        textObject.name = "TurnTimerText";

        TextMeshProUGUI timerText = textObject.GetComponent<TextMeshProUGUI>();
        timerText.raycastTarget = false;

        if (nameText.transform.parent.GetComponent<LayoutGroup>() == null)
        {
            RectTransform nameRect = nameText.rectTransform;
            RectTransform timerRect = timerText.rectTransform;
            timerRect.anchoredPosition = nameRect.anchoredPosition - new Vector2(0f, nameRect.rect.height);
        }

        return timerText;
    }

    void UpdateText(Clock clock, bool running)
    {
        if (clock.Text == null)
        {
            return;
        }

        int seconds = Mathf.Max(0, Mathf.CeilToInt(clock.Remaining));
        int interactionSeconds = Mathf.Max(0, Mathf.CeilToInt(clock.InteractionRemaining));
        bool lowTime = seconds <= LowTimeSeconds;

        // Turn clock always; the action clock only while this player has a prompt open.
        if (running)
        {
            clock.Text.text = $"{FormatTime(seconds)}  ({FormatTime(interactionSeconds)})";
            lowTime = lowTime || interactionSeconds <= LowInteractionTimeSeconds;
        }
        else
        {
            clock.Text.text = FormatTime(seconds);
        }

        Color color = lowTime ? LowTimeColor : NormalColor;
        color.a = running ? 1f : 0.5f;
        clock.Text.color = color;
    }

    static string FormatTime(int seconds) => $"{seconds}s";
    #endregion
}
