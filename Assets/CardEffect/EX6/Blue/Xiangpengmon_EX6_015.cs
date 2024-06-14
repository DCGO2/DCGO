using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.EX6
{
    public class Xiangpengmon_EX6_015 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Rule Text
            
            if (timing == EffectTiming.None)
            {
                // Trait Rule Aquatic Type
                ChangeTraitsClass changeTraitsClass = new ChangeTraitsClass();
                changeTraitsClass.SetUpICardEffect("Trait: Has [Aquatic] Type", CanUseCondition, card);
                changeTraitsClass.SetUpChangeTraitsClass(changeeTraits: ChangeTraits);
                cardEffects.Add(changeTraitsClass);
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }
                
                List<string> ChangeTraits(CardSource cardSource, List<string> cardTraits)
                {
                    if (cardSource == card)
                    {
                        cardTraits.Add("Aquatic");
                    }
                    
                    return cardTraits;
                }
            }
            
            #endregion
            
            #region On Play/When Digivolving Shared
            
            string EffectSharedDescription()
            {
                return
                    "[On Play] [When Digivolving] You may place up to 3 of your other blue Digimon as this Digimon's bottom digivolution cards. Then, return all other level 4 or lower Digimon to the hand. For each card placed in this Digimon's digivolution cards, add 1 to the level this effect may return.";
            }
            
            bool CanSelectSharedOwnPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    if (permanent != card.PermanentOfThisCard())
                    {
                        if (permanent.TopCard.CardColors.Contains(CardColor.Blue))
                        {
                            if (!permanent.TopCard.Equals(card))
                                return true;
                        }
                    }
                }
                
                return false;
            }
            
            bool CanActivateSharedCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (!card.PermanentOfThisCard().IsToken)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSharedOwnPermanentCondition))
                        {
                            return true;
                        }
                    }
                }
                
                return false;
            }
            
            #endregion
            
            #region On Play/ When Digivolving
            
            // On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "You may place up to 3 of your other blue Digimon as this Digimon's bottom digivolution cards. Then, return all other level 4 or lower Digimon to the hand. For each card placed in this Digimon's digivolution cards, add 1 to the level this effect may return.",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, true,
                    EffectSharedDescription());
                cardEffects.Add(activateClass);
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CanActivateSharedCondition(hashtable))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();
                        
                        // If there is other blue Digimon in owner Battle Area  
                        int maxCount = Math.Min(3,
                            CardEffectCommons.MatchConditionPermanentCount(CanSelectSharedOwnPermanentCondition));
                        
                        SelectPermanentEffect selectPermanentEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();
                        
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectSharedOwnPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: CanEndSelectCondition,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: true,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);
                        
                        selectPermanentEffect.SetUpCustomMessage(
                            "Select up to 3 Digimon to place on bottom of digivolution cards.",
                            "The opponent is selecting up to 3 Digimon to place on bottom of digivolution cards.");
                        
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        
                        bool CanEndSelectCondition(List<Permanent> permanents)
                        {
                            if (CardEffectCommons.HasNoElement(permanents))
                            {
                                return false;
                            }
                            
                            return true;
                        }
                        
                        IEnumerator SelectPermanentCoroutine(Permanent selectedPermanent)
                        {
                            selectedCards.Add(selectedPermanent.TopCard);
                            
                            yield return null;
                        }
                        
                        if (selectedCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard()
                                .AddDigivolutionCardsBottom(
                                    selectedCards,
                                    activateClass));
                            
                            bool PermanentConditionEnemy(Permanent enemyPermanent)
                            {
                                return enemyPermanent.Level <= 4 + selectedCards.Count;
                            }
                            
                            // Return all opponent's Digimon with a level lower or equal to 4 plus the number of chosen Digimon
                            List<Permanent> bounceTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons()
                                .Filter(PermanentConditionEnemy);
                            yield return ContinuousController.instance.StartCoroutine(
                                new HandBounceClaass(bounceTargetPermanents,
                                    CardEffectCommons.CardEffectHashtable(activateClass)).Bounce());
                        }
                    }
                }
            }
            
            // When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "You may place up to 3 of your other blue Digimon as this Digimon's bottom digivolution cards. Then, return all other level 4 or lower Digimon to the hand. For each card placed in this Digimon's digivolution cards, add 1 to the level this effect may return.",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, true,
                    EffectSharedDescription());
                cardEffects.Add(activateClass);
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CanActivateSharedCondition(hashtable))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();
                        
                        // If there is other blue Digimon in owner Battle Area  
                        int maxCount = Math.Min(3,
                            CardEffectCommons.MatchConditionPermanentCount(CanSelectSharedOwnPermanentCondition));
                        
                        SelectPermanentEffect selectPermanentEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();
                        
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectSharedOwnPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: CanEndSelectCondition,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: true,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);
                        
                        selectPermanentEffect.SetUpCustomMessage(
                            "Select up to 3 Digimon to place on bottom of digivolution cards.",
                            "The opponent is selecting up to 3 Digimon to place on bottom of digivolution cards.");
                        
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        
                        bool CanEndSelectCondition(List<Permanent> permanents)
                        {
                            if (CardEffectCommons.HasNoElement(permanents))
                            {
                                return false;
                            }
                            
                            return true;
                        }
                        
                        IEnumerator SelectPermanentCoroutine(Permanent selectedPermanent)
                        {
                            selectedCards.Add(selectedPermanent.TopCard);
                            
                            yield return null;
                        }
                        
                        if (selectedCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard()
                                .AddDigivolutionCardsBottom(
                                    selectedCards,
                                    activateClass));
                            
                            bool PermanentConditionEnemy(Permanent enemyPermanent)
                            {
                                return enemyPermanent.Level <= 4 + selectedCards.Count;
                            }
                            
                            // Return all opponent's Digimon with a level lower or equal to 4 plus the number of chosen Digimon
                            List<Permanent> bounceTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons()
                                .Filter(PermanentConditionEnemy);
                            yield return ContinuousController.instance.StartCoroutine(
                                new HandBounceClaass(bounceTargetPermanents,
                                    CardEffectCommons.CardEffectHashtable(activateClass)).Bounce());
                        }
                    }
                }
            }
            
            #endregion
            
            #region All Turns
            
            if (timing == EffectTiming.OnAddDigivolutionCards)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 digivolution card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true,
                    EffectDescription());
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return
                        "[Your Turn] [Once Per Turn] When an effect places a digivolution card under this Digimon, you may play 1 level 5 or lower Digimon card with [Aqua]/[Sea Animal] in one of its traits from this Digimon's digivolution cards without paying the cost.";
                }
                
                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.Level <= 5)
                        {
                            if (cardSource.HasAquaTraits)
                            {
                                if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false,
                                        cardEffect: activateClass))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    
                    return false;
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
                                    cardEffectCondition: cardEffect =>
                                        cardEffect != null,
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
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }
                    
                    return false;
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        int maxCount = Math.Min(1,
                            card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition));
                        
                        List<CardSource> selectedCards = new List<CardSource>();
                        
                        SelectCardEffect selectCardEffect =
                            GManager.instance.GetComponent<SelectCardEffect>();
                        
                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 digivolution card to play.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);
                        
                        selectCardEffect.SetUpCustomMessage(
                            "Select 1 digivolution card to play.",
                            "The opponent is selecting 1 digivolution card to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");
                        
                        yield return StartCoroutine(selectCardEffect.Activate());
                        
                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            
                            yield return null;
                        }
                        
                        // Play the selected Digivolution card
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.PlayPermanentCards(
                                cardSources: selectedCards,
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.DigivolutionCards,
                                activateETB: true));
                    }
                }
            }
            
            #endregion
            
            return cardEffects;
        }
    }
}