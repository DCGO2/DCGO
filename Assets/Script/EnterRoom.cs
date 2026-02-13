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
public class EnterRoom : MonoBehaviourPunCallbacks
{
    [Header("Room ID InputField")]
    public InputField RoomIDInputField;

    [Header("Animator")]
    public Animator anim;

    [Header("Room screen")]
    public RoomManager roomManager;

    [Header("Enter Room Button")]
    public Button EnterRoomButton;

    Image EnterRoomButtonImage;

    private void Start()
    {
        EnterRoomButtonImage = EnterRoomButton.GetComponent<Image>();
    }


    public void SetUpEnterRoom()
    {
        if (this.gameObject.activeSelf)
        {
            return;
        }

        RoomIDInputField.text = "";

        this.gameObject.SetActive(true);

        anim.SetInteger("Open", 1);
        anim.SetInteger("Close", 0);
    }

    public void Close_(bool playSE)
    {
        if (playSE)
        {
            Opening.instance.PlayCancelSE();
        }

        anim.SetInteger("Open", 0);
        anim.SetInteger("Close", 1);
    }

    public void Off()
    {
        this.gameObject.SetActive(false);
    }

    bool canClick = true;

    public void OnClickEnterRoomButton()
    {
        if (CanClickEnterRoomButton() && canClick)
        {
            PhotonNetwork.JoinRoom(RoomIDInputField.text);
        }
    }

    IEnumerator JoinRoomCoroutine()
    {
        canClick = false;

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

        PhotonNetwork.JoinRoom(RoomIDInputField.text);

        canClick = true;
    }

    public override void OnJoinedRoom()
    {
        ContinuousController.instance.isAI = false;
        ContinuousController.instance.isRandomMatch = false;
        roomManager.SetUpRoom();
        Close_(false);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Opening.instance.PlayDecisionSE();
        Opening.instance.SetUpActiveYesNoObject(
            new List<UnityAction>() { null },
            new List<string>() { "OK" },
            LocalizeUtility.GetLocalizedString(
            EngMessage: "Error!\nThe room could not be found.",
            JpnMessage: "エラー!\nルームが見つかりませんでした"
            ),
            true);
    }

    bool CanClickEnterRoomButton()
    {
        if (!string.IsNullOrEmpty(RoomIDInputField.text))
        {
            if (RoomIDInputField.text.Length == 5)
            {
                return true;
            }
        }

        return false;
    }

    private void Update()
    {
        if (CanClickEnterRoomButton())
        {
            EnterRoomButton.enabled = true;

            if (EnterRoomButtonImage != null)
            {
                EnterRoomButtonImage.color = Color.white;
            }
        }

        else
        {
            EnterRoomButton.enabled = false;

            if (EnterRoomButtonImage != null)
            {
                EnterRoomButtonImage.color = new Color32(144, 144, 144, 255);
            }
        }
    }
}
