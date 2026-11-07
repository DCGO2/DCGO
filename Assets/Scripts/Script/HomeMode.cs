using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCGO.Networking;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
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
    }

    public void SetUpHomeMode_Disconnect()
    {
        StartCoroutine(SetUpHomeMode_DisconnectCoroutine());
    }

    public IEnumerator SetUpHomeMode_DisconnectCoroutine()
    {
        if (DCGONetwork.IsConnected)
        {
            yield return ContinuousController.instance.StartCoroutine(loadingObject.StartLoading("Disconnecting"));

            bool disconnected = false;

            DCGONetwork.Provider.Disconnect(() =>
            {
                disconnected = true;
            });

            yield return new WaitUntil(() => disconnected);

            yield return ContinuousController.instance.StartCoroutine(loadingObject.EndLoading());
        }

        SetUpHome();
    }
}