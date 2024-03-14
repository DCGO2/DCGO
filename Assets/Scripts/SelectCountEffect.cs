using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using System.Linq;


public class SelectCountEffect : MonoBehaviourPunCallbacks
{
    public void SetUp
        (Player SelectPlayer,
        Permanent targetPermanent,
        int MaxCount,
        bool CanNoSelect,
        string Message,
        string Message_Enemy,
        Func<int, IEnumerator> SelectCountCoroutine)
    {
        _selectPlayer = SelectPlayer;
        _targetPermanent = targetPermanent;
        _maxCount = MaxCount;
        _canNoSelect = CanNoSelect;
        _messageForOwner = Message;
        _messageForEnemy = Message_Enemy;
        _selectCountCoroutine = SelectCountCoroutine;
        _preferMin = false;
        _notDoSync = false;
        _isDigivolutionCost = false;

        _candidates = new List<int>();
    }

    public void SetCandidates(List<int> candidates)
    {
        _candidates = candidates;
    }

    public void SetPreferMin(bool preferMin)
    {
        _preferMin = preferMin;
    }

    public void SetNotDoSync(bool notDoSync)
    {
        _notDoSync = notDoSync;
    }

    public void SetIsDigivolutionCost(bool isDigivolutionCost)
    {
        _isDigivolutionCost = isDigivolutionCost;
    }

    Player _selectPlayer = null;
    Permanent _targetPermanent = null;
    int _maxCount = 0;
    bool _canNoSelect = false;
    string _messageForOwner = "";
    string _messageForEnemy = "";
    Func<int, IEnumerator> _selectCountCoroutine = null;
    List<int> _candidates = new List<int>();
    int _selectedCount = 0;
    bool _endSelect = false;
    bool _preferMin = true;
    bool _notDoSync = false;
    bool _isDigivolutionCost = false;

    public IEnumerator Activate()
    {
        if (!_notDoSync)
        {
            yield return GManager.instance.photonWaitController.StartWait("SelectCountEffect");
        }

        if (_maxCount >= 1)
        {
            bool isOldActive_Outline = false;

            if (_targetPermanent != null)
            {
                if (_targetPermanent.ShowingPermanentCard != null)
                {
                    isOldActive_Outline = _targetPermanent.ShowingPermanentCard.Outline_Select.gameObject.activeSelf;
                    _targetPermanent.ShowingPermanentCard.SetOrangeOutline();
                    _targetPermanent.ShowingPermanentCard.Outline_Select.gameObject.SetActive(true);
                }
            }

            List<int> candidates = new List<int>();

            for (int i = 0; i < _maxCount + 1; i++)
            {
                if (i == 0)
                {
                    if (!_canNoSelect)
                    {
                        continue;
                    }
                }

                candidates.Add(i);
            }

            if (_candidates != null)
            {
                if (_candidates.Count >= 1)
                {
                    candidates = new List<int>();

                    foreach (int candidate in _candidates)
                    {
                        candidates.Add(candidate);
                    }
                }
            }

            candidates = candidates.Distinct().OrderBy((value) => value).ToList();

            if (candidates.Count >= 1)
            {
                if (candidates.Count == 1)
                {
                    SetCount(candidates[0]);
                }

                else
                {
                    if (_selectPlayer.isYou)
                    {
                        if ((!_isDigivolutionCost && ContinuousController.instance.autoMaxCardCount && !_preferMin)
                        || (_isDigivolutionCost && ContinuousController.instance.autoMinDigivolutionCost && _preferMin))
                        {
                            int preferedNumber = _preferMin ? candidates.Min() : candidates.Max();
                            photonView.RPC("SetCount", RpcTarget.All, preferedNumber);
                        }

                        else
                        {
                            GManager.instance.commandText.OpenCommandText($"{_messageForOwner}");

                            List<Command_SelectCommand> command_SelectCommands = new List<Command_SelectCommand>()
                            {

                            };

                            for (int i = 0; i < candidates.Count; i++)
                            {
                                int k = candidates[i];

                                string text = $"{k}";

                                command_SelectCommands.Add(new Command_SelectCommand(text, () => photonView.RPC("SetCount", RpcTarget.All, k), 0));
                            }

                            GManager.instance.selectCommandPanel.SetUpCommandButton(command_SelectCommands);
                        }
                    }

                    else
                    {
                        #region AI���[�h
                        if (GManager.instance.IsAI)
                        {
                            int preferedNumber = _preferMin ? candidates.Min() : candidates.Max();
                            SetCount(preferedNumber);
                        }
                        #endregion

                        else
                        {
                            GManager.instance.commandText.OpenCommandText($"{_messageForEnemy}");
                        }
                    }
                }

                yield return new WaitWhile(() => !_endSelect);
                _endSelect = false;

                GManager.instance.commandText.CloseCommandText();
                yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

                if (_selectCountCoroutine != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(_selectCountCoroutine(_selectedCount));
                }
            }

            if (_targetPermanent != null)
            {
                if (_targetPermanent.ShowingPermanentCard != null)
                {
                    _targetPermanent.ShowingPermanentCard.Outline_Select.gameObject.SetActive(isOldActive_Outline);
                }
            }
        }
    }

    [PunRPC]
    public void SetCount(int selectedCount)
    {
        _selectedCount = selectedCount;
        _endSelect = true;
    }
}
