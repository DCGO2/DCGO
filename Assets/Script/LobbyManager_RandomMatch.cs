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
public class LobbyManager_RandomMatch : MonoBehaviourPunCallbacks
{
    [Header("message text")]
    public Text MessageText;

    [Header("tiem count text")]
    public Text TimeText;

    [Header("return to title button")]
    public GameObject ReturnButton;

    [Header("deck info panel")]
    public DeckInfoPanel deckInfoPanel;

    public Animator anim;

    public LoadingObject loadingObject;

    public LoadingObject disconnectLoadingObject;

    //Room name for an already existing random match room
    string RandomRoomName;

    bool endLoadingText = false;
    private bool isCoroutineRunning = false;

    //String that must be included in the random match room
    string RandomKey
    {
        get
        {
            return "randomMatchRoom";
        }
    }

    //Whether or not a room is being processed
    bool startJoin = false;

    #region Open Lobby Screen
    public void SetUpLobby()
    {
        Opening.instance.OffYesNoObjects();

        Opening.instance.deck.trialDraw.Close();

        Opening.instance.deck.deckListPanel.Close();

        ContinuousController.instance.isAI = false;
        ContinuousController.instance.isRandomMatch = true;
        this.gameObject.SetActive(true);
        //this.battleRule = battleRule;
        ContinuousController.instance.StartCoroutine(ConnectCoroutine());

        anim.SetInteger("Open", 1);
        anim.SetInteger("Close", 0);
    }
    #endregion

    #region Close Lobby Screen
    public void OffLobby()
    {
        this.gameObject.SetActive(false);
    }
    #endregion

    #region Leave from Random Match Screen
    public void CloseLobby()
    {
        ContinuousController.instance.StartCoroutine(CloseLobbyCoroutine());
    }
    public IEnumerator CloseLobbyCoroutine()
    {
        yield return ContinuousController.instance.StartCoroutine(disconnectLoadingObject.StartLoading("Now Loading"));
        ReturnButton.SetActive(false);

        once1 = true;
        DoneCompleteMatching = true;

        m = true;
        n = true;

        #region Leave From Room
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        yield return new WaitWhile(() => PhotonNetwork.InRoom);
        #endregion

        OffLobby();

        yield return ContinuousController.instance.StartCoroutine(disconnectLoadingObject.EndLoading());
    }
    #endregion

    #region 初期化
    public IEnumerator Init()
    {
        n = false;
        m = false;
        once1 = false;
        endLoadingText = false;
        time = 0;
        RandomRoomName = "";
        startJoin = false;
        DoneCompleteMatching = false;
        ContinuousController.instance.LoadingTextCoroutine = null;
        ReturnButton.SetActive(true);
        MessageText.text = "";
        TimeText.gameObject.SetActive(true);
        StartCoroutine(TimeCountUp());
        timer = 0;
        count = true;

        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        yield return new WaitWhile(() => PhotonNetwork.InLobby);

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }

