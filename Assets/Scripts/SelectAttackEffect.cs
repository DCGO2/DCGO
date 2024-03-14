using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectAttackEffect : MonoBehaviourPunCallbacks
{
    public void SetUp
        (
        Permanent attacker,
        Func<bool> canAttackPlayerCondition,
        Func<Permanent, bool> defenderCondition,
        ICardEffect cardEffect)
    {
        _attacker = attacker;
        _canAttackPlayerCondition = canAttackPlayerCondition;
        _defenderCondition = defenderCondition;
        _cardEffect = cardEffect;

        _canSelectNotAttack = true;
        _withoutTap = false;
        _customMessage = null;
        _customMessage_Enemy = null;
        _beforeOnAttackCoroutine = null;
    }

    public void SetUpCustomMessage(string customMessage, string customMessage_Enemy)
    {
        _customMessage = customMessage;
        _customMessage_Enemy = customMessage_Enemy;
    }

    public void SetCanNotSelectNotAttack()
    {
        _canSelectNotAttack = false;
    }

    public void SetWithoutTap()
    {
        _withoutTap = true;
    }

    public void SetBeforeOnAttackCoroutine(Func<IEnumerator> beforeOnAttackCoroutine)
    {
        _beforeOnAttackCoroutine = beforeOnAttackCoroutine;
    }

    Permanent _attacker = null;
    Func<bool> _canAttackPlayerCondition = null;
    Func<Permanent, bool> _defenderCondition = null;
    bool _canSelectNotAttack = true;
    bool _withoutTap = false;
    bool _noSelect = false;

    bool _endSelect = false;

    string _customMessage = null;
    string _customMessage_Enemy = null;

    Permanent _defender = null;
    ICardEffect _cardEffect = null;
    Func<IEnumerator> _beforeOnAttackCoroutine = null;

    #region パーマネントを選択できるか
    bool CanTarget(Permanent permanent)
    {
        if (_attacker != null)
        {
            if (_attacker.TopCard != null)
            {
                if (_attacker.CanAttack(_cardEffect, _withoutTap))
                {
                    if (_attacker.CanAttackTargetDigimon(permanent, _cardEffect, _withoutTap))
                    {
                        if (_defenderCondition != null)
                        {
                            if (!_defenderCondition(permanent))
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                }
            }

        }

        return false;
    }
    #endregion

    #region 攻撃できる相手パーマネントがいるか
    public bool CanAttackDigimon()
    {
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
        {
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                if (CanTarget(permanent))
                {
                    return true;
                }
            }
        }

        return false;
    }
    #endregion

    #region プレイヤーに攻撃が可能か
    bool CanAttackPlayer()
    {
        if (_attacker != null)
        {
            if (_attacker.TopCard != null)
            {
                if (_attacker.CanAttack(_cardEffect, _withoutTap))
                {
                    if (_attacker.CanAttackTargetDigimon(null, _cardEffect, _withoutTap))
                    {
                        if (_canAttackPlayerCondition != null)
                        {
                            if (!_canAttackPlayerCondition())
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                }
            }

        }

        return false;
    }
    #endregion

    #region 選択が可能かどうか
    public bool active()
    {
        if (!CanAttackDigimon())
        {
            if (!CanAttackPlayer())
            {
                return false;
            }
        }

        if (GManager.instance.attackProcess.IsAttacking)
        {
            return false;
        }

        return true;
    }
    #endregion

    #region 終了できるか判定
    bool CanEndSelect(Permanent selectedPermanent)
    {
        if (selectedPermanent == null)
        {
            if (!CanAttackPlayer())
            {
                return false;
            }
        }

        else
        {
            if (!CanAttackDigimon())
            {
                return false;
            }
        }

        return true;
    }
    #endregion

    public IEnumerator Activate()
    {
        _defender = null;
        _noSelect = false;

        if (_attacker == null) yield break;
        if (_attacker.TopCard == null) yield break;
        if (!_attacker.CanAttack(_cardEffect, _withoutTap)) yield break;

        if (active())
        {
            yield return GManager.instance.photonWaitController.StartWait("SelectAttackEffect");

            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
            {
                GManager.instance.turnStateMachine.OffFieldCardTarget(player);
                GManager.instance.turnStateMachine.OffHandCardTarget(player);
            }

            GManager.instance.turnStateMachine.IsSelecting = true;

            if (_attacker != null)
            {
                if (_attacker.TopCard != null)
                {
                    if (_attacker.CanAttack(_cardEffect, _withoutTap))
                    {
                        if (_attacker.TopCard.Owner.isYou)
                        {
                            #region メッセージ表示
                            if (!string.IsNullOrEmpty(_customMessage))
                            {
                                GManager.instance.commandText.OpenCommandText(_customMessage);
                            }

                            else
                            {
                                if (CanAttackDigimon())
                                {
                                    GManager.instance.commandText.OpenCommandText("Which target will you attack?");
                                }

                                else
                                {
                                    GManager.instance.commandText.OpenCommandText("Will you attack to the opponent?");
                                }
                            }

                            #endregion

                            Permanent selectedPermanent = null;

                            #region プレイヤーへの攻撃
                            if (CanAttackPlayer())
                            {
                                if (_attacker.TopCard.Owner.Enemy.SecurityCards.Count >= 1)
                                {
                                    _attacker.TopCard.Owner.Enemy.securityObject.securityBreakGlass.ShowBlueMatarial();
                                }

                                _attacker.TopCard.Owner.Enemy.securityObject.SetSecurityAttackObject();
                                _attacker.TopCard.Owner.Enemy.securityObject.SetSecurityOutline(true);

                                _attacker.TopCard.Owner.Enemy.securityObject.AddClickTarget(() =>
                                {
                                    selectedPermanent = null;
                                    EndSelect_RPC();
                                });

                            }
                            #endregion

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

                            #region 場のパーマネントがクリックされた時の処理
                            void OnClickFieldPermanentCard(FieldPermanentCard fieldPermanentCard)
                            {
                                if (selectedPermanent == fieldPermanentCard.ThisPermanent)
                                {
                                    selectedPermanent = null;
                                }

                                else
                                {
                                    selectedPermanent = fieldPermanentCard.ThisPermanent;
                                }

                                CheckEndSelect();
                            }
                            #endregion

                            #region 選択を終了できるかの判定とアウトライン表示
                            void CheckEndSelect()
                            {
                                #region 終了できるかによってUI表示
                                if (CanEndSelect(selectedPermanent))
                                {

                                }

                                else
                                {

                                }
                                #endregion

                                #region アウトライン表示
                                foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
                                {
                                    foreach (Permanent permanent in player.GetFieldPermanents())
                                    {
                                        permanent.ShowingPermanentCard.RemoveSelectEffect();

                                        if (CanTarget(permanent))
                                        {
                                            if (selectedPermanent == permanent)
                                            {
                                                permanent.ShowingPermanentCard.OnSelectEffect(1.1f);
                                                permanent.ShowingPermanentCard.SetOrangeOutline();
                                            }

                                            else
                                            {
                                                permanent.ShowingPermanentCard.OnSelectEffect(1.1f);
                                                permanent.ShowingPermanentCard.SetBlueOutline();
                                            }
                                        }
                                    }
                                }
                                #endregion

                                if (selectedPermanent == null)
                                {
                                    if (_canSelectNotAttack)
                                    {
                                        GManager.instance.BackButton.OpenSelectCommandButton("Not Attack", () => { photonView.RPC("SetNotAttack", RpcTarget.All); }, 0);
                                    }

                                    GManager.instance.selectCommandPanel.CloseSelectCommandPanel();
                                }

                                else
                                {
                                    GManager.instance.selectCommandPanel.SetUpCommandButton(new List<Command_SelectCommand>() { new Command_SelectCommand("End Selection", EndSelect_RPC, 0) });

                                    GManager.instance.BackButton.CloseSelectCommandButton();
                                }
                            }

                            #endregion

                            #region 選択終了
                            void EndSelect_RPC()
                            {
                                foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
                                {
                                    foreach (Permanent permanent in player.GetFieldPermanents())
                                    {
                                        permanent.ShowingPermanentCard.RemoveSelectEffect();
                                        permanent.ShowingPermanentCard.RemoveClickTarget();
                                    }
                                }

                                bool isPlayer = true;
                                bool isTurnPlayer = false;
                                int PermanentIndex = -1;

                                if (selectedPermanent != null)
                                {
                                    if (selectedPermanent.TopCard != null)
                                    {
                                        isPlayer = false;
                                        isTurnPlayer = selectedPermanent.TopCard.Owner == GManager.instance.turnStateMachine.gameContext.TurnPlayer;
                                        PermanentIndex = selectedPermanent.TopCard.Owner.GetFieldPermanents().IndexOf(selectedPermanent);
                                    }
                                }

                                photonView.RPC("SetAttackTarget", RpcTarget.All, isPlayer, isTurnPlayer, PermanentIndex);

                                GManager.instance.BackButton.CloseSelectCommandButton();
                            }
                            #endregion
                        }

                        else
                        {
                            #region メッセージ表示
                            if (!string.IsNullOrEmpty(_customMessage_Enemy))
                            {
                                GManager.instance.commandText.OpenCommandText(_customMessage_Enemy);
                            }

                            else
                            {
                                GManager.instance.commandText.OpenCommandText("The opponent is selecting the attack target.");
                            }
                            #endregion

                            #region AI
                            if (GManager.instance.IsAI)
                            {
                                List<Permanent> AttackcTargetCandidates = new List<Permanent>();

                                foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                                {
                                    foreach (Permanent permanent in player.GetFieldPermanents())
                                    {
                                        if (CanTarget(permanent))
                                        {
                                            AttackcTargetCandidates.Add(permanent);
                                        }
                                    }
                                }

                                Permanent selectedPermanent = null;

                                if (AttackcTargetCandidates.Count >= 1)
                                {
                                    if (!_attacker.CanAttackTargetDigimon(null, _cardEffect, _withoutTap) || (_attacker.TopCard.Owner.Enemy.SecurityCards.Count >= 3 && RandomUtility.IsSucceedProbability(0.5f)))
                                    {
                                        selectedPermanent = AttackcTargetCandidates[UnityEngine.Random.Range(0, AttackcTargetCandidates.Count)];
                                    }
                                }

                                bool isPlayer = true;
                                bool isTurnPlayer = false;
                                int PermanentIndex = -1;

                                if (selectedPermanent != null)
                                {
                                    if (selectedPermanent.TopCard != null)
                                    {
                                        isPlayer = false;
                                        isTurnPlayer = selectedPermanent.TopCard.Owner == GManager.instance.turnStateMachine.gameContext.TurnPlayer;
                                        PermanentIndex = selectedPermanent.TopCard.Owner.GetFieldPermanents().IndexOf(selectedPermanent);
                                    }
                                }

                                SetAttackTarget(isPlayer, isTurnPlayer, PermanentIndex);
                            }
                            #endregion
                        }
                    }
                }
            }

            //選択が完了するまで待機
            yield return new WaitWhile(() => !_endSelect);
            _endSelect = false;

            #region リセット
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
            {
                GManager.instance.turnStateMachine.OffFieldCardTarget(player);
                GManager.instance.turnStateMachine.OffHandCardTarget(player);

                foreach (Permanent permanent in player.GetFieldPermanents())
                {
                    permanent.ShowingPermanentCard.RemoveSelectEffect();
                }

                player.securityObject.OffShowSecurityAttackObject();
                player.securityObject.RemoveClickTarget();

                if (_defender != null || _noSelect)
                {
                    player.securityObject.securityBreakGlass.gameObject.SetActive(false);
                }
            }

            GManager.instance.hideCannotSelectObject.Close();

            GManager.instance.selectCommandPanel.CloseSelectCommandPanel();
            GManager.instance.BackButton.CloseSelectCommandButton();

            GManager.instance.commandText.CloseCommandText();
            yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

            #endregion

            if (!_noSelect)
            {
                if (CanEndSelect(_defender))
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.Attack(_attacker, _defender, _cardEffect, _withoutTap, _beforeOnAttackCoroutine));
                }
            }
        }
    }

    #region 選択決定
    [PunRPC]
    public void SetAttackTarget(bool isPlayer, bool isTurnPlayer, int PermanentIndex)
    {
        _defender = null;

        if (!isPlayer)
        {
            Player player = null;

            if (isTurnPlayer)
            {
                player = GManager.instance.turnStateMachine.gameContext.TurnPlayer;
            }

            else
            {
                player = GManager.instance.turnStateMachine.gameContext.NonTurnPlayer;
            }

            Permanent permanent = null;

            if (0 <= PermanentIndex && PermanentIndex <= player.GetFieldPermanents().Count - 1)
            {
                permanent = player.GetFieldPermanents()[PermanentIndex];
            }

            if (permanent != null)
            {
                _defender = permanent;
            }
        }

        _endSelect = true;
    }
    #endregion

    #region 選択しない
    [PunRPC]
    public void SetNotAttack()
    {
        GManager.instance.selectCommandPanel.CloseSelectCommandPanel();

        _defender = null;

        _noSelect = true;

        _endSelect = true;
    }
    #endregion
}
