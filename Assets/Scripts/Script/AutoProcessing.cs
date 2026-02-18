using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class AutoProcessing : MonoBehaviourPunCallbacks
{
    //Skill list before triggering and entering resolution timing
    public List<SkillInfo> StackedSkillInfos { get; set; } = new List<SkillInfo>();
    //Skill Processing Component List
    public List<MultipleSkills> multipleSkills = new List<MultipleSkills>();

    [SerializeField] bool SkipSameEffect;

    public List<SkillInfo> skipSkillInfos = new List<SkillInfo>();

    #region Available Skill Processing Component
    public MultipleSkills availableMultipleSkills
    {
        get
        {
            foreach (MultipleSkills _multipleSkills in multipleSkills)
            {
                if (!_multipleSkills.IsUsing)
                {
                    return _multipleSkills;
                }
            }

            return null;
        }
    }
    #endregion

    #region Stack during effect processing
    public MultipleSkills executingMultipleSkills
    {
        get
        {
            for (int i = 0; i < multipleSkills.Count; i++)
            {
                if (multipleSkills[multipleSkills.Count - 1 - i].IsUsing)
                {
                    return multipleSkills[multipleSkills.Count - 1 - i];
                }
            }

            return null;
        }
    }
    #endregion

    #region put the effect on the stack
    public void PutStackedSkill(SkillInfo skillInfo)
    {
        if (skillInfo == null) return;
        if (skillInfo.CardEffect == null) return;
        if (skillInfo.CardEffect.EffectSourceCard == null) return;
        if (!(skillInfo.CardEffect is ActivateICardEffect)) return;

        CardSource card = skillInfo.CardEffect.EffectSourceCard;
        Permanent permanent = card.PermanentOfThisCard();

        //Inherited and Link effects can only ever be digimon effects
        if (skillInfo.CardEffect.IsInheritedEffect || skillInfo.CardEffect.IsLinkedEffect)
        {
            skillInfo.CardEffect.SetIsDigimonEffect(true);
        }
        else
        {
            #region set the flag whether it is Digimon's effect
            if (permanent != null)
            {
                if (permanent.IsDigimon)
                {
                    skillInfo.CardEffect.SetIsDigimonEffect(true);
                }
            }

            if (card == GManager.instance.attackProcess.SecurityDigimon)
            {
                skillInfo.CardEffect.SetIsDigimonEffect(true);
            }
            #endregion

            #region set the flag whether it is Tamer's effect
            if (permanent != null)
            {
                if (permanent.IsTamer)
                {
                    skillInfo.CardEffect.SetIsTamerEffect(true);
                }
            }

            else if (card.IsTamer)
            {
                skillInfo.CardEffect.SetIsTamerEffect(true);
            }
            #endregion
        }

        #region set permanent and topCard when triggered
        ((ActivateICardEffect)skillInfo.CardEffect).PermanentWhenTriggered = permanent;

        if (permanent != null)
        {
            ((ActivateICardEffect)skillInfo.CardEffect).TopCardWhenTriggered = permanent.TopCard;
        }
        #endregion

        StackedSkillInfos.Add(skillInfo);
    }
    #endregion

    #region Automated processing checks
    public IEnumerator AutoProcessCheck()
    {
        if (!GManager.instance.turnStateMachine.DoneStartGame) yield break;

        yield return ContinuousController.instance.StartCoroutine(ShrinkSecurityDigimonDisplay());

        GManager.instance.turnStateMachine.IsSelecting = true;
        GManager.instance.turnStateMachine.isSync = true;

        //Rule processing
        yield return ContinuousController.instance.StartCoroutine(RuleProcess());

        //Rule Timing
        yield return ContinuousController.instance.StartCoroutine(StackSkillInfos(null, EffectTiming.RulesTiming));

        //Trigger effect processing
        yield return ContinuousController.instance.StartCoroutine(TriggeredSkillProcess(false, null));

        GManager.instance.turnStateMachine.IsSelecting = false;
        GManager.instance.turnStateMachine.isSync = false;
    }
    #endregion

    #region Rule processing
    public bool IsRuleProcessing { get; set; } = false;

    bool IsNotDigimonInBreeding(Permanent permanent)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (CardEffectCommons.IsExistOnBreedingArea(permanent.TopCard))
                {
                    if (!permanent.IsDigimon)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    bool IsNotHavingDP(Permanent permanent)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (permanent.DP < 0)
                {
                    if (permanent.IsPlaceToTrashDueToNotHavingDP)
                    {
                        if (permanent.IsDigimon)
                        {
                            return true;
                        }

                        if (permanent.TopCard.IsOption)
                        {
                            if (!permanent.IsPlayedOptionPermanent)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    bool IsDigimonLackDP(Permanent permanent)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (permanent.DP == 0)
                {
                    if (permanent.IsDigimon)
                    {
                        if (permanent.CanBeDestroyed())
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    bool IsAttackerNotADigimon()
    {
        if (!GManager.instance.attackProcess.IsAttacking)
            return false;

        if (GManager.instance.attackProcess.AttackingPermanent != null)
        {
            if (!GManager.instance.attackProcess.AttackingPermanent.IsDigimon)
                return true;

            //if (GManager.instance.attackProcess.HasDefender && GManager.instance.attackProcess.DefendingPermanent == null)
            //    return true;
        }

        return false;
    }

    bool IsDigimonLackLinkCondition(Permanent permanent)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (permanent.LinkedCards.Any(source => !source.CanLinkToTargetPermanent(permanent, false, true)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    bool IsDigimonLackLinkCount(Permanent permanent)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (permanent.LinkedCards.Count > permanent.LinkedMax)
                {
                    return true;
                }
            }
        }

        return false;
    }

    bool IsPermanentFaceDown(Permanent permanent)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (permanent.TopCard.IsFlipped)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public IEnumerator RuleProcess()
    {
        while (DoRuleProcess())
        {
            IsRuleProcessing = true;

            //敗北処理
            yield return ContinuousController.instance.StartCoroutine(EndGameProcess());

            if (GManager.instance.turnStateMachine.endGame) yield break;

            //Trash Non Digimon from Breeding
            yield return ContinuousController.instance.StartCoroutine(TrashNonDigimonPermanentProcess());

            //DPを持たないカードをトラッシュする処理
            yield return ContinuousController.instance.StartCoroutine(TrashNoDPPermanentProcess());

            //DP不足処理
            yield return ContinuousController.instance.StartCoroutine(DigimonLackDPProcess());

            //Battle as Tamer
            yield return ContinuousController.instance.StartCoroutine(BattleWithoutDigimon());

            //Link Lacking condition
            yield return ContinuousController.instance.StartCoroutine(DigimonLackLinkConditionProcess());

            //Add link count correction
            yield return ContinuousController.instance.StartCoroutine(DigimonLackLinkMaxCountProcess());

            //Permanent Face Down
            yield return ContinuousController.instance.StartCoroutine(CardFaceDownProcess());

            IsRuleProcessing = false;
        }
    }

    #region Whether rule processing needs to be done
    bool DoRuleProcess()
    {
        if (IsRuleProcessing) return false;

        #region Whether it is necessary to perform game end processing
        if (GManager.instance.turnStateMachine.gameContext.Players.Count(player => player.IsLose) >= 1)
        {
            return true;
        }
        #endregion

        #region Is it necessary to discard cards in breeding?
        if (CardEffectCommons.HasMatchConditionPermanent(IsNotDigimonInBreeding, true))
        {
            return true;
        }
        #endregion
        
        #region Is it necessary to discard cards without DP?
        if (CardEffectCommons.HasMatchConditionPermanent(IsNotHavingDP))
        {
            return true;
        }
        #endregion

        #region Is it necessary to deal with Digimon's DP shortage?
        if (CardEffectCommons.HasMatchConditionPermanent(IsDigimonLackDP))
        {
            return true;
        }
        #endregion

        #region Is it necessary to deal with Battle Without Digimon?
        if (IsAttackerNotADigimon())
        {
            //return true;
        }
        #endregion

        #region Is it necessary to deal with Digimon's Link Cards?
        if (CardEffectCommons.HasMatchConditionPermanent(IsDigimonLackLinkCondition, true))
        {
            return true;
        }
        #endregion

        #region Is it necessary to deal with Digimon's Link Count?
        if (CardEffectCommons.HasMatchConditionPermanent(IsDigimonLackLinkCount, true))
        {
            return true;
        }
        #endregion

        #region Is it necessary to deal with card being face down?
        if (CardEffectCommons.HasMatchConditionPermanent(IsPermanentFaceDown, true))
        {
            return true;
        }
        #endregion

        return false;
    }
    #endregion

    #region Each rule processing

    #region Game end processing
    IEnumerator EndGameProcess()
    {
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
        {
            if (player.IsLose)
            {
                if (!player.Enemy.IsLose)
                {
                    GManager.instance.turnStateMachine.EndGame(player.Enemy, false);
                }

                else
                {
                    GManager.instance.turnStateMachine.EndGame(null, false);
                }

                yield break;
            }
        }
    }
    #endregion

    #region Process of trashing cards in Breeding
    IEnumerator TrashNonDigimonPermanentProcess()
    {
        List<Permanent> BreedingPermanents = GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
            .Map(player => player.GetBreedingAreaPermanents()
            .Filter(IsNotDigimonInBreeding)).Flat();

        if (BreedingPermanents.Count >= 1)
        {
            foreach (Permanent permanent in BreedingPermanents)
            {
                if (permanent != null)
                {
                    if (permanent.TopCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(permanent.DiscardEvoRoots());

                        CardSource cardSource = permanent.TopCard;

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { cardSource }, "Cards put to trash", true, true));

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveField(permanent));
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(cardSource));
                    }
                }
            }
        }
    }
    #endregion

    #region Process of trashing cards without DP
    IEnumerator TrashNoDPPermanentProcess()
    {
        List<Permanent> DigitamaPermanents = GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
            .Map(player => player.GetBattleAreaPermanents()
            .Filter(IsNotHavingDP)).Flat();

        if (DigitamaPermanents.Count >= 1)
        {
            foreach (Permanent permanent in DigitamaPermanents)
            {
                if (permanent != null)
                {
                    if (permanent.TopCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(permanent.DiscardEvoRoots());

                        CardSource cardSource = permanent.TopCard;

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { cardSource }, "Cards put to trash", true, true));

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveField(permanent));
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(cardSource));
                    }
                }
            }
        }
    }
    #endregion

    #region Digimon DP shortage handling
    IEnumerator DigimonLackDPProcess()
    {
        List<Permanent> LackPowerPermanents = GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
            .Map(player => player.GetFieldPermanents()
            .Filter(IsDigimonLackDP)).Flat();

        if (LackPowerPermanents.Count >= 1)
        {
            Hashtable hashtable = new Hashtable()
            {
                { "DPZero", true }
            };

            yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(LackPowerPermanents, hashtable).Destroy());
        }
    }
    #endregion

    #region Battle Concerning Tamer
    IEnumerator BattleWithoutDigimon()
    {
        if(GManager.instance.attackProcess.AttackingPermanent == null)
            yield break;

        if(!GManager.instance.attackProcess.AttackingPermanent.IsDigimon)
            GManager.instance.attackProcess.IsEndAttack = true;

        if(GManager.instance.attackProcess.DefendingPermanent == null && GManager.instance.attackProcess.HasDefender)
            GManager.instance.attackProcess.IsEndAttack = true;
    }
    #endregion

    #region Link Card Lacking conditions handling
    IEnumerator DigimonLackLinkConditionProcess()
    {
        List<Permanent> LackLinkConditionPermanents = GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
            .Map(player => player.GetFieldPermanents()
            .Filter(IsDigimonLackLinkCondition)).Flat();

        if (LackLinkConditionPermanents.Count >= 1)
        {
            foreach(Permanent permanent in LackLinkConditionPermanents)
            {
                List<CardSource> selectedCards = permanent.LinkedCards.FindAll(source => !source.CanLinkToTargetPermanent(permanent, false));

                yield return ContinuousController.instance.StartCoroutine(new ITrashLinkCards(
                               permanent,
                               selectedCards,
                               null).TrashLinkCards());
            }
        }
    }
    #endregion

    #region Link Card Lacking Max Count handling
    IEnumerator DigimonLackLinkMaxCountProcess()
    {
        List<Permanent> LackLinkCountPermanents = GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
            .Map(player => player.GetFieldPermanents()
            .Filter(IsDigimonLackLinkCount)).Flat();

        if (LackLinkCountPermanents.Count >= 1)
        {
            foreach (Permanent permanent in LackLinkCountPermanents)
            {
                yield return ContinuousController.instance.StartCoroutine(permanent.RemoveLinkedCard(null, (permanent.LinkedCards.Count - permanent.LinkedMax)));
            }
        }
    }
    #endregion

    #region Process of trashing cards that are face down
    IEnumerator CardFaceDownProcess()
    {
        List<Permanent> FacedownPermanents = GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
            .Map(player => player.GetBattleAreaPermanents()
            .Filter(IsPermanentFaceDown)).Flat();

        foreach (Permanent permanent in FacedownPermanents)
        {
            if (permanent != null)
            {
                if (permanent.TopCard != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(permanent.DiscardEvoRoots());

                    CardSource cardSource = permanent.TopCard;

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { cardSource }, "Cards put to trash", true, true));

                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveField(permanent));
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(cardSource));
                }
            }
        }
    }
    #endregion

    #endregion

    #endregion

    #region Trigger effect processing
    public IEnumerator TriggeredSkillProcess(bool CheckNewTriggredSkill_mainStack, Func<List<SkillInfo>, SkillInfo, bool> skipCondition)
    {
        yield return ContinuousController.instance.StartCoroutine(ShrinkSecurityDigimonDisplay());

        if (StackedSkillInfos.Count > 0)
        {
            List<SkillInfo> skillInfos = StackedSkillInfos.Filter(skillInfo => skillInfo != null && !skipSkillInfos.Contains(skillInfo));

            if (skillInfos.Count >= 1)
            {
                //Clear the list of triggering skills not included in the resolution timing
                foreach (SkillInfo skillInfo in skillInfos)
                {
                    StackedSkillInfos.Remove(skillInfo);
                }

                //Process the list of skills being triggered
                if (availableMultipleSkills != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(availableMultipleSkills.ActivateMultipleSkills(
                        skillInfos,
                        this,
                        CheckNewTriggredSkill_mainStack,
                        skipCondition));
                }
                yield return ContinuousController.instance.StartCoroutine(StackSkillInfos(null, EffectTiming.AfterEffectsActivate));
            }
        }
    }
    #endregion

    #region List of effects already achieved
    public List<SkillInfo> skillInfos_used
    {
        get
        {
            List<SkillInfo> skillInfos_used = new List<SkillInfo>();

            foreach (MultipleSkills multipleSkills in multipleSkills)
            {
                foreach (SkillInfo skillInfo in multipleSkills.SkillInfos_used)
                {
                    skillInfos_used.Add(skillInfo);
                }
            }

            return skillInfos_used;
        }
    }
    #endregion

    #region The same effect has already been achieved
    public static bool HasExecutedSameEffect(List<SkillInfo> skillInfos, SkillInfo skillInfo)
    {
        return skillInfos.Some((skillInfo1) => skillInfo1.CardEffect.IsSameEffect(skillInfo.CardEffect));
    }
    #endregion

    #region Check end of turn
    public IEnumerator EndTurnCheck()
    {
        if (GManager.instance.turnStateMachine.gameContext.TurnPhase != GameContext.phase.End)
        {
            if (GManager.instance.turnStateMachine.gameContext.NonTurnPlayer.MemoryForPlayer >= TurnEndMinMemory)
            {
                GManager.instance.turnStateMachine.Passed = false;
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.EndTurnProcess());
            }
        }
    }
    #endregion

    #region Minimum memory to end turn
    static int TurnEndMinMemory
    {
        get
        {
            int turnEndMinMemory = 1;

            #region the effects of players
            GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                    .Map(player => player.EffectList(EffectTiming.None))
                    .Flat()
                    .Filter(cardEffect => cardEffect is IChangeEndTurnMinMemoryEffect && cardEffect.CanUse(null))
                    .ForEach(cardEffect => turnEndMinMemory = ((IChangeEndTurnMinMemoryEffect)cardEffect).GetMinMemory(turnEndMinMemory));
            #endregion

            #region the effects of permanents
            GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                    .Map(player => player.GetFieldPermanents())
                    .Flat()
                    .Map(permanent => permanent.EffectList(EffectTiming.None))
                    .Flat()
                    .Filter(cardEffect => cardEffect is IChangeEndTurnMinMemoryEffect && cardEffect.CanUse(null))
                    .ForEach(cardEffect => turnEndMinMemory = ((IChangeEndTurnMinMemoryEffect)cardEffect).GetMinMemory(turnEndMinMemory));
            #endregion

            return turnEndMinMemory;
        }
    }
    #endregion

    #region Turn end processing
    public IEnumerator EndTurnProcess()
    {
        if (GManager.instance.turnStateMachine.gameContext.TurnPhase != GameContext.phase.End)
        {
            GManager.instance.turnStateMachine.isSync = true;

            // yield return GManager.instance.photonWaitController.StartWait("EndTurnProcess");

            if (GManager.instance.turnStateMachine.Passed && GManager.instance.turnStateMachine.gameContext.TurnPhase == GameContext.phase.Main)
            {
                if (GManager.instance.turnStateMachine.gameContext.TurnPlayer.PlayerID == 0)
                {
                    GManager.instance.turnStateMachine.gameContext.Memory = 3;
                }

                else
                {
                    GManager.instance.turnStateMachine.gameContext.Memory = -3;
                }

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.memoryObject.SetMemory());
            }

            GManager.instance.turnStateMachine.Passed = true;

            //Effect at end of turn
            yield return ContinuousController.instance.StartCoroutine(StackSkillInfos(null, EffectTiming.OnEndTurn));

            //Automatic processing check timing
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.AutoProcessCheck());

            if (GManager.instance.turnStateMachine.gameContext.NonTurnPlayer.MemoryForPlayer >= TurnEndMinMemory)
            {
                GManager.instance.turnStateMachine.gameContext.TurnPhase = GameContext.phase.End;
            }

            else
            {
                GManager.instance.turnStateMachine.isSync = false;

                if (GManager.instance.turnStateMachine.gameContext.TurnPhase == GameContext.phase.Main)
                {
                    StartCoroutine(GManager.instance.turnStateMachine.SetMainPhase());
                }
            }
        }
    }
    #endregion

    #region Shrink security Digimon display
    public IEnumerator ShrinkSecurityDigimonDisplay()
    {
        if (!HasAwaitingActivateEffects()) yield break;

        if (GManager.instance.attackProcess.IsAttacking)
        {
            if (GManager.instance.attackProcess.SecurityDigimon != null)
            {
                if (GManager.instance.GetComponent<Effects>().ShowUseHandCard.gameObject.activeSelf && GManager.instance.GetComponent<Effects>().ShowUseHandCard.cardSource == GManager.instance.attackProcess.SecurityDigimon)
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().MoveToExecuteCardEffect(GManager.instance.attackProcess.SecurityDigimon));
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SecurityDigimon.Owner.brainStormObject.BrainStormCoroutine(GManager.instance.attackProcess.SecurityDigimon));
                }
            }
        }
    }
    #endregion

    #region Whether there is awaiting activate effects
    public bool HasAwaitingActivateEffects()
    {
        if (StackedSkillInfos.Count >= 2)
        {
            return true;
        }

        else if (StackedSkillInfos.Count == 1)
        {
            if (StackedSkillInfos[0].CardEffect.CanActivate(StackedSkillInfos[0].Hashtable))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Get skillInfos
    public static List<SkillInfo> GetSkillInfos(Hashtable hashtable, EffectTiming timing, Func<ICardEffect, bool> cardEffectCondition = null)
    {
        List<SkillInfo> skillInfos = new List<SkillInfo>();

        #region player effect
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            foreach (ICardEffect cardEffect in player.EffectList(timing).Filter(cardEffect => cardEffectCondition == null || cardEffectCondition(cardEffect)))
            {
                if (cardEffect is ActivateICardEffect)
                {
                    if (!cardEffect.IsBackgroundProcess)
                    {
                        if (cardEffect.CanTrigger(hashtable))
                        {
                            skillInfos.Add(new SkillInfo(cardEffect, hashtable, timing));
                        }
                    }
                }
            }
        }
        #endregion

        #region Effects of permanents in play
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                foreach (ICardEffect cardEffect in permanent.EffectList(timing).Filter(cardEffect => cardEffectCondition == null || cardEffectCondition(cardEffect)))
                {
                    if (cardEffect is ActivateICardEffect)
                    {
                        if (!cardEffect.IsBackgroundProcess)
                        {
                            if (cardEffect.CanTrigger(hashtable))
                            {
                                skillInfos.Add(new SkillInfo(cardEffect, hashtable, timing));
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Trash card effect
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            foreach (CardSource cardSource in player.TrashCards)
            {
                foreach (ICardEffect cardEffect in cardSource.EffectList(timing).Filter(cardEffect => cardEffectCondition == null || cardEffectCondition(cardEffect)))
                {
                    if (cardEffect is ActivateICardEffect)
                    {
                        if (!cardEffect.IsBackgroundProcess)
                        {
                            if (cardEffect.CanTrigger(hashtable))
                            {
                                skillInfos.Add(new SkillInfo(cardEffect, hashtable, timing));
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Effects of cards in hand
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            foreach (CardSource cardSource in player.HandCards)
            {
                foreach (ICardEffect cardEffect in cardSource.EffectList(timing).Filter(cardEffect => cardEffectCondition == null || cardEffectCondition(cardEffect)))
                {
                    if (cardEffect is ActivateICardEffect)
                    {
                        if (!cardEffect.IsBackgroundProcess)
                        {
                            if (cardEffect.CanTrigger(hashtable))
                            {
                                skillInfos.Add(new SkillInfo(cardEffect, hashtable, timing));
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Effects of faceup security
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            foreach (CardSource source in player.SecurityCards)
            {
                if (source.IsFlipped)
                    continue;

                foreach (ICardEffect cardEffect in source.EffectList(timing).Filter(cardEffect => cardEffectCondition == null || cardEffectCondition(cardEffect)))
                {
                    if (cardEffect is ActivateICardEffect)
                    {
                        if (!cardEffect.IsBackgroundProcess)
                        {
                            if (cardEffect.CanTrigger(hashtable))
                            {
                                skillInfos.Add(new SkillInfo(cardEffect, hashtable, timing));
                            }
                        }
                    }
                }
            }
        }            
        #endregion

        return skillInfos;
    }
    #endregion

    #region Activate background effects
    public static IEnumerator ActivateBackgroundEffects(Hashtable hashtable, EffectTiming timing, Func<ICardEffect, bool> cardEffectCondition = null)
    {
        #region Player effect
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            foreach (ICardEffect cardEffect in player.EffectList(timing).Filter(cardEffect => cardEffectCondition == null || cardEffectCondition(cardEffect)))
            {
                if (cardEffect is ActivateICardEffect)
                {
                    if (cardEffect.IsBackgroundProcess)
                    {
                        if (cardEffect.CanUse(hashtable))
                        {
                            cardEffect.EffectSourceCard.cEntity_EffectController.RegisterUseEfffectThisTurn(cardEffect);
                            yield return ContinuousController.instance.StartCoroutine(((ActivateICardEffect)cardEffect).Activate(hashtable));
                        }
                    }
                }
            }
        }
        #endregion

        #region Effects of permanents in play
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {

            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                foreach (ICardEffect cardEffect in permanent.EffectList(timing).Filter(cardEffect => cardEffectCondition == null || cardEffectCondition(cardEffect)))
                {
                    if (cardEffect is ActivateICardEffect)
                    {
                        if (cardEffect.IsBackgroundProcess)
                        {
                            if (cardEffect.CanUse(hashtable))
                            {
                                cardEffect.EffectSourceCard.cEntity_EffectController.RegisterUseEfffectThisTurn(cardEffect);
                                yield return ContinuousController.instance.StartCoroutine(((ActivateICardEffect)cardEffect).Activate(hashtable));
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Trash card effect
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            foreach (CardSource cardSource in player.TrashCards)
            {
                foreach (ICardEffect cardEffect in cardSource.EffectList(timing).Filter(cardEffect => cardEffectCondition == null || cardEffectCondition(cardEffect)))
                {
                    if (cardEffect is ActivateICardEffect)
                    {
                        if (cardEffect.IsBackgroundProcess)
                        {
                            if (cardEffect.CanUse(hashtable))
                            {
                                cardEffect.EffectSourceCard.cEntity_EffectController.RegisterUseEfffectThisTurn(cardEffect);
                                yield return ContinuousController.instance.StartCoroutine(((ActivateICardEffect)cardEffect).Activate(hashtable));
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Effects of cards in hand
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            foreach (CardSource cardSource in player.HandCards)
            {
                foreach (ICardEffect cardEffect in cardSource.EffectList(timing).Filter(cardEffect => cardEffectCondition == null || cardEffectCondition(cardEffect)))
                {
                    if (cardEffect is ActivateICardEffect)
                    {
                        if (cardEffect.IsBackgroundProcess)
                        {
                            if (cardEffect.CanUse(hashtable))
                            {
                                cardEffect.EffectSourceCard.cEntity_EffectController.RegisterUseEfffectThisTurn(cardEffect);
                                yield return ContinuousController.instance.StartCoroutine(((ActivateICardEffect)cardEffect).Activate(hashtable));
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
    #endregion

    #region Stack skillInfos
    public IEnumerator StackSkillInfos(Hashtable hashtable, EffectTiming timing, Func<ICardEffect, bool> cardEffectCondition = null)
    {
        GetSkillInfos(hashtable, timing, cardEffectCondition).ForEach(skillInfo => PutStackedSkill(skillInfo));

        yield return ContinuousController.instance.StartCoroutine(ActivateBackgroundEffects(hashtable, timing, cardEffectCondition));
    }
    #endregion

    #region Get skillInfos of cards
    public static List<SkillInfo> GetSkillInfosOfCards(Hashtable hashtable, EffectTiming timing, List<CardSource> cardSources, Func<ICardEffect, bool> cardEffectCondition = null)
    {
        List<SkillInfo> skillInfos = new List<SkillInfo>();

        #region カードリストの効果
        foreach (CardSource cardSource in cardSources)
        {
            if (cardSource.PermanentOfThisCard() == null)
            {
                foreach (ICardEffect cardEffect in cardSource.EffectList(timing).Filter(cardEffect => cardEffectCondition == null || cardEffectCondition(cardEffect)))
                {
                    if (cardEffect is ActivateICardEffect)
                    {
                        if (!cardEffect.IsBackgroundProcess)
                        {
                            if (cardEffect.CanTrigger(hashtable))
                            {
                                skillInfos.Add(new SkillInfo(cardEffect, hashtable, timing));
                            }
                        }
                    }
                }
            }
        }
        #endregion

        return skillInfos;
    }
    #endregion

    #region Activate background effects of cards
    public static IEnumerator ActivateBackgroundEffectsOfCards(Hashtable hashtable, EffectTiming timing, List<CardSource> cardSources, Func<ICardEffect, bool> cardEffectCondition = null)
    {
        #region Effect of card list
        foreach (CardSource cardSource in cardSources)
        {
            if (cardSource.PermanentOfThisCard() == null)
            {
                foreach (ICardEffect cardEffect in cardSource.EffectList(timing).Filter(cardEffect => cardEffectCondition == null || cardEffectCondition(cardEffect)))
                {
                    if (cardEffect is ActivateICardEffect)
                    {
                        if (cardEffect.IsBackgroundProcess)
                        {
                            if (cardEffect.CanUse(hashtable))
                            {
                                cardEffect.EffectSourceCard.cEntity_EffectController.RegisterUseEfffectThisTurn(cardEffect);
                                yield return ContinuousController.instance.StartCoroutine(((ActivateICardEffect)cardEffect).Activate(hashtable));
                            }
                        }
                    }
                }
            }
        }
        #endregion

    }
    #endregion

    #region Stack skillInfos of cards
    public IEnumerator StackSkillInfosOfCards(Hashtable hashtable, EffectTiming timing, List<CardSource> cardSources, Func<ICardEffect, bool> cardEffectCondition = null)
    {
        GetSkillInfosOfCards(hashtable, timing, cardSources, cardEffectCondition).ForEach(skillInfo => PutStackedSkill(skillInfo));

        yield return ContinuousController.instance.StartCoroutine(ActivateBackgroundEffectsOfCards(hashtable, timing, cardSources, cardEffectCondition));
    }
    #endregion

    public ICardEffect MainProcessingEffect { get; private set; } = null;
    List<ICardEffect> _usedCutinEffects = new List<ICardEffect>();
    public IEnumerator ActivateEffectProcess(ICardEffect cardEffect, Hashtable hashtable, bool isCheckOptional = true)
    {
        if (cardEffect == null) yield break;
        if (!(cardEffect is ActivateICardEffect)) yield break;

        if (cardEffect.CanActivate(hashtable) || cardEffect.IsDeclarative)
        {
            bool isEffectProcessing = MainProcessingEffect != null;

            if (!isEffectProcessing)
            {
                MainProcessingEffect = cardEffect;

                _usedCutinEffects = new List<ICardEffect>();
            }

            yield return ContinuousController.instance.StartCoroutine(((ActivateICardEffect)cardEffect).Activate_Optional_Effect_Execute(hashtable, isCheckOptional));

            if (!isEffectProcessing)
            {
                MainProcessingEffect = null;

                _usedCutinEffects = new List<ICardEffect>();
            }
        }
    }

    public bool IsCutInEffectHasUsed(ICardEffect cutinEffect)
    {
        return false;//TODO: General line 17 - _usedCutinEffects.Some(cardEffect => cardEffect.IsSameEffect(cutinEffect));
    }

    public bool IsCutInEffectUsedMaxCount(ICardEffect cutinEffect)
    {
        return _usedCutinEffects.Count(cardEffect => cardEffect.IsSameEffect(cutinEffect)) < cutinEffect.ChainActivations;
    }

    public void AddCutinEffect(ICardEffect cardEffect)
    {
        if (GManager.instance.autoProcessing.MainProcessingEffect != null)
        {
            _usedCutinEffects.Add(cardEffect);
        }
    }
}