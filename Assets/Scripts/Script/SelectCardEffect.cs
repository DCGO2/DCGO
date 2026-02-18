using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;
using System;

public class SelectCardEffect : MonoBehaviourPunCallbacks
{
    public void SetUp(
        Func<CardSource, bool> canTargetCondition,
        Func<List<CardSource>, CardSource, bool> canTargetCondition_ByPreSelecetedList,
        Func<List<CardSource>, bool> canEndSelectCondition,
        Func<bool> canNoSelect,
        Func<CardSource, IEnumerator> selectCardCoroutine,
        Func<List<CardSource>, IEnumerator> afterSelectCardCoroutine,
        string message,
        int maxCount,
        bool canEndNotMax,
        bool isShowOpponent,
        Mode mode,
        Root root,
        List<CardSource> customRootCardList,
        bool canLookReverseCard,
        Player selectPlayer,
        ICardEffect cardEffect)
    {
        _canTargetCondition = canTargetCondition;
        _canTargetCondition_ByPreSelecetedList = canTargetCondition_ByPreSelecetedList;
        _canEndSelectCondition = canEndSelectCondition;
        _canNoSelect = canNoSelect;
        _selectCardCoroutine = selectCardCoroutine;
        _afterSelectCardCoroutine = afterSelectCardCoroutine;
        _message = message;
        _maxCount = maxCount;
        _canEndNotMax = canEndNotMax;
        _isShowOpponent = isShowOpponent;
        _mode = mode;
        _root = root;
        _customRootCardList = customRootCardList;
        _canLookReverseCard = canLookReverseCard;
        _selectPlayer = selectPlayer;
        _cardEffect = cardEffect;

        _isLocal = false;
        _customMessage = null;
        _customMessage_Enemy = null;
        _customMessage_ShowCard = null;
        _customCountText = null;
        _showReverseCard = true;
        _showCard = true;
        _isDigiXros = false;
        _isAssembly = false;
        _isDeckBottom = false;
        _isDeckTop = false;
        _notAddLog = false;
        _isSecurity = false;
        _allowFaceDown = false;

        _skillInfos = new List<SkillInfo>();

        _afterSelectIndexCoroutine = null;

        _endSelect = false;
    }

    public void SetIsLocal()
    {
        _isLocal = true;
    }

    public void SetIsDeckBottom()
    {
        _isDeckBottom = true;
    }

    public void SetIsDeckTop()
    {
        _isDeckTop = true;
    }

    public void SetNotShowCard()
    {
        _showCard = false;
    }

    public void SetNotAddLog()
    {
        _notAddLog = true;
    }

    public void SetDigiXros()
    {
        _isDigiXros = true;
    }

    public void SetAssembly()
    {
        _isAssembly = true;
    }

    public void SetIsSecurity()
    {
        _isSecurity = true;
    }

    public void SetUseFaceDown()
    {
        _allowFaceDown = true;
    }
    public void SetUpSkillInfos(List<SkillInfo> skillInfos)
    {
        _skillInfos = skillInfos.Clone();
    }

    public void SetUpCustomMessage(string CustomMessage, string CustomMessage_Enemy)
    {
        _customMessage = CustomMessage;
        _customMessage_Enemy = CustomMessage_Enemy;
    }

    public void SetUpCustomMessage_ShowCard(string CustomMessage_ShowCard)
    {
        _customMessage_ShowCard = CustomMessage_ShowCard;
    }

    public void SetUpCustomCountText(string CustomCountText)
    {
        _customCountText = CustomCountText;
    }

    public void SetShowReverseCard()
    {
        _showReverseCard = false;
    }

    Func<CardSource, bool> _canTargetCondition = null;
    Func<List<CardSource>, CardSource, bool> _canTargetCondition_ByPreSelecetedList = null;
    Func<List<CardSource>, bool> _canEndSelectCondition = null;
    Func<bool> _canNoSelect = null;

    List<CardSource> _targetCards = new List<CardSource>();

    Func<CardSource, IEnumerator> _selectCardCoroutine = null;

    Func<List<CardSource>, IEnumerator> _afterSelectCardCoroutine = null;

    string _message = "";

    int _maxCount = 0;

    bool _canEndNotMax = false;

    bool _isShowOpponent = false;

    bool _canLookReverseCard = false;

    List<CardSource> _customRootCardList { get; set; } = new List<CardSource>();

