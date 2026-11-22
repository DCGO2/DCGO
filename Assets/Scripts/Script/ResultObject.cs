using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System;

public class ResultObject : MonoBehaviour
{
    [SerializeField] Image _winImage;
    [SerializeField] Image _loseImage;
    [SerializeField] Image _drawImage;
    [SerializeField] Text _resultText;
    public Button ReturnToResultButton;

    public void Init()
    {
        this.gameObject.SetActive(false);
    }

    public void SetResultText(string text)
    {
        ResultText.text = text;
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

        _resultText.text = "";

        if (String.IsNullOrEmpty(effectName))
        {
            log += $"\nEffect:{effectName}";
            _resultText.text = effectName;
        }            

        if (Winner == GManager.instance.You)
        {
            ContinuousController.instance.PlaySE(GManager.instance.WinSE);

            _winImage.gameObject.SetActive(true);
            _loseImage.gameObject.SetActive(false);
            _drawImage.gameObject.SetActive(false);

            if (!GManager.instance.IsAI)
            {
                ContinuousController.instance.WinCount++;
                ContinuousController.instance.SaveWinCount();

                if (Surrendered)
                    _resultText.text = "The opponent has surrendered.";

                if (String.IsNullOrEmpty(effectName))
                    _resultText.text = effectName;
            }
        }

        else if (Winner != null)
        {
            ContinuousController.instance.PlaySE(GManager.instance.LoseSE);

            _winImage.gameObject.SetActive(false);
            _loseImage.gameObject.SetActive(true);
            _drawImage.gameObject.SetActive(false);

            if (Winner != null)
            {
                if (Surrendered)
                    _resultText.text = "You have surrendered.";
            }
        }
        else
        {
            _winImage.gameObject.SetActive(false);
            _loseImage.gameObject.SetActive(true);
            _drawImage.gameObject.SetActive(false);

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

                _winImage.gameObject.SetActive(true);
                _loseImage.gameObject.SetActive(false);
                _drawImage.gameObject.SetActive(false);
            }

            if (isDisconnected)
            {
                log += $"\nDisconnected";

                _resultText.text = "Disconnected.";
            }

            else
            {
                _winImage.gameObject.SetActive(false);
                _loseImage.gameObject.SetActive(false);
                _drawImage.gameObject.SetActive(true);

                log += $"\nDraw";

                _resultText.text = "Reconnection Error.";
            }
        }

        PlayLog.OnAddLog?.Invoke(log);
    }
    #region Temporarily display/hide Result Screen
    public void OnClickReturnToResultsdButton()
    {
        this.gameObject.SetActive(true);
        ReturnToResultButton.gameObject.SetActive(false);
    }

    public void OnClickCheckFieldResultsButton()
    {
        this.gameObject.SetActive(false);
        ReturnToResultButton.gameObject.SetActive(true);
        ReturnToResultButton.onClick.RemoveAllListeners();
        ReturnToResultButton.onClick.AddListener(() => OnClickReturnToResultsdButton());
    }
    #endregion
}
