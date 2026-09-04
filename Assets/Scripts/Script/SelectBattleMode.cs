using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SelectBattleMode : MonoBehaviour
{
    [Header("バトルモード選択")]
    public YesNoObject selectBattleModeWindow;

    [Header("ルームマッチ選択")]
    [SerializeField] YesNoObject selectRoomMatchWindow;

    [Header("ルームID入力")]
    [SerializeField] EnterRoom enterRoom;

    [Header("ルームマッチマネージャ")]
    [SerializeField] RoomManager roomManager;

    [Header("LoadingObject")]
    public LoadingObject loadingObject;

    public void HideOverlayDialogs()
    {
        selectRoomMatchWindow.Off();
        enterRoom.Off();
        // === DCGO-CUSTOM:tournament begin ===
        enterRoom.JoinTournament = false;
        // === DCGO-CUSTOM:tournament end ===
        Opening.instance.OffYesNoObjects();
    }

    public void OffSelectBattleMode()
    {
        Off();
    }
    // === DCGO-CUSTOM:tournament begin ===
    public void StartSelectTournament()
    {
        Opening.instance.OffYesNoObjects();

        Opening.instance.deck.trialDraw.Close();

        Opening.instance.deck.deckListPanel.Close();

        Opening.instance.battle.selectBattleDeck.Off();
        ContinuousController.instance.isAI = false;
        ContinuousController.instance.isRanked = false;
        // === DCGO-CUSTOM:tournament begin ===
        Opening.instance.battle.tournamentLobbyManager?.Off();
        ContinuousController.instance.ClearTournament();
        // === DCGO-CUSTOM:tournament end ===
        ContinuousController.instance.isRandomMatch = false;
        // === DCGO-CUSTOM:ranked begin ===
        Opening.instance.battle.lobbyManager_RandomMatch?.OffLobby();
        Opening.instance.battle.lobbyManager_RankedMatch?.OffLobby();
        ContinuousController.instance.isRanked = false;
        // === DCGO-CUSTOM:ranked end ===
        ContinuousController.instance.isTournament = true;
        // === DCGO-CUSTOM:friends begin ===
        ContinuousController.instance.ClearFriendDuel();
        // === DCGO-CUSTOM:friends end ===

        List<UnityAction> Commands = new List<UnityAction>()
            {
                () =>
                {
                    StartSelectTournamentSize();
                },

                () =>
                {
                    StartEnterTournamentID();
                },
            };

        List<string> CommandTexts = new List<string>()
            {
                LocalizeUtility.GetLocalizedString(
                    EngMessage:"Create Tournament",
                    JpnMessage:"トーナメント作成"
                ),
                LocalizeUtility.GetLocalizedString(
                    EngMessage:"Join Tournament",
                    JpnMessage:"トーナメントに入る"
                ),
            };

        selectRoomMatchWindow.SetUpYesNoObject(
            Commands,
            CommandTexts,
            LocalizeUtility.GetLocalizedString(
                    EngMessage: "Create a 4 / 8 / 16 player tournament or join with a room ID. Host can start early — empty seats become byes.",
                    JpnMessage: "4 / 8 / 16人トーナメントを作成するか、ルームIDで参加。人数が足りなくても開始でき、空き枠はBYEになります"
                ),
            true);
    }

    void StartSelectTournamentSize()
    {
        Opening.instance.OffYesNoObjects();

        Opening.instance.deck.trialDraw.Close();

        Opening.instance.deck.deckListPanel.Close();

        List<UnityAction> Commands = new List<UnityAction>();
        List<string> CommandTexts = new List<string>();

        foreach (int size in TournamentKeys.AllowedPlayerCounts)
        {
            int captured = size;
            Commands.Add(() => StartCreateTournament(captured));
            CommandTexts.Add(LocalizeUtility.GetLocalizedString(
                EngMessage: $"{captured} Players",
                JpnMessage: $"{captured}人"));
        }

        selectRoomMatchWindow.SetUpYesNoObject(
            Commands,
            CommandTexts,
            LocalizeUtility.GetLocalizedString(
                EngMessage: "Choose tournament size (Best of 3, single elimination). Fewer players → byes.",
                JpnMessage: "トーナメント人数を選んでください（3本先取・シングルエリミネーション）。不足分はBYE。"),
            true);
    }

    void StartCreateTournament(int playerCount)
    {
        HideOverlayDialogs();
        TournamentKeys.ActivePlayerCount = playerCount;
        TournamentLobbyManager.EnsureExists().SetUpLobby(createNew: true);
    }

    void StartEnterTournamentID()
    {
        HideOverlayDialogs();

        Opening.instance.deck.trialDraw.Close();

        Opening.instance.deck.deckListPanel.Close();

        enterRoom.JoinTournament = true;
        enterRoom.SetUpEnterRoom();
    }
    // === DCGO-CUSTOM:tournament end ===
    // === DCGO-CUSTOM:ranked begin ===
    IEnumerator ShowRankOnBattleModeSelect(string baseInfo)
    {
        yield return RankedServices.EnsureExists().BootstrapForRanked();

        var profile = RankedServices.Instance != null ? RankedServices.Instance.Profile.Cached : null;
        string rankLine = profile != null
            ? profile.FormatStatusLine()
            : LocalizeUtility.GetLocalizedString(
                EngMessage: "Ranked: —",
                JpnMessage: "ランク: —");

        if (selectBattleModeWindow != null && selectBattleModeWindow.InfoText != null)
        {
            selectBattleModeWindow.InfoText.text = $"{baseInfo}\n{rankLine}";
        }

        // Append short tier/MMR under the Ranked Match button label (index 1).
        if (selectBattleModeWindow != null &&
            selectBattleModeWindow.Buttons != null &&
            selectBattleModeWindow.Buttons.Count > 1 &&
            profile != null)
        {
            var label = selectBattleModeWindow.Buttons[1].transform.GetChild(0).GetComponent<Text>();
            if (label != null)
            {
                string modeName = LocalizeUtility.GetLocalizedString(
                    EngMessage: "Ranked Match",
                    JpnMessage: "ランクマッチ");
                label.text = $"{modeName}\n{profile.FormatShort()}";
            }
        }
    }
    // === DCGO-CUSTOM:ranked end ===

    public void Off()
    {
        this.gameObject.SetActive(false);
    }

    bool connecting = false;

    public void SetUpSelectBattleMode()
    {
        if (connecting)
        {
            return;
        }

        ContinuousController.instance.StartCoroutine(SetUpSelectBattleModeCoroutine());
    }

    public IEnumerator SetUpSelectBattleModeCoroutine()
    {
        selectBattleModeWindow.CloseOnButtonClicked = false;
        selectRoomMatchWindow.CloseOnButtonClicked = false;

        selectBattleModeWindow.Off();
        selectRoomMatchWindow.Off();
        enterRoom.Off();
        // === DCGO-CUSTOM:tournament begin ===
        enterRoom.JoinTournament = false;
        // === DCGO-CUSTOM:tournament end ===

        // === DCGO-CUSTOM:friends begin ===
        ContinuousController.instance.ClearFriendDuel();
        FriendListPanel.HideIfOpen();
        // === DCGO-CUSTOM:friends end ===

        Opening.instance.battle.selectBattleDeck.Off();

        if (PhotonNetwork.IsConnected)
        {
            yield return ContinuousController.instance.StartCoroutine(PhotonUtility.DisconnectCoroutine());
        }

        this.gameObject.SetActive(true);

        StartSelectBattleMode();
    }

    public void StartSelectBattleMode()
    {
        List<UnityAction> Commands = new List<UnityAction>()
            {
                () =>
                {
                    StartSelectBattleDeck(isAI: false, isRanked: false);
                },
                () =>
                {
                    StartSelectBattleDeck(isAI: false, isRanked: true);
                },
                () =>
                {
                    StartSelectRoomMatch();
                },
                () =>
                {
                    StartSelectTournament();
                },
                () =>
                {
                    StartSelectBattleDeck(isAI: true, isRanked: false);
                },
            };

        List<string> CommandTexts = new List<string>()
            {
                LocalizeUtility.GetLocalizedString(
                    EngMessage:"Random Match",
                    JpnMessage:"ランダムマッチ"
                ),
                LocalizeUtility.GetLocalizedString(
                    EngMessage:"Ranked Match",
                    JpnMessage:"ランクマッチ"
                ),
                LocalizeUtility.GetLocalizedString(
                    EngMessage:"Room Match",
                    JpnMessage:"ルームマッチ"
                ),
                LocalizeUtility.GetLocalizedString(
                    EngMessage:"Tournament",
                    JpnMessage:"トーナメント"
                ),
                LocalizeUtility.GetLocalizedString(
                    EngMessage:"Bot Match",
                    JpnMessage:"Bot戦"
                ),
            };

        string baseInfo = LocalizeUtility.GetLocalizedString(
                    EngMessage: "Please select the mode to play.",
                    JpnMessage: "対戦モードを選択してください");

        selectBattleModeWindow.SetUpYesNoObject(
            Commands,
            CommandTexts,
            baseInfo,
            true);
        ContinuousController.instance.StartCoroutine(ShowRankOnBattleModeSelect(baseInfo));
    }

    void StartSelectBattleDeck(bool isAI, bool isRanked = false)
    {
        Opening.instance.OffYesNoObjects();

        Opening.instance.deck.trialDraw.Close();

        Opening.instance.deck.deckListPanel.Close();

        enterRoom.Close_(false);
        selectRoomMatchWindow.Close_(false);
        // === DCGO-CUSTOM:tournament begin ===
        if (ContinuousController.instance.isTournament && !PhotonNetwork.InRoom)
        {
            ContinuousController.instance.ClearTournament();
        }
        // === DCGO-CUSTOM:tournament end ===

        ContinuousController.instance.isAI = isAI;
        ContinuousController.instance.isRanked = isRanked && !isAI;
        ContinuousController.instance.isRandomMatch = !isAI && !isRanked;
        ContinuousController.instance.isTournament = false;
        ContinuousController.instance.ClearFriendDuel();

        Opening.instance.battle.selectBattleDeck.Off();

        if (ContinuousController.instance.isRanked)
        {
            Opening.instance.battle.selectBattleDeck.SetUpSelectBattleDeck(Opening.instance.battle.selectBattleDeck.OnClickSelectButton_RankedMatch, 0);
        }
        else if (!ContinuousController.instance.isAI)
        {
            Opening.instance.battle.selectBattleDeck.SetUpSelectBattleDeck(Opening.instance.battle.selectBattleDeck.OnClickSelectButton_RandomMatch, 0);
        }

        else
        {
            Opening.instance.battle.selectBattleDeck.SetUpSelectBattleDeck(() =>
            {
                Opening.instance.battle.selectBattleDeck.OnClickSelectButton_BotMatch();
                ContinuousController.instance.StartCoroutine(StartBattleCoroutine());
            }

            , 0);
        }

        IEnumerator StartBattleCoroutine()
        {
            selectRoomMatchWindow.Close_(false);
            enterRoom.Close_(false);

            ContinuousController.instance.StartCoroutine(Opening.instance.OpeningBGM.FadeOut(0.1f));
            yield return ContinuousController.instance.StartCoroutine(Opening.instance.LoadingObject.StartLoading("Now Loading"));

            foreach (Camera camera in Opening.instance.openingCameras)
            {
                camera.gameObject.SetActive(false);
            }

            Opening.instance.OffYesNoObjects();

            Opening.instance.deck.trialDraw.Close();

            Opening.instance.deck.deckListPanel.Close();

            yield return new WaitForSeconds(0.1f);
            SceneManager.LoadSceneAsync("BattleScene", LoadSceneMode.Additive);
        }
    }

    public void StartSelectRoomMatch()
    {
        Opening.instance.OffYesNoObjects();

        Opening.instance.deck.trialDraw.Close();

        Opening.instance.deck.deckListPanel.Close();

        Opening.instance.battle.selectBattleDeck.Off();
        ContinuousController.instance.isAI = false;
        // === DCGO-CUSTOM:ranked begin ===
        ContinuousController.instance.isRanked = false;
        // === DCGO-CUSTOM:ranked end ===

        List<UnityAction> Commands = new List<UnityAction>()
            {
                () =>
                {
                    //部屋を作る
                    StartCreateRoom();
                },

                () =>
                {
                    //部屋に入る
                    StartEnterRoomID();
                },
            };

        List<string> CommandTexts = new List<string>()
            {
                LocalizeUtility.GetLocalizedString(
                    EngMessage:"Create Room",
                    JpnMessage:"ルーム作成"
                ),
                LocalizeUtility.GetLocalizedString(
                    EngMessage:"Join Room",
                    JpnMessage:"ルームに入る"
                ),
            };

        selectRoomMatchWindow.SetUpYesNoObject(
            Commands,
            CommandTexts,
            LocalizeUtility.GetLocalizedString(
                    EngMessage: "Please choose between creating a room or joining an existing one.",
                    JpnMessage: "ルームを作成するかルームに入るか\n選択してください"
                ),
            true);
    }

    void StartCreateRoom()
    {
        roomManager.SetUpRoom();
    }

    void StartEnterRoomID()
    {
        Opening.instance.OffYesNoObjects();

        Opening.instance.deck.trialDraw.Close();

        Opening.instance.deck.deckListPanel.Close();

        enterRoom.SetUpEnterRoom();
    }

    public void OnClickCloseEnterRoomWindow()
    {
        enterRoom.Close_(false);
        ContinuousController.instance.PlaySE(Opening.instance.CancelSE);

        Opening.instance.OffYesNoObjects();

        Opening.instance.deck.trialDraw.Close();

        Opening.instance.deck.deckListPanel.Close();
    }

    public void OnClickCloseSelectRoomMatchWindow()
    {
        enterRoom.Close_(false);
        selectRoomMatchWindow.Close_(false);
        ContinuousController.instance.PlaySE(Opening.instance.CancelSE);

        Opening.instance.OffYesNoObjects();

        Opening.instance.deck.trialDraw.Close();

        Opening.instance.deck.deckListPanel.Close();
    }

    public void OnClickSelectBattleModeWindow()
    {
        enterRoom.Close_(true);
        selectRoomMatchWindow.Close_(false);
        selectBattleModeWindow.Close_(false);
        ContinuousController.instance.PlaySE(Opening.instance.CancelSE);

        Opening.instance.OffYesNoObjects();

        Opening.instance.deck.trialDraw.Close();

        Opening.instance.deck.deckListPanel.Close();

        ContinuousController.instance.StartCoroutine(OnClickSelectBattleModeWindowIEnumerator());
    }

    IEnumerator OnClickSelectBattleModeWindowIEnumerator()
    {
        yield return new WaitForSeconds(0.3f);
        Opening.instance.battle.OffBattle();
        Opening.instance.home.SetUpHomeMode_Disconnect();
        this.gameObject.SetActive(false);
    }
}
