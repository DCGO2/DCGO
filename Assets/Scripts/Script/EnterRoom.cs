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
    // === DCGO-CUSTOM:tournament begin ===
    [System.NonSerialized]
    public bool JoinTournament;
    // === DCGO-CUSTOM:tournament end ===

    Image _enterRoomButtonImage;

    private void Start()
    {
        _enterRoomButtonImage = EnterRoomButton.GetComponent<Image>();
    }
    // === DCGO-CUSTOM:tournament begin ===
    IEnumerator JoinTournamentLobbyCoroutine(string tourneyId)
    {
        bool preferBanlist = ContinuousController.instance.useBanlist;
        string[] candidates = TournamentKeys.LobbyRoomNameJoinCandidates(tourneyId, preferBanlist);

        for (int i = 0; i < candidates.Length; i++)
        {
            string roomName = candidates[i];
            if (string.IsNullOrEmpty(roomName))
            {
                continue;
            }

            if (!PhotonNetwork.IsConnectedAndReady)
            {
                yield break;
            }

            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
                yield return new WaitUntil(() => PhotonNetwork.InLobby && PhotonNetwork.IsConnectedAndReady);
            }

            _joinAttemptFailed = false;
            Debug.Log($"[Tournament] Join lobby attempt: {roomName}");
            PhotonNetwork.JoinRoom(roomName);

            float t = 0f;
            while (!PhotonNetwork.InRoom && !_joinAttemptFailed && t < 12f)
            {
                t += Time.deltaTime;
                yield return null;
            }

            if (PhotonNetwork.InRoom)
            {
                SyncBanlistFromJoinedRoom();
                yield break;
            }
        }

        if (_expectingJoin)
        {
            _expectingJoin = false;
            ShowRoomNotFoundDialog();
        }
    }

    static void SyncBanlistFromJoinedRoom()
    {
        if (!PhotonNetwork.InRoom)
        {
            return;
        }

        var props = PhotonNetwork.CurrentRoom.CustomProperties;
        if (props != null &&
            props.TryGetValue(TournamentKeys.UseBanlistProperty, out object banObj) &&
            banObj is bool ban)
        {
            ContinuousController.instance.useBanlist = ban;
        }
    }
    // === DCGO-CUSTOM:tournament end ===


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
        // === DCGO-CUSTOM:tournament begin ===
        if (JoinTournament)
        {
            SyncBanlistFromJoinedRoom();
            ContinuousController.instance.isTournament = true;
            TournamentLobbyManager.EnsureExists().SetUpAfterJoin();
            JoinTournament = false;
            Close_(false);
            return;
        }

        ContinuousController.instance.isTournament = false;
        // === DCGO-CUSTOM:tournament end ===
        roomManager.SetUpRoom();
        Close_(false);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"{returnCode} - {message}");
        // === DCGO-CUSTOM:tournament begin ===
        // Tournament join retries other room-name candidates in the coroutine.
        // Casual room match: show dialog when the wait loop notices the failure.
        if (!JoinTournament)
        {
            // Dialog is shown by JoinRoomCoroutine after the wait loop.
        }
        // === DCGO-CUSTOM:tournament end ===
        Opening.instance.PlayDecisionSE();
        Opening.instance.SetUpActiveYesNoObject(
            new List<UnityAction>() { null },
            new List<string>() { "OK" },
            LocalizeUtility.GetLocalizedString(
                // === DCGO-CUSTOM:tournament begin ===
                EngMessage: JoinTournament
                    ? "Error!\nTournament room not found.\nCheck the Room ID (host must still be in the lobby)."
                    : "Error!\nThe room could not be found.",
                JpnMessage: JoinTournament
                    ? "エラー!\nトーナメントルームが見つかりません。\nルームIDを確認してください（ホストがロビーにいる必要があります）。"
                    : "エラー!\nルームが見つかりませんでした"
                // === DCGO-CUSTOM:tournament end ===
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
