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

public class RoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] bool _isReady = false;
    int playerCount { get; set; }

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

    void Awake()
    {
        foreach (FirstPlayerIndexIdToggle firstPlayerIndexIdToggle in _firstPlayerIndexIdToggles)
        {
            firstPlayerIndexIdToggle.OnClickAction = OnClickFirstPlayerIndexIDButton;
        }
    }

    #region Open Room Screen

    #region Open Room Screen
    public void SetUpRoom()
    {
        ContinuousController.instance.StartCoroutine(SetUpRoomCoroutine());
    }

    public IEnumerator SetUpRoomCoroutine()
    {
        Opening.instance.OffYesNoObjects();

        Opening.instance.deck.trialDraw.Close();

        Opening.instance.deck.deckListPanel.Close();

        ContinuousController.instance.isAI = false;
        ContinuousController.instance.isRandomMatch = false;

        yield return ContinuousController.instance.StartCoroutine(loadingObject.StartLoading("Now Loading"));

        endSetUp = false;

        Parent.SetActive(true);

        if (!PhotonNetwork.InRoom)
        {
            yield return ContinuousController.instance.StartCoroutine(CreateRoomCoroutine());
        }

        yield return ContinuousController.instance.StartCoroutine(ShowRoomInfo());

        yield return ContinuousController.instance.StartCoroutine(Init(false));

        SetDeckInfoPanel();
        CheckReadyButton();

        yield return ContinuousController.instance.StartCoroutine(loadingObject.EndLoading());

        endSetUp = true;
    }
    #endregion

    #region Process to create a room
    bool CreateRoomFailed = false;
    public IEnumerator CreateRoomCoroutine()
    {
        CreateRoomFailed = false;

        yield return new WaitWhile(() => !PhotonNetwork.IsConnectedAndReady);

        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        yield return new WaitWhile(() => !PhotonNetwork.InLobby);

        while (true)
        {
            //Setting up the room to be created
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = true;   //Make the room visible in the lobby
            roomOptions.IsOpen = true;      //Allow other players to enter the room
            roomOptions.PublishUserId = true;

            roomOptions.MaxPlayers = 2;

            //Stores the creator's name to display the room creator in the room's custom properties
            roomOptions.CustomRoomProperties = new Hashtable()
            {
                { "RoomCreator",PhotonNetwork.NickName },

            };

            //Display custom property information in the lobby
            roomOptions.CustomRoomPropertiesForLobby = new string[]
            {
                "RoomCreator",
            };

            string RoomName = StringUtils.GeneratePassword_Num(5);

            //Create Room
            PhotonNetwork.CreateRoom(RoomName, roomOptions, null);

            while (true)
            {
                if (CreateRoomFailed || PhotonNetwork.InRoom)
                {
                    break;
                }

                yield return null;
            }

            //Room Creation Success
            if (!CreateRoomFailed && PhotonNetwork.InRoom)
            {
                yield break;
            }
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        CreateRoomFailed = true;
    }
    #endregion

    #region Initialized when room screen is opened
    public IEnumerator Init(bool OnUnload)
    {
        Opening.instance.battle.selectBattleDeck.Off();
        DoneStartBattle = false;
        _isReady = false;

        playerCount = 0;//PhotonNetwork.CurrentRoom.PlayerCount;
        DestroyChildObject();//Delete PlayerElement
        GetPlayers();

        if (!OnUnload)
        {
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
            yield return ContinuousController.instance.StartCoroutine(PhotonUtility.SignUpBattleDeckData());
            //yield return ContinuousController.instance.StartCoroutine(SignUpBattleDeck(FirstValidDeckData()));
            SetReadyProperty(PhotonNetwork.LocalPlayer, _isReady);
        }

        else
        {
            yield return ContinuousController.instance.StartCoroutine(PhotonUtility.SignUpBattleDeckData());
        }
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

    #endregion

    #region Room information is acquired and reflected in the UI
    public IEnumerator ShowRoomInfo()
    {
        yield return new WaitWhile(() => !PhotonNetwork.IsConnectedAndReady);
        yield return new WaitWhile(() => !PhotonNetwork.InRoom);

        #region RoomName
        string RoomName = PhotonNetwork.CurrentRoom.Name;
        RoomIDText.text = RoomName;
        #endregion
    }
    #endregion

    #endregion

    #region Register Battle Deck
    public IEnumerator SignUpBattleDeck(DeckData deckData)
    {
        ContinuousController.instance.BattleDeckData = deckData;

        bool IsSignUpDeckData()
        {
            if (deckData != null)
            {
                if (ContinuousController.instance.DeckDatas != null)
                {
                    if (ContinuousController.instance.DeckDatas.Contains(deckData))
                    {
                        if (deckData.IsValidDeckData())
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        if (IsSignUpDeckData())
        {
            #region Save deck data to custom properties
            yield return ContinuousController.instance.StartCoroutine(PhotonUtility.SignUpBattleDeckData());
            #endregion
        }

        else
        {
            #region Remove custom properties from deck data
            yield return ContinuousController.instance.StartCoroutine(PhotonUtility.DeleteBattleDeckData());
            #endregion
        }
    }
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

        SetReadyProperty(PhotonNetwork.LocalPlayer, _isReady);

        #region Leave Room
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        yield return new WaitWhile(() => PhotonNetwork.InRoom);
        #endregion

        if (!Opening.instance.battle.selectBattleMode.selectBattleModeWindow.gameObject.activeSelf)
        {
            Opening.instance.battle.selectBattleMode.SetUpSelectBattleMode();

            Opening.instance.battle.selectBattleMode.StartSelectRoomMatch();
        }

        yield return new WaitForSeconds(0.1f);

        yield return ContinuousController.instance.StartCoroutine(disconnectLoadingObject.EndLoading());
    }

    public void Off()
    {
        Parent.SetActive(false);
    }
    #endregion

    #region Key of the property indicating readiness
    public static string ReadyKey()
    {
        if (PhotonNetwork.InRoom)
        {
            return "IsReady" + PhotonNetwork.CurrentRoom.Name;
        }

        return "IsReady";// + PhotonNetwork.CurrentRoom.Name;
    }
    #endregion

    #region Processing when the Ready button is pressed
    public void OnClickGetReadyButton()
    {
        _isReady = !_isReady;

        SetReadyProperty(PhotonNetwork.LocalPlayer, _isReady);

        CheckReadyButton();

        photonView.RPC("DestroyChildObject", RpcTarget.All);
        photonView.RPC("GetPlayers", RpcTarget.All);

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

    #region Set properties to indicate readiness
    void SetReadyProperty(Photon.Realtime.Player player, bool _isReady)
    {
        Hashtable PlayerProp = player.CustomProperties;

        object value;

        if (PlayerProp.TryGetValue(ReadyKey(), out value))
        {
            PlayerProp[ReadyKey()] = _isReady;
        }

        else
        {
            PlayerProp.Add(ReadyKey(), _isReady);
        }

        player.SetCustomProperties(PlayerProp);
    }
    #endregion

    #region All players in the room are determined to be ready.
    public bool AllPlayerIsReady()
    {
        foreach (var p in PhotonNetwork.PlayerList)
        {
            object value;

            if (p.CustomProperties.TryGetValue(ReadyKey(), out value))
            {
                if (!(bool)value)
                {
                    return false;
                }
            }

            else
            {
                return false;
            }
        }

        return true;
    }
    #endregion

    #region Monitor the number of players and standby status → If OK, start battle
    bool DoneStartBattle;
    public void CheckPlayerState()
    {
        if (PhotonNetwork.InRoom && endSetUp)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers && AllPlayerIsReady())
            {
                if (PhotonNetwork.IsMasterClient && !DoneStartBattle)
                {
                    DoneStartBattle = true;

                    if (PhotonNetwork.IsMasterClient)
                    {
                        Hashtable RoomProp = PhotonNetwork.CurrentRoom.CustomProperties;

                        // 0: MasterClient, -1: Random, 1: non-MasterClient
                        int firstPlayerIndexID = -1;
                        int firstPlayerId = -1;

                        if (RoomProp.TryGetValue(DataBase.FirstPlayerIndexIdKey, out object value))
                        {
                            if (value is int)
                            {
                                firstPlayerIndexID = (int)value;
                            }
                        }

                        if (firstPlayerIndexID == 0)
                        {
                            firstPlayerId = PhotonNetwork.MasterClient.ActorNumber;
                        }

                        else if (firstPlayerIndexID == 1)
                        {
                            Photon.Realtime.Player nonMasterPlayer = PhotonNetwork.PlayerList.ToList().Find(player => !player.IsMasterClient);

                            if (nonMasterPlayer != null)
                            {
                                firstPlayerId = nonMasterPlayer.ActorNumber;
                            }
                        }

                        if (RoomProp.TryGetValue(DataBase.FirstPlayerKey, out value))
                        {
                            RoomProp[DataBase.FirstPlayerKey] = firstPlayerId;
                        }

                        else
                        {
                            RoomProp.Add(DataBase.FirstPlayerKey, firstPlayerId);
                        }

                        PhotonNetwork.CurrentRoom.SetCustomProperties(RoomProp);
                    }

                    photonView.RPC("GoToBattleScene", RpcTarget.All);
                }

            }

            else if (playerCount != PhotonNetwork.CurrentRoom.PlayerCount)
            {
                playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
                DestroyChildObject();//PlayerElementを削除
                GetPlayers();
            }
        }
    }
    #endregion

    #region Start of battle
    [PunRPC]
    public void GoToBattleScene()
    {
        StartCoroutine(GoToBattleSceneCoroutine());
    }

    IEnumerator GoToBattleSceneCoroutine()
    {
        yield return ContinuousController.instance.StartCoroutine(Opening.instance.LoadingObject_Unload.StartLoading("Now Loading"));
        object value;

        DoneStartBattle = true;

        Hashtable PlayerProp = PhotonNetwork.LocalPlayer.CustomProperties;

        List<DictionaryEntry> dictionaryEntries = new List<DictionaryEntry>();

        foreach (DictionaryEntry dictionaryEntry in PlayerProp)
        {
            dictionaryEntries.Add(dictionaryEntry);
        }

        foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
        {
            if (dictionaryEntry.Key.ToString().Contains("PhotonWaitController"))
            {
                if (PlayerProp.TryGetValue(dictionaryEntry.Key.ToString(), out value))
                {
                    PlayerProp.Remove(dictionaryEntry.Key.ToString());
                }
            }
        }

        if (PlayerProp.TryGetValue("isBattle", out value))
        {
            PlayerProp["isBattle"] = true;
        }

        else
        {
            PlayerProp.Add("isBattle", true);
        }

        if (PlayerProp.TryGetValue(ReadyKey(), out value))
        {
            PlayerProp[ReadyKey()] = false;
        }

        else
        {
            PlayerProp.Add(ReadyKey(), false);
        }

        PlayerProp[ContinuousController.DeckDataPropertyKey] = ContinuousController.instance.BattleDeckData.GetThisDeckCode();

        PhotonNetwork.LocalPlayer.SetCustomProperties(PlayerProp);
        yield return new WaitForSeconds(0.1f);

        //Opening.instance.MainCamera.gameObject.SetActive(false);

        foreach (Camera camera in Opening.instance.openingCameras)
        {
            camera.gameObject.SetActive(false);
        }

        ContinuousController.instance.StartCoroutine(Opening.instance.OpeningBGM.FadeOut(0.5f));

        yield return new WaitForSeconds(1f);

        SceneManager.LoadSceneAsync("BattleScene", LoadSceneMode.Additive);
        yield return ContinuousController.instance.StartCoroutine(Opening.instance.LoadingObject_Unload.EndLoading());
    }
    #endregion

    #region Create PlayerElement corresponding to the information of players in the room
    [PunRPC]
    public void GetPlayers()
    {
        if (PhotonNetwork.PlayerList == null || PhotonNetwork.CurrentRoom.PlayerCount == 0)
        {
            return;
        }

        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            GameObject PlayerElement = Instantiate(PlayerElementPrefab, PlayerParent.transform);

            object value;

            string PlayerName = "Player";

            #region Get player name from custom property
            if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer)
            {
                PlayerName = "You";
            }

            else
            {
                PlayerName = "Opponent";
            }

            Hashtable hash = PhotonNetwork.PlayerList[i].CustomProperties;

            if (hash.TryGetValue(ContinuousController.PlayerNameKey, out value))
            {
                PlayerName = (string)value;
            }
            #endregion

            if (PhotonNetwork.PlayerList[i].CustomProperties.TryGetValue(ReadyKey(), out value))
            {
                PlayerElement.GetComponent<CPlayerElement>().SetPlayerInfo(PlayerName, (bool)value);
            }

            else
            {
                PlayerElement.GetComponent<CPlayerElement>().SetPlayerInfo(PlayerName, false);
            }
        }
    }
    #endregion

    #region Delete PlayerElement displayed
    [PunRPC]
    public void DestroyChildObject()
    {
        for (int i = 0; i < PlayerParent.transform.childCount; ++i)
        {
            Destroy(PlayerParent.transform.GetChild(i).gameObject);
        }
    }
    #endregion

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
            SetReadyProperty(PhotonNetwork.LocalPlayer, _isReady);

            if (oldIsReady)
            {
                photonView.RPC("DestroyChildObject", RpcTarget.All);
                photonView.RPC("GetPlayers", RpcTarget.All);
            }
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

        SetReadyProperty(PhotonNetwork.LocalPlayer, _isReady);

        photonView.RPC("DestroyChildObject", RpcTarget.All);
        photonView.RPC("GetPlayers", RpcTarget.All);
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

            yield return ContinuousController.instance.StartCoroutine(PhotonUtility.SignUpBattleDeckData());

            yield return new WaitForSeconds(Time.deltaTime);

            SetDeckInfoPanel();

            _isReady = _oldIsReady;

            if (!CanReady())
            {
                _isReady = false;
            }

            CheckReadyButton();

            SetReadyProperty(PhotonNetwork.LocalPlayer, _isReady);

            #region プレイヤーの準備完了状態がプロパティにセットされるまで待機
            while (true)
            {
                Hashtable _hash = PhotonNetwork.LocalPlayer.CustomProperties;

                if (_hash.TryGetValue(ReadyKey(), out object value))
                {
                    if ((bool)value == _isReady)
                    {
                        break;
                    }
                }

                yield return null;
            }
            #endregion

            photonView.RPC("DestroyChildObject", RpcTarget.All);
            photonView.RPC("GetPlayers", RpcTarget.All);
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
        if (PhotonNetwork.InRoom)
        {
            #region クリップボードにデッキコードをコピー
            GUIUtility.systemCopyBuffer = PhotonNetwork.CurrentRoom.Name;
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
                CheckPlayerState();
            }
        }

        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.LocalPlayer != null)
                {
                    _switchFirstPlayerToggleCover.SetActive(!PhotonNetwork.LocalPlayer.IsMasterClient);

                    Hashtable RoomProp = PhotonNetwork.CurrentRoom.CustomProperties;

                    // 0: MasterClient, -1: Random, 1: non-MasterClient
                    int firstPlayerIndexID = -1;

                    if (RoomProp.TryGetValue(DataBase.FirstPlayerIndexIdKey, out object value))
                    {
                        if (value is int)
                        {
                            firstPlayerIndexID = (int)value;
                        }
                    }

                    foreach (FirstPlayerIndexIdToggle firstPlayerIndexIdToggle in _firstPlayerIndexIdToggles)
                    {
                        firstPlayerIndexIdToggle.Toggle.isOn = firstPlayerIndexIdToggle.FirstPlayerIndexID == firstPlayerIndexID;
                    }
                }
            }
        }
    }

    public void OnClickFirstPlayerIndexIDButton(int firstPlayerIndexID)
    {
        if (!PhotonNetwork.IsConnectedAndReady) return;
        if (!PhotonNetwork.InRoom) return;
        if (PhotonNetwork.LocalPlayer == null) return;
        if (!PhotonNetwork.LocalPlayer.IsMasterClient) return;

        Hashtable RoomProp = PhotonNetwork.CurrentRoom.CustomProperties;

        if (RoomProp.TryGetValue(DataBase.FirstPlayerIndexIdKey, out object value))
        {
            RoomProp[DataBase.FirstPlayerIndexIdKey] = firstPlayerIndexID;
        }

        else
        {
            RoomProp.Add(DataBase.FirstPlayerIndexIdKey, firstPlayerIndexID);
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(RoomProp);
    }
}