using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.EX6
{
    public class EX6_010 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Alternate Digivolution
            
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 5)
                    {
                        return targetPermanent.TopCard.ContainsTraits("Legend-Arms");
                    }
                    
                    return false;
                }
                
                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 4,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            
            #endregion
            
            #region Raid
            
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card,
                    condition: null));
            }
            
            #endregion
            
            #region Piercing
            
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(
                    CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
                
                cardEffects.Add(
                    CardEffectFactory.PierceSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }
            
            #endregion
            
            #region Hand - Main
            
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Delete 1 of your opponent's Digimon with as much or less DP as that Digimon", CanUseCondition,
                    card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetIsDigimonEffect(true);
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return
                        "[Hand] [Main] By paying 3 cost and placing this card as the bottom digivolution card of 1 of your Digimon that's level 6 or has the [Legend-Arms] trait, delete 1 of your opponent's Digimon with as much or less DP as that Digimon.";
                }
                
                bool IsLevel6OrHasLegendArmsTrait(Permanent targetPermanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(targetPermanent, card))
                    {
                        if (targetPermanent.TopCard.HasLevel && targetPermanent.Level == 6)
                            return true;
                        
                        if (targetPermanent.TopCard.ContainsTraits("Legend-Arms"))
                            return true;
                    }
                    
                    return false;
                }
                
                bool IsEnemyPermanentWithAsMuchOrLessDp(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (CardEffectCommons.IsExistOnBattleArea(card))
                        {
                            if (permanent.DP <= card.PermanentOfThisCard().DP)
                            {
                                return true;
                            }
                        }
                    }
                    
                    return false;
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnHand(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsLevel6OrHasLegendArmsTrait))
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;
                    
                    if (CardEffectCommons.HasMatchConditionPermanent(IsLevel6OrHasLegendArmsTrait))
                    {
                        int maxCount = Math.Min(1,
                            CardEffectCommons.MatchConditionPermanentCount(IsLevel6OrHasLegendArmsTrait));
                        
                        SelectPermanentEffect selectPermanentEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();
                        
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsLevel6OrHasLegendArmsTrait,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);
                        
                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to add bottom digivolution source.",
                            "The opponent is selecting 1 Digimon to add bottom digivolution source.");
                        
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        
                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }
                    }
                    
                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            card.Owner.AddMemory(-3, activateClass));
                        
                        yield return ContinuousController.instance.StartCoroutine(
                            selectedPermanent.AddDigivolutionCardsBottom(
                                new List<CardSource>() { card },
                                activateClass));
                        
                        if (CardEffectCommons.HasMatchConditionPermanent(IsEnemyPermanentWithAsMuchOrLessDp))
                        {
                            int enemyCount = Math.Min(1,
                                CardEffectCommons.MatchConditionPermanentCount(IsEnemyPermanentWithAsMuchOrLessDp));
                            
                            SelectPermanentEffect selectEnemyEffect =
                                GManager.instance.GetComponent<SelectPermanentEffect>();
                            
                            selectEnemyEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsEnemyPermanentWithAsMuchOrLessDp,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: enemyCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectEnemyEffect.Activate());
                        }
                    }
                }
            }
            
            #endregion
            
            #region When Digivolving
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true,
                    EffectDescription());
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return
                        "[When Digivolving]  1 of your Digimon may attack.";
                }
                
                bool IsYourDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
                
                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;
                    
                    if (CardEffectCommons.HasMatchConditionPermanent(IsYourDigimon))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsYourDigimon));
                        
                        SelectPermanentEffect selectPermanentEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();
                        
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsYourDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);
                        
                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will attack.",
                            "The opponent is selecting 1 Digimon that will attack.");
                        
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        
                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }
                    }
                    
                    if (selectedPermanent != null)
                    {
                        if (selectedPermanent.CanAttack(activateClass))
                        {
                            SelectAttackEffect selectAttackEffect =
                                GManager.instance.GetComponent<SelectAttackEffect>();
                            
                            selectAttackEffect.SetUp(
                                attacker: selectedPermanent,
                                canAttackPlayerCondition: () => true,
                                defenderCondition: (permanent) => true,
                                cardEffect: activateClass);
                            
                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                        }
                    }
                }
            }
            
            #endregion
            
            #region Your Turn - ESS
            
            if (timing == EffectTiming.None)
            {
                DisableEffectClass invalidationClass = new DisableEffectClass();
                invalidationClass.SetUpICardEffect("Ignore Security Effect", CanUseCondition, card);
                invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
                invalidationClass.SetIsInheritedEffect(true);
                
                cardEffects.Add(invalidationClass);
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                bool InvalidateCondition(ICardEffect cardEffect)
                {
                    if (cardEffect != null)
                    {
                        if (cardEffect.EffectSourceCard != null)
                        {
                            if (cardEffect.IsSecurityEffect)
                            {
                                if (GManager.instance.attackProcess.AttackingPermanent ==
                                    card.PermanentOfThisCard())
                                {
                                    if (card.PermanentOfThisCard().TopCard.EqualsCardName("RagnaLoardmon"))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }
            }
            
            #endregion
            
            return cardEffects;
        }
    }
}