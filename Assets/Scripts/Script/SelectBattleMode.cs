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

    public void OffSelectBattleMode()
    {
        Off();
    }

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

        Opening.instance.battle.selectBattleDeck.Off();

        yield return ContinuousController.instance.StartCoroutine(loadingObject.StartLoading("Connecting"));

        connecting = true;

        yield return ContinuousController.instance.StartCoroutine(PhotonUtility.ConnectToLobbyCoroutine());

        // ContinuousController.instance.BattleDeckData = null;
        yield return ContinuousController.instance.StartCoroutine(PhotonUtility.DeleteBattleDeckData());

        connecting = false;

        yield return ContinuousController.instance.StartCoroutine(loadingObject.EndLoading());

        this.gameObject.SetActive(true);

        StartSelectBattleMode();
    }

    public void StartSelectBattleMode()
    {
        List<UnityAction> Commands = new List<UnityAction>()
            {
                () =>
                {
                    //ランダムマッチ
                    StartSelectBattleDeck(false);
                },

                () =>
                {
                    //ルームマッチ
                    StartSelectRoomMatch();
                },

                () =>
                {
                    //AI戦
                    StartSelectBattleDeck(true);
                },
            };

        List<string> CommandTexts = new List<string>()
            {
                LocalizeUtility.GetLocalizedString(
                    EngMessage:"Random Match",
                    JpnMessage:"ランダムマッチ"
                ),
                LocalizeUtility.GetLocalizedString(
                    EngMessage:"Room Match",
                    JpnMessage:"ルームマッチ"
                ),
                LocalizeUtility.GetLocalizedString(
                    EngMessage:"Bot Match",
                    JpnMessage:"Bot戦"
                ),
            };

        selectBattleModeWindow.SetUpYesNoObject(
            Commands,
            CommandTexts,
            LocalizeUtility.GetLocalizedString(
                    EngMessage: "Please select the mode to play.",
                    JpnMessage: "対戦モードを選択してください"
                ),
            true);
    }

    void StartSelectBattleDeck(bool isAI)
    {
        Opening.instance.OffYesNoObjects();

        Opening.instance.deck.trialDraw.Close();

        Opening.instance.deck.deckListPanel.Close();

        enterRoom.Close_(false);
        selectRoomMatchWindow.Close_(false);

        ContinuousController.instance.isAI = isAI;
        ContinuousController.instance.isRandomMatch = true;

        Opening.instance.battle.selectBattleDeck.Off();

        if (!ContinuousController.instance.isAI)
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
        Opening.instance.home.SetUpHome();
        this.gameObject.SetActive(false);
    }
}
