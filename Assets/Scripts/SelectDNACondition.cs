using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using System.Linq;


public class SelectDNACondition : MonoBehaviourPunCallbacks
{
    public void SetUp
        (Player SelectPlayer,
        CardSource targetDNA,
        Func<int, IEnumerator> SelectDNACoroutine)
    {
        _selectPlayer = SelectPlayer;
        _targetDNA = targetDNA;
        _selectDNACoroutine = SelectDNACoroutine;
        _notDoSync = false;
        _isDigivolutionCost = false;
    }

    Player _selectPlayer = null;
    CardSource _targetDNA = null;
    Func<int, IEnumerator> _selectDNACoroutine = null;
    List<int> _candidates = new List<int>();
    public int _selectedCount = 0;
    public bool _endSelect = false;
    bool _notDoSync = false;
    bool _isDigivolutionCost = false;

    public void ResetSelectDNAConditionClass()
    {
        _selectedCount = 0;
        _endSelect = false;
    }

    public IEnumerator Activate()
    {
        if (!_notDoSync)
        {
            //yield return GManager.instance.photonWaitController.StartWait("SelectDNACondition");
        }

        if(_targetDNA.jogressCondition.Count > 1)
        {
            if (_targetDNA.jogressCondition.Count == 1)
            {
                SetDNACondition(0);
            }
            else
            {
                List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                for (int i = 0; i < _targetDNA.jogressCondition.Count; i++)
                {
                    string message = $"";

                    for (int e = 0; e < _targetDNA.jogressCondition[i].elements.Length; e++)
                    {
                        message += $"{_targetDNA.jogressCondition[i].elements[e].SelectMessage}";

                        if (e < _targetDNA.jogressCondition[i].elements.Length - 1)
                            message += " + ";
                    }

                    selectionElements.Add(new(message: $"{message}", value: i, spriteIndex: 0));
                }

                string selectPlayerMessage = "Which DNA do you want?";
                string notSelectPlayerMessage = "The opponent is choosing from which DNA to do.";

                GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: _targetDNA.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                GManager.instance.photonView.RPC("SetDNACondition", RpcTarget.All, GManager.instance.userSelectionManager.SelectedIntValue);
            }
        }

        yield return new WaitWhile(() => !_endSelect);
        _endSelect = false;

        GManager.instance.commandText.CloseCommandText();
        yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

        if (_selectDNACoroutine != null)
        {
            yield return ContinuousController.instance.StartCoroutine(_selectDNACoroutine(_selectedCount));
        }
    }

    [PunRPC]
    public void SetDNACondition(int selectedCount)
    {
        _selectedCount = selectedCount;
        _endSelect = true;
    }
}
