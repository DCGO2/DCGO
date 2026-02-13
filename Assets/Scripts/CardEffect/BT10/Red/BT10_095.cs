using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.BT10
{
    public class BT10_095 : CEntity_Effect
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
                    return "[Main] Activate 1 of the effects below. If you have a Digimon with [Shoutmon X5] in its name in play, activate all of the effects below instead. - 1 of your Digimon with [Xros Heart] in its traits gains <Security Attack +1> for the turn. - <Draw 2> (Draw 2 cards from your deck.)";
                }
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    #region セキュリティアタック

                    Func<IEnumerator> _SecurityAttack1Plus = () => SecurityAttack1Plus();

                    IEnumerator SecurityAttack1Plus()
                    {
                        bool CanSelectPermanentCondition(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                            {
                                if (permanent.TopCard.CardTraits.Contains("Xros Heart") || permanent.TopCard.CardTraits.Contains("XrosHeart"))
                                {
                                    return true;
                                }
                            }

                            return false;
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
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get Security Attack +1.", "The opponent is selecting 1 Digimon that will get Security Attack +1.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(targetPermanent: permanent, changeValue: 1, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                            }
                        }
                    }

                    #endregion

                    #region 2ドロー

                    Func<IEnumerator> _Draw2 = () => Draw2();

                    IEnumerator Draw2()
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 2, activateClass).Draw());
                    }

                    #endregion

                    List<Func<IEnumerator>> canSelectEffects = new List<Func<IEnumerator>>() { _SecurityAttack1Plus, _Draw2 };

                    #region シャウトモンX5がいる場合(順番を選択

                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && permanent.TopCard.ContainsCardName("Shoutmon X5")))
                    {
                        List<Func<IEnumerator>> activatedEffects = new List<Func<IEnumerator>>();

                        while (canSelectEffects.Count((effect) => !activatedEffects.Contains(effect)) >= 1)
                        {
                            yield return GManager.instance.photonWaitController.StartWait("FlyingHero_selectFirst");

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
                                                    message = "S Attack +1";
                                                    break;

                                                case 1:
                                                    message = "Draw 2";
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

                    #region シャウトモンX5がいない場合(1つ選択

                    else
                    {
                        if (card.Owner.isYou)
                        {
                            GManager.instance.commandText.OpenCommandText("Which effect will you activate?");

                            List<Command_SelectCommand> command_SelectCommands = new List<Command_SelectCommand>()
                                {
                                    new Command_SelectCommand("S Attack +1", () => photonView.RPC("SetEffectIndex", RpcTarget.All, 0), 0),
                                    new Command_SelectCommand("Draw 2", () => photonView.RPC("SetEffectIndex", RpcTarget.All, 1), 0),
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
                activateClass.SetUpICardEffect($"Add this card to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] Add this card to its owner's hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
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