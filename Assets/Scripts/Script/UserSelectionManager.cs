using DCGO.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UserSelectionManager : MonoBehaviour
{
    bool _endSelect = false;
    int _selectedIntValue = 0;
    bool _isLocal = false;
    bool _selectedBoolValue => getBoolFromInt(_selectedIntValue);
    public int SelectedIntValue => _selectedIntValue;
    public bool SelectedBoolValue => _selectedBoolValue;

    Player _selectPlayer;

    public void SetInt(int value)
    {
        _selectedIntValue = value;
        _endSelect = true;
    }

    public void SetBool(bool value)
    {
        _selectedIntValue = getIntFromBool(value);
        _endSelect = true;
    }

    internal int getIntFromBool(bool value)
    {
        return value ? 1 : 0;
    }

    internal bool getBoolFromInt(int value)
    {
        return value != 0;
    }

    public IEnumerator WaitForEndSelect()
    {
        if (_selectPlayer != null)
        {
            yield return new WaitUntil(() => _selectPlayer.HasPlayerSelection());

            ValueSelection valueSeletion = _selectPlayer.DequeuePlayerSelection<ValueSelection>();

            if (valueSeletion != null)
            {
                _selectedIntValue = valueSeletion.ValueAsInt();
            }
        }
        else
        {
            yield return new WaitWhile(() => !_endSelect);
        }

        _endSelect = false;
        _selectPlayer = null;

        GManager.instance.commandText.CloseCommandText();
        yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);
    }

    public void SetIntSelection(List<SelectionElement<int>> selectionElements, Player selectPlayer, string selectPlayerMessage, string notSelectPlayerMessage, bool IsLocal = false)
    {
        _endSelect = false;
        _selectedIntValue = 0;
        _selectPlayer = selectPlayer;
        _isLocal = IsLocal;

        if (selectPlayer.isYou)
        {
            GManager.instance.commandText.OpenCommandText(selectPlayerMessage);

            List<Command_SelectCommand> command_SelectCommands = new List<Command_SelectCommand>();

            foreach (SelectionElement<int> selectionElement in selectionElements)
            {
                command_SelectCommands.Add(new Command_SelectCommand(selectionElement.Message, () => SendSelection(selectionElement.Value), selectionElement.SpriteIndex));
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

                int value = canSelectValue.Count >= 1 ? canSelectValue[UnityEngine.Random.Range(0, canSelectValue.Count)] : 0;
                SendSelection(value);
            }
            #endregion
        }

        void SendSelection(int value)
        {
            ValueSelection selection = new ValueSelection(value);
            if (_isLocal)
            {
                selectPlayer.QueuePlayerSelection(selection);
            }
            else
            {
                DCGONetwork.Provider.SendPlayerSelection(selectPlayer, selection);
            }
        }
    }

    public void SetBoolSelection(List<SelectionElement<bool>> selectionElements, Player selectPlayer, string selectPlayerMessage, string notSelectPlayerMessage, bool IsLocal = false)
    {
        _endSelect = false;
        _selectedIntValue = 0;
        _selectPlayer = selectPlayer;
        _isLocal = IsLocal;

        if (selectPlayer.isYou)
        {
            GManager.instance.commandText.OpenCommandText(selectPlayerMessage);

            List<Command_SelectCommand> command_SelectCommands = new List<Command_SelectCommand>();

            foreach (SelectionElement<bool> selectionElement in selectionElements)
            {
                command_SelectCommands.Add(new Command_SelectCommand(selectionElement.Message, () => SendSelection(selectionElement.Value), selectionElement.SpriteIndex));
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

                bool value = canSelectValue.Count >= 1 ? canSelectValue[UnityEngine.Random.Range(0, canSelectValue.Count)] : false;
                SendSelection(value);
            }
            #endregion
        }

        void SendSelection(bool value)
        {
            ValueSelection selection = new ValueSelection(value);
            if (_isLocal)
            {
                selectPlayer.QueuePlayerSelection(selection);
            }
            else
            {
                DCGONetwork.Provider.SendPlayerSelection(selectPlayer, selection);
            }
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