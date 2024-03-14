using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;
using System;

public class LobbyManager_FriendMatch : MonoBehaviourPunCallbacks
{
    public GameObject RoomParent;//ScrolView content object
    public GameObject RoomElementPrefab;//Room Information Prefab

    public GameObject CreateRoomButton;
    public Text MessageText;

    public static LobbyManager_FriendMatch instance = null;

    string RandomKey = "ランダムマッチ部屋";

    public OptionPanel optionPanel;

    public void ReturnToTitle()
    {
        SceneManager.LoadSceneAsync("Opening");
    }

    private void Awake()
    {
        MessageText.text = "Connecting...";

        instance = this;

        optionPanel.Init();
    }

    private void Update()
    {
        if (PrivateLobby.activeSelf)
        {
            if (PhotonNetwork.InLobby)
            {
                CreateRoomButton.SetActive(true);
                ReturnButton.SetActive(true);
            }

            else
            {
                CreateRoomButton.SetActive(false);
                ReturnButton.SetActive(false);
            }
        }
    }

    #region GetRooms
    public void GetRooms(List<RoomInfo> roomInfo)
    {
        if (startJoin)
        {
            return;
        }

        m = false;
        n = false;
        if (roomInfo == null || roomInfo.Count == 0)
        {
            MessageText.text = "There is no room";

            return;
        }

        int roomCount = 0;

        for (int i = 0; i < roomInfo.Count; i++)
        {
            int p = roomInfo[i].PlayerCount;
            string n = roomInfo[i].Name;
            int m = roomInfo[i].MaxPlayers;
            object c = roomInfo[i].CustomProperties["RoomCreator"];

            if (p != 0 && m != 0 && c != null)
            {
                if (!n.Contains(RandomKey))
                {
                    roomCount++;
                    GameObject RoomElement = Instantiate(RoomElementPrefab, RoomParent.transform);
                    RoomElement.GetComponent<CRoomElement>().SetRoomInfo(roomInfo[i].Name, roomInfo[i].PlayerCount, roomInfo[i].CustomProperties["RoomCreator"].ToString());

                    RoomElement.GetComponent<CRoomElement>().OnClick = EnterRoom;

                    void EnterRoom()
                    {
                        if (!PhotonNetwork.InLobby)
                        {
                            return;
                        }

                        PrivateRoom.SetActive(true);
                        PrivateLobby.SetActive(false);


                        StartCoroutine(wait());

                        //Enter the room with roomname
                        PhotonNetwork.JoinRoom(RoomElement.GetComponent<CRoomElement>().RoomName);

                        MessageText.text = "When the two are ready, it begins.";
                    }

                    IEnumerator wait()
                    {
                        ReturnButton.SetActive(false);

                        yield return new WaitWhile(() => !PhotonNetwork.InRoom);

                        ReturnButton.SetActive(true);
                    }
                }
            }
        }

        if (roomCount > 0)
        {
            MessageText.text = "There are " + roomCount + " rooms.";
        }

    }

    //Batch deletion of RoomElement
    public static void DestroyChildObject(Transform parent_trans)
    {
        for (int i = 0; i < parent_trans.childCount; ++i)
        {

            Destroy(parent_trans.GetChild(i).gameObject);
        }
    }
    #endregion

    bool m;
    bool n;
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (!PhotonNetwork.InRoom && PhotonNetwork.InLobby && !m)
        {
            m = true;
            PhotonNetwork.LeaveLobby();
        }

