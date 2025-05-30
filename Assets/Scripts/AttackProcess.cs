using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using System.Linq;
using UnityEngine.UI;

public class AttackProcess : MonoBehaviourPunCallbacks
{
    public Permanent AttackingPermanent { get; private set; } = null;
    public Permanent DefendingPermanent { get; private set; } = null;
    public int AttackCount { get; set; } = 0;
    public bool IsAttacking { get; set; } = false;
    public bool HasDefender { get; set; } = false;
    public bool IsBlocking { get; set; } = false;
    public CardSource SecurityDigimon { get; set; } = null;
    public bool DoSecurityCheck { get; set; } = false;
    public bool IsEndAttack { get; set; } = false;

    public bool UsedBlitz { get; set; } = false;

    void SetAttackerDefender(Permanent attackingPermanent, Permanent defendingPermanent)
    {
        AttackingPermanent = attackingPermanent;
        DefendingPermanent = defendingPermanent;

        if (defendingPermanent != null)
            HasDefender = true;
    }

    public IEnumerator Attack(Permanent attackingPermanent, Permanent defendingPermanent, ICardEffect attackEffect, bool withoutTap = false, Func<IEnumerator> beforeOnAttackCoroutine = null)
    {
        if (IsAttacking)
        {
            if (beforeOnAttackCoroutine != null)
            {
                yield return ContinuousController.instance.StartCoroutine(beforeOnAttackCoroutine());
            }

            yield break;
        }

        AttackingPermanent = null;
        DefendingPermanent = null;

        DoSecurityCheck = false;
        IsBlocking = false;
        SecurityDigimon = null;
        IsAttacking = false;
        HasDefender = false;
        IsEndAttack = false;

        SetAttackerDefender(attackingPermanent, defendingPermanent);

        Hashtable effectHashtable = CardEffectCommons.OnAttackCheckHashtableOfPermanent(AttackingPermanent, attackEffect);
        Hashtable counterEffectHashtable = CardEffectCommons.OnAttackCheckHashtableOfPermanent(new Permanent(AttackingPermanent.cardSources), attackEffect);

        IsAttacking = true;

        // check timing
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.AutoProcessCheck());

        GManager.instance.turnStateMachine.IsSelecting = true;

        // force to end attack
        if (IsEndAttack)
        {
            goto EndAttack;
        }

        #region Attack Process
        if (CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(AttackingPermanent))
        {
            AttackCount++;

            GManager.instance.turnStateMachine.IsSelecting = true;

            if (DefendingPermanent != null)
            {
                if (CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(DefendingPermanent))
                {
                    DefendingPermanent.ShowingPermanentCard.SetOrangeOutline();
                    DefendingPermanent.ShowingPermanentCard.Outline_Select.gameObject.SetActive(true);
                    GManager.instance.turnStateMachine.gameContext.NonTurnPlayer.securityObject.securityBreakGlass.gameObject.SetActive(false);
                }
            }

            else
            {
                if (GManager.instance.turnStateMachine.gameContext.NonTurnPlayer.SecurityCards.Count >= 1)
                {
                    GManager.instance.turnStateMachine.gameContext.NonTurnPlayer.securityObject.securityBreakGlass.ShowBlueMatarial();
                }
            }

            AttackingPermanent.ShowingPermanentCard.SetOrangeOutline();
            AttackingPermanent.ShowingPermanentCard.Outline_Select.gameObject.SetActive(true);

            // add log
            string log = $"\nAttack:\n{AttackingPermanent.TopCard.BaseENGCardNameFromEntity}({AttackingPermanent.TopCard.CardID})";

            if (DefendingPermanent != null)
            {
                if (CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(DefendingPermanent))
                    log += $"\nÅ´\n{DefendingPermanent.TopCard.BaseENGCardNameFromEntity}({DefendingPermanent.TopCard.CardID})\n";
            }

            else
            {
                log += $"\nÅ´\nSecurity\n";
            }

            PlayLog.OnAddLog?.Invoke(log);

            List<SkillInfo> stackedSkillInfos = new List<SkillInfo>();

            if (GManager.instance.autoProcessing.executingMultipleSkills != null)
            {
                if (GManager.instance.autoProcessing.executingMultipleSkills.StackedSkillInfos.Count >= 1)
                {
                    foreach (SkillInfo skillInfo in GManager.instance.autoProcessing.executingMultipleSkills.StackedSkillInfos)
                    {
                        stackedSkillInfos.Add(skillInfo);
                    }
                }

                foreach (SkillInfo skillInfo in stackedSkillInfos)
                {
                    GManager.instance.autoProcessing.executingMultipleSkills.StackedSkillInfos.Remove(skillInfo);
                }
            }

            // suspend
            if (!withoutTap)
            {
                Hashtable attackerTapHashtable = new Hashtable()
                {
                    {"IsAttack", true}
                };
                yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(
                    new List<Permanent>() { AttackingPermanent },
                    attackerTapHashtable).Tap());
            }

