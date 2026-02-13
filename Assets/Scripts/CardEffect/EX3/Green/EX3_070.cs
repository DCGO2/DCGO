using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.EX3
{
    public class EX3_070 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] Activate 1 of the effects below. If you have a Digimon with [Examon] in its name in play, activate all of the effects below instead. - Suspend 1 of your opponent's Digimon, and 1 of your Digimon gains <Piercing> for the turn. - Unsuspend 1 of your Digimon.";
                }
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    #region レスト&貫通
                    Func<IEnumerator> _SuspendAndPierce = () => SuspendAndPierce();

                    IEnumerator SuspendAndPierce()
                    {
                        bool CanSelectPermanentCondition(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                        }

                        bool CanSelectPermanentCondition1(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Tap,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                        {
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition1));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition1,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will gain Pierce.", "The opponent is selecting 1 Digimon that will gain Pierce.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainPierce(targetPermanent: permanent, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                            }
                        }
                    }
                    #endregion

                    #region アクティブ
                    Func<IEnumerator> _Unsuspend = () => Unsuspend();

                    IEnumerator Unsuspend()
                    {
                        bool CanSelectPermanentCondition(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.UnTap,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }
                    #endregion

                    List<Func<IEnumerator>> canSelectEffects = new List<Func<IEnumerator>>() { _SuspendAndPierce, _Unsuspend };

                    #region 名称に「エグザモン」を含むデジモンがいる場合(順番を選択)
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && permanent.TopCard.ContainsCardName("Examon")))
                    {
                        List<Func<IEnumerator>> activatedEffects = new List<Func<IEnumerator>>();

                        while (canSelectEffects.Count((effect) => !activatedEffects.Contains(effect)) >= 1)
                        {
                            yield return GManager.instance.photonWaitController.StartWait("AvalonsGate_selectFirst");

                            if (card.Owner.isYou)
                            {
                                if (canSelectEffects.Count((effect) => !activatedEffects.Contains(effect)) >= 2)
                                {
                                    if (canSelectEffects.Count((effect) => !activatedEffects.Contains(effect)) == 2)
                                    {
                                        GManager.instance.commandText.OpenCommandText("Which effect will you activate the first?");
                                    }

                                    List<Command_SelectCommand> command_SelectCommands = new List<Command_SelectCommand>();

                                    for (int i = 0; i < canSelectEffects.Count; i++)
                                    {
                                        if (!activatedEffects.Contains(canSelectEffects[i]))
                                        {
                                            int k = i;

                                            string message = "";

                                            switch (k)
                                            {
                                                case 0:
                                                    message = "Suspend & gain Pierce";
                                                    break;

                                                case 1:
                                                    message = "Unsuspend";
                                                    break;
                                            }

                                            command_SelectCommands.Add(new Command_SelectCommand(message, () => photonView.RPC("SetEffectIndex", RpcTarget.All, k), 0));
                                        }
                                    }

                                    GManager.instance.selectCommandPanel.SetUpCommandButton(command_SelectCommands);
                                }

                                else
                                {
                                    for (int i = 0; i < canSelectEffects.Count; i++)
                                    {
                                        if (!activatedEffects.Contains(canSelectEffects[i]))
                                        {
                                            photonView.RPC("SetEffectIndex", RpcTarget.All, i);
                                            break;
                                        }
                                    }
                                }
                            }

                            else
                            {
                                GManager.instance.commandText.OpenCommandText("The opponent is choosing which effect activates.");

                                #region AIモード
                                if (GManager.instance.IsAI)
                                {
                                    List<int> indexes = new List<int>();

                                    for (int i = 0; i < canSelectEffects.Count; i++)
                                    {
                                        if (!activatedEffects.Contains(canSelectEffects[i]))
                                        {
                                            int k = i;
                                            indexes.Add(k);
                                        }
                                    }

                                    if (indexes.Count >= 1)
                                    {
                                        SetEffectIndex(UnityEngine.Random.Range(0, indexes.Count));
                                    }
                                }
                                #endregion
                            }

                            yield return new WaitWhile(() => !endSelect);
                            endSelect = false;

                            GManager.instance.commandText.CloseCommandText();
                            yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

                            if (0 <= effectIndex && effectIndex <= canSelectEffects.Count - 1)
                            {
                                IEnumerator effect = canSelectEffects[effectIndex]();

                                yield return ContinuousController.instance.StartCoroutine(effect);

                                if (!activatedEffects.Contains(canSelectEffects[effectIndex]))
                                {
                                    activatedEffects.Add(canSelectEffects[effectIndex]);
                                }
                            }
                        }
                    }
                    #endregion

                    #region 名称に「エグザモン」を含むデジモンがいない場合(1つ選択)
                    else
                    {
                        if (card.Owner.isYou)
                        {
                            GManager.instance.commandText.OpenCommandText("Which effect will you activate?");

                            List<Command_SelectCommand> command_SelectCommands = new List<Command_SelectCommand>()
                                {
                                    new Command_SelectCommand("Suspend & gain Pierce", () => photonView.RPC("SetEffectIndex", RpcTarget.All, 0), 0),
                                    new Command_SelectCommand("Unsuspend", () => photonView.RPC("SetEffectIndex", RpcTarget.All, 1), 0),
                                };

                            GManager.instance.selectCommandPanel.SetUpCommandButton(command_SelectCommands);
                        }

                        else
                        {
                            GManager.instance.commandText.OpenCommandText("The opponent is choosing which effect activates.");

                            #region AIモード
                            if (GManager.instance.IsAI)
                            {
                                SetEffectIndex(UnityEngine.Random.Range(0, canSelectEffects.Count));
                            }
                            #endregion
                        }

                        yield return new WaitWhile(() => !endSelect);
                        endSelect = false;

                        GManager.instance.commandText.CloseCommandText();
                        yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

                        if (0 <= effectIndex && effectIndex <= canSelectEffects.Count - 1)
                        {
                            IEnumerator effect = canSelectEffects[effectIndex]();

                            yield return ContinuousController.instance.StartCoroutine(effect);
                        }
                    }
                    #endregion
                }
            }


            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Suspend 1 Digimon and unsuspend 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] Suspend 1 of your opponent's Digimon, and unsuspend 1 of your Digimon.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition1));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition1,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.UnTap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            return cardEffects;
        }

        int effectIndex = 0;
        bool endSelect = false;

        [PunRPC]
        public void SetEffectIndex(int effectIndex)
        {
            this.effectIndex = effectIndex;
            endSelect = true;
        }
    }
}