        if (!PhotonNetwork.InRoom && PhotonNetwork.InLobby && n)
        {
            DestroyChildObject(RoomParent.transform);   //Delete RoomElement

            GetRooms(roomList);
        }

    }

    public override void OnLeftLobby()
    {
        if (m)
        {
            n = true;
            PhotonNetwork.JoinLobby();
        }

    }

    // Callback called when connection to master server succeeds
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (isWaitingRandom)
        {
            startJoin = false;

            PrivateLobby.SetActive(false);
            PrivateRoom.SetActive(false);
        }
    }

    public IEnumerator CreateRoomCoroutine()
    {
        yield return new WaitWhile(() => !PhotonNetwork.InLobby);

        //Setting up the room to be created
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;   //Make the room visible in the lobby
        roomOptions.IsOpen = true;      //Allow other players to enter the room
        roomOptions.PublishUserId = true;

        roomOptions.MaxPlayers = 2;

        //To display room creator in room custom properties, store creator's name
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
        {
            { "RoomCreator",ContinuousController.instance.PlayerName },

        };

        //Display custom property information in the lobby
        roomOptions.CustomRoomPropertiesForLobby = new string[]
        {
            "RoomCreator",
        };

        string RoomName = StringUtils.GeneratePassword(8);

        //Create Room
        PhotonNetwork.CreateRoom(RoomName, roomOptions, null);
    }

    bool OnceCreatedPrivateRoom = false;

    public void OnClick_CreateRoomButton()
    {
        if (OnceCreatedPrivateRoom)
        {
            return;
        }

        OnceCreatedPrivateRoom = true;

        StartCoroutine(CreateRoomCoroutine());

        StartCoroutine(Wait_CreatePrivateRoom());
    }

    IEnumerator Wait_CreatePrivateRoom()
    {
        PrivateLobby.SetActive(false);
        ReturnButton.SetActive(false);

        MessageText.text = "Creating Room...";

        yield return new WaitWhile(() => !PhotonNetwork.InRoom);


        PrivateRoom.SetActive(true);
        ReturnButton.SetActive(true);

        MessageText.text = "When the two are ready, it begins.";

        OnceCreatedPrivateRoom = false;
    }

    public static class StringUtils
    {
        private const string PASSWORD_CHARS =
            "0123456789abcdefghijklmnopqrstuvwxyz";

        public static string GeneratePassword(int length)
        {
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

    public GameObject PrivateLobby;
    public GameObject PrivateRoom;

    bool isWaitingRandom = false;
    bool startJoin = false;

    public GameObject ReturnButton;

    private void Start()
    {
        PrivateRoom.SetActive(false);
        PrivateLobby.SetActive(false);

        StartCoroutine(ConnectCoroutine());
    }

    IEnumerator ConnectCoroutine()
    {
        if (!PhotonNetwork.IsConnected)
        {
            yield return ContinuousController.instance.StartCoroutine(PhotonUtility.ConnectToMasterServerCoroutine());
        }

        MessageText.text = "Connecting...";
        yield return new WaitWhile(() => !PhotonNetwork.IsConnected);

        #region Save deck data to custom properties
        Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;

        if (hash.TryGetValue(ContinuousController.DeckDataPropertyKey, out object value))
        {
            hash[ContinuousController.DeckDataPropertyKey] = ContinuousController.instance.BattleDeckData.GetThisDeckCode();
        }

        else
        {
            hash.Add(ContinuousController.DeckDataPropertyKey, ContinuousController.instance.BattleDeckData.GetThisDeckCode());
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        while (true)
        {
            Hashtable _hash = PhotonNetwork.LocalPlayer.CustomProperties;

            if (_hash.TryGetValue(ContinuousController.DeckDataPropertyKey, out value))
            {
                if ((string)value == ContinuousController.instance.BattleDeckData.GetThisDeckCode())
                {
                    break;
                }
            }

            yield return null;
        }
        #endregion

        #region Save player name to custom properties
        hash = PhotonNetwork.LocalPlayer.CustomProperties;

        if (hash.TryGetValue(ContinuousController.PlayerNameKey, out value))
        {
            hash[ContinuousController.PlayerNameKey] = ContinuousController.instance.PlayerName;
        }

        else
        {
            hash.Add(ContinuousController.PlayerNameKey, ContinuousController.instance.PlayerName);
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        while (true)
        {
            Hashtable _hash = PhotonNetwork.LocalPlayer.CustomProperties;

            if (_hash.TryGetValue(ContinuousController.PlayerNameKey, out value))
            {
                if ((string)value == ContinuousController.instance.PlayerName)
                {
                    break;
                }
            }

            yield return null;
        }
        #endregion

        #region Save the number of wins to a custom property
        hash = PhotonNetwork.LocalPlayer.CustomProperties;

        if (hash.TryGetValue(ContinuousController.WinCountKey, out value))
        {
            hash[ContinuousController.WinCountKey] = ContinuousController.instance.WinCount;
        }

        else
        {
            hash.Add(ContinuousController.WinCountKey, ContinuousController.instance.WinCount);
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        while (true)
        {
            Hashtable _hash = PhotonNetwork.LocalPlayer.CustomProperties;

            if (_hash.TryGetValue(ContinuousController.WinCountKey, out value))
            {
                if ((int)value == ContinuousController.instance.WinCount)
                {
                    break;
                }

            }

            yield return null;
        }
        #endregion

        yield return new WaitWhile(() => !PhotonNetwork.InLobby);
        MessageText.text = "";
        PrivateRoom.SetActive(false);
        PrivateLobby.SetActive(true);
    }
}