using DCGO.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomManager : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1f);
    private static WaitForSeconds _waitForSeconds0_1 = new WaitForSeconds(0.1f);
    [SerializeField] bool _isReady = false;

    bool endSetUp { get; set; }

    [Header("準備完了ボタンテキスト")]
    public Text ReadyButtonText;

    [Header("準備完了ボタン")]
    public Button ReadyButton;

    [Header("プレイヤー情報プレハブ")]
    public GameObject PlayerElementPrefab;//部屋情報Prefab

    [Header("プレイヤー情報プレハブの親")]
    public GameObject PlayerParent;//ScrolViewのcontentオブジェクト

    [Header("読み込み中オブジェクト")]
    public LoadingObject loadingObject;

    [Header("切断時読み込み中オブジェクト")]
    public LoadingObject disconnectLoadingObject;

    [Header("ルームIDテキスト")]
    public Text RoomIDText;

    [Header("デッキ情報パネル")]
    public DeckInfoPanel deckInfoPanel;

    [Header("無効なデッキ表示")]
    public GameObject InvalidDeckObject;

    [Header("デッキ無表示")]
    public GameObject NoDeckSetObject;
    [SerializeField] List<FirstPlayerIndexIdToggle> _firstPlayerIndexIdToggles = new();
    [SerializeField] GameObject _switchFirstPlayerToggleCover;

    public GameObject Parent;

    bool DoneStartBattle = false;

    bool _leftRoom = false;

    void Awake()
    {
        foreach (FirstPlayerIndexIdToggle firstPlayerIndexIdToggle in _firstPlayerIndexIdToggles)
        {
            firstPlayerIndexIdToggle.OnClickAction = OnClickFirstPlayerIndexIDButton;
        }
    }

    #region Open Room Screen

    #region Open Room Screen
    public void SetUpRoom(bool createRoom)
    {
        ContinuousController.instance.StartCoroutine(SetUpRoomCoroutine(createRoom));
    }

    public IEnumerator SetUpRoomCoroutine(bool createRoom)
    {
        Opening.instance.OffYesNoObjects();

        Opening.instance.deck.trialDraw.Close();

        Opening.instance.deck.deckListPanel.Close();

        ContinuousController.instance.isAI = false;
        ContinuousController.instance.isRandomMatch = false;

        yield return ContinuousController.instance.StartCoroutine(loadingObject.StartLoading("Now Loading"));

        endSetUp = false;

        Parent.SetActive(true);

        if (createRoom)
        {
            yield return ContinuousController.instance.StartCoroutine(CreateRoomCoroutine());
        }

        Init();

        SetDeckInfoPanel();
        CheckReadyButton();

        yield return ContinuousController.instance.StartCoroutine(loadingObject.EndLoading());

        endSetUp = true;
    }
    #endregion

    #region Process to create a room
    public IEnumerator CreateRoomCoroutine()
    {
        bool waitingOnRequest = false;
        bool inRoom = false;

        while (!inRoom)
        {
            waitingOnRequest = true;
            DCGONetwork.Provider.CreatePrivateRoom(ContinuousController.instance.useBanlist, GetPrivateRoomEvents((bool success) =>
            {
                waitingOnRequest = false;
                if (success)
                {
                    inRoom = true;
                }
            }));

            yield return new WaitUntil(() => !waitingOnRequest);
            yield return new WaitForSeconds(1f);
        }
    }
    #endregion

    #region Private room events
    public PrivateRoomEvents GetPrivateRoomEvents(Action<bool> onJoined)
    {
        PrivateRoomEvents privateRoomEvents = new PrivateRoomEvents();

        privateRoomEvents.OnJoined += onJoined;
        privateRoomEvents.OnRoomInfoUpdated += OnRoomInfoUpdated;
        privateRoomEvents.OnAllPlayersReady += GoToBattleScene;
        privateRoomEvents.OnLeft += OnLeftRoom;
        privateRoomEvents.OnDisconnected += OnRoomDisconnected;

        return privateRoomEvents;
    }

    void OnLeftRoom()
    {
        _leftRoom = true;
    }

    void OnRoomDisconnected()
    {
        if (Parent.activeSelf)
        {
            CloseRoom();
        }
    }
    #endregion

    #region Initialized when room screen is opened
    public void Init()
    {
        Opening.instance.battle.selectBattleDeck.Off();
        DoneStartBattle = false;
        _isReady = false;

        if (ContinuousController.instance.LastBattleDeckData != null
           && ContinuousController.instance.DeckDatas.Contains(ContinuousController.instance.LastBattleDeckData)
           && ContinuousController.instance.LastBattleDeckData.IsValidDeckData())
        {
            ContinuousController.instance.BattleDeckData = ContinuousController.instance.LastBattleDeckData;
        }

        else
        {
            ContinuousController.instance.BattleDeckData = FirstValidDeckData();
        }

        DCGONetwork.Provider.SetDeckData(ContinuousController.instance.BattleDeckData);
        DCGONetwork.Provider.SetRoomReadyState(_isReady);
        DCGONetwork.Provider.SetRoomSetting(DataBase.FirstPlayerKey, -1);

        OnRoomInfoUpdated(DCGONetwork.Provider.GetRoomInfo());
    }

    #region The first proper deck in the deck list
    DeckData FirstValidDeckData()
    {
        if (ContinuousController.instance.DeckDatas != null)
        {
            foreach (DeckData deckData in ContinuousController.instance.DeckDatas)
            {
                if (deckData != null)
                {
                    if (deckData.IsValidDeckData())
                    {
                        return deckData;
                    }
                }
            }
        }

        return null;
    }
    #endregion

    void OnRoomInfoUpdated(RoomInfo RoomInfo)
    {
        RoomIDText.text = RoomInfo.RoomCode;
        UpdatePlayersUI(RoomInfo.Players);

        int firstPlayerIndexID = -1;

        if (RoomInfo.RoomSettings.TryGetValue(DataBase.FirstPlayerKey, out int value))
        {
            firstPlayerIndexID = (int)value;
        }

        foreach (FirstPlayerIndexIdToggle firstPlayerIndexIdToggle in _firstPlayerIndexIdToggles)
        {
            firstPlayerIndexIdToggle.Toggle.isOn = firstPlayerIndexIdToggle.FirstPlayerIndexID == firstPlayerIndexID;
        }
    }

    #endregion

    #endregion

    #region Close the room screen
    public void CloseRoom()
    {
        ContinuousController.instance.StartCoroutine(CloseRoomCoroutine());
    }

    public IEnumerator CloseRoomCoroutine()
    {
        Opening.instance.battle.selectBattleDeck.OnCloseSelectBattleDeckAction = null;
        Opening.instance.battle.selectBattleDeck.Off();
        yield return ContinuousController.instance.StartCoroutine(disconnectLoadingObject.StartLoading("Now Loading"));

        Off();

        _isReady = false;
        endSetUp = false;

        _leftRoom = false;

        DCGONetwork.Provider.LeavePrivateRoom();

        yield return new WaitUntil(() => _leftRoom);

        if (!Opening.instance.battle.selectBattleMode.selectBattleModeWindow.gameObject.activeSelf)
        {
            Opening.instance.battle.selectBattleMode.SetUpSelectBattleMode();

            Opening.instance.battle.selectBattleMode.StartSelectRoomMatch();
        }

        yield return ContinuousController.instance.StartCoroutine(disconnectLoadingObject.EndLoading());
    }
   

    public void Off()
    {
        Parent.SetActive(false);
    }
    #endregion


    #region Processing when the Ready button is pressed
    public void OnClickGetReadyButton()
    {
        _isReady = !_isReady;

        DCGONetwork.Provider.SetRoomReadyState(_isReady);

        if (_isReady)
        {
            Opening.instance.PlayDecisionSE();
        }

        else
        {
            Opening.instance.PlayCancelSE();
        }
    }
    #endregion

    #region Start of battle
    public void GoToBattleScene()
    {
        if (!DoneStartBattle)
        {
            DoneStartBattle = true;
            StartCoroutine(GoToBattleSceneCoroutine());
        }
    }

    IEnumerator GoToBattleSceneCoroutine()
    {
        yield return ContinuousController.instance.StartCoroutine(Opening.instance.LoadingObject_Unload.StartLoading("Now Loading"));

        yield return _waitForSeconds0_1;

        Opening.instance.MainCamera.gameObject.SetActive(false);

        foreach (Camera camera in Opening.instance.openingCameras)
        {
            camera.gameObject.SetActive(false);
        }

        ContinuousController.instance.StartCoroutine(Opening.instance.OpeningBGM.FadeOut(0.5f));

        yield return _waitForSeconds1;

        SceneManager.LoadSceneAsync("BattleScene", LoadSceneMode.Additive);
        yield return ContinuousController.instance.StartCoroutine(Opening.instance.LoadingObject_Unload.EndLoading());
    }
    #endregion

    #region Create PlayerElement corresponding to the information of players in the room
    public void UpdatePlayersUI(List<NetworkPlayerInfo> PlayerInfo)
    {
        DestroyChildObject();

        if (PlayerInfo == null)
        {
            return;
        }

        for (int i = 0; i < PlayerInfo.Count; i++)
        {
            GameObject PlayerElement = Instantiate(PlayerElementPrefab, PlayerParent.transform);

            PlayerElement.GetComponent<CPlayerElement>().SetPlayerInfo(PlayerInfo[i].Name, PlayerInfo[i].IsReady);
        }
    }
    #endregion

    public void DestroyChildObject()
    {
        for (int i = 0; i < PlayerParent.transform.childCount; ++i)
        {
            Destroy(PlayerParent.transform.GetChild(i).gameObject);
        }
    }

    #region Check the Ready button
    public void CheckReadyButton()
    {
        bool oldIsReady = _isReady;

        if (CanReady())
        {
            ReadyButton.interactable = true;

            if (_isReady)
            {
                ReadyButtonText.text = LocalizeUtility.GetLocalizedString(
                    EngMessage: "Cancel",
                    JpnMessage: "キャンセル"
                );
            }

            else
            {
                ReadyButtonText.text = LocalizeUtility.GetLocalizedString(
                    EngMessage: "Finish Preparation",
                    JpnMessage: "準備完了する"
                );
            }
        }

        else
        {
            ReadyButton.interactable = false;
            ReadyButtonText.text = "Finish Preparation";
            _isReady = false;
            DCGONetwork.Provider.SetRoomReadyState(_isReady);
        }
    }

    bool CanReady()
    {
        if (!DoneStartBattle && !Opening.instance.battle.selectBattleDeck.gameObject.activeSelf)
        {
            if (ContinuousController.instance.BattleDeckData != null && ContinuousController.instance.DeckDatas != null)
            {
                //if (ContinuousController.instance.DeckDatas.Contains(ContinuousController.instance.BattleDeckData))
                {
                    if (ContinuousController.instance.BattleDeckData.IsValidDeckData())
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    #endregion

    #region Battle deck selection begins
    bool _oldIsReady = false;
    public void OnClickSelectBattleDeckButton()
    {
        int defaulSelectDeckIndex = 0;

        if (ContinuousController.instance.BattleDeckData != null)
        {
            defaulSelectDeckIndex = ContinuousController.instance.DeckDatas.IndexOf(ContinuousController.instance.BattleDeckData);
        }

        Opening.instance.battle.selectBattleDeck.SetUpSelectBattleDeck(
            () =>
            {
                Opening.instance.battle.selectBattleDeck.OnCloseSelectBattleDeckAction = null;
                ContinuousController.instance.StartCoroutine(OnEndCoroutine());

                IEnumerator OnEndCoroutine()
                {
                    yield return ContinuousController.instance.StartCoroutine(Opening.instance.battle.selectBattleDeck.OnClickSelectButton_RoomMatchCoroutine());

                    yield return ContinuousController.instance.StartCoroutine(EndSelectBattleDeckCoroutine());
                }
            },

            defaulSelectDeckIndex);

        Opening.instance.battle.selectBattleDeck.OnCloseSelectBattleDeckAction = () => ContinuousController.instance.StartCoroutine(EndSelectBattleDeckCoroutine());

        _oldIsReady = _isReady;

        _isReady = false;
    }
    #endregion

    #region End of battle deck selection
    IEnumerator EndSelectBattleDeckCoroutine()
    {
        //if (this.gameObject.activeSelf)
        {
            //yield return ContinuousController.instance.StartCoroutine(SignUpBattleDeck(ContinuousController.instance.BattleDeckData));

            yield return new WaitWhile(() => Opening.instance.battle.selectBattleDeck.gameObject.activeSelf);

            yield return new WaitForSeconds(Time.deltaTime);

            ContinuousController.instance.BattleDeckData = Opening.instance.battle.selectBattleDeck.deckInfoPanel.ShowingDeckData;

            yield return new WaitForSeconds(Time.deltaTime);

            SetDeckInfoPanel();

            _isReady = _oldIsReady;

            if (!CanReady())
            {
                _isReady = false;
            }

            CheckReadyButton();

            DCGONetwork.Provider.SetRoomReadyState(_isReady);
        }
    }
    #endregion

    #region Display deck information panel
    public async void SetDeckInfoPanel()
    {
        if (ContinuousController.instance.BattleDeckData == null)
        {
            await deckInfoPanel.SetUpDeckInfoPanel(DeckData.EmptyDeckData());

            NoDeckSetObject.SetActive(true);
            InvalidDeckObject.SetActive(false);
        }

        else
        {
            await deckInfoPanel.SetUpDeckInfoPanel(ContinuousController.instance.BattleDeckData);

            NoDeckSetObject.SetActive(ContinuousController.instance.BattleDeckData.AllDeckCards().Count == 0);
            InvalidDeckObject.SetActive(!ContinuousController.instance.BattleDeckData.IsValidDeckData());
        }
    }
    #endregion

    #region Processing when the Copy Room ID button is pressed
    public void OnClickCopyRoomIDButton()
    {
        if (RoomIDText.text != "")
        {
            #region クリップボードにデッキコードをコピー
            GUIUtility.systemCopyBuffer = RoomIDText.text;
            #endregion

            List<UnityAction> Commands = new List<UnityAction>()
            {
                null
            };

            List<string> CommandTexts = new List<string>()
            {
                "OK"
            };

            Opening.instance.SetUpActiveYesNoObject(
                Commands,
                CommandTexts,
                LocalizeUtility.GetLocalizedString(
                EngMessage: "Room ID copied!",
                JpnMessage: "ルームIDをクリップボードにコピーしました!"
                ),
                false);
        }
    }
    #endregion

    public void Update()
    {
        if (GManager.instance == null)
        {
            if (endSetUp && !DoneStartBattle)
            {
                CheckReadyButton();
            }
        }

        if (gameObject.activeSelf && DCGONetwork.Provider != null)
        {
            _switchFirstPlayerToggleCover.SetActive(!DCGONetwork.Provider.IsRoomHost());
        }
        
    }

    public void OnClickFirstPlayerIndexIDButton(int firstPlayerIndexID)
    {
        DCGONetwork.Provider.SetRoomSetting(DataBase.FirstPlayerKey, firstPlayerIndexID);
    }
}