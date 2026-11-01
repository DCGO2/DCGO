using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DCGO.Networking;
public class EnterRoom : MonoBehaviour
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
            _canClick = false;

            string roomCode = RoomIDInputField.text + "-" + ContinuousController.instance.useBanlist;
            DCGONetwork.Provider.JoinPrivateRoom(roomCode , (bool success) =>
            {
                _canClick = true;
                if (success)
                {
                    OnJoinedRoom();
                }
                else
                {
                    OnJoinRoomFailed();
                }
            });
        }
    }

    public void OnJoinedRoom()
    {
        ContinuousController.instance.isAI = false;
        ContinuousController.instance.isRandomMatch = false;
        roomManager.SetUpRoom(false);
        Close_(false);
    }

    public void OnJoinRoomFailed()
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
