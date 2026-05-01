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

                        if (canSelectEffects.Count((effect) => !activatedEffects.Contains(effect)) >= 2)
                        {
                            List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                            {
                                new SelectionElement<int>(message: $"Memory +1", value : 0, spriteIndex: 0),
                                new SelectionElement<int>(message: $"DP +2000", value : 1, spriteIndex: 0),
                                new SelectionElement<int>(message: $"Delete Digimons", value : 2, spriteIndex: 0),
                            };

                            string selectPlayerMessage = "Which effect will you activate?";
                            string notSelectPlayerMessage = "The opponent is choosing which effect activates.";

                            if (canSelectEffects.Count((effect) => !activatedEffects.Contains(effect)) == 3)
                            {
                                selectPlayerMessage = "Which effect will you activate the first?";
                            }

                            else if (canSelectEffects.Count((effect) => !activatedEffects.Contains(effect)) == 2)
                            {
                                selectPlayerMessage = "Which effect will you activate the second?";
                            }

                            GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        }
                        else
                        {
                            for (int i = 0; i < canSelectEffects.Count; i++)
                            {
                                if (!activatedEffects.Contains(canSelectEffects[i]))
                                {
                                    GManager.instance.userSelectionManager.SetInt(i);
                                    break;
                                }
                            }
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        int effectIndex = GManager.instance.userSelectionManager.SelectedIntValue;

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
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                    {
                        new SelectionElement<int>(message: $"Memory +1", value : 0, spriteIndex: 0),
                        new SelectionElement<int>(message: $"DP +2000", value : 1, spriteIndex: 0),
                        new SelectionElement<int>(message: $"Delete Digimons", value : 2, spriteIndex: 0),
                    };

                    string selectPlayerMessage = "Which effect will you activate?";
                    string notSelectPlayerMessage = "The opponent is choosing which effect activates.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    int effectIndex = GManager.instance.userSelectionManager.SelectedIntValue;

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
}
