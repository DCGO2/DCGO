using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.EX6
{
    public class EX6_066 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Main Effect
            
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return
                        "[Main] By placing 1 Digimon card with [Aqua]/[Sea Animal] in one of its traits from your hand as the bottom digivolution card of 1 of your blue Digimon, return all of your opponent's Digimon with the same level as the placed card to the hand.";
                }
                
                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.HasAquaTraits)
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.CardColors.Contains(CardColor.Blue))
                        {
                            if (!permanent.IsToken)
                            {
                                return true;
                            }
                        }
                    }
                    
                    return false;
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    // If there is a Digimon card with Aqua traits in hand and blue Digimon on owner's field
                    if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            CardSource selectedCard = null;
                            
                            int maxCount = 1;
                            
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                            
                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);
                            
                            selectHandEffect.SetUpCustomMessage(
                                "Select 1 card to place at the bottom of digivolution cards.",
                                "The opponent is selecting 1 card to place at the bottom of digivolution cards.");
                            
                            yield return StartCoroutine(selectHandEffect.Activate());
                            
                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCard = cardSource;
                                
                                yield return null;
                            }
                            
                            if (selectedCard != null)
                            {
                                maxCount = Math.Min(1,
                                    CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));
                                
                                SelectPermanentEffect selectPermanentEffect =
                                    GManager.instance.GetComponent<SelectPermanentEffect>();
                                
                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);
                                
                                selectPermanentEffect.SetUpCustomMessage(
                                    "Select 1 Digimon that will get a bottom digivolution card.",
                                    "The opponent is selecting 1 Digimon that will get a bottom digivolution card.");
                                
                                yield return ContinuousController.instance.StartCoroutine(
                                    selectPermanentEffect.Activate());
                                
                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    Permanent selectedPermanent = permanent;
                                    
                                    if (selectedPermanent != null)
                                    {
                                        // Add chosen card to Digimon's sources
                                        yield return ContinuousController.instance.StartCoroutine(
                                            selectedPermanent.AddDigivolutionCardsBottom(
                                                new List<CardSource>() { selectedCard }, activateClass));
                                        
                                        bool PermanentConditionEnemy(Permanent enemyPermanent)
                                        {
                                            return enemyPermanent.Level == selectedCard.Level;
                                        }
                                        
                                        // Return all opponent's Digimon with the same level as the chosen card to their hand
                                        List<Permanent> bounceTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons()
                                            .Filter(PermanentConditionEnemy);
                                        yield return ContinuousController.instance.StartCoroutine(new HandBounceClaass(bounceTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Bounce());
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            #endregion
            
            #region Security Effect
            
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Return all of your opponent's Digimon with the lowest level to the hand.", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return "[Security] Return all of your opponent's Digimon with the lowest level to the hand.";
                }
                
                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy);
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> bounceTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(PermanentCondition);
                    yield return ContinuousController.instance.StartCoroutine(new HandBounceClaass(bounceTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Bounce());
                }
            }
            #endregion
            
            return cardEffects;
        }
    }
}