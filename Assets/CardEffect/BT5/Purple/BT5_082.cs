using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT5_082 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnAllyAttack)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Activate Effects", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Attacking] Activate 1 of the effects below. If you have no other Digimon in play, activate all of the effects below instead. - Gain 1 memory. - This Digimon gets +2000 DP for the turn. - Delete up to 3 of your opponent's level 3 Digimon.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    return true;
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                #region メモリー+1
                Func<IEnumerator> _Gain1Memory = () => Gain1Memory();

                IEnumerator Gain1Memory()
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
                #endregion

                #region DP+2000
                Func<IEnumerator> _DP2000Plus = () => DP2000Plus();

                IEnumerator DP2000Plus()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: card.PermanentOfThisCard(), changeValue: 2000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                    }
                }
                #endregion

                #region レベル3デジモン消滅
                Func<IEnumerator> _DeleteDigimons = () => DeleteDigimons();

                IEnumerator DeleteDigimons()
                {
                    bool CanSelectPermanentCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                        {
                            if (permanent.Level == 3)
                            {
                                if (permanent.TopCard.HasLevel)
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(3, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: CanEndSelectCondition,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: true,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        bool CanEndSelectCondition(List<Permanent> permanents)
                        {
                            if (CardEffectCommons.HasNoElement(permanents))
                            {
                                return false;
                            }

                            return true;
                        }
                    }
                }
                #endregion

                List<Func<IEnumerator>> canSelectEffects = new List<Func<IEnumerator>>() { _Gain1Memory, _DP2000Plus, _DeleteDigimons };

                #region 他のデジモンがいない場合(順番を選択)
                if (card.Owner.GetBattleAreaDigimons().Count((permanent) => permanent != card.PermanentOfThisCard()) == 0)
                {
                    List<Func<IEnumerator>> activatedEffects = new List<Func<IEnumerator>>();

                    while (canSelectEffects.Count((effect) => !activatedEffects.Contains(effect)) >= 1)
                    {
                        yield return GManager.instance.photonWaitController.StartWait("Tactimon_selectFirst");

                        if (card.Owner.isYou)
                        {
                            if (canSelectEffects.Count((effect) => !activatedEffects.Contains(effect)) >= 2)
                            {
                                if (canSelectEffects.Count((effect) => !activatedEffects.Contains(effect)) == 3)
                                {
                                    GManager.instance.commandText.OpenCommandText("Which effect will you activate the first?");
                                }

                                else if (canSelectEffects.Count((effect) => !activatedEffects.Contains(effect)) == 2)
                                {
                                    GManager.instance.commandText.OpenCommandText("Which effect will you activate the second?");
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
                                                message = "Memory +1";
                                                break;

                                            case 1:
                                                message = "DP +2000";
                                                break;

                                            case 2:
                                                message = "Delete Digimons";
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

                #region 他のデジモンがいる場合(1つ選択)
                else
                {
                    if (card.Owner.isYou)
                    {
                        GManager.instance.commandText.OpenCommandText("Which effect will you activate?");

                        List<Command_SelectCommand> command_SelectCommands = new List<Command_SelectCommand>()
                                {
                                    new Command_SelectCommand("Memory +1", () => photonView.RPC("SetEffectIndex", RpcTarget.All, 0), 0),
                                    new Command_SelectCommand("DP +2000", () => photonView.RPC("SetEffectIndex", RpcTarget.All, 1), 0),
                                    new Command_SelectCommand("Delete Digimons", () => photonView.RPC("SetEffectIndex", RpcTarget.All, 2), 0),
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
