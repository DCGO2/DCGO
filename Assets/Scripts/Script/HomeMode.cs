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
public class HomeMode : MonoBehaviour
{
    [Header("playerInfo")]
    public PlayerInfo playerInfo;

    [Header("loadingObject")]
    public LoadingObject loadingObject;

    public GameObject UpdateButtonParent;

    bool first = false;
    public void OffHome()
    {
        if(!first)
        {
            first = true;
        }
        
        playerInfo.OffPlayerInfo();

        Opening.instance.OffModeButtons();

        if(Opening.instance.checkUpdate.UpdateButton != null)
        {
            Opening.instance.checkUpdate.UpdateButton.SetActive(false);
        }
        
        if(UpdateButtonParent != null)
        {
            UpdateButtonParent.SetActive(false);
        }

        // === DCGO-CUSTOM:friends begin ===
        FriendListPanel.HideIfOpen();
        FriendServices.Instance?.Duel?.SetInviteListening(false);
        // === DCGO-CUSTOM:recovery begin ===
        AccountRecoveryPanel.HideIfOpen();
        // === DCGO-CUSTOM:recovery end ===
        // === DCGO-CUSTOM:friends end ===
    }

    public void SetUpHome()
    {
        playerInfo.SetPlayerInfo();

        Opening.instance.OnModeButtons();

        if (Opening.instance.OpeningBGM != null)
        {
            if (!Opening.instance.OpeningBGM.isPlaying)
            {
                Opening.instance.OpeningBGM.StartPlayBGM(Opening.instance.bgm);
            }
        }

        Opening.instance.OffYesNoObjects();

        Opening.instance.deck.trialDraw.Close();

        Opening.instance.deck.deckListPanel.Close();

        for (int i = 0; i < Opening.instance.deck.selectDeck.deckInfoPrefabParentScroll.content.childCount; i++)
        {    
            if (Opening.instance.deck.selectDeck.deckInfoPrefabParentScroll.content.GetChild(i).TryGetComponent<CreateNewDeckButton>(out var createNewDeckButton))
            {
                createNewDeckButton.CreateNewDeckWayObject.Off();
                break;
            }
        }

        Opening.instance.optionPanel.CloseOptionPanel();
        // === DCGO-CUSTOM:onlinecount begin ===
        if (Opening.instance != null)
        {
            Opening.instance.EnsureOnlinePlayerCountUi();
        }
        OnlinePlayerCountService.EnsureExists().SetMenuPresenceEnabled(true);
        // === DCGO-CUSTOM:onlinecount end ===
        // === DCGO-CUSTOM:friends begin ===
        Opening.instance?.EnsureFriendsButton();
        ContinuousController.instance?.StartCoroutine(BootstrapFriendsHomeCoroutine());
        // === DCGO-CUSTOM:recovery begin ===
        Opening.instance?.EnsureAccountButton();
        // === DCGO-CUSTOM:recovery end ===
        // === DCGO-CUSTOM:friends end ===
    }

    // === DCGO-CUSTOM:friends begin ===
    IEnumerator BootstrapFriendsHomeCoroutine()
    {
        var friends = FriendServices.EnsureExists();
        yield return friends.List.EnsureLoggedIn();

        var ranked = RankedServices.EnsureExists();
        string expectedId = ranked.Auth.PlayFabId;
        ranked.Auth.ApplyPhotonAuthValues();

        if (Photon.Pun.PhotonNetwork.IsConnectedAndReady &&
            !string.IsNullOrEmpty(expectedId) &&
            Photon.Pun.PhotonNetwork.LocalPlayer != null &&
            Photon.Pun.PhotonNetwork.LocalPlayer.UserId != expectedId)
        {
            OnlinePlayerCountService.EnsureExists().SetMenuPresenceEnabled(false);
            yield return ContinuousController.instance.StartCoroutine(PhotonUtility.DisconnectCoroutine());
            OnlinePlayerCountService.EnsureExists().SetMenuPresenceEnabled(true);
            float wait = 0f;
            while ((!Photon.Pun.PhotonNetwork.IsConnectedAndReady || !Photon.Pun.PhotonNetwork.InLobby) && wait < 20f)
            {
                wait += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        if (Photon.Pun.PhotonNetwork.IsConnectedAndReady)
        {
            yield return ContinuousController.instance.StartCoroutine(
                PhotonUtility.SetRankedPlayerProperties());
        }

        if (PhotonNetwork.InRoom ||
            (ContinuousController.instance != null && ContinuousController.instance.isFriendDuel))
        {
            yield break;
        }

        friends.Duel.SetInviteListening(true);
    }
    // === DCGO-CUSTOM:friends end ===

    public void SetUpHomeMode_Disconnect()
    {
        StartCoroutine(SetUpHomeMode_DisconnectCoroutine());
    }

    public IEnumerator SetUpHomeMode_DisconnectCoroutine()
    {
        if(PhotonNetwork.IsConnected)
        {
            yield return ContinuousController.instance.StartCoroutine(loadingObject.StartLoading("Disconnecting"));

            yield return ContinuousController.instance.StartCoroutine(PhotonUtility.DisconnectCoroutine());

            yield return ContinuousController.instance.StartCoroutine(loadingObject.EndLoading());
        }

        SetUpHome();
    }
}