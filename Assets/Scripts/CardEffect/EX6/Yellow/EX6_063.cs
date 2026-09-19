using System.Collections;
using System.Collections.Generic;
using System;

namespace DCGO.CardEffects.EX6
{
    public class EX6_063 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Start of Your Main Phase/ On Play Shared
            
            bool IsOwnerYellowDigimonSharedCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    return permanent.TopCard.CardColors.Contains(CardColor.Yellow);
                }
                
                return false;
            }
            
            bool CanActivateSharedCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsOwnerYellowDigimonSharedCondition))
                    {
                        return true;
                    }
                }
                
                return false;
            }
            
            #endregion
            
            #region On Play
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "1 of your yellow Digimon gains [Barrier] until the end of your opponent's turn.", CanUseCondition,
                    card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);
            
                string EffectDescription()
                {
                    return
                        "[On Play] 1 of your yellow Digimon gains [Barrier] until the end of your opponent's turn.";
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(IsOwnerYellowDigimonSharedCondition))
                        {
                            Permanent selectedPermanent = null;

                            int maxCount = Math.Min(1,
                                CardEffectCommons.MatchConditionPermanentCount(IsOwnerYellowDigimonSharedCondition));
                            
                            SelectPermanentEffect selectPermanentEffect =
                                GManager.instance.GetComponent<SelectPermanentEffect>();
                            
                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsOwnerYellowDigimonSharedCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);
                            
                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will gain Barrier.",
                                "The opponent is selecting 1 Digimon that will gain Barrier.");
                            
                            yield return ContinuousController.instance.StartCoroutine(
                                selectPermanentEffect.Activate());
                            
                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;
                                yield return null;
                            }

                            if(selectedPermanent != null)
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBarrier(targetPermanent: selectedPermanent, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                        }
                    }
                }
            }
            
            #endregion
            
            #region Start of Your Main Phase
            
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "1 of your yellow Digimon gains [Barrier] until the end of your opponent's turn.", CanUseCondition,
                    card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);
            
                string EffectDescription()
                {
                    return
                        "[Start Of Your Main Phase] 1 of your yellow Digimon gains [Barrier] until the end of your opponent's turn.";
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(IsOwnerYellowDigimonSharedCondition))
                        {
                            Permanent selectedPermanent = null;

                            int maxCount = Math.Min(1,
                                CardEffectCommons.MatchConditionPermanentCount(IsOwnerYellowDigimonSharedCondition));

                            SelectPermanentEffect selectPermanentEffect =
                                GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsOwnerYellowDigimonSharedCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will gain Barrier.",
                                "The opponent is selecting 1 Digimon that will gain Barrier.");

                            yield return ContinuousController.instance.StartCoroutine(
                                selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;
                                yield return null;
                            }

                            if (selectedPermanent != null)
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBarrier(targetPermanent: selectedPermanent, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                        }
                    }
                }
            }
            
            #endregion
            
            #region Your Turn
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain 1 Memory", CanUseCondition,
                    card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true,
                    EffectDescription());
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return "[Your Turn] When one of your Digimon is played or digivolves, if it has the [Angel]/[Archangel]/[Three Great Angels] trait, by suspending this Tamer, gain 1 memory.";
                }
                
                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        return permanent.TopCard.HasAngelTraitRestrictive;
                    }
                    
                    return false;
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition) ||
                                CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition))
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
                        if (CardEffectCommons.CanActivateSuspendCostEffect(card))
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() },
                            CardEffectCommons.CardEffectHashtable(activateClass)).Tap());
                    
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }
            
            #endregion
            
            #region Security Effect
            
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            
            #endregion
            
            return cardEffects;
        }
    }
}