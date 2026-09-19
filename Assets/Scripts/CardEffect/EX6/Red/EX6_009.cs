using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.EX6
{
    public class EX6_009 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Alternate Digivolution
            
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 4)
                    {
                        return targetPermanent.TopCard.ContainsTraits("Legend-Arms");
                    }
                    
                    return false;
                }
                
                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            
            #endregion
            
            #region Hand - Main
            
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain [Security Attack +1]", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return
                        "[Hand] [Main] By paying 2 cost and placing this card as the bottom digivolution card of 1 of your Digimon that's level 5 or has the [Legend-Arms] trait, that Digimon gains [Security Attack +1] for the turn.";
                }
                
                bool IsLevel5OrHasLegendArmsTrait(Permanent targetPermanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(targetPermanent, card))
                    {
                        if (targetPermanent.TopCard.HasLevel && targetPermanent.Level == 5)
                            return true;
                        
                        if (targetPermanent.TopCard.ContainsTraits("Legend-Arms"))
                            return true;
                    }
                    
                    return false;
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnHand(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsLevel5OrHasLegendArmsTrait))
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;
                    
                    if (CardEffectCommons.HasMatchConditionPermanent(IsLevel5OrHasLegendArmsTrait))
                    {
                        int maxCount = Math.Min(1,
                            CardEffectCommons.MatchConditionPermanentCount(IsLevel5OrHasLegendArmsTrait));
                        
                        SelectPermanentEffect selectPermanentEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();
                        
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsLevel5OrHasLegendArmsTrait,
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
                        
                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                card.Owner.AddMemory(-2, activateClass));
                            
                            yield return ContinuousController.instance.StartCoroutine(
                                selectedPermanent.AddDigivolutionCardsBottom(
                                    new List<CardSource>() { card },
                                    activateClass));
                            
                            yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.ChangeDigimonSAttack(
                                    targetPermanent: selectedPermanent,
                                    changeValue: 1,
                                    effectDuration: EffectDuration.UntilEachTurnEnd,
                                    activateClass: activateClass));
                        }
                    }
                }
            }
            
            #endregion
            
            #region Your Turn - Place Digivolution
            
            if (timing == EffectTiming.OnAddDigivolutionCards)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain <Raid> and <Piercing>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false,
                    EffectDiscription());
                activateClass.SetHashString("GainEffects_EX6_009");
                cardEffects.Add(activateClass);
                
                string EffectDiscription()
                {
                    return
                        "[Your Turn] [Once Per Turn] When an effect places a digivolution card under this Digimon, it gains <Raid> and <Piercing> for the turn.";
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnAddDigivolutionCard(
                                    hashtable: hashtable,
                                    permanentCondition: permanent => permanent == card.PermanentOfThisCard(),
                                    cardEffectCondition: cardEffect => cardEffect.EffectSourceCard != null,
                                    cardCondition: null))
                            {
                                return true;
                            }
                        }
                    }
                    
                    return false;
                }
                
                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRaid(
                        targetPermanent: card.PermanentOfThisCard(),
                        effectDuration: EffectDuration.UntilEachTurnEnd,
                        activateClass: activateClass));
                    
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainPierce(
                        targetPermanent: card.PermanentOfThisCard(),
                        effectDuration: EffectDuration.UntilEachTurnEnd,
                        activateClass: activateClass));
                }
            }
            
            #endregion
            
            #region Your Turn - ESS
            
            if (timing == EffectTiming.OnAttackTargetChanged)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash the top card of opponent's security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false,
                    EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("TrashSecurity_BT11_014");
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return
                        "[Your Turn][Once Per Turn] When this Digimon's attack target is switched, trash the top card of your opponent's security stack.";
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerOnAttackTargetSwitch(hashtable, card))
                        {
                            if (CardEffectCommons.IsOwnerTurn(card))
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
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                }
            }
            
            #endregion
            
            return cardEffects;
        }
    }
}