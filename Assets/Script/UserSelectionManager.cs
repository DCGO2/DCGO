using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

public class UserSelectionManager : MonoBehaviourPunCallbacks
{
    bool _endSelect = false;
    int _selectedIntValue = 0;
    bool _selectedBoolValue => getBoolFromInt(_selectedIntValue);
    public int SelectedIntValue => _selectedIntValue;
    public bool SelectedBoolValue => _selectedBoolValue;

    [PunRPC]
    public void SetInt(int value)
    {
        _selectedIntValue = value;
        _endSelect = true;
    }

    protected void SetInt_RPC(int value)
    {
        photonView.RPC("SetInt", RpcTarget.All, value);
    }

    [PunRPC]
    public void SetBool(bool value)
    {
        _selectedIntValue = getIntFromBool(value);
        _endSelect = true;
    }

    protected void SetBool_RPC(bool value)
    {
        photonView.RPC("SetBool", RpcTarget.All, value);
    }

    internal int getIntFromBool(bool value)
    {
        return value ? 0 : 1;
    }

    internal bool getBoolFromInt(int value)
    {
        return value == 0;
    }

    public IEnumerator WaitForEndSelect()
    {
        yield return new WaitWhile(() => !_endSelect);
        _endSelect = false;

        GManager.instance.commandText.CloseCommandText();
        yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);
    }

    public void SetIntSelection(List<SelectionElement<int>> selectionElements, Player selectPlayer, string selectPlayerMessage, string notSelectPlayerMessage)
    {
        _endSelect = false;
        _selectedIntValue = 0;

        if (selectPlayer.isYou)
        {
            GManager.instance.commandText.OpenCommandText(selectPlayerMessage);

            List<Command_SelectCommand> command_SelectCommands = new List<Command_SelectCommand>();

            foreach (SelectionElement<int> selectionElement in selectionElements)
            {
                command_SelectCommands.Add(new Command_SelectCommand(selectionElement.Message, () => SetInt_RPC(selectionElement.Value), selectionElement.SpriteIndex));
            }

            GManager.instance.selectCommandPanel.SetUpCommandButton(command_SelectCommands);
        }

        else
        {
            GManager.instance.commandText.OpenCommandText(notSelectPlayerMessage);

            #region AIモード
            if (GManager.instance.IsAI)
            {
                List<int> canSelectValue = new List<int>();

                foreach (SelectionElement<int> selectionElement in selectionElements)
                {
                    canSelectValue.Add(selectionElement.Value);
                }

                if (canSelectValue.Count >= 1)
                {
                    SetInt(canSelectValue[UnityEngine.Random.Range(0, canSelectValue.Count)]);
                }

                else
                {
                    SetInt(0);
                }
            }
            #endregion
        }
    }

    public void SetBoolSelection(List<SelectionElement<bool>> selectionElements, Player selectPlayer, string selectPlayerMessage, string notSelectPlayerMessage)
    {
        if (selectPlayer.isYou)
        {
            GManager.instance.commandText.OpenCommandText(selectPlayerMessage);

            List<Command_SelectCommand> command_SelectCommands = new List<Command_SelectCommand>();

            foreach (SelectionElement<bool> selectionElement in selectionElements)
            {
                command_SelectCommands.Add(new Command_SelectCommand(selectionElement.Message, () => SetBool_RPC(selectionElement.Value), selectionElement.SpriteIndex));
            }

            GManager.instance.selectCommandPanel.SetUpCommandButton(command_SelectCommands);
        }

        else
        {
            GManager.instance.commandText.OpenCommandText(notSelectPlayerMessage);

            #region AIモード
            if (GManager.instance.IsAI)
            {
                List<bool> canSelectValue = new List<bool>();

                foreach (SelectionElement<bool> selectionElement in selectionElements)
                {
                    canSelectValue.Add(selectionElement.Value);
                }

                if (canSelectValue.Count >= 1)
                {
                    SetBool(canSelectValue[UnityEngine.Random.Range(0, canSelectValue.Count)]);
                }

                else
                {
                    SetBool(false);
                }
            }
            #endregion
        }
    }
}

public class SelectionElement<T>
{
    public SelectionElement(string message, T value, int spriteIndex)
    {
        this.Message = message;
        this.Value = value;
        this.SpriteIndex = spriteIndex;
    }
    public string Message { get; private set; }
    public T Value { get; private set; }
    public int SpriteIndex { get; private set; }
}