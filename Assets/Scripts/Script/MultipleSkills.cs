using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using System;
using System.Linq;

public class MultipleSkills : MonoBehaviourPunCallbacks
{
    public bool IsUsing { get; private set; } = false;
    AutoProcessing _autoProcessing = null;
    public List<SkillInfo> SkillInfos_used { get; private set; } = new List<SkillInfo>();
    public List<SkillInfo> StackedSkillInfos = new List<SkillInfo>();
    public bool IsOnlyHandEffectStacked => StackedSkillInfos.Every(skillInfo =>
        skillInfo.CardEffect != null && skillInfo.CardEffect.EffectSourceCard != null && CardEffectCommons.IsExistOnHand(skillInfo.CardEffect.EffectSourceCard) && skillInfo.CardEffect.EffectDiscription.Contains("[Hand]"));

    bool IsOnlyOptionalEffectStacked => StackedSkillInfos.Every(skillInfo => skillInfo.CardEffect != null && skillInfo.CardEffect.IsOptional);
    bool IsEachStackedEffectHasDistinctSourceCard => StackedSkillInfos.Filter(skillInfo1 => skillInfo1.CardEffect != null && skillInfo1.CardEffect.EffectSourceCard != null)
        .Every(skillInfo => StackedSkillInfos.Filter(skillInfo1 => skillInfo1.CardEffect != null && skillInfo1.CardEffect.EffectSourceCard != null)
            .Count(otherSkillInfo => otherSkillInfo != skillInfo && skillInfo.CardEffect.EffectSourceCard == otherSkillInfo.CardEffect.EffectSourceCard) == 0);
    public IEnumerator ActivateMultipleSkills(List<SkillInfo> skillInfos, AutoProcessing autoProcessing, bool CheckNewTriggredSkill_mainStack, Func<List<SkillInfo>, SkillInfo, bool> skipCondition)
    {
        _skillIndex = 0;
        IsUsing = true;

        SkillInfos_used = new List<SkillInfo>();
        StackedSkillInfos = new List<SkillInfo>();

        _autoProcessing = autoProcessing;

        List<SkillInfo> TurnPlayerSkillInfos = new List<SkillInfo>();
        List<SkillInfo> NonTurnPlayerSkillInfos = new List<SkillInfo>();

        foreach (SkillInfo skillInfo in skillInfos)
        {
            if (skillInfo != null)
            {
                ICardEffect cardEffect = skillInfo.CardEffect;

                if (cardEffect.EffectSourceCard != null)
                {
                    if (cardEffect.EffectSourceCard.Owner == GManager.instance.turnStateMachine.gameContext.TurnPlayer)
                    {
                        TurnPlayerSkillInfos.Add(skillInfo);
                    }

                    else if (cardEffect.EffectSourceCard.Owner == GManager.instance.turnStateMachine.gameContext.NonTurnPlayer)
                    {
                        NonTurnPlayerSkillInfos.Add(skillInfo);
                    }
                }
            }
        }

        GManager.instance.turnStateMachine.isSync = true;
        yield return ContinuousController.instance.StartCoroutine(ActivateMultipleSkills_OnePlayer(TurnPlayerSkillInfos, GManager.instance.turnStateMachine.gameContext.TurnPlayer, CheckNewTriggredSkill_mainStack, skipCondition));
        yield return ContinuousController.instance.StartCoroutine(ActivateMultipleSkills_OnePlayer(NonTurnPlayerSkillInfos, GManager.instance.turnStateMachine.gameContext.NonTurnPlayer, CheckNewTriggredSkill_mainStack, skipCondition));
        GManager.instance.turnStateMachine.isSync = false;

        SkillInfos_used = new List<SkillInfo>();

        IsUsing = false;
    }

    bool _endSelect = false;
    int _skillIndex;

    bool IsCutinEffect(bool CheckNewTriggredSkill_mainStack) => !CheckNewTriggredSkill_mainStack && _autoProcessing != GManager.instance.autoProcessing;

