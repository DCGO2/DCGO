using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.EX6
{
    public class EX6_015 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region On Play/When Digivolving Shared
            
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
  
            #endregion
            
            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Place digvolution cards and activate effects", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] You may place up to 3 of your other blue Digimon as this Digimon's bottom digivolution cards. Then, return all other level 4 or lower Digimon to the hand. For each card placed in this Digimon's digivolution cards, add 1 to the level this effect may return.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSharedOwnPermanentCondition))
                    {
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

                            yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(
                                new List<Permanent[]>() { new Permanent[] { selectedPermanent, card.PermanentOfThisCard() } },
                                false,
                                activateClass).PlacePermanentToDigivolutionCards());

                            yield return null;
                        }
                    }

                    bool BouncePermanentCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                        {
                            if(permanent != card.PermanentOfThisCard())
                            {
                                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    return permanent.TopCard.HasLevel && permanent.Level <= 4 + selectedCards.Count;
                                }
                            }
                        }

                        return false;
                    }

                    // Return all Digimon with a level lower or equal to 4 plus the number of chosen Digimon
                    List<Permanent> bounceTargetPermanents = new List<Permanent>();

                    bounceTargetPermanents.AddRange(card.Owner.Enemy.GetBattleAreaDigimons().Filter(BouncePermanentCondition));
                    bounceTargetPermanents.AddRange(card.Owner.GetBattleAreaDigimons().Filter(BouncePermanentCondition));

                    yield return ContinuousController.instance.StartCoroutine(
                        new HandBounceClaass(bounceTargetPermanents,
                            CardEffectCommons.CardEffectHashtable(activateClass)).Bounce());
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Place digvolution cards and activate effects", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] You may place up to 3 of your other blue Digimon as this Digimon's bottom digivolution cards. Then, return all other level 4 or lower Digimon to the hand. For each card placed in this Digimon's digivolution cards, add 1 to the level this effect may return.";
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
                    List<CardSource> selectedCards = new List<CardSource>();

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSharedOwnPermanentCondition))
                    {
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

                            yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(
                                new List<Permanent[]>() { new Permanent[] { selectedPermanent, card.PermanentOfThisCard() } },
                                false,
                                activateClass).PlacePermanentToDigivolutionCards());

                            yield return null;
                        }
                    }

                    bool BouncePermanentCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                        {
                            if (permanent != card.PermanentOfThisCard())
                            {
                                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    return permanent.TopCard.HasLevel && permanent.Level <= 4 + selectedCards.Count;
                                }
                            }
                        }

                        return false;
                    }

                    // Return all Digimon with a level lower or equal to 4 plus the number of chosen Digimon
                    List<Permanent> bounceTargetPermanents = new List<Permanent>();

                    bounceTargetPermanents.AddRange(card.Owner.Enemy.GetBattleAreaDigimons().Filter(BouncePermanentCondition));
                    bounceTargetPermanents.AddRange(card.Owner.GetBattleAreaDigimons().Filter(BouncePermanentCondition));

                    yield return ContinuousController.instance.StartCoroutine(
                        new HandBounceClaass(bounceTargetPermanents,
                            CardEffectCommons.CardEffectHashtable(activateClass)).Bounce());
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnAddDigivolutionCards)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 digivolution card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("Play1DigivolutionCard_EX6_015");
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return "[Your Turn] [Once Per Turn] When an effect places a digivolution card under this Digimon, you may play 1 level 5 or lower Digimon card with [Aqua]/[Sea Animal] in one of its traits from this Digimon's digivolution cards without paying the cost.";
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
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnAddDigivolutionCard(
                                    hashtable: hashtable,
                                    permanentCondition: permanent => permanent == card.PermanentOfThisCard(),
                                    cardEffectCondition: cardEffect => cardEffect != null,
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
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                    {
                        int maxCount = Math.Min(1,card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition));
                        
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