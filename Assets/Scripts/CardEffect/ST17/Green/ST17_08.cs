using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.ST17
{
    public class ST17_08 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Rapidmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, 
                    digivolutionCost: 5, 
                    ignoreDigivolutionRequirement: true, 
                    card: card, 
                    condition: null));
            }
            #endregion

            #region Blocker and Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Counter Timing
            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }
            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend Digimons and Tamers and give effects", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetHashString("SuspendAndGiveEffects_ST17_08");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Suspend 2 of your opponent's Digimon and/or Tamers. Then, 2 of your opponent's Digimon or Tamers can't unsuspend or digivolve until the end of your opponent's turn.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        if (permanent.IsDigimon)
                        {
                            return true;
                        }

                        if (permanent.IsTamer)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        if (permanent.IsDigimon)
                        {
                            return true;
                        }

                        if (permanent.IsTamer)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                    {
                        int maxCount = Math.Min(2, card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: CanEndSelectCondition,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
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

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                    {
                        int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition1));

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
                         
                        selectPermanentEffect.SetUpCustomMessage("Select 2 Digimon or Tamers that will not unsuspend or digivolve.", "The opponent is selecting 2 Digimon that will not unsuspend or digivolve.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            CanNotDigivolveClass canNotPutFieldClass = new CanNotDigivolveClass();
                            canNotPutFieldClass.SetUpICardEffect("Can't Digivolve", CanUseCondition1, card);
                            canNotPutFieldClass.SetUpCanNotEvolveClass(permanentCondition: PermanentCondition, cardCondition: CardCondition);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

                            ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().DebuffSE);

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                return true;
                            }

                            bool PermanentCondition(Permanent permanent)
                            {
                                if (permanent == selectedPermanent)
                                {
                                    if (permanent.TopCard != null)
                                    {
                                        if (permanent.IsDigimon || permanent.IsTamer)
                                        {
                                            if (!permanent.TopCard.CanNotBeAffected(canNotPutFieldClass))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }

                                return false;
                            }

                            bool CardCondition(CardSource cardSource)
                            {

                                if (cardSource.Owner == card.Owner.Enemy)
                                {
                                    if (cardSource.IsDigimon || cardSource.IsTamer)
                                    {
                                        if (!cardSource.CanNotBeAffected(canNotPutFieldClass))
                                        {
                                            return true;
                                        }
                                    }
                                }

                                return false;
                                
                            }

                            ICardEffect GetCardEffect(EffectTiming _timing)
                            {
                                if (_timing == EffectTiming.None)
                                {
                                    return canNotPutFieldClass;
                                }

                                return null;
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCantUnsuspendUntilOpponentTurnEnd(
                                        targetPermanent: selectedPermanent,
                                        activateClass: activateClass
                                    ));

                        }
                    }
                }
            }
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Unsuspend_ST17_08");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may unsuspend this Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable,card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard()))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass).Unsuspend());
                }

            }


            #endregion

            #region End of Attack

            if(timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Unsuspend_ST17_08");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Attack] You may unsuspend this Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnAttack(hashtable, card))
                            {
                                if (CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard()))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass).Unsuspend());
                }
            }

            #endregion


            return cardEffects;
        }
    }
}