    IEnumerator ActivateMultipleSkills_OnePlayer(List<SkillInfo> skillInfos, Player player, bool CheckNewTriggredSkill_mainStack, Func<List<SkillInfo>, SkillInfo, bool> skipCondition)
    {
        StackedSkillInfos = new List<SkillInfo>();

        foreach (SkillInfo skillInfo in skillInfos)
        {
            StackedSkillInfos.Add(skillInfo);
        }

        while (true)
        {
            List<SkillInfo> skillInfos_active = new List<SkillInfo>();

            foreach (SkillInfo skillInfo in StackedSkillInfos)
            {
                #region set the flag whether it is Digimon's effect or Tamer's effect
                if (skillInfo.CardEffect != null)
                {
                    if (skillInfo.CardEffect.EffectSourceCard != null)
                    {
                        CardSource card = skillInfo.CardEffect.EffectSourceCard;

                        if (card != null)
                        {
                            if (skillInfo.CardEffect.IsInheritedEffect || skillInfo.CardEffect.IsLinkedEffect)
                            {
                                skillInfo.CardEffect.SetIsDigimonEffect(true);
                            }
                            else
                            {
                                if (card.PermanentOfThisCard() != null)
                                {
                                    skillInfo.CardEffect.SetIsDigimonEffect(card.PermanentOfThisCard().IsDigimon);
                                    skillInfo.CardEffect.SetIsTamerEffect(card.PermanentOfThisCard().IsTamer);
                                }

                                else
                                {
                                    skillInfo.CardEffect.SetIsTamerEffect(card.IsTamer);
                                }

                                if (card == GManager.instance.attackProcess.SecurityDigimon)
                                {
                                    skillInfo.CardEffect.SetIsDigimonEffect(true);
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Check if the effect can be activated
                if (!skillInfo.CardEffect.CanActivate(skillInfo.Hashtable))
                {
                    Debug.Log($"{skillInfo.CardEffect.EffectName} Can't Activate");
                    continue;
                }

                if (skipCondition != null)
                {
                    if (skipCondition(_autoProcessing.skillInfos_used, skillInfo))
                    {
                        Debug.Log($"{skillInfo.CardEffect.EffectName} has been skipped");

                        continue;
                    }
                }

                if (skillInfo.CardEffect.ChainActivations > 0)
                {
                    if (GManager.instance.autoProcessing.IsCutInEffectUsedMaxCount(skillInfo.CardEffect))
                    {
                        Debug.Log($"{skillInfo.CardEffect.EffectName} has exceeded its use");

                        continue;
                    }
                }

                if (IsCutinEffect(CheckNewTriggredSkill_mainStack))
                {
                    Debug.Log($"{skillInfo.CardEffect.EffectName} is Cut In effect");

                    if (GManager.instance.autoProcessing.IsCutInEffectHasUsed(skillInfo.CardEffect))
                    {
                        Debug.Log($"{skillInfo.CardEffect.EffectName} has been used");

                        continue;
                    }
                }

                skillInfos_active.Add(skillInfo);
                #endregion
            }

            skillInfos_active = skillInfos_active.Filter(skillInfo => skillInfo != null && skillInfo.CardEffect != null
                && skillInfo.CardEffect.CanActivate(skillInfo.Hashtable) && skillInfo.CardEffect.EffectSourceCard != null);
            
            if (skillInfos_active.Count > 0)
            {
                bool oldIsSecurityGlassBlue = player.securityObject.securityBreakGlass.IsBlueGlass && IsOnlyHandEffectStacked && IsOnlyOptionalEffectStacked && IsEachStackedEffectHasDistinctSourceCard;

                #region If the effect list is one, process normally.
                if (skillInfos_active.Count == 1)
                {
                    _skillIndex = 0;

                    yield return ContinuousController.instance.StartCoroutine(Activate(true));
                }
                #endregion

                #region If there are multiple effect lists, select which one to process first
                else
                {
                    List<CardSource> RootCardSources = skillInfos_active.Map(skillInfo => skillInfo.CardEffect.EffectSourceCard);

                    yield return GManager.instance.photonWaitController.StartWait("StartSelectMultipleSkill");

                    // Blast Digivolution
                    if (IsOnlyHandEffectStacked && IsOnlyOptionalEffectStacked && IsEachStackedEffectHasDistinctSourceCard)
                    {
                        if (player.isYou)
                        {
                            int skillIndex = 0;

                            //if (!ContinuousController.instance.SkipSelectOrderOrCount)
                            {
                                if (oldIsSecurityGlassBlue)
                                {
                                    player.securityObject.securityBreakGlass.gameObject.SetActive(false);
                                }

                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: player,
                                    canTargetCondition: (cardSource) => RootCardSources.Contains(cardSource),
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                    mode: SelectHandEffect.Mode.Custom,
                                    cardEffect: null);

                                selectHandEffect.SetUpCustomMessage("Multiple effects are triggered.\nChoose which effect to process first.", "");
                                selectHandEffect.SetNotShowCard();
                                selectHandEffect.SetNotShowOpponentMessage();
                                selectHandEffect.SetIsLocal();

                                yield return StartCoroutine(selectHandEffect.Activate());

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    for (int i = 0; i < skillInfos_active.Count; i++)
                                    {
                                        SkillInfo skillInfo = skillInfos_active[i];

                                        if (skillInfo.CardEffect.EffectSourceCard == cardSource)
                                        {
                                            skillIndex = i;
                                            break;
                                        }
                                    }

                                    yield return null;
                                }

                                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                                {
                                    if (cardSources.Count == 0)
                                    {
                                        skillIndex = -1;
                                    }

                                    yield return null;
                                }
                            }

                            photonView.RPC("SetTargetSkill", RpcTarget.All, skillIndex);
                        }

                        else
                        {
                            #region AI
                            if (GManager.instance.IsAI)
                            {
                                SetTargetSkill(0);
                            }
                            #endregion
                        }
                    }

                    else
                    {
                        if (player.isYou)
                        {
                            int skillIndex = 0;

                            if (!ContinuousController.instance.autoEffectOrder)
                            {
                                yield return StartCoroutine(GManager.instance.selectCardPanel.OpenSelectCardPanel(
                                Message: "Multiple effects are triggered.\nChoose which effect to process.",
                                RootCardSources: RootCardSources,
                                _CanTargetCondition: (cardSource) => true,
                                _CanTargetCondition_ByPreSelecetedList: null,
                                _CanEndSelectCondition: null,
                                _MaxCount: 1,
                                _CanEndNotMax: false,
                                _CanNoSelect: () => false,
                                CanLookReverseCard: true,
                                skillInfos: skillInfos_active,
                                root: SelectCardEffect.Root.None));

                                if (GManager.instance.selectCardPanel.SelectedIndex.Count > 0)
                                {
                                    skillIndex = GManager.instance.selectCardPanel.SelectedIndex[0];
                                }
                            }

                            else
                            {
                                skillIndex = AutomaticOrder.GetSkillIndexAutomaticOrder(skillInfos_active);
                            }

                            photonView.RPC("SetTargetSkill", RpcTarget.All, skillIndex);
                        }

                        else
                        {
                            if (!IsOnlyHandEffectStacked)
                            {
                                GManager.instance.commandText.OpenCommandText("The opponent is choosing which effect to process first.");
                            }

                            #region AI
                            if (GManager.instance.IsAI)
                            {
                                SetTargetSkill(0);
                            }
                            #endregion
                        }
                    }

                    yield return new WaitWhile(() => !_endSelect);
                    _endSelect = false;

                    GManager.instance.commandText.CloseCommandText();
                    yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

                    yield return ContinuousController.instance.StartCoroutine(Activate(!(IsOnlyHandEffectStacked && IsOnlyOptionalEffectStacked && IsEachStackedEffectHasDistinctSourceCard)));

                    yield return GManager.instance.photonWaitController.StartWait("EndSelectMultipleSkill");
                }
                #endregion

                #region Executing the effect
                IEnumerator Activate(bool isCheckOptional)
                {
                    if (_skillIndex < 0 || skillInfos_active.Count < _skillIndex)
                    {
                        StackedSkillInfos = new List<SkillInfo>();
                        yield break;
                    }

                    if (oldIsSecurityGlassBlue)
                    {
                        player.securityObject.securityBreakGlass.gameObject.SetActive(false);
                    }

                    ICardEffect cardEffect = skillInfos_active[_skillIndex].CardEffect;
                    Hashtable hashtable = skillInfos_active[_skillIndex].Hashtable;

                    StackedSkillInfos.Remove(skillInfos_active[_skillIndex]);

                    skillInfos_active[_skillIndex].CardEffect.SetOnProcessCallbuck(() =>
                        {
                            SkillInfos_used.Add(skillInfos_active[_skillIndex]);
                            cardEffect.EffectSourceCard.cEntity_EffectController.RegisterUseEfffectThisTurn(cardEffect);
                        });

                    if (cardEffect is ActivateICardEffect)
                    {
                        if (cardEffect.CanActivate(hashtable))
                        {
                            // For interrupt processing
                            if (IsCutinEffect(CheckNewTriggredSkill_mainStack))
                            {
                                // GManager.instance.autoProcessing.AddCutinEffect(cardEffect);

                                yield return ContinuousController.instance.StartCoroutine(((ActivateICardEffect)cardEffect)
                                .Activate_Optional_Effect_Execute(
                                    hashtable,
                                    isCheckOptional,
                                    useEffectCallback: GManager.instance.autoProcessing.AddCutinEffect));
                            }

                            else
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.ActivateEffectProcess(
                                    cardEffect,
                                    hashtable,
                                    isCheckOptional));
                            }
                        }
                    }

                    if (oldIsSecurityGlassBlue)
                    {
                        player.securityObject.securityBreakGlass.ShowBlueMatarial();
                    }
                }
                #endregion

                //Rule processing
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.RuleProcess());

                if (GManager.instance.turnStateMachine.endGame)
                {
                    yield break;
                }

                #region If there are any newly triggered effects, resolve those first.
                if (!CheckNewTriggredSkill_mainStack)
                {
                    yield return ContinuousController.instance.StartCoroutine(_autoProcessing.TriggeredSkillProcess(CheckNewTriggredSkill_mainStack, skipCondition));
                }

                else
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.TriggeredSkillProcess(CheckNewTriggredSkill_mainStack, null));
                }
                #endregion
            }

            else
            {
                break;
            }
        }
    }

    [PunRPC]
    public void SetTargetSkill(int skillIndex)
    {
        _skillIndex = skillIndex;
        _endSelect = true;
    }
}
