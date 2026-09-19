using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.Events;
public class EnterRoom : MonoBehaviourPunCallbacks
{
    private static readonly int OpenHash = Animator.StringToHash("Open");
    private static readonly int CloseHash = Animator.StringToHash("Close");
    [Header("Room ID InputField")]
    public InputField RoomIDInputField;

    [Header("Animator")]
    public Animator anim;

    [Header("Room screen")]
    public RoomManager roomManager;

    [Header("Enter Room Button")]
    public Button EnterRoomButton;

    Image _enterRoomButtonImage;

    private void Start()
    {
        _enterRoomButtonImage = EnterRoomButton.GetComponent<Image>();
    }


    public void SetUpEnterRoom()
    {
        if (this.gameObject.activeSelf)
        {
            return;
        }

        RoomIDInputField.text = "";

        this.gameObject.SetActive(true);

        anim.SafeSetInt(OpenHash, 1);
        anim.SafeSetInt(CloseHash, 0);
    }

    public void Close_(bool playSE)
    {
        if (playSE)
        {
            Opening.instance.PlayCancelSE();
        }

        anim.SafeSetInt(OpenHash, 0);
        anim.SafeSetInt(CloseHash, 1);
    }

    public void Off()
    {
        this.gameObject.SetActive(false);
    }

    bool _canClick = true;

    public void OnClickEnterRoomButton()
    {
        if (CanClickEnterRoomButton() && _canClick)
        {
            ContinuousController.instance.StartCoroutine(JoinRoomCoroutine());
        }
    }

    IEnumerator JoinRoomCoroutine()
    {
        _canClick = false;

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

        PhotonNetwork.JoinRoom(RoomIDInputField.text + "-" + ContinuousController.instance.useBanlist);

        _canClick = true;
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
        Debug.Log($"{returnCode} - {message}");
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

            if (_enterRoomButtonImage != null)
            {
                _enterRoomButtonImage.color = Color.white;
            }
        }

        else
        {
            EnterRoomButton.enabled = false;

            if (_enterRoomButtonImage != null)
            {
                _enterRoomButtonImage.color = new Color32(144, 144, 144, 255);
            }
        }
    }
}