        yield return new WaitWhile(() => PhotonNetwork.IsConnected);
    }
    #endregion

    #region Time text count up
    int time = 0;

    IEnumerator TimeCountUp()
    {
        time = 0;
        TimeText.gameObject.SetActive(true);

        while (!DoneCompleteMatching)
        {
            time += (int)Time.deltaTime;
            string min = (time / 60).ToString();
            string sec = (time % 60).ToString();

            if (min.Length == 1)
            {
                min = $"0{min}";
            }

            if (sec.Length == 1)
            {
                sec = $"0{sec}";
            }

            TimeText.text = $"{min}:{sec}";
            time++;

            yield return new WaitForSeconds(1f);
        }

        TimeText.gameObject.SetActive(false);
    }
    #endregion

    #region Connect to Photon and Lobby
    IEnumerator ConnectCoroutine()
    {
        //yield return StartCoroutine(loadingObject.StartLoading("Now Loading"));

        if (ContinuousController.instance.BattleDeckData != null)
        {
            deckInfoPanel.SetUpDeckInfoPanel(ContinuousController.instance.BattleDeckData);
        }

        if (ReturnButton != null)
        {
            ReturnButton.SetActive(false);
        }

        endLoadingText = true;

        yield return ContinuousController.instance.StartCoroutine(Init());

        yield return new WaitForSeconds(0.5f);

        endLoadingText = false;

        ContinuousController.instance.LoadingTextCoroutine = ContinuousController.instance.StartCoroutine(SetWaitingText(
            LocalizeUtility.GetLocalizedString(
            EngMessage: "Connecting",
            JpnMessage: "接続中"
        )));

        MessageText.transform.localPosition = new Vector3(-180, -193, 0);

        yield return ContinuousController.instance.StartCoroutine(PhotonUtility.ConnectToLobbyCoroutine());

        yield return ContinuousController.instance.StartCoroutine(PhotonUtility.SignUpBattleDeckData());

        yield return new WaitForSeconds(0.5f);

        StartRandomMatch();

        yield return new WaitWhile(() => !PhotonNetwork.InRoom);

        yield return new WaitForSeconds(0.1f);

        yield return ContinuousController.instance.StartCoroutine(loadingObject.EndLoading());

        ContinuousController.instance.StartCoroutine(TimeCountUp());

        ReturnButton.SetActive(true);
    }
    #endregion

    #region ReturnToTitle()
    public void ReturnToTitle()
    {
        SceneManager.LoadSceneAsync("Opening");

        DoneCompleteMatching = true;
    }
    #endregion

    #region Callback on room list update
    bool m;
    bool n;
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (this.gameObject.activeSelf)
        {
            if (!PhotonNetwork.InRoom && PhotonNetwork.InLobby && !m)
            {
                m = true;
                PhotonNetwork.LeaveLobby();
            }

            if (!PhotonNetwork.InRoom && PhotonNetwork.InLobby && n)
            {
                GetRandomMatcingRoom(roomList);
            }
        }

    }
    #endregion

    #region Search for random matching rooms available
    public void GetRandomMatcingRoom(List<RoomInfo> roomInfo)
    {
        if (startJoin)
        {
            return;
        }

        m = false;
        n = false;

        if (roomInfo == null || roomInfo.Count == 0)
        {
            return;
        }

        RandomRoomName = null;

        for (int i = 0; i < roomInfo.Count; i++)
        {
            int p = roomInfo[i].PlayerCount;
            string n = roomInfo[i].Name;
            int m = roomInfo[i].MaxPlayers;
            object c = roomInfo[i].CustomProperties["RoomCreator"];

            if (p != 0 && m != 0 && c != null)
            {
                if (n.Contains(RandomKey))
                {
                    RandomRoomName = n;
                }
            }
        }
    }
    #endregion

    #region Callback when leaving the lobby
    public override void OnLeftLobby()
    {
        if (this.gameObject.activeSelf)
        {
            if (m)
            {
                n = true;
                PhotonNetwork.JoinLobby();
            }
        }

    }
    #endregion

    #region Process to create a room
    public IEnumerator CreateRoomCoroutine(bool isRandomMatch)
    {
        yield return new WaitWhile(() => !PhotonNetwork.IsConnectedAndReady);
        yield return new WaitWhile(() => !PhotonNetwork.InLobby);

        //Setting up the room to be created
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;   //Make the room visible in the lobby.
        roomOptions.IsOpen = true;      //Allow other players to enter the room
        roomOptions.PublishUserId = true;

        roomOptions.MaxPlayers = 2;

        //To display room creator in room custom properties, store creator's name
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
        {
            { "RoomCreator",PhotonNetwork.NickName },

        };

        //Display custom property information in the lobby
        roomOptions.CustomRoomPropertiesForLobby = new string[]
        {
            "RoomCreator",
        };

        string RoomName = StringUtils.GeneratePassword_AlpahabetNum(8);

        if (isRandomMatch)
        {
            RoomName += RandomKey;
        }

        //Room Creation
        PhotonNetwork.CreateRoom(RoomName, roomOptions, null);

        while (!PhotonNetwork.InRoom)
        {
            yield return null;
        }

        endLoadingText = true;

        yield return new WaitForSeconds(0.2f);

        endLoadingText = false;

        ContinuousController.instance.LoadingTextCoroutine = StartCoroutine(SetWaitingText(
            LocalizeUtility.GetLocalizedString(
            EngMessage: "Matching",
            JpnMessage: "マッチング中"
        )));

        MessageText.transform.localPosition = new Vector3(-148, -193, 0);

        if (ReturnButton != null)
        {
            ReturnButton.SetActive(true);
        }
    }
    #endregion

    #region Random match starts after entering the lobby
    public void StartRandomMatch()
    {
        if (ReturnButton != null)
        {
            ReturnButton.SetActive(true);
        }

        //If there is no room, make room.
        if (String.IsNullOrEmpty(RandomRoomName))
        {
            StartCoroutine(CreateRoomCoroutine(true));
        }

        //If there's room, I'll go in.
        else
        {
            startJoin = true;

            PhotonNetwork.JoinRoom(RandomRoomName);
        }
    }
    #endregion


    private IEnumerator SetWaitingText(string defaultString)
    {
        if (isCoroutineRunning)
        {
            yield break; // Exit if already running
        }

        isCoroutineRunning = true;
        float waitTime = 0.18f;
        int count = 0;

        while (!endLoadingText)
        {
            count++;

            if (count >= 4)
            {
                count = 0;
            }

            MessageText.text = defaultString;

            for (int i = 0; i < count; i++)
            {
                MessageText.text += ".";
            }

            yield return new WaitForSeconds(waitTime);
        }

        isCoroutineRunning = false;
    }

    public void StartLoadingText(string defaultString)
    {
        if (!isCoroutineRunning)
        {
            endLoadingText = false; // Ensure the flag is reset when starting
            StartCoroutine(SetWaitingText(defaultString));
        }
    }

    public void StopLoadingText()
    {
        endLoadingText = true;
    }

    bool count = true;
    float timer = 0;
    Button ReturnButtonButton;
    private void Start()
    {
        ReturnButtonButton = ReturnButton.transform.GetChild(0).GetComponent<Button>();
    }
    private void LateUpdate()
    {
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    StartCoroutine(GoNextScene());
                }

                if (ReturnButtonButton != null)
                {
                    ReturnButtonButton.enabled = false;
                }
            }

            else
            {
                if (ReturnButtonButton != null)
                {
                    ReturnButtonButton.enabled = true;
                }
            }
        }
        else
        {
            ReturnButtonButton.enabled = true;
        }

        if (DoneCompleteMatching)
        {
            if (ReturnButtonButton != null)
            {
                ReturnButton.SetActive(false);
            }
        }
    }


    #region After matching is complete, transition to the battle scene.
    bool DoneCompleteMatching = false;
    bool once1 = false;
    IEnumerator GoNextScene()
    {
        if (DoneCompleteMatching || once1)
        {
            yield break;
        }

        once1 = true;
        yield return new WaitForSeconds(0.1f);

        PhotonNetwork.CurrentRoom.IsVisible = false;

        photonView.RPC("GoToBattleScene", RpcTarget.All);
    }
    #endregion

    [PunRPC]
    public void GoToBattleScene()
    {
        if (DoneCompleteMatching)
        {
            return;
        }

        endLoadingText = true;
        DoneCompleteMatching = true;

        TimeText.gameObject.SetActive(false);

        MessageText.text = LocalizeUtility.GetLocalizedString(
            EngMessage: "Matching completed!",
            JpnMessage: "マッチングしました!"
        );

        MessageText.transform.localPosition = new Vector3(-390, -193, 0);
        ReturnButton.SetActive(false);

        ContinuousController.instance.StartCoroutine(GoToBattleSceneCoroutine());
    }

    IEnumerator GoToBattleSceneCoroutine()
    {
        ContinuousController.instance.StartCoroutine(Opening.instance.OpeningBGM.FadeOut(0.2f));

        Debug.Log("Matching complted!");

        yield return new WaitForSeconds(0.1f);

        //Opening.instance.MainCamera.gameObject.SetActive(false);

        foreach (Camera camera in Opening.instance.openingCameras)
        {
            camera.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadSceneAsync("BattleScene", LoadSceneMode.Additive);
        yield return null;
    }
}

#region Generation of random strings
public static class StringUtils
{
    public static string GeneratePassword_AlpahabetNum(int length)
    {
        string PASSWORD_CHARS =
        "0123456789abcdefghijklmnopqrstuvwxyz";

        var sb = new System.Text.StringBuilder(length);
        var r = new System.Random();

        for (int i = 0; i < length; i++)
        {
            int pos = r.Next(PASSWORD_CHARS.Length);
            char c = PASSWORD_CHARS[pos];
            sb.Append(c);
        }

        return sb.ToString();
    }

    public static string GeneratePassword_Num(int length)
    {
        string PASSWORD_CHARS =
        "0123456789";

        var sb = new System.Text.StringBuilder(length);
        var r = new System.Random();

        for (int i = 0; i < length; i++)
        {
            int pos = r.Next(PASSWORD_CHARS.Length);
            char c = PASSWORD_CHARS[pos];
            sb.Append(c);
        }

        return sb.ToString();
    }
}
#endregion