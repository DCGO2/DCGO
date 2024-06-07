using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.BT16
{
    public class MaloMyotismon_BT16_081 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region When Digivolving / When Attacking
            if (timing == EffectTiming.OnEnterFieldAnyone || timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete your Digimon or Tamer to delete an opponent's unsuspended Digimon. Otherwise delete 1 of your opponent's tamers.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] [When Attacking] By deleting one of your Digimon or Tamers, delete 1 of your opponent's unsuspended Digimon. If no Digimon is deleted by this effect, delete one of your opponent's tamers.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if(CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent,card))
                    {
                        if(permanent.IsDigimon || permanent.IsTamer)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanSelectOpponentUnsuspendedDigimon(Permanent permanent)
                {
                    if(CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent,card))
                    {
                        if(permanent.IsDigimon && !permanent.IsSuspended)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanSelectOpponentTamer(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent,card))
                    {
                        if(permanent.IsTamer)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card) || CardEffectCommons.CanTriggerOnAttack(hashtable,card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<Permanent> deleteTargetPermanents = new List<Permanent>();

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
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 of your Digimon or Tamers to delete.", "The opponent is selecting 1 Digimon or Tamer to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            foreach (Permanent permanent in permanents)
                            {
                                deleteTargetPermanents.Add(permanent);
                            }

                            yield return null;
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: deleteTargetPermanents, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        bool attemptedToDeletedUnsuspended = false;


                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentUnsuspendedDigimon))
                            {
                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectOpponentUnsuspendedDigimon));

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectOpponentUnsuspendedDigimon,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                                {
                                    foreach (Permanent permanent in permanents)
                                    {
                                        deleteTargetPermanents.Add(permanent);
                                    }

                                    yield return null;
                                }


                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: deleteTargetPermanents, activateClass: activateClass, successProcess: null, failureProcess: FailureProcess));

                                attemptedToDeletedUnsuspended = true;

                                IEnumerator FailureProcess()
                                {
                                    if (CardEffectCommons.IsExistOnBattleArea(card))
                                    {
                                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentTamer))
                                        {
                                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectOpponentTamer));

                                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                            selectPermanentEffect.SetUp(
                                                selectPlayer: card.Owner,
                                                canTargetCondition: CanSelectOpponentTamer,
                                                canTargetCondition_ByPreSelecetedList: null,
                                                canEndSelectCondition: null,
                                                maxCount: maxCount,
                                                canNoSelect: false,
                                                canEndNotMax: false,
                                                selectPermanentCoroutine: null,
                                                afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                                                mode: SelectPermanentEffect.Mode.Custom,
                                                cardEffect: activateClass);

                                            selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer to delete.", "The opponent is selecting 1 Tamer to delete.");

                                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                            IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                                            {
                                                foreach (Permanent permanent in permanents)
                                                {
                                                    deleteTargetPermanents.Add(permanent);
                                                }

                                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: deleteTargetPermanents, activateClass: activateClass, successProcess: null, failureProcess: null));
                                            }
                                        }
                                    }
                                }
                            }

                        if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentUnsuspendedDigimon) && CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentTamer) && attemptedToDeletedUnsuspended == false)
                        {

                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectOpponentTamer));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectOpponentTamer,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer to delete.", "The opponent is selecting 1 Tamer to delete.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                            {
                                foreach (Permanent permanent in permanents)
                                {
                                    deleteTargetPermanents.Add(permanent);
                                }

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: deleteTargetPermanents, activateClass: activateClass, successProcess: null, failureProcess: null));
                            }

                        }
                        

                        
                        
                    }

                    
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("When one of your Digimon or Tamers are deleted by an effect, trash the top card of your opponent's security.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("TrashSecurity_BT16_081");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When one of your Digimon or Tamers is deleted by an effect, trash the top card of your opponent's security stack.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if(CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if(permanent.IsDigimon || permanent.IsTamer)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermanentCondition))
                        {
                            if (CardEffectCommons.IsByEffect(hashtable, null))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        card.Owner.Enemy,
                        1,
                        activateClass,
                        true).DestroySecurity());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}