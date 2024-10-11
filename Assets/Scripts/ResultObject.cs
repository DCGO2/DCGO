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
                ContinuousController.instance.WinCount++;
                ContinuousController.instance.SaveWinCount();

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

        PlayLog.OnAddLog?.Invoke(log);
    }
}
