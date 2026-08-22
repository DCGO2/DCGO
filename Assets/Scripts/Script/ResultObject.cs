using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;

public class ResultObject : MonoBehaviour
{
    [SerializeField] Image WinImage;
    [SerializeField] Image LoseImage;
    [SerializeField] Text ResultText;

    public void Init()
    {
        this.gameObject.SetActive(false);
    }

    public void ShowResult(Player Winner, bool Surrendered, string effectName = "")
    {
        this.gameObject.SetActive(true);

        string log = "";

        log += "\nEnd Game";

        if (Winner != null)
        {
            log += $"\nWinner:{Winner.PlayerName}";
        }

        ResultText.text = "";

        if (String.IsNullOrEmpty(effectName))
        {
            log += $"\nEffect:{effectName}";
            ResultText.text = effectName;
        }            

        if (Winner == GManager.instance.You)
        {
            ContinuousController.instance.PlaySE(GManager.instance.WinSE);

            WinImage.gameObject.SetActive(true);
            LoseImage.gameObject.SetActive(false);

            if (!GManager.instance.IsAI)
            {
                // === DCGO-CUSTOM:friends begin ===
                bool isFriendDuel = ContinuousController.instance != null && ContinuousController.instance.isFriendDuel;
                if (!isFriendDuel)
                {
                    ContinuousController.instance.WinCount++;
                    ContinuousController.instance.SaveWinCount();
                }
                // === DCGO-CUSTOM:friends end ===

                if (Surrendered)
                    ResultText.text = "The opponent has surrendered.";

                if (String.IsNullOrEmpty(effectName))
                    ResultText.text = effectName;
            }
        }

        else if (Winner != null)
        {
            ContinuousController.instance.PlaySE(GManager.instance.LoseSE);

            WinImage.gameObject.SetActive(false);
            LoseImage.gameObject.SetActive(true);

            if (Winner != null)
            {
                if (Surrendered)
                    ResultText.text = "You have surrendered.";
            }
        }
        else
        {
            WinImage.gameObject.SetActive(false);
            LoseImage.gameObject.SetActive(true);

            bool isDisconnected = true;

            if (PhotonNetwork.IsConnected)
            {
                if (PhotonNetwork.PlayerList.Length == 2)
                {
                    isDisconnected = false;
                }

                if (GManager.instance.IsAI)
                {
                    isDisconnected = false;
                }

                WinImage.gameObject.SetActive(true);
                LoseImage.gameObject.SetActive(false);
            }

            if (isDisconnected)
            {
                log += $"\nDisconnected";

                ResultText.text = "Disconnected.";
            }

            else
            {
                log += $"\nDraw";

                ResultText.text = "Draw.";
            }
        }

        // === DCGO-CUSTOM:friends begin ===
        if (!GManager.instance.IsAI)
        {
            RememberLastOpponentFromRoom();
        }

        bool friendDuel = ContinuousController.instance != null && ContinuousController.instance.isFriendDuel;
        if (friendDuel && !GManager.instance.IsAI)
        {
            bool? localWon = null;
            if (Winner == GManager.instance.You)
            {
                localWon = true;
            }
            else if (Winner != null)
            {
                localWon = false;
            }

            bool disconnect = Winner == null;
            var director = FriendServices.EnsureExists().Director;
            director.NotifyGameEnded(localWon, disconnect, Winner == null && !disconnect);

            string seriesLine = director.FormatSeriesStatusLine();
            if (!string.IsNullOrEmpty(seriesLine))
            {
                if (string.IsNullOrEmpty(ResultText.text))
                {
                    ResultText.text = seriesLine;
                }
                else
                {
                    ResultText.text += "\n" + seriesLine;
                }
            }
        }
        // === DCGO-CUSTOM:friends end ===

        PlayLog.OnAddLog?.Invoke(log);
    }

    // === DCGO-CUSTOM:friends begin ===
    static void RememberLastOpponentFromRoom()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.PlayerList == null)
        {
            return;
        }

        string localId = FriendListService.LocalPlayFabId();
        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (p == null || p.IsLocal)
            {
                continue;
            }

            string id = FriendDuelDirector.ReadPlayerId(p);
            string name = p.NickName;
            if (p.CustomProperties != null &&
                p.CustomProperties.TryGetValue(ContinuousController.PlayerNameKey, out object nObj) &&
                nObj is string ns &&
                !string.IsNullOrEmpty(ns))
            {
                name = ns;
            }

            if (string.IsNullOrEmpty(id) ||
                (!string.IsNullOrEmpty(localId) &&
                 string.Equals(id, localId, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            FriendServices.EnsureExists().List.RememberLastOpponent(id, name);
            break;
        }
    }
    // === DCGO-CUSTOM:friends end ===
}