    Player _selectPlayer = null;
    ICardEffect _cardEffect = null;

    bool _isLocal = false;
    bool _showReverseCard = true;
    bool _showCard = true;
    bool _notAddLog = false;
    bool _isDigiXros = false;
    bool _isAssembly = false;
    bool _isSecurity = false;
    bool _allowFaceDown = false;

    public enum Mode
    {
        AddHand,
        Discard,

        // PutLibraryTop,
        // PutLibraryBottom,
        Custom,
    }

    Mode _mode;

    public enum Root
    {
        Library,
        Trash,
        Clock,
        Security,
        Custom,
        Hand,
        Recollection,
        Execution,
        DigivolutionCards,
        LinkedCards,
        None,
    }

    Root _root;
    bool _endSelect = false;
    bool _isDeckBottom = false;
    bool _isDeckTop = false;

    string _customMessage = null;
    string _customMessage_Enemy = null;
    string _customMessage_ShowCard = null;
    string _customCountText = null;

    List<SkillInfo> _skillInfos = new List<SkillInfo>();
    List<int> _slectedInexesInList = new List<int>();
    Func<List<int>, IEnumerator> _afterSelectIndexCoroutine = null;

    public void SetUpAfterSelectIndexCoroutine(Func<List<int>, IEnumerator> AfterSelectIndexCoroutine)
    {
        _afterSelectIndexCoroutine = AfterSelectIndexCoroutine;
    }

    public virtual List<CardSource> RootCardList()
    {
        List<CardSource> RootCardList = new List<CardSource>();

        if (_customRootCardList == null)
        {
            switch (_root)
            {
                case Root.Library:
                    foreach (CardSource cardSource in _selectPlayer.LibraryCards)
                    {
                        RootCardList.Add(cardSource);
                    }
                    break;

                case Root.Trash:
                    foreach (CardSource cardSource in _selectPlayer.TrashCards)
                    {
                        RootCardList.Add(cardSource);
                    }
                    break;

                case Root.Security:
                    foreach (CardSource cardSource in _selectPlayer.SecurityCards)
                    {
                        RootCardList.Add(cardSource);
                    }
                    break;

                case Root.Recollection:
                    foreach (CardSource cardSource in _selectPlayer.LostCards)
                    {
                        RootCardList.Add(cardSource);
                    }
                    break;
            }
        }
        else
        {
            foreach (CardSource cardSource in _customRootCardList)
            {
                RootCardList.Add(cardSource);
            }
        }

        return RootCardList;
    }

    private bool CanSelectCard(CardSource cardSource)
    {
        
        if (_root != Root.Library && _root != Root.Security && _root != Root.Custom)
        {
            if (cardSource.IsFlipped)
                return false;
        }

        if (_canTargetCondition != null)
        {
            if (_canTargetCondition(cardSource))
            {
                if (!_allowFaceDown)
                {
                    if (cardSource.IsFlipped)
                        return false;
                }

                return true;
            }
        }

        return false;
    }

    public bool active()
    {
        if (RootCardList().Count > 0)
        {
            if (_root != Root.Library && _root != Root.Security)
            {
                if (RootCardList().Count(CanSelectCard) > 0)
                {
                    return true;
                }
            }
            else
            {
                if (_root == Root.Library)
                    SetUseFaceDown();

                if(_root == Root.Security)
                {
                    if (_canLookReverseCard)
                        SetUseFaceDown();
                }

                return true;
            }
        }

        return false;
    }

