using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerInfo : MonoBehaviour
{
    public InputField PlayerNameInputField;
    public Text WinCountText;

    private void Start()
    {
        PlayerNameInputField.onEndEdit.RemoveAllListeners();
        PlayerNameInputField.onEndEdit.AddListener(SavePlayerName);
    }

    public void SetPlayerInfo()
    {
        PlayerNameInputField.characterLimit = ContinuousController.instance.PlayerNameMaxLength;
        PlayerNameInputField.onEndEdit.RemoveAllListeners();
        PlayerNameInputField.text = ContinuousController.instance.PlayerName;
        WinCountText.text = ContinuousController.instance.WinCount.ToString();

        this.gameObject.SetActive(true);
        PlayerNameInputField.onEndEdit.AddListener(SavePlayerName);
    }

    public void OffPlayerInfo()
    {
        this.gameObject.SetActive(false);
    }

    public void SavePlayerName(string text)
    {
        string playerName = text;

        playerName.Trim();

        while (playerName.Length > ContinuousController.instance.PlayerNameMaxLength)
        {
            playerName = playerName.Substring(0, playerName.Length - 1);
        }

        ContinuousController.instance.SavePlayerName(playerName);

        SetPlayerInfo();
    }
}
