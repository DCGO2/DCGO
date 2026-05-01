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

                            if (canSelectEffects.Count((effect) => !activatedEffects.Contains(effect)) >= 2)
                            {
                                List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                                {
                                    new SelectionElement<int>(message: $"S Attack +1", value : 0, spriteIndex: 0),
                                    new SelectionElement<int>(message: $"Draw 2", value : 1, spriteIndex: 0),
                                };

                                string selectPlayerMessage = "Which effect will you activate the first?";
                                string notSelectPlayerMessage = "The opponent is choosing which effect activates.";

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

                    #region シャウトモンX5がいない場合(1つ選択

                    else
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                            {
                                new SelectionElement<int>(message: $"S Attack +1", value : 0, spriteIndex: 0),
                                new SelectionElement<int>(message: $"Draw 2", value : 1, spriteIndex: 0),
                            };

                        string selectPlayerMessage = "Which effect will you activate?";
                        string notSelectPlayerMessage = "The opponent is choosing which effect activates.";

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        int effectIndex = GManager.instance.userSelectionManager.SelectedIntValue;

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
    }
}