    public virtual IEnumerator Activate()
    {
        bool oldIsSelecting = GManager.instance.turnStateMachine.IsSelecting;

        List<CardSource> handCards = new List<CardSource>();

        _targetCards = new List<CardSource>();

        _slectedInexesInList = new List<int>();
        
        if (!_isLocal)
        {
            yield return GManager.instance.photonWaitController.StartWait("SelectCardEffect");
        }

        bool oldIsActiveOutline_AttackingPermanent = false;

        if (GManager.instance.attackProcess.AttackingPermanent != null)
        {
            if (GManager.instance.attackProcess.AttackingPermanent.ShowingPermanentCard != null)
            {
                oldIsActiveOutline_AttackingPermanent = GManager.instance.attackProcess.AttackingPermanent.ShowingPermanentCard.Outline_Select.gameObject.activeSelf;
            }
        }

        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
        {
            GManager.instance.turnStateMachine.OffFieldCardTarget(player);
            GManager.instance.turnStateMachine.OffHandCardTarget(player);
        }

        if (GManager.instance.attackProcess.AttackingPermanent != null)
        {
            if (GManager.instance.attackProcess.AttackingPermanent.ShowingPermanentCard != null)
            {
                GManager.instance.attackProcess.AttackingPermanent.ShowingPermanentCard.Outline_Select.gameObject.SetActive(oldIsActiveOutline_AttackingPermanent);
            }
        }

        if (_maxCount == 0)
        {
            _canNoSelect = () => true;
        }

        if (_root == Root.Security)
        {
            SetIsSecurity();
        }

        if (active())
        {
            if (_isSecurity)
            {
                GManager.instance.turnStateMachine.gameContext.IsSecurityLooking = true;
            }

            if (_selectPlayer.isYou)
            {
                if ((_isDeckBottom && ContinuousController.instance.autoDeckBottomOrder)
                || (_isDeckTop && ContinuousController.instance.autoDeckTopOrder))
                {
                    AutoSelect();
                }
                else
                {
                    List<int> targetCardIDs = new List<int>();

                    GManager.instance.turnStateMachine.IsSelecting = true;

                    #region Message Display

                    if (!string.IsNullOrEmpty(_customMessage))
                    {
                        GManager.instance.commandText.OpenCommandText(_customMessage, _isDigiXros, _isAssembly);
                    }
                    else
                    {
                        string message = "";

                        switch (_mode)
                        {
                            case Mode.AddHand:
                                message = "Select cards to add to your hand.";
                                break;

                            case Mode.Discard:
                                message = "Select cards to trash.";
                                break;

                            case Mode.Custom:
                                message = "Select cards.";
                                break;
                        }

                        if (!string.IsNullOrEmpty(message))
                        {
                            GManager.instance.commandText.OpenCommandText(message, _isDigiXros, _isAssembly);
                        }
                    }

                    #endregion

                    List<CardSource> RootCards = new List<CardSource>();

                    foreach (CardSource cardSource in RootCardList())
                    {
                        RootCards.Add(cardSource);
                    }

                    #region Sort

                    if (_root == Root.Library || _root == Root.Trash)
                    {
                        RootCards = DeckData.SortedCardsList(RootCards);

                        List<CardSource> matchConditionCards = new List<CardSource>();
                        List<CardSource> notMatchConditionCards = new List<CardSource>();

                        foreach (CardSource cardSource in RootCards)
                        {
                            if (CanSelectCard(cardSource))
                            {
                                matchConditionCards.Add(cardSource);
                            }
                            else
                            {
                                notMatchConditionCards.Add(cardSource);
                            }
                        }

                        RootCards = new List<CardSource>();

                        foreach (CardSource cardSource in matchConditionCards)
                        {
                            RootCards.Add(cardSource);
                        }

                        foreach (CardSource cardSource in notMatchConditionCards)
                        {
                            RootCards.Add(cardSource);
                        }
                    }

                    #endregion

                    #region Effect in progress/waiting to be activated

                    if (_cardEffect != null)
                    {
                        if (_cardEffect.EffectSourceCard != null)
                        {
                            if (_skillInfos.Count == 0)
                            {
                                if (_root == Root.Trash || _root == Root.DigivolutionCards)
                                {
                                    SkillInfo[] skillInfoArray = new SkillInfo[RootCards.Count];

                                    for (int i = 0; i < skillInfoArray.Length; i++)
                                    {
                                        if (0 <= i && i <= RootCards.Count - 1)
                                        {
                                            if (RootCards[i] == _cardEffect.EffectSourceCard)
                                            {
                                                ICardEffect cardEffect = new ChangeBaseDPClass();
                                                cardEffect.SetUpICardEffect("Effect Processing", null, RootCards[i]);

                                                skillInfoArray[i] = new SkillInfo(cardEffect, null, EffectTiming.None);
                                            }
                                            else
                                            {
                                                if (GManager.instance.autoProcessing.executingMultipleSkills != null)
                                                {
                                                    bool isEffectWaiting(SkillInfo skillInfo)
                                                    {
                                                        if (skillInfo != null)
                                                        {
                                                            if (skillInfo.CardEffect != null)
                                                            {
                                                                if (skillInfo.CardEffect.EffectSourceCard != null)
                                                                {
                                                                    if (skillInfo.CardEffect.EffectSourceCard == RootCards[i])
                                                                    {
                                                                        return true;
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        return false;
                                                    }

                                                    if (GManager.instance.autoProcessing.executingMultipleSkills.StackedSkillInfos.Count(isEffectWaiting) >= 1)
                                                    {
                                                        ICardEffect cardEffect = new ChangeBaseDPClass();
                                                        cardEffect.SetUpICardEffect("Effect Waiting", null, RootCards[i]);

                                                        skillInfoArray[i] = new SkillInfo(cardEffect, null, EffectTiming.None);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    _skillInfos = skillInfoArray.ToList();
                                }
                            }
                        }
                    }

                    #endregion

                    GManager.instance.selectCardPanel._customCountText = _customCountText;

                    yield return StartCoroutine(GManager.instance.selectCardPanel.OpenSelectCardPanel(
                        Message: _message,
                        RootCardSources: RootCards,
                        _CanTargetCondition: CanSelectCard,
                        _CanTargetCondition_ByPreSelecetedList: _canTargetCondition_ByPreSelecetedList,
                        _CanEndSelectCondition: _canEndSelectCondition,
                        _MaxCount: _maxCount,
                        _CanEndNotMax: _canEndNotMax,
                        _CanNoSelect: _canNoSelect,
                        CanLookReverseCard: _canLookReverseCard,
                        skillInfos: _skillInfos,
                        root: _root));

                    foreach (CardSource selectedCard in GManager.instance.selectCardPanel.SelectedList)
                    {
                        targetCardIDs.Add(selectedCard.CardIndex);
                    }

                    foreach (int selectedIndex in GManager.instance.selectCardPanel.SelectedIndex)
                    {
                        _slectedInexesInList.Add(selectedIndex);
                    }

                    photonView.RPC("SetTargetIndicies", RpcTarget.All, _slectedInexesInList.ToArray());
                    photonView.RPC("SetTargetCard", RpcTarget.All, targetCardIDs.ToArray());
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_customMessage_Enemy))
                {
                    GManager.instance.commandText.OpenCommandText(_customMessage_Enemy, _isDigiXros, _isAssembly);
                }
                else
                {
                    GManager.instance.commandText.OpenCommandText("The opponent is selecting cards.", _isDigiXros, _isAssembly);
                }

                #region AI

                if (GManager.instance.IsAI)
                {
                    AutoSelect();
                }

                #endregion
            }

            void AutoSelect()
            {
                List<CardSource> ValidCards = new List<CardSource>();

                foreach (CardSource cardSource in RootCardList())
                {
                    if (CanSelectCard(cardSource))
                    {
                        ValidCards.Add(cardSource);
                    }
                }

                IList<int> indexList = Enumerable.Range(0, ValidCards.Count).ToList();

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

                        if (_canEndSelectCondition != null)
                        {
                            if (!_canEndSelectCondition(GetCards))
                            {
                                continue;
                            }
                        }

                        List<int> CardIDs = new List<int>();

                        foreach (CardSource cardSource in GetCards)
                        {
                            CardIDs.Add(cardSource.CardIndex);
                        }

                        if (GManager.instance.IsAI)
                        {
                            SetTargetCard(CardIDs.ToArray());
                        }
                        else
                        {
                            photonView.RPC("SetTargetCard", RpcTarget.All, CardIDs.ToArray());
                        }

                        break;
                    }
                }

                if (GManager.instance.IsAI)
                {
                    _endSelect = true;
                }
            }

            yield return new WaitWhile(() => !_endSelect);
            _endSelect = false;

            GManager.instance.commandText.CloseCommandText();
            yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

            #region Show selected card

            if (_targetCards.Count > 0 && _showCard)
            {
                if (_targetCards.Count((cardSource) => cardSource.IsFlipped) > 0 && !_showReverseCard)
                {
                    //表示しない
                }
                else
                {
                    if (_isShowOpponent || (_selectPlayer.isYou && _targetCards.Count((cardSource) => cardSource.Owner == _selectPlayer) > 0))
                    {
                        if (!string.IsNullOrEmpty(_customMessage_ShowCard))
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(_targetCards, _customMessage_ShowCard, true, true));
                        }
                        else
                        {
                            switch (_mode)
                            {
                                case Mode.AddHand:
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(_targetCards, "Cards added to hand", true, true));
                                    break;

                                case Mode.Discard:
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(_targetCards, "Cards put on the trash", true, true));
                                    break;

                                case Mode.Custom:
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(_targetCards, "Selected Cards", true, true));
                                    break;
                            }
                        }
                    }
                }
            }

            #endregion

            Hashtable hashtable = CardEffectCommons.CardEffectHashtable(_cardEffect);

            string log = "";

            if (_mode == Mode.AddHand)
            {
                log += $"\nCard{Utils.PluralFormSuffix(_targetCards.Count)} added to hand:";
            }
            else if (_mode == Mode.AddHand)
            {
                log += $"\nTrash Card{Utils.PluralFormSuffix(_targetCards.Count)}:";
            }
            else
            {
                log += $"\nSelected Card{Utils.PluralFormSuffix(_targetCards.Count)}:";
            }

            if (_root == Root.Library)
            {
                //yield return ContinuousController.instance.StartCoroutine(CardObjectController.Shuffle(_selectPlayer));
            }

            foreach (CardSource cardSource in _targetCards)
            {
                log += $"\n{cardSource.BaseENGCardNameFromEntity}({cardSource.CardID})";
            }

            switch (_mode)
            {
                case Mode.AddHand:
                    foreach (CardSource cardSource in _targetCards)
                    {
                        cardSource.SetFace();

                        if (cardSource.IsDigiEgg) yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(new List<CardSource> { cardSource }));
                        else
                        {
                            if (cardSource.PermanentOfThisCard() != null)
                            {
                                if (cardSource.PermanentOfThisCard().DigivolutionCards.Contains(cardSource))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().RemoveDigivolveRootEffect(cardSource, cardSource.PermanentOfThisCard()));
                                }
                            }

                            handCards.Add(cardSource);
                        }
                    }
                    break;

                case Mode.Discard:
                    foreach (CardSource cardSource in _targetCards)
                    {
                        if (CardEffectCommons.IsExistOnHand(cardSource))
                        {
                            List<IDiscardHand> discardHands = _targetCards.Map(cardSource => new IDiscardHand(cardSource, hashtable));
                            yield return ContinuousController.instance.StartCoroutine(new IDiscardHands(discardHands, _cardEffect).DiscardHands());
                        }
                        else if (CardEffectCommons.IsExistLinked(cardSource))
                        {
                            yield return ContinuousController.instance.StartCoroutine(new ITrashLinkCards(
                                cardSource.PermanentOfThisCard(),
                                new List<CardSource> { cardSource },
                                _cardEffect).TrashLinkCards());
                        }
                        else
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(cardSource));
                    }
                    break;