            // target arrow
            if (DefendingPermanent == null)
            {
                yield return GManager.instance.OnTargetArrow(
                        AttackingPermanent.PermanentFrame.GetLocalCanvasPosition() + AttackingPermanent.TopCard.Owner.playerUIObjectParent.localPosition,
                        GManager.instance.turnStateMachine.gameContext.NonTurnPlayer.SecurityAttackLocalCanvasPosition + GManager.instance.turnStateMachine.gameContext.NonTurnPlayer.playerUIObjectParent.localPosition,
                        null,
                        null);
            }
            else
            {
                if (CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(DefendingPermanent))
                {
                    yield return GManager.instance.OnTargetArrow(
                        AttackingPermanent.PermanentFrame.GetLocalCanvasPosition() + AttackingPermanent.TopCard.Owner.playerUIObjectParent.localPosition,
                        DefendingPermanent.PermanentFrame.GetLocalCanvasPosition() + DefendingPermanent.TopCard.Owner.playerUIObjectParent.localPosition,
                        null,
                        null);
                }
            }

            // activate cutin effects
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.RuleProcess());
            //yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.TriggeredSkillProcess(true, null));

            // callback that is processed before [On Attack]
            if (beforeOnAttackCoroutine != null)
            {
                yield return ContinuousController.instance.StartCoroutine(beforeOnAttackCoroutine());
            }

            // trigger [On Attack] effect
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.StackSkillInfos(
                effectHashtable,
                EffectTiming.OnAllyAttack));

            // activate cutin effects
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.RuleProcess());
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.TriggeredSkillProcess(true, null));

            if (stackedSkillInfos.Count >= 1)
            {
                foreach (SkillInfo skillInfo in stackedSkillInfos)
                {
                    GManager.instance.autoProcessing.PutStackedSkill(skillInfo);
                }
            }

            GManager.instance.turnStateMachine.IsSelecting = true;

            if (AttackingPermanent.TopCard != null)
            {
                AttackingPermanent.ShowingPermanentCard.SetOrangeOutline();
                AttackingPermanent.ShowingPermanentCard.Outline_Select.gameObject.SetActive(true);
            }

            if (DefendingPermanent != null)
            {
                if (CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(DefendingPermanent))
                {
                    DefendingPermanent.ShowingPermanentCard.SetOrangeOutline();
                    DefendingPermanent.ShowingPermanentCard.Outline_Select.gameObject.SetActive(true);
                }
            }

            // check timing
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.AutoProcessCheck());

            GManager.instance.turnStateMachine.IsSelecting = true;

            // force to end attack
            if (IsEndAttack)
            {
                goto EndAttack;
            }

            if (AttackingPermanent.TopCard != null)
            {
                AttackingPermanent.ShowingPermanentCard.SetOrangeOutline();
                AttackingPermanent.ShowingPermanentCard.Outline_Select.gameObject.SetActive(true);
            }

            if (DefendingPermanent != null)
            {
                if (CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(DefendingPermanent))
                {
                    DefendingPermanent.ShowingPermanentCard.SetOrangeOutline();
                    DefendingPermanent.ShowingPermanentCard.Outline_Select.gameObject.SetActive(true);
                }
            }

            // trigger effects when counter timing (except [Counter] effects)
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.StackSkillInfos(
                counterEffectHashtable,
                EffectTiming.OnCounterTiming,
                cardEffect => !cardEffect.IsCounterEffect));

            // activate cutin effects
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.RuleProcess());
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.TriggeredSkillProcess(true, null));

            GManager.instance.turnStateMachine.IsSelecting = true;

            // force to end attack
            if (IsEndAttack)
            {
                goto EndAttack;
            }

            // trigger effects when counter timing ([Counter] effects)
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.StackSkillInfos(
                counterEffectHashtable,
                EffectTiming.OnCounterTiming,
                cardEffect => cardEffect.IsCounterEffect));

            bool HasCounterEffect(List<SkillInfo> skillInfos, SkillInfo skillInfo)
            {
                return skillInfos.Count((skillInfo1) => skillInfo1.CardEffect.IsCounterEffect) >= 1;
            }

            // activate cutin effects
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.RuleProcess());
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.TriggeredSkillProcess(true, HasCounterEffect));

            GManager.instance.turnStateMachine.IsSelecting = true;

            // force to end attack
            if (IsEndAttack)
            {
                goto EndAttack;
            }

            if (AttackingPermanent.TopCard == null || !AttackingPermanent.IsDigimon)
            {
                goto EndAttack;
            }

            AttackingPermanent.ShowingPermanentCard.SetOrangeOutline();
            AttackingPermanent.ShowingPermanentCard.Outline_Select.gameObject.SetActive(true);

            if (DefendingPermanent != null)
            {
                if (CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(DefendingPermanent))
                {
                    DefendingPermanent.ShowingPermanentCard.SetOrangeOutline();
                    DefendingPermanent.ShowingPermanentCard.Outline_Select.gameObject.SetActive(true);
                }
            }

            #region select blocker
            bool CanSelectBlockerCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, AttackingPermanent.TopCard))
                {
                    if (permanent != DefendingPermanent)
                    {
                        if (permanent.HasBlocker && permanent.CanBlock(AttackingPermanent))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            if (AttackingPermanent.TopCard.Owner.Enemy.GetBattleAreaDigimons().Count(CanSelectBlockerCondition) >= 1)
            {
                int maxCount = 1;

                Permanent selectedPermanent = null;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: AttackingPermanent.TopCard.Owner.Enemy,
                    canTargetCondition: CanSelectBlockerCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: (!IsBlocking),
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: null);

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will block.", "The opponent is selecting 1 Digimon that will block.");
                
                if(!IsBlocking)
                    selectPermanentEffect.SetUpCustomBackButtonMessage("Not Block");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    selectedPermanent = permanent;

                    yield return null;
                }

                if (selectedPermanent != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(SwitchDefender(null, true, selectedPermanent));
                }
            }
            #endregion

            GManager.instance.turnStateMachine.IsSelecting = true;

            //end attack
            if (IsEndAttack)
            {
                goto EndAttack;
            }

            if (AttackingPermanent.TopCard == null)
            {
                goto EndAttack;
            }

            AttackingPermanent.ShowingPermanentCard.SetOrangeOutline();
            AttackingPermanent.ShowingPermanentCard.Outline_Select.gameObject.SetActive(true);

            if (DefendingPermanent != null)
            {
                if (CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(DefendingPermanent))
                {
                    DefendingPermanent.ShowingPermanentCard.SetOrangeOutline();
                    DefendingPermanent.ShowingPermanentCard.Outline_Select.gameObject.SetActive(true);
                }
            }

            #region there is no Defending Permanent
            if (DefendingPermanent == null)
            {
                if (AttackingPermanent.Strike >= 1)
                {
                    //if there is no security, end the game
                    if (AttackingPermanent.TopCard.Owner.Enemy.SecurityCards.Count == 0)
                    {
                        GManager.instance.turnStateMachine.EndGame(AttackingPermanent.TopCard.Owner, false);
                        yield break;
                    }
                }

                DoSecurityCheck = true;
            }
            #endregion

            #region there is Defending Permanent
            else
            {
                if (!CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(DefendingPermanent) || !CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(AttackingPermanent))
                {
                    goto EndAttack;
                }

                DefendingPermanent.TopCard.Owner.securityObject.securityBreakGlass.gameObject.SetActive(false);

                DefendingPermanent.ShowingPermanentCard.SetOrangeOutline();
                DefendingPermanent.ShowingPermanentCard.Outline_Select.gameObject.SetActive(true);

                // battle
                IBattle battle = new IBattle(AttackingPermanent: AttackingPermanent, DefendingPermanent: DefendingPermanent, null);
                yield return ContinuousController.instance.StartCoroutine(battle.Battle());

                /*#region effect when determine whether to do security check
                Hashtable hashtable = new Hashtable()
                {
                    {"battle", battle}
                };

                List<SkillInfo> skillInfos_Pierce = AutoProcessing.GetSkillInfos(hashtable, EffectTiming.OnDetermineDoSecurityCheck)
                    .Filter(skillInfo => skillInfo.CardEffect != null && skillInfo.CardEffect.CanActivate(skillInfo.Hashtable));

                if (skillInfos_Pierce.Count >= 1)
                {
                    GManager.instance.autoProcessing.PutStackedSkill(skillInfos_Pierce[0]);
                }
                #endregion*/
                
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.TriggeredSkillProcess(true, null));
                GManager.instance.turnStateMachine.IsSelecting = true;
            }
            #endregion

            if (AttackingPermanent.TopCard == null)
            {
                goto EndAttack;
            }

            #region do security check
            if (DoSecurityCheck && AttackingPermanent.TopCard.Owner.Enemy.SecurityCards.Count >= 1)
            {
                //security check process
                yield return ContinuousController.instance.StartCoroutine(new ISecurityCheck(
                    AttackingPermanent: AttackingPermanent,
                    player: GManager.instance.turnStateMachine.gameContext.NonTurnPlayer).SecurityCheck());
            }
            #endregion
        }

        else
        {
            if (beforeOnAttackCoroutine != null)
            {
                yield return ContinuousController.instance.StartCoroutine(beforeOnAttackCoroutine());
            }
        }
    #endregion

    #region End Attack
    EndAttack:;

        // [On End Attack] effect
        if (AttackingPermanent != null && AttackingPermanent.TopCard != null)
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(effectHashtable, EffectTiming.OnEndAttack));
        }
        
        // activate cutin effects
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.AutoProcessCheck());

        #region reset effects which continues until the end of attack
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                permanent.UntilEndAttackEffects = new List<Func<EffectTiming, ICardEffect>>();
            }
        }
        #endregion

        GManager.instance.turnStateMachine.IsSelecting = true;
        #endregion

        DoSecurityCheck = false;
        IsBlocking = false;
        SecurityDigimon = null;
        IsAttacking = false;
        IsEndAttack = false;
    }

    public IEnumerator SwitchDefender(ICardEffect cardEffect, bool isBlock, Permanent newDefendingPermanent)
    {
        if (AttackingPermanent == null) yield break;
        if (AttackingPermanent.TopCard == null) yield break;
        if (newDefendingPermanent != null && newDefendingPermanent.TopCard == null) yield break;
        if (!AttackingPermanent.CanSwitchAttackTarget) yield break;

        Permanent oldDefendingPermanent = DefendingPermanent;

        DefendingPermanent = newDefendingPermanent;

        IsBlocking = isBlock;

        if (DefendingPermanent != null)
        {
            if (DefendingPermanent.ShowingPermanentCard != null)
            {
                DefendingPermanent.ShowingPermanentCard.SetOrangeOutline();
                DefendingPermanent.ShowingPermanentCard.Outline_Select.gameObject.SetActive(true);
            }
        }

        Hashtable hashtable = new Hashtable(){
                {"AttackingPermanent", AttackingPermanent},
                {"DefendingPermanent", DefendingPermanent},
                {"CardEffect", cardEffect}
                };

        if (isBlock)
        {
            hashtable.Add("IsBlock", isBlock);
        }

        if (isBlock)
        {
            if (DefendingPermanent != null)
            {
                // add log
                string log1 = $"\nBlock:\n{DefendingPermanent.TopCard.BaseENGCardNameFromEntity}({DefendingPermanent.TopCard.CardID})\n";

                PlayLog.OnAddLog?.Invoke(log1);

                // suspend
                yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { DefendingPermanent }, hashtable).Tap());

                // the effects when blocking
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(
                    hashtable,
                    EffectTiming.OnBlockAnyone));
            }
        }

        if (AttackingPermanent != null)
        {
            if (AttackingPermanent.TopCard == null)
            {
                IsBlocking = false;
                yield break;
            }
        }

        if (DefendingPermanent != null)
        {
            if (DefendingPermanent.TopCard == null)
            {
                IsBlocking = false;
                yield break;
            }
        }

        if (AttackingPermanent != null)
        {
            // target arrow
            for (int i = 0; i < 3; i++)
            {
                GManager.instance.OffTargetArrow();

                yield return null;
            }

            if (DefendingPermanent != null)
            {
                DefendingPermanent.ShowingPermanentCard.SetOrangeOutline();
                DefendingPermanent.ShowingPermanentCard.Outline_Select.gameObject.SetActive(true);

                yield return GManager.instance.OnTargetArrow(
                    AttackingPermanent.PermanentFrame.GetLocalCanvasPosition() + AttackingPermanent.TopCard.Owner.playerUIObjectParent.localPosition,
                    DefendingPermanent.PermanentFrame.GetLocalCanvasPosition() + DefendingPermanent.TopCard.Owner.playerUIObjectParent.localPosition,
                    null,
                    null);
            }

            else
            {
                yield return GManager.instance.OnTargetArrow(
                                    AttackingPermanent.PermanentFrame.GetLocalCanvasPosition() + AttackingPermanent.TopCard.Owner.playerUIObjectParent.localPosition,
                                    GManager.instance.turnStateMachine.gameContext.NonTurnPlayer.SecurityAttackLocalCanvasPosition + GManager.instance.turnStateMachine.gameContext.NonTurnPlayer.playerUIObjectParent.localPosition,
                                    null,
                                    null);
            }

            AttackingPermanent.ShowingPermanentCard.SetOrangeOutline();
            AttackingPermanent.ShowingPermanentCard.Outline_Select.gameObject.SetActive(true);
        }

        if (newDefendingPermanent != oldDefendingPermanent)
        {
            // the effects when attack target switched
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(
                hashtable,
                EffectTiming.OnAttackTargetChanged));
        }
    }
}

