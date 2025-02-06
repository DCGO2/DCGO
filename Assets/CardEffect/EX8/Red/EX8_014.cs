using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX8
{
    public class EX8_014 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.ContainsTraits("Dinosaur"))
                    {
                        if (targetPermanent.TopCard.HasLevel)
                        {
                            if (targetPermanent.Level == 4)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Fortitude
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.FortitudeSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend and delete an opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] You may suspend 1 Digimon. Then, if this Digimon is suspended, delete 1 of your opponent's Digimon with 8000 DP or less.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) || CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        return true;
                    }
                    return false;
                }

                bool CanSelectOpponentPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(8000, activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                        };

                    string selectPlayerMessage = "Will you suspend 1 Digimon?";
                    string notSelectPlayerMessage = "The opponent is choosing whether or not to suspend a Digimon.";

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool willSuspend = GManager.instance.userSelectionManager.SelectedBoolValue;

                    if (willSuspend)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                        {

                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(PermanentCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: PermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Tap,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to suspend.", "The opponent is selecting 1 Digimon to suspend.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        }
                    }

                    if(card.PermanentOfThisCard().IsSuspended && CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentCondition))
                    {
                        List<Permanent> deleteTargetPermanents = new List<Permanent>();

                        int maxCount = Math.Min(1,
                             CardEffectCommons.MatchConditionPermanentCount(CanSelectOpponentPermanentCondition));
                   
                         SelectPermanentEffect selectPermanentEffect =
                             GManager.instance.GetComponent<SelectPermanentEffect>();
                   
                         selectPermanentEffect.SetUp(
                             selectPlayer: card.Owner,
                             canTargetCondition: CanSelectOpponentPermanentCondition,
                             canTargetCondition_ByPreSelecetedList: null,
                             canEndSelectCondition: null,
                             maxCount: maxCount,
                             canNoSelect: false,
                             canEndNotMax: false,
                             selectPermanentCoroutine: null,
                             afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                             mode: SelectPermanentEffect.Mode.Custom,
                             cardEffect: activateClass);
                   
                         selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.",
                             "The opponent is selecting 1 Digimon to delete.");
                   
                         yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                   
                         IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                         {
                             deleteTargetPermanents = permanents.Clone();
                             yield return null;
                         }

                         yield return ContinuousController.instance.StartCoroutine(
                             CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                 targetPermanents: deleteTargetPermanents, activateClass: activateClass,
                                 successProcess: null, failureProcess: null));
                    }

                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend and delete an opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may suspend 1 Digimon. Then, if this Digimon is suspended, delete 1 of your opponent's Digimon with 8000 DP or less.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) || CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        return true;
                    }
                    return false;
                }

                bool CanSelectOpponentPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(8000, activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                        };

                    string selectPlayerMessage = "Will you suspend 1 Digimon?";
                    string notSelectPlayerMessage = "The opponent is choosing whether or not to suspend a Digimon.";

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool willSuspend = GManager.instance.userSelectionManager.SelectedBoolValue;

                    if (willSuspend)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                        {

                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(PermanentCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: PermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Tap,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to suspend.", "The opponent is selecting 1 Digimon to suspend.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        }
                    }

                    if (card.PermanentOfThisCard().IsSuspended && CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentCondition))
                    {
                        List<Permanent> deleteTargetPermanents = new List<Permanent>();

                        int maxCount = Math.Min(1,
                             CardEffectCommons.MatchConditionPermanentCount(CanSelectOpponentPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOpponentPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.",
                            "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            deleteTargetPermanents = permanents.Clone();
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                targetPermanents: deleteTargetPermanents, activateClass: activateClass,
                                successProcess: null, failureProcess: null));
                    }

                }
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}