                case Mode.Custom:
                    if (_selectCardCoroutine != null)
                    {
                        foreach (CardSource cardSource in _targetCards)
                        {
                            yield return StartCoroutine(_selectCardCoroutine(cardSource));
                        }
                    }
                    break;
            }

            if (handCards.Count >= 1)
            {
                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(handCards, false, _cardEffect));
            }

            #region ログ追加

            if (!_notAddLog)
            {
                if (_isShowOpponent || _selectPlayer.isYou)
                {
                    if (_targetCards.Count >= 1)
                    {
                        log += "\n";

                        PlayLog.OnAddLog?.Invoke(log);
                    }
                }
            }

            #endregion
        }

        if (_afterSelectCardCoroutine != null)
        {
            yield return StartCoroutine(_afterSelectCardCoroutine(_targetCards));
        }

        if (_afterSelectIndexCoroutine != null)
        {
            yield return StartCoroutine(_afterSelectIndexCoroutine(_slectedInexesInList));
        }

        GManager.instance.turnStateMachine.gameContext.IsSecurityLooking = false;

        GManager.instance.turnStateMachine.IsSelecting = oldIsSelecting;
    }

    [PunRPC]
    public void SetTargetCard(int[] CardIDs)
    {
        _targetCards = new List<CardSource>();

        foreach (int CardID in CardIDs)
        {
            _targetCards.Add(GManager.instance.turnStateMachine.gameContext.ActiveCardList[CardID]);
        }

        _endSelect = true;
    }

    [PunRPC]
    public void SetTargetIndicies(int[] CardIDs)
    {
        _slectedInexesInList = new List<int>();

        foreach (int index in CardIDs)
        {
            _slectedInexesInList.Add(index);
        }
    }
}