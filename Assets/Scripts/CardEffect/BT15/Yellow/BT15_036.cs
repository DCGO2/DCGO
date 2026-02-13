using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT15
{
    public class BT15_036 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #region On Deletion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash a security to give opponent's Digimon -6000 DP.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] By trashing the top or bottom card of your security stack, 1 of your opponent's Digimon gets -6000 DP until the end of the opponent's turn.";
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanActivateOnDeletion(card))
                    {
                        if (card.Owner.SecurityCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanSelectPermanent(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        return true;
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                    {
                        new SelectionElement<bool>(message: $"Security Top", value : true, spriteIndex: 0),
                        new SelectionElement<bool>(message: $"Security Bottom", value : false, spriteIndex: 1),
                    };

                        string selectPlayerMessage = "Which will you trash the top or bottom card of the security?";
                        string notSelectPlayerMessage = "The opponent is selecting whether to trash the top or bottom card of security.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool fromTop = GManager.instance.userSelectionManager.SelectedBoolValue;

                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                card.Owner,
                                1,
                                activateClass,
                                fromTop).DestroySecurity());

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanent))
                        {
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanent));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanent,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -6000.", "The opponent is selecting 1 Digimon that will get DP -6000.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                    targetPermanent: permanent,
                                    changeValue: -6000,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                    activateClass: activateClass));
                            }
                        }
                    }
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash a security to give opponent's Digimon -6000 DP.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By trashing the top or bottom card of your security stack, 1 of your opponent's Digimon gets -6000 DP until the end of the opponent's turn.";
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.SecurityCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanSelectPermanent(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        return true;
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                    {
                        new SelectionElement<bool>(message: $"Security Top", value : true, spriteIndex: 0),
                        new SelectionElement<bool>(message: $"Security Bottom", value : false, spriteIndex: 1),
                    };

                        string selectPlayerMessage = "Which will you trash the top or bottom card of the security?";
                        string notSelectPlayerMessage = "The opponent is selecting whether to trash the top or bottom card of security.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool fromTop = GManager.instance.userSelectionManager.SelectedBoolValue;

                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                card.Owner,
                                1,
                                activateClass,
                                fromTop).DestroySecurity());

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanent))
                        {
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanent));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanent,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -6000.", "The opponent is selecting 1 Digimon that will get DP -6000.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                    targetPermanent: permanent,
                                    changeValue: -6000,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                    activateClass: activateClass));
                            }
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}