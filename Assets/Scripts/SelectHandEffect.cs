using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using Photon.Pun;
using System;

public class SelectHandEffect : MonoBehaviourPunCallbacks
{
    public void SetUp(
        Player selectPlayer,
        Func<CardSource, bool> canTargetCondition,
        Func<List<CardSource>, CardSource, bool> canTargetCondition_ByPreSelecetedList,
        Func<List<CardSource>, bool> canEndSelectCondition,
        int maxCount,
        bool canNoSelect,
        bool canEndNotMax,
        bool isShowOpponent,
        Func<CardSource, IEnumerator> selectCardCoroutine,
        Func<List<CardSource>, IEnumerator> afterSelectCardCoroutine,
        Mode mode,
        ICardEffect cardEffect)
    {
        _selectPlayer = selectPlayer;
        _canTargetCondition = canTargetCondition;
        _canTargetCondition_ByPreSelecetedList = canTargetCondition_ByPreSelecetedList;
        _canEndSelectCondition = canEndSelectCondition;
        _maxCount = maxCount;
        _canNoSelect = canNoSelect;
        _canEndNotMax = canEndNotMax;
        _isShowOpponent = isShowOpponent;
        _selectCardCoroutine = selectCardCoroutine;
        _afterSelectCardCoroutine = afterSelectCardCoroutine;
        _mode = mode;
        _cardEffect = cardEffect;
        _showCard = true;
        _digiXros = false;

        _customMessage_ShowCard = null;
        _customMessage = "";
        _customMessage_Enemy = "";
        _showOpponentMessage = true;
        _isLocal = false;
        _isFaceUp = false;
    }

    public void SetIsLocal()
    {
        _isLocal = true;
    }

    public void SetNotShowCard()
    {
        _showCard = false;
    }

    public void SetDigiXros()
    {
        _digiXros = true;
    }

    public void SetIsFaceup()
    {
        _isFaceUp = true;
    }

    public void SetUpCustomMessage_ShowCard(string CustomMessage_ShowCard)
    {
        _customMessage_ShowCard = CustomMessage_ShowCard;
    }

    public void SetUpCustomMessage(string CustomMessage, string CustomMessage_Enemy)
    {
        _customMessage = CustomMessage;
        _customMessage_Enemy = CustomMessage_Enemy;
    }

    public void SetNotShowOpponentMessage()
    {
        _showOpponentMessage = false;
    }

    //The player who selects the hand
    Player _selectPlayer;
    //Conditions of the cards that can be selected
    Func<CardSource, bool> _canTargetCondition = null;
    //Whether the card can be selected with the current selection list status
    Func<List<CardSource>, CardSource, bool> _canTargetCondition_ByPreSelecetedList = null;
    //Conditions under which a selection can be terminated (see list of selection termination points)
    Func<List<CardSource>, bool> _canEndSelectCondition = null;
    //Maximum number of sheets to be selected
    int _maxCount = 0;
    //Whether you can choose not to choose
    bool _canNoSelect = false;
    //Whether you can finish your selection with less than the maximum number
    bool _canEndNotMax = false;
    //Whether or not to show it to the other player
    bool _isShowOpponent = false;
    //(Limited to Mode.Custom) Processing to be performed by selecting
    Func<CardSource, IEnumerator> _selectCardCoroutine = null;
    //Processing to be done after selection
    Func<List<CardSource>, IEnumerator> _afterSelectCardCoroutine = null;
    //Classification of processing to be done by selection
    Mode _mode = Mode.Custom;
    //Skill in making card selections
    ICardEffect _cardEffect;
    bool _showCard = true;
    bool _digiXros = false;
    bool _isLocal = false;
    bool _isFaceUp = false;

    public enum Mode
    {
        Discard,
        PutLibraryTop,
        PutLibraryBottom,
        PutSecurityBottom,
        Custom
    }

    //Selected Hand Card List
    List<CardSource> _targetCards = new List<CardSource>();
    //No Selection Flag
    bool _noSelect = false;

    bool _endSelect = false;

