using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class SelectPermanentEffect : MonoBehaviourPunCallbacks
{
    public void SetUp
        (Player selectPlayer,
        Func<Permanent, bool> canTargetCondition,
        Func<List<Permanent>, Permanent, bool> canTargetCondition_ByPreSelecetedList,
        Func<List<Permanent>, bool> canEndSelectCondition,
        int maxCount,
        bool canNoSelect,
        bool canEndNotMax,
        Func<Permanent, IEnumerator> selectPermanentCoroutine,
        Func<List<Permanent>, IEnumerator> afterSelectPermanentCoroutine,
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
        _selectPermanentCoroutine = selectPermanentCoroutine;
        _afterSelectPermanentCoroutine = afterSelectPermanentCoroutine;
        _mode = mode;
        _cardEffect = cardEffect;

        _isLocal = false;
        _isdigiXros = false;

        _customMessage = null;
        _customMessage_Enemy = null;

        _customBackButtonMessage = null;
    }

    public void SetIsLocal()
    {
        _isLocal = true;
    }

    public void SetDigiXros()
    {
        _isdigiXros = true;
    }

    public void SetUpCustomMessage(string CustomMessage = "", string CustomMessage_Enemy = "", string[] customMessageArray = null)
    {
        _customMessage = CustomMessage;
        _customMessage_Enemy = CustomMessage_Enemy;

        if (customMessageArray != null)
        {
            if (customMessageArray.Length == 2)
            {
                _customMessage = customMessageArray[0];
                _customMessage_Enemy = customMessageArray[1];
            }
        }
    }

    public void SetUpCustomBackButtonMessage(string CustomBackButtonMessage)
    {
        _customBackButtonMessage = CustomBackButtonMessage;
    }

    //Player to select
    Player _selectPlayer = null;
    //Conditions of units that can be selected
    Func<Permanent, bool> _canTargetCondition = null;
    //Whether the unit can be selected with the current selection list status
    Func<List<Permanent>, Permanent, bool> _canTargetCondition_ByPreSelecetedList = null;
    //Conditions under which a selection can be terminated (see list of selection termination points)
    Func<List<Permanent>, bool> _canEndSelectCondition = null;
    //Maximum number of sheets to be selected
    int _maxCount = 0;
    //Whether you can choose not to choose
    bool _canNoSelect = false;
    //Can you finish your selection with less than the maximum number?
    bool _canEndNotMax = false;
    //(Limited to Mode.Custom) Processing to be performed by selecting
    Func<Permanent, IEnumerator> _selectPermanentCoroutine = null;
    //選択した後にする処理
    Func<List<Permanent>, IEnumerator> _afterSelectPermanentCoroutine = null;
    //Classification of processing to be done by selection
    Mode _mode = Mode.Custom;
    //Skill in making unit selections
    ICardEffect _cardEffect = null;
    bool _isLocal = false;
    bool _isdigiXros = false;

    public enum Mode
    {
        Tap,
        UnTap,
        Destroy,
        Bounce,
        PutLibraryBottom,
        Custom
    }

    //Selected unit list
    List<Permanent> _targetPermanents = new List<Permanent>();
    //No Selection Flag
    public bool _noSelect = false;

    bool _endSelect = false;

    string _customMessage = null;
    string _customMessage_Enemy = null;

    string _customBackButtonMessage = null;
    bool CanTarget(Permanent permanent)
    {
        if (_cardEffect != null)
        {
            if (permanent.TopCard != null)
            {
                if (_cardEffect.EffectSourceCard != null)
                {
                    if (permanent.TopCard.Owner != _cardEffect.EffectSourceCard.Owner && permanent.TopCard.Owner != _selectPlayer)
                    {
                        if (!permanent.CanSelectBySkill(_cardEffect))
                        {
                            return false;
                        }
                    }
                }
            }
        }

        if (_canTargetCondition != null)
        {
            if (_canTargetCondition(permanent))
            {
                return !permanent.TopCard.IsFlipped;
            }
        }

        return false;
    }

    #region 選択が可能かどうか
    public bool active()
    {
        List<Permanent> CanSelectedPermanets = new List<Permanent>();

        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
        {
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                if (CanTarget(permanent))
                {
                    CanSelectedPermanets.Add(permanent);
                }
            }
        }

        if (CanSelectedPermanets.Count == 0)
        {
            return false;
        }

        if (!_canNoSelect && !_canEndNotMax)
        {
            if (CanSelectedPermanets.Count < _maxCount)
            {
                return false;
            }

            List<Permanent[]> permanentsList = ParameterComparer.Enumerate(CanSelectedPermanets, _maxCount).ToList();

            if (permanentsList.Count((permanents) => CanEndSelect(permanents.ToList())) == 0)
            {
                return false;
            }
        }

        return true;
    }
    #endregion

    #region 終了できるか判定
    bool CanEndSelect(List<Permanent> permanents)
    {
        //選択枚数が必要枚数に達していない場合
        if (!(permanents.Count == _maxCount || (permanents.Count <= _maxCount && _canEndNotMax)))
        {
            return false;
        }

        //特定の条件により失敗
        if (_canEndSelectCondition != null)
        {
            if (!_canEndSelectCondition(permanents))
            {
                return false;
            }
        }

        return true;
    }
    #endregion

    public IEnumerator Activate()
    {
        bool oldIsSelecting = GManager.instance.turnStateMachine.IsSelecting;

        _targetPermanents = new List<Permanent>();

        List<Permanent> destroyPermanents = new List<Permanent>();
        List<Permanent> tapPermanents = new List<Permanent>();
        List<Permanent> untapPermanents = new List<Permanent>();
        List<Permanent> libraryBottomPermanents = new List<Permanent>();
        List<Permanent> handBouncePermanents = new List<Permanent>();

        _noSelect = false;

        if (active())
        {
            #region Show trash cards
            if (_cardEffect != null)
            {
                if (_cardEffect.EffectSourceCard != null)
                {
                    if (_cardEffect.EffectSourceCard.Owner.TrashCards.Contains(_cardEffect.EffectSourceCard) || _cardEffect.EffectSourceCard.Owner.LostCards.Contains(_cardEffect.EffectSourceCard))
                    {
                        if (_cardEffect.EffectSourceCard.Owner.TrashHandCard != null)
                        {
                            if (!_cardEffect.EffectSourceCard.Owner.TrashHandCard.gameObject.activeSelf)
                            {
                                _cardEffect.EffectSourceCard.Owner.TrashHandCard.gameObject.SetActive(true);
                                _cardEffect.EffectSourceCard.Owner.TrashHandCard.SetUpHandCard(_cardEffect.EffectSourceCard);
                                _cardEffect.EffectSourceCard.Owner.TrashHandCard.SetUpHandCardImage();
                                _cardEffect.EffectSourceCard.Owner.TrashHandCard.OnOutline();
                                _cardEffect.EffectSourceCard.Owner.TrashHandCard.SetBlueOutline();
                                _cardEffect.EffectSourceCard.Owner.TrashHandCard.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
                            }
                        }
                    }
                }
            }
            #endregion

            if (!_isLocal)
            {
                yield return GManager.instance.photonWaitController.StartWait("SelectPermanentEffect");
            }

            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
            {
                GManager.instance.turnStateMachine.OffFieldCardTarget(player);
                GManager.instance.turnStateMachine.OffHandCardTarget(player);
            }

            GManager.instance.turnStateMachine.IsSelecting = true;

            if (_selectPlayer.isYou)
            {
                #region Message display
                if (!string.IsNullOrEmpty(_customMessage))
                {
                    GManager.instance.commandText.OpenCommandText(_customMessage, _isdigiXros);
                }

                else
                {
                    string message = "";

                    switch (_mode)
                    {
                        case Mode.Tap:
                            message = "Select cards to suspend.";
                            break;

                        case Mode.UnTap:
                            message = "Select cards to unsuspend.";
                            break;

                        case Mode.Destroy:
                            message = "Select cards to delete.";
                            break;

                        case Mode.Bounce:
                            message = "Select cards to return to hand.";
                            break;

                        case Mode.PutLibraryBottom:
                            message = "Select cards to put on bottom of the deck.";
                            break;

                        case Mode.Custom:
                            message = "Select cards.";
                            break;
                    }

                    if (!string.IsNullOrEmpty(message))
                    {
                        GManager.instance.commandText.OpenCommandText(message, _isdigiXros);
                    }
                }

                #endregion

                List<Permanent> PreSelectedPermanents = new List<Permanent>();

                bool forcesSelection = false;

                #region forced selection
                if (!_canNoSelect && !_canEndNotMax)
                {
                    int canSelectCount = 0;

                    foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
                    {
                        foreach (Permanent chara in player.GetFieldPermanents())
                        {
                            if (CanTarget(chara))
                            {
                                canSelectCount++;
                            }
                        }
                    }

                    if (canSelectCount == _maxCount)
                    {
                        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
                        {
                            foreach (Permanent chara in player.GetFieldPermanents())
                            {
                                if (CanTarget(chara))
                                {
                                    PreSelectedPermanents.Add(chara);
                                }
                            }
                        }

                        forcesSelection = true;

                        EndSelect_RPC();
                    }
                }
                #endregion

                if (!forcesSelection)
                {
                    GManager.instance.sideBar.SetUpSideBar();

                    List<FieldPermanentCard> candidates = new List<FieldPermanentCard>();

                    foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
                    {
                        foreach (Permanent permanent in player.GetFieldPermanents())
                        {
                            if (CanTarget(permanent))
                            {
                                permanent.ShowingPermanentCard.AddClickTarget(OnClickFieldPermanentCard);
                                candidates.Add(permanent.ShowingPermanentCard);
                            }
                        }
                    }

                    if (candidates.Count >= 1)
                    {
                        GManager.instance.hideCannotSelectObject.SetUpHideCannotSelectObject(candidates, false);
                    }

                    CheckEndSelect();

                    #region Processing when a permanent on the field is clicked
                    void OnClickFieldPermanentCard(FieldPermanentCard feldPermanentCard)
                    {
                        if (PreSelectedPermanents.Contains(feldPermanentCard.ThisPermanent))
                        {
                            PreSelectedPermanents.Remove(feldPermanentCard.ThisPermanent);
                        }

                        else
                        {
                            bool CanNotSelected = false;

                            if (_canTargetCondition_ByPreSelecetedList != null)
                            {
                                if (!_canTargetCondition_ByPreSelecetedList(PreSelectedPermanents, feldPermanentCard.ThisPermanent))
                                {
                                    CanNotSelected = true;
                                }
                            }

                            if (CanNotSelected)
                            {
                                return;
                            }

                            if (PreSelectedPermanents.Count < _maxCount)
                            {
                                PreSelectedPermanents.Add(feldPermanentCard.ThisPermanent);
                            }

                            else
                            {
                                if (PreSelectedPermanents.Count > 0)
                                {
                                    PreSelectedPermanents.RemoveAt(PreSelectedPermanents.Count - 1);
                                    PreSelectedPermanents.Add(feldPermanentCard.ThisPermanent);
                                }
                            }

                            if (!ContinuousController.instance.checkBeforeEndingSelection
                            && !_canNoSelect
                            && !_canEndNotMax
                            && _maxCount == PreSelectedPermanents.Count)
                            {
                                EndSelect_RPC();
                                return;
                            }
                        }

                        CheckEndSelect();
                    }
                    #endregion

                    #region Determining whether the selection can be completed and displaying the outline
                    void CheckEndSelect()
                    {
                        #region UI display depending on whether it can be terminated
                        if (CanEndSelect(PreSelectedPermanents))
                        {
                            GManager.instance.selectCommandPanel.SetUpCommandButton(new List<Command_SelectCommand>()
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

                        #region Outline display by selection list
                        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
                        {
                            foreach (Permanent permanent in player.GetFieldPermanents())
                            {
                                permanent.ShowingPermanentCard.RemoveSelectEffect();

                                if (CanTarget(permanent))
                                {
                                    if (PreSelectedPermanents.Contains(permanent))
                                    {
                                        permanent.ShowingPermanentCard.OnSelectEffect(1.1f);
                                        permanent.ShowingPermanentCard.SetOrangeOutline();
                                    }

                                    else
                                    {
                                        permanent.ShowingPermanentCard.OnSelectEffect(1.1f);
                                        permanent.ShowingPermanentCard.SetBlueOutline();

                                        bool CanNotSelected = false;

                                        if (_canTargetCondition_ByPreSelecetedList != null)
                                        {
                                            if (!_canTargetCondition_ByPreSelecetedList(PreSelectedPermanents, permanent))
                                            {
                                                CanNotSelected = true;
                                            }
                                        }

                                        if (CanNotSelected)
                                        {
                                            permanent.ShowingPermanentCard.RemoveSelectEffect();
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        if (PreSelectedPermanents.Count >= 1)
                        {
                            GManager.instance.BackButton.CloseSelectCommandButton();
                        }

                        else
                        {
                            if (_canNoSelect)
                            {
                                string backButtonMessage = "No Selection";

                                if (!string.IsNullOrEmpty(_customBackButtonMessage))
                                {
                                    backButtonMessage = _customBackButtonMessage;
                                }

                                GManager.instance.BackButton.OpenSelectCommandButton(backButtonMessage, () => NoSelect_RPC(), 0);

                                void NoSelect_RPC()
                                {
                                    if (!_isLocal)
                                    {
                                        photonView.RPC("SetNoSelectChara", RpcTarget.All);
                                    }

                                    else
                                    {
                                        SetNoSelectChara();
                                    }
                                }
                            }
                        }

                        GManager.instance.sideBar.SetUpSideBar();
                    }

                    #endregion
                }

                #region Selection finished
                void EndSelect_RPC()
                {
                    foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
                    {
                        foreach (Permanent unit in player.GetFieldPermanents())
                        {
                            unit.ShowingPermanentCard.RemoveSelectEffect();
                            unit.ShowingPermanentCard.RemoveClickTarget();
                        }
                    }

                    List<bool> isTurnPlayer = new List<bool>();
                    List<int> CharaIndex = new List<int>();

                    foreach (Permanent chara in PreSelectedPermanents)
                    {
                        isTurnPlayer.Add(chara.TopCard.Owner == GManager.instance.turnStateMachine.gameContext.TurnPlayer);
                        CharaIndex.Add(chara.TopCard.Owner.GetFieldPermanents().IndexOf(chara));
                    }

                    if (!_isLocal)
                    {
                        photonView.RPC("SetTargetFrames", RpcTarget.All, isTurnPlayer.ToArray(), CharaIndex.ToArray());
                    }

                    else
                    {
                        SetTargetFrames(isTurnPlayer.ToArray(), CharaIndex.ToArray());
                    }

                    GManager.instance.BackButton.CloseSelectCommandButton();
                }
                #endregion
            }

            else
            {
                #region Message display
                if (!string.IsNullOrEmpty(_customMessage_Enemy))
                {
                    GManager.instance.commandText.OpenCommandText(_customMessage_Enemy, _isdigiXros);
                }

                else
                {
                    GManager.instance.commandText.OpenCommandText("The opponent is selecting cards.", _isdigiXros);
                }
                #endregion

                #region AI
                if (GManager.instance.IsAI)
                {
                    List<Permanent> ValidCharas = new List<Permanent>();

                    foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                    {
                        foreach (Permanent unit in player.GetFieldPermanents())
                        {
                            if (_canTargetCondition(unit))
                            {
                                ValidCharas.Add(unit);
                            }
                        }
                    }

                    IList<int> indexList = Enumerable.Range(0, ValidCharas.Count).ToList();

                    if (ValidCharas.Count >= _maxCount)
                    {
                        for (int i = 0; i < 200; i++)
                        {
                            List<int> GetIndexes = indexList.GetRandom(_maxCount).ToList();

                            List<Permanent> GetCharas = new List<Permanent>();

                            foreach (int index in GetIndexes)
                            {
                                GetCharas.Add(ValidCharas[index]);
                            }

                            if (_canEndSelectCondition != null)
                            {
                                if (!_canEndSelectCondition(GetCharas))
                                {
                                    continue;
                                }
                            }

                            List<bool> isTurnPlayer = new List<bool>();
                            List<int> UnitIDs = new List<int>();

                            foreach (Permanent chara in GetCharas)
                            {
                                if (chara.TopCard != null)
                                {
                                    isTurnPlayer.Add(chara.TopCard.Owner == GManager.instance.turnStateMachine.gameContext.TurnPlayer);
                                    UnitIDs.Add(chara.TopCard.Owner.GetFieldPermanents().IndexOf(chara));
                                }
                            }

                            SetTargetFrames(isTurnPlayer.ToArray(), UnitIDs.ToArray());
                            break;
                        }
                    }

                    _endSelect = true;
                }
                #endregion
            }

            //Wait until selection is complete
                        yield return new WaitWhile(() => !_endSelect);
            _endSelect = false;

            #region reset
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
            {
                GManager.instance.turnStateMachine.OffFieldCardTarget(player);
                GManager.instance.turnStateMachine.OffHandCardTarget(player);

                foreach (Permanent chara in player.GetFieldPermanents())
                {
                    chara.ShowingPermanentCard.RemoveSelectEffect();
                }
            }

            GManager.instance.hideCannotSelectObject.Close();

            GManager.instance.selectCommandPanel.CloseSelectCommandPanel();
            GManager.instance.BackButton.CloseSelectCommandButton();

            GManager.instance.commandText.CloseCommandText();
            yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

            GManager.instance.sideBar.OffSideBar();
            #endregion

            if (CanEndSelect(_targetPermanents))
            {
                Hashtable hashtable = new Hashtable();
                hashtable.Add("CardEffect", _cardEffect);

                if (!_noSelect)
                {
                    string log = "";

                    log += $"\nSelected Cards:";

                    foreach (Permanent targetPermanent in _targetPermanents)
                    {
                        log += $"\n{targetPermanent.TopCard.BaseENGCardNameFromEntity}({targetPermanent.TopCard.CardID})";

                        FieldPermanentCard fieldPermanentCard = targetPermanent.ShowingPermanentCard;

                        fieldPermanentCard.OnSelectEffect(1.1f);

                        #region target arrow display
                        if (_cardEffect != null)
                        {
                            if (_cardEffect.EffectSourceCard != null)
                            {
                                if (_cardEffect.EffectSourceCard.PermanentOfThisCard() == null)
                                {
                                    #region processing area card
                                    if (_cardEffect.EffectSourceCard.Owner.ExecutingCards.Contains(_cardEffect.EffectSourceCard) || _cardEffect.EffectSourceCard.Owner.brainStormObject.BrainStormHandCards.Count((handCard) => handCard.cardSource == _cardEffect.EffectSourceCard && handCard.gameObject.activeSelf) >= 1)
                                    {
                                        if (_cardEffect.EffectSourceCard.Owner.isYou)
                                        {
                                            yield return GManager.instance.OnTargetArrow(
                                            new Vector3(-840, -100, 0),
                                            fieldPermanentCard.GetLocalCanvasPosition() + fieldPermanentCard.ThisPermanent.TopCard.Owner.playerUIObjectParent.localPosition,
                                            null,
                                            null);
                                        }

                                        else
                                        {
                                            yield return GManager.instance.OnTargetArrow(
                                            new Vector3(810, 240, 0),
                                            fieldPermanentCard.GetLocalCanvasPosition() + fieldPermanentCard.ThisPermanent.TopCard.Owner.playerUIObjectParent.localPosition,
                                            null,
                                            null);
                                        }
                                    }
                                    #endregion

                                    #region Cards in the trash or lost zone or deck
                                    else if (_cardEffect.EffectSourceCard.Owner.TrashCards.Contains(_cardEffect.EffectSourceCard) || _cardEffect.EffectSourceCard.Owner.LostCards.Contains(_cardEffect.EffectSourceCard) || _cardEffect.EffectSourceCard.Owner.LibraryCards.Contains(_cardEffect.EffectSourceCard))
                                    {
                                        if (_cardEffect.EffectSourceCard.Owner.isYou)
                                        {
                                            yield return GManager.instance.OnTargetArrow(
                                            new Vector3(725, -220, 0),
                                            fieldPermanentCard.GetLocalCanvasPosition() + fieldPermanentCard.ThisPermanent.TopCard.Owner.playerUIObjectParent.localPosition,
                                            null,
                                            null);
                                        }

                                        else
                                        {
                                            yield return GManager.instance.OnTargetArrow(
                                            new Vector3(-684, 287, 0),
                                            fieldPermanentCard.GetLocalCanvasPosition() + fieldPermanentCard.ThisPermanent.TopCard.Owner.playerUIObjectParent.localPosition,
                                            null,
                                            null);
                                        }
                                    }
                                    #endregion

                                    #region cards in hand
                                    else if (_cardEffect.EffectSourceCard.Owner.HandCards.Contains(_cardEffect.EffectSourceCard) && (_cardEffect.IsDeclarative || _cardEffect.EffectName.Contains("Digisorption")))
                                    {

                                        if (_cardEffect.EffectSourceCard.Owner.isYou)
                                        {
                                            yield return GManager.instance.OnTargetArrow(
                                            new Vector3(0, -380, 0),
                                            fieldPermanentCard.GetLocalCanvasPosition() + fieldPermanentCard.ThisPermanent.TopCard.Owner.playerUIObjectParent.localPosition,
                                            null,
                                            null);
                                        }

                                        else
                                        {
                                            yield return GManager.instance.OnTargetArrow(
                                            new Vector3(0, 480, 0),
                                            fieldPermanentCard.GetLocalCanvasPosition() + fieldPermanentCard.ThisPermanent.TopCard.Owner.playerUIObjectParent.localPosition,
                                            null,
                                            null);
                                        }

                                    }
                                    #endregion

                                    #region Other area cards
                                    else
                                    {

                                    }
                                    #endregion
                                }

                                else
                                {
                                    #region character card in place
                                    if (_cardEffect.EffectSourceCard.PermanentOfThisCard().ShowingPermanentCard != null)
                                    {
                                        yield return GManager.instance.OnTargetArrow(
                                            _cardEffect.EffectSourceCard.PermanentOfThisCard().ShowingPermanentCard.GetLocalCanvasPosition() + _cardEffect.EffectSourceCard.Owner.playerUIObjectParent.localPosition,
                                            fieldPermanentCard.GetLocalCanvasPosition() + fieldPermanentCard.ThisPermanent.TopCard.Owner.playerUIObjectParent.localPosition,
                                            _cardEffect.EffectSourceCard.PermanentOfThisCard().ShowingPermanentCard,
                                            fieldPermanentCard);
                                    }
                                    #endregion
                                }
                            }
                        }

                        yield return new WaitForSeconds(0.2f);
                        #endregion

                        #region Perform processing on the selected unit
                        if (_selectPermanentCoroutine != null)
                        {
                            yield return StartCoroutine(_selectPermanentCoroutine(targetPermanent));
                        }

                        switch (_mode)
                        {
                            case Mode.Tap:

                                tapPermanents.Add(targetPermanent);
                                break;

                            case Mode.UnTap:

                                untapPermanents.Add(targetPermanent);
                                break;

                            case Mode.Destroy:

                                destroyPermanents.Add(targetPermanent);
                                break;

                            case Mode.Bounce:

                                handBouncePermanents.Add(targetPermanent);
                                break;

                            case Mode.PutLibraryBottom:

                                libraryBottomPermanents.Add(targetPermanent);
                                break;
                        }
                        #endregion

                        for (int i = 0; i < 1; i++)
                        {
                            GManager.instance.OffTargetArrow();
                            yield return new WaitForSeconds(Time.deltaTime);
                        }

                        if (fieldPermanentCard != null)
                        {
                            fieldPermanentCard.RemoveSelectEffect();
                        }
                    }

                    #region Add log
                    if (_targetPermanents.Count >= 1)
                    {
                        log += "\n";

                        PlayLog.OnAddLog?.Invoke(log);
                    }
                    #endregion

                    if (destroyPermanents.Count > 0)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(destroyPermanents, hashtable).Destroy());
                    }

                    if (tapPermanents.Count > 0)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(tapPermanents, hashtable).Tap());
                    }

                    if (untapPermanents.Count > 0)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(untapPermanents, _cardEffect).Unsuspend());
                    }

                    if (libraryBottomPermanents.Count > 0)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(libraryBottomPermanents, hashtable).DeckBounce());
                    }

                    if (handBouncePermanents.Count > 0)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new HandBounceClaass(handBouncePermanents, hashtable).Bounce());
                    }
                }
            }
        }

        if (_afterSelectPermanentCoroutine != null)
        {
            yield return StartCoroutine(_afterSelectPermanentCoroutine(_targetPermanents));
        }

        GManager.instance.turnStateMachine.IsSelecting = oldIsSelecting;
    }

    #region 選択決定
    [PunRPC]
    public void SetTargetFrames(bool[] isTurnPlayer, int[] UnitIndex)
    {
        _targetPermanents = new List<Permanent>();

        for (int i = 0; i < isTurnPlayer.Length; i++)
        {
            Player player = null;

            if (isTurnPlayer[i])
            {
                player = GManager.instance.turnStateMachine.gameContext.TurnPlayer;
            }

            else
            {
                player = GManager.instance.turnStateMachine.gameContext.NonTurnPlayer;
            }

            Permanent chara = player.GetFieldPermanents()[UnitIndex[i]];

            _targetPermanents.Add(chara);
        }

        _endSelect = true;
    }
    #endregion

    #region 選択しない
    [PunRPC]
    public void SetNoSelectChara()
    {
        GManager.instance.selectCommandPanel.CloseSelectCommandPanel();

        _targetPermanents = new List<Permanent>();

        _noSelect = true;

        _endSelect = true;
    }
    #endregion
}