    string _customMessage_ShowCard = null;
    string _customMessage = null;
    string _customMessage_Enemy = null;
    bool _showOpponentMessage = true;

    #region Whether choice is possible
    public bool active()
    {
        if (_maxCount <= 0) return false;

        if (_selectPlayer != null)
        {
            if (_selectPlayer.HandCards.Count((cardSource) => _canTargetCondition(cardSource)) > 0)
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    public virtual IEnumerator Activate()
    {
        bool oldIsSelecting = GManager.instance.turnStateMachine.IsSelecting;

        List<PlayPermanentClass> playCharas = new List<PlayPermanentClass>();
        List<IDiscardHand> discardHands = new List<IDiscardHand>();
        List<CardSource> putClockCards = new List<CardSource>();

        _noSelect = false;

        if (active())
        {
            if (_maxCount == 0)
            {
                _canNoSelect = true;
            }

            _targetCards = new List<CardSource>();

            if (!_isLocal)
            {
                yield return GManager.instance.photonWaitController.StartWait("SelectHandEffect");
            }

            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
            {
                GManager.instance.turnStateMachine.OffFieldCardTarget(player);
                GManager.instance.turnStateMachine.OffHandCardTarget(player);
            }

            if (_selectPlayer.isYou)
            {
                GManager.instance.sideBar.SetUpSideBar();

                GManager.instance.turnStateMachine.IsSelecting = true;

                #region Message Display
                if (!string.IsNullOrEmpty(_customMessage))
                {
                    GManager.instance.commandText.OpenCommandText(_customMessage, _digiXros);
                }

                else
                {
                    string message = "";

                    switch (_mode)
                    {
                        case Mode.Discard:
                            message = "Select cards to discard.";
                            break;

                        case Mode.PutLibraryTop:
                            message = "Select cards to put on top of the deck.";
                            break;

                        case Mode.PutLibraryBottom:

                            if (_maxCount >= 2)
                            {
                                message = "Select cards to put on bottom of the deck\n(cards will be placed back to the bottom of the deck so that cards with lower numbers are on top).";
                            }

                            else
                            {
                                message = "Select cards to put on bottom of the deck.";
                            }

                            break;
                        case Mode.PutSecurityBottom:
                            string str = _isFaceUp ? "faceup" : "facedown";
                            message = $"Select card(s) to put on bottom of the security {str}.";

                            break;

                        case Mode.Custom:
                            message = "Select cards in your hand.";
                            break;
                    }

                    if (!string.IsNullOrEmpty(message))
                    {
                        GManager.instance.commandText.OpenCommandText(message, _digiXros);
                    }
                }
                #endregion

                List<CardSource> PreSelectedHandCards = new List<CardSource>();

                foreach (HandCard handCard in _selectPlayer.HandCardObjects)
                {
                    if (handCard != null)
                    {
                        if (_canTargetCondition(handCard.cardSource))
                        {
                            handCard.AddClickTarget(OnClickHandCard);
                        }
                    }
                }

                CheckEndSelect();

                #region Processing when a card in the hand is clicked
                void OnClickHandCard(HandCard handCard)
                {
                    if (PreSelectedHandCards.Contains(handCard.cardSource))
                    {
                        PreSelectedHandCards.Remove(handCard.cardSource);
                    }

                    else
                    {
                        if (_canTargetCondition_ByPreSelecetedList != null)
                        {
                            List<CardSource> _PreSelectedList = new List<CardSource>();

                            for (int i = 0; i < PreSelectedHandCards.Count; i++)
                            {
                                if (PreSelectedHandCards.Count >= _maxCount && i == PreSelectedHandCards.Count - 1)
                                {
                                    continue;
                                }

                                _PreSelectedList.Add(PreSelectedHandCards[i]);
                            }

                            if (!_canTargetCondition_ByPreSelecetedList(_PreSelectedList, handCard.cardSource))
                            {
                                return;
                            }
                        }

                        if (PreSelectedHandCards.Count < _maxCount)
                        {
                            PreSelectedHandCards.Add(handCard.cardSource);
                        }

                        else
                        {
                            if (PreSelectedHandCards.Count > 0)
                            {
                                PreSelectedHandCards.RemoveAt(PreSelectedHandCards.Count - 1);
                                PreSelectedHandCards.Add(handCard.cardSource);
                            }
                        }

                        if (!ContinuousController.instance.checkBeforeEndingSelection
                            && !_canNoSelect
                            && !_canEndNotMax
                            && _maxCount == PreSelectedHandCards.Count)
                        {
                            EndSelect_RPC();
                            return;
                        }
                    }

                    CheckEndSelect();
                }
                #endregion

                void EndSelect_RPC()
                {
                    foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
                    {
                        foreach (Permanent permanent in player.GetFieldPermanents())
                        {
                            permanent.ShowingPermanentCard.RemoveSelectEffect();
                            permanent.ShowingPermanentCard.RemoveClickTarget();
                        }

                        foreach (HandCard handCard in player.HandCardObjects)
                        {
                            if (handCard != null)
                            {
                                handCard.RemoveClickTarget();
                                handCard.RemoveSelectEffect();
                            }
                        }
                    }

                    List<int> CardIDs = new List<int>();

                    foreach (CardSource cardSource in PreSelectedHandCards)
                    {
                        CardIDs.Add(cardSource.CardIndex);
                    }

                    if (!_isLocal)
                    {
                        photonView.RPC("SetTargetHandCards", RpcTarget.All, CardIDs.ToArray());
                    }

                    else
                    {
                        SetTargetHandCards(CardIDs.ToArray());
                    }

                    GManager.instance.BackButton.CloseSelectCommandButton();
                }

                #region Determines if selection can be terminated and displays UI
                void CheckEndSelect()
                {
                    #region UI display depending on whether it can be terminated
                    if (CanEndSelect(PreSelectedHandCards))
                    {
                        GManager.instance.selectCommandPanel.SetUpCommandButton(
                            new List<Command_SelectCommand>()
                            {
                                new Command_SelectCommand("End Selection", EndSelect_RPC, 0)
                            });
                    }

                    else
                    {
                        GManager.instance.selectCommandPanel.Off(false);
                        GManager.instance.sideBar.SetUpSideBar();
                    }
                    #endregion

                    #region Card UI display by selection list
                    foreach (CardSource cardSource in _selectPlayer.HandCards)
                    {
                        if (cardSource.ShowingHandCard != null)
                        {
                            cardSource.ShowingHandCard.RemoveSelectEffect();
                            cardSource.ShowingHandCard.OffSelectedIndexText();

                            if (_canTargetCondition(cardSource))
                            {
                                if (PreSelectedHandCards.Contains(cardSource))
                                {
                                    cardSource.ShowingHandCard.OnSelect();
                                    cardSource.ShowingHandCard.SetOrangeOutline();
                                    cardSource.ShowingHandCard.SetSelectedIndexText(PreSelectedHandCards.IndexOf(cardSource) + 1);
                                }

                                else
                                {
                                    cardSource.ShowingHandCard.OnSelect();
                                    cardSource.ShowingHandCard.SetBlueOutline();

                                    if (_canTargetCondition_ByPreSelecetedList != null)
                                    {
                                        List<CardSource> _PreSelectedList = new List<CardSource>();

                                        for (int i = 0; i < PreSelectedHandCards.Count; i++)
                                        {
                                            if (PreSelectedHandCards.Count >= _maxCount && i == PreSelectedHandCards.Count - 1)
                                            {
                                                continue;
                                            }

                                            _PreSelectedList.Add(PreSelectedHandCards[i]);
                                        }

                                        if (!_canTargetCondition_ByPreSelecetedList(_PreSelectedList, cardSource))
                                        {
                                            cardSource.ShowingHandCard.RemoveSelectEffect();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    if (PreSelectedHandCards.Count >= 1)
                    {
                        GManager.instance.BackButton.CloseSelectCommandButton();
                    }

                    else
                    {
                        if (_canNoSelect)
                        {
                            GManager.instance.BackButton.OpenSelectCommandButton("No Selection", () => NoSelect_RPC(), 0);

                            void NoSelect_RPC()
                            {
                                if (!_isLocal)
                                {
                                    photonView.RPC("SetNoSelectHand", RpcTarget.All);
                                }

                                else
                                {
                                    SetNoSelectHand();
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            else
            {
                #region Message Display
                if (_showOpponentMessage)
                {
                    if (!string.IsNullOrEmpty(_customMessage_Enemy))
                    {
                        GManager.instance.commandText.OpenCommandText(_customMessage_Enemy, _digiXros);
                    }

                    else
                    {
                        GManager.instance.commandText.OpenCommandText("The opponent is selecting cards.", _digiXros);
                    }
                }

                #endregion

                #region AI
                if (GManager.instance.IsAI)
                {
                    List<CardSource> ValidCards = new List<CardSource>();

                    foreach (CardSource cardSource in _selectPlayer.HandCards)
                    {
                        if (_canTargetCondition(cardSource))
                        {
                            ValidCards.Add(cardSource);
                        }
                    }

                    if (_canEndNotMax)
                    {
                        _noSelect = true;

                        for (int maxCount = 0; maxCount < _maxCount; maxCount++)
                        {
                            IList<int> indexList = Enumerable.Range(0, ValidCards.Count).ToList();

                            if (ValidCards.Count >= maxCount)
                            {
                                for (int i = 0; i < 1000; i++)
                                {
                                    List<int> GetIndexes = indexList.GetRandom(maxCount).ToList();

                                    List<CardSource> GetCards = new List<CardSource>();

                                    foreach (int index in GetIndexes)
                                    {
                                        GetCards.Add(ValidCards[index]);
                                    }

                                    if (CanEndSelect(GetCards))
                                    {
                                        List<int> CardIDs = new List<int>();

                                        foreach (CardSource cardSource in GetCards)
                                        {
                                            CardIDs.Add(cardSource.CardIndex);
                                        }

                                        SetTargetHandCards(CardIDs.ToArray());
                                        break;
                                    }
                                }
                            }
                        }

                        _endSelect = true;
                    }

                    else
                    {
                        IList<int> indexList = Enumerable.Range(0, ValidCards.Count).ToList();

                        _noSelect = true;

                        if (ValidCards.Count >= _maxCount)
                        {
                            for (int i = 0; i < 1000; i++)
                            {
                                List<int> GetIndexes = indexList.GetRandom(_maxCount).ToList();

                                List<CardSource> GetCards = new List<CardSource>();

                                foreach (int index in GetIndexes)
                                {
                                    GetCards.Add(ValidCards[index]);
                                }

                                if (CanEndSelect(GetCards))
                                {
                                    List<int> CardIDs = new List<int>();

                                    foreach (CardSource cardSource in GetCards)
                                    {
                                        CardIDs.Add(cardSource.CardIndex);
                                    }

                                    SetTargetHandCards(CardIDs.ToArray());
                                    break;
                                }
                            }
                        }

                        _endSelect = true;

                    }
                }
                #endregion
            }

            #region Determine if you can exit
            bool CanEndSelect(List<CardSource> PreSelectedHandCards)
            {
                //If the number of cards does not meet the requirements
                if (!(PreSelectedHandCards.Count == _maxCount || (PreSelectedHandCards.Count <= _maxCount && _canEndNotMax)))
                {
                    return false;
                }

                //Failure to meet specified conditions
                if (_canEndSelectCondition != null)
                {
                    if (!_canEndSelectCondition(PreSelectedHandCards))
                    {
                        return false;
                    }
                }

                return true;
            }
            #endregion

            //Wait for selection to be completed
            yield return new WaitWhile(() => !_endSelect);
            _endSelect = false;

            #region Reset
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
            {
                GManager.instance.turnStateMachine.OffFieldCardTarget(player);
                GManager.instance.turnStateMachine.OffHandCardTarget(player);

                foreach (Permanent chara in player.GetFieldPermanents())
                {
                    chara.ShowingPermanentCard.RemoveSelectEffect();
                }

                foreach (HandCard handCard in player.HandCardObjects)
                {
                    if (handCard != null)
                    {
                        handCard.RemoveClickTarget();
                        handCard.RemoveSelectEffect();
                        handCard.OffSelectedIndexText();
                    }
                }
            }

            GManager.instance.commandText.CloseCommandText();
            yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

            GManager.instance.sideBar.OffSideBar();
            #endregion

            if (!_noSelect)
            {
                #region Show selected cards
                if (_targetCards.Count > 0 && _showCard)
                {
                    if (_isShowOpponent || _selectPlayer.isYou)
                    {
                        if (!string.IsNullOrEmpty(_customMessage_ShowCard))
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(_targetCards, _customMessage_ShowCard, true, true));
                        }

                        else
                        {
                            switch (_mode)
                            {
                                case Mode.Discard:

                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(_targetCards, "Discarded cards", true, true));

                                    break;

                                case Mode.PutLibraryTop:

                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(_targetCards, "Cards put on top of deck", true, true));

                                    break;

                                case Mode.PutLibraryBottom:

                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(_targetCards, "Cards put on bottom of deck", true, true));

                                    break;

                                case Mode.PutSecurityBottom:

                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(_targetCards, "Cards put on bottom of security", _isFaceUp, true));

                                    break;

                                case Mode.Custom:

                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(_targetCards, "Selected cards", true, true));

                                    break;
                            }
                        }
                    }
                }
                #endregion

                Hashtable hashtable = new Hashtable();

                if (_cardEffect != null)
                {
                    hashtable.Add("CardEffect", _cardEffect);
                }

                string log = "";

                log += $"\nSelected Hand Card:";

                #region Processes the selected card
                foreach (CardSource cardSource in _targetCards)
                {
                    log += $"\n{cardSource.BaseENGCardNameFromEntity}({cardSource.CardID})";

                    if(_selectCardCoroutine != null)
                        yield return StartCoroutine(_selectCardCoroutine(cardSource));
                }

                switch (_mode)
                {
                    case Mode.Discard:

                        discardHands = _targetCards.Map(cardSource => new IDiscardHand(cardSource, hashtable));
                        break;

                    case Mode.PutLibraryTop:

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryTopCards(_targetCards));
                        break;

                    case Mode.PutLibraryBottom:

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(_targetCards));
                        break;

                    case Mode.PutSecurityBottom:

                        foreach (CardSource cardSource in _targetCards)
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(cardSource,false, _isFaceUp));
                        break;
                }
                #endregion

                if (discardHands.Count > 0)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDiscardHands(discardHands, _cardEffect).DiscardHands());
                }

                #region ログ追加
                if (_isShowOpponent || _selectPlayer.isYou)
                {
                    if (_targetCards.Count >= 1)
                    {
                        log += "\n";

                        PlayLog.OnAddLog?.Invoke(log);
                    }
                }
                #endregion
            }

            if (_afterSelectCardCoroutine != null)
            {
                yield return StartCoroutine(_afterSelectCardCoroutine(_targetCards));
            }
        }

        GManager.instance.turnStateMachine.IsSelecting = oldIsSelecting;

    }

    #region カード選択を決定
    [PunRPC]
    public void SetTargetHandCards(int[] CardIDs)
    {
        _targetCards = new List<CardSource>();

        foreach (int CardID in CardIDs)
        {
            _targetCards.Add(GManager.instance.turnStateMachine.gameContext.ActiveCardList[CardID]);
        }

        _noSelect = false;

        _endSelect = true;
    }
    #endregion

    #region 何も選択しない
    [PunRPC]
    public void SetNoSelectHand()
    {
        GManager.instance.selectCommandPanel.CloseSelectCommandPanel();

        _targetCards = new List<CardSource>();

        _noSelect = true;

        _endSelect = true;
    }
    #endregion
}

