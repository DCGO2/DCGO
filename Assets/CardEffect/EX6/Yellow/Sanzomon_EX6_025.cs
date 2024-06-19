using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.EX6
{
    public class Sanzomon_EX6_025 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region DigiXros
            
            if (timing == EffectTiming.None)
            {
                AddDigiXrosConditionClass addDigiXrosConditionClass = new AddDigiXrosConditionClass();
                addDigiXrosConditionClass.SetUpICardEffect($"DigiXros -2", CanUseCondition, card);
                addDigiXrosConditionClass.SetUpAddDigiXrosConditionClass(getDigiXrosCondition: GetDigiXros);
                addDigiXrosConditionClass.SetNotShowUI(true);
                cardEffects.Add(addDigiXrosConditionClass);
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }
                
                DigiXrosCondition GetDigiXros(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        DigiXrosConditionElement element = new DigiXrosConditionElement(CanSelectCardCondition,
                            "[Gokuumon] or [Sagomon] or [Cho-Hakkaimon]");
                        
                        bool CanSelectCardCondition(CardSource xrosCardSource)
                        {
                            if (xrosCardSource != null)
                            {
                                if (xrosCardSource.Owner == card.Owner)
                                {
                                    if (xrosCardSource.IsDigimon)
                                    {
                                        if (xrosCardSource.ContainsCardName("Gokuumon") ||
                                            xrosCardSource.ContainsCardName("Sagomon") ||
                                            xrosCardSource.ContainsCardName("Cho-Hakkaimon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                            
                            return false;
                        }
                        
                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>() { element };
                        
                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, null, 2);
                        
                        return digiXrosCondition;
                    }
                    
                    return null;
                }
            }
            
            #endregion
            
            #region On Play/ When Attacking/ ESS Shared
            
            string EffectSharedDescription()
            {
                return
                    "[On Play] [When Attacking] [Once Per Turn] 1 Digimon may gain [Security Attack -1] until the end of your opponent's turn. Then, if DigiXrosing, reveal the top 4 cards of your deck. Add 1 of each [Gokuumon], [Sagomon], [Cho-Hakkaimon] and [Shakamon] among them to the hand. Return the rest to the bottom of the deck.";
            }
            
            bool CanActivateSharedCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSecMinusPermanentSharedCondition))
                    {
                        return true;
                    }
                    
                    if (CardEffectCommons.IsDijiXros(hashtable, count => count >= 1))
                    {
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            return true;
                        }
                    }
                }
                
                return false;
            }
            
            bool CanSelectSecMinusPermanentSharedCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                {
                    if (permanent.IsDigimon)
                    {
                        return true;
                    }
                }
                
                return false;
            }
            
            bool CanSelectDigimonGokuumonCardSharedCondition(CardSource cardSource)
            {
                if (cardSource.IsDigimon)
                {
                    if (cardSource.ContainsCardName("Gokuumon"))
                    {
                        return true;
                    }
                }
                
                return false;
            }
            
            bool CanSelectDigimonSagomonCardSharedCondition(CardSource cardSource)
            {
                if (cardSource.IsDigimon)
                {
                    if (cardSource.ContainsCardName("Sagomon"))
                    {
                        return true;
                    }
                }
                
                return false;
            }
            
            bool CanSelectDigimonChoHakkaimonCardSharedCondition(CardSource cardSource)
            {
                if (cardSource.IsDigimon)
                {
                    if (cardSource.ContainsCardName("Cho-Hakkaimon"))
                    {
                        return true;
                    }
                }
                
                return false;
            }
            
            bool CanSelectDigimonShakamonCardSharedCondition(CardSource cardSource)
            {
                if (cardSource.IsDigimon)
                {
                    if (cardSource.ContainsCardName("Shakamon"))
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
                    "1 Digimon may gain [Security Attack -1] until the end of your opponent's turn. Then, if DigiXrosing, reveal the top 4 cards of your deck. Add 1 of each [Gokuumon], [Sagomon], [Cho-Hakkaimon] and [Shakamon] among them to the hand. Return the rest to the bottom of the deck.",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, 1, false,
                    EffectSharedDescription());
                activateClass.SetHashString("SecurityAttack-1Search_EX6-025");
                cardEffects.Add(activateClass);
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSecMinusPermanentSharedCondition))
                    {
                        int maxCount = Math.Min(1,
                            CardEffectCommons.MatchConditionPermanentCount(CanSelectSecMinusPermanentSharedCondition));
                        
                        SelectPermanentEffect selectPermanentEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();
                        
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectSecMinusPermanentSharedCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);
                        
                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get Security Attack -1.",
                            "The opponent is selecting 1 Digimon that will get Security Attack -1.");
                        
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        
                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.ChangeDigimonSAttack(targetPermanent: permanent, changeValue: -1,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                        }
                    }
                    
                    if (CardEffectCommons.IsDijiXros(hashtable, count => count >= 1))
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                                revealCount: 4,
                                simplifiedSelectCardConditions:
                                new[]
                                {
                                    new SimplifiedSelectCardConditionClass(
                                        canTargetCondition: CanSelectDigimonGokuumonCardSharedCondition,
                                        message: "Select 1 Gokuumon card.",
                                        mode: SelectCardEffect.Mode.AddHand,
                                        maxCount: 1,
                                        selectCardCoroutine: null),
                                    new SimplifiedSelectCardConditionClass(
                                        canTargetCondition: CanSelectDigimonSagomonCardSharedCondition,
                                        message: "Select 1 Sagomon card.",
                                        mode: SelectCardEffect.Mode.AddHand,
                                        maxCount: 1,
                                        selectCardCoroutine: null),
                                    new SimplifiedSelectCardConditionClass(
                                        canTargetCondition: CanSelectDigimonChoHakkaimonCardSharedCondition,
                                        message: "Select 1 Cho-Hakkaimon card.",
                                        mode: SelectCardEffect.Mode.AddHand,
                                        maxCount: 1,
                                        selectCardCoroutine: null),
                                    new SimplifiedSelectCardConditionClass(
                                        canTargetCondition: CanSelectDigimonShakamonCardSharedCondition,
                                        message: "Select 1 Shakamon card.",
                                        mode: SelectCardEffect.Mode.AddHand,
                                        maxCount: 1,
                                        selectCardCoroutine: null),
                                },
                                remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                                activateClass: activateClass
                            ));
                    }
                }
            }
            
            #endregion
            
            #region When Attacking
            
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "1 Digimon may gain [Security Attack -1] until the end of your opponent's turn. Then, if DigiXrosing, reveal the top 4 cards of your deck. Add 1 of each [Gokuumon], [Sagomon], [Cho-Hakkaimon] and [Shakamon] among them to the hand. Return the rest to the bottom of the deck.",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, 1, false,
                    EffectSharedDescription());
                activateClass.SetHashString("SecurityAttack-1Search_EX6-025");
                cardEffects.Add(activateClass);
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSecMinusPermanentSharedCondition))
                    {
                        int maxCount = Math.Min(1,
                            CardEffectCommons.MatchConditionPermanentCount(CanSelectSecMinusPermanentSharedCondition));
                        
                        SelectPermanentEffect selectPermanentEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();
                        
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectSecMinusPermanentSharedCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);
                        
                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get Security Attack -1.",
                            "The opponent is selecting 1 Digimon that will get Security Attack -1.");
                        
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        
                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.ChangeDigimonSAttack(targetPermanent: permanent, changeValue: -1,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                        }
                    }
                    
                    if (CardEffectCommons.IsDijiXros(hashtable, count => count >= 1))
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                                revealCount: 4,
                                simplifiedSelectCardConditions:
                                new[]
                                {
                                    new SimplifiedSelectCardConditionClass(
                                        canTargetCondition: CanSelectDigimonGokuumonCardSharedCondition,
                                        message: "Select 1 Gokuumon card.",
                                        mode: SelectCardEffect.Mode.AddHand,
                                        maxCount: 1,
                                        selectCardCoroutine: null),
                                    new SimplifiedSelectCardConditionClass(
                                        canTargetCondition: CanSelectDigimonSagomonCardSharedCondition,
                                        message: "Select 1 Sagomon card.",
                                        mode: SelectCardEffect.Mode.AddHand,
                                        maxCount: 1,
                                        selectCardCoroutine: null),
                                    new SimplifiedSelectCardConditionClass(
                                        canTargetCondition: CanSelectDigimonChoHakkaimonCardSharedCondition,
                                        message: "Select 1 Cho-Hakkaimon card.",
                                        mode: SelectCardEffect.Mode.AddHand,
                                        maxCount: 1,
                                        selectCardCoroutine: null),
                                    new SimplifiedSelectCardConditionClass(
                                        canTargetCondition: CanSelectDigimonShakamonCardSharedCondition,
                                        message: "Select 1 Shakamon card.",
                                        mode: SelectCardEffect.Mode.AddHand,
                                        maxCount: 1,
                                        selectCardCoroutine: null),
                                },
                                remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                                activateClass: activateClass
                            ));
                    }
                }
            }
            
            #endregion
            
            #region All Turns
            
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Return 1 yellow Digimon card from this Digimon's digivolution cards to the hand",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                activateClass.SetHashString("AllTurns_EX6_025");
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return
                        "[All Turns] When this Digimon would leave the battle area, return 1 yellow Digimon card from this Digimon's digivolution cards to the hand.";
                }
                
                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.CardColors.Contains(CardColor.Yellow))
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
                        if (CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card))
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
                        if (card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        Permanent cardPermanent = card.PermanentOfThisCard();
                        
                        if (cardPermanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                        {
                            int maxCount = Math.Min(1, cardPermanent.DigivolutionCards.Count(CanSelectCardCondition));
                            
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                            
                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => false,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to return to hand.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.AddHand,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: cardPermanent.DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);
                            
                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }
                    }
                }
            }
            
            #endregion
            
            #region When Attacking - ESS
            
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Security Attack -1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true,
                    EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("SecurityAttack-1_EX6-025");
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return
                        "[When Attacking][Once Per Turn] 1 Digimon may gain [Security Attack -1] until the end of your opponent's turn.";
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
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
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSecMinusPermanentSharedCondition))
                    {
                        int maxCount = Math.Min(1,
                            CardEffectCommons.MatchConditionPermanentCount(CanSelectSecMinusPermanentSharedCondition));
                        
                        SelectPermanentEffect selectPermanentEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();
                        
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectSecMinusPermanentSharedCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);
                        
                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get Security Attack -1.",
                            "The opponent is selecting 1 Digimon that will get Security Attack -1.");
                        
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        
                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.ChangeDigimonSAttack(targetPermanent: permanent, changeValue: -1,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                        }
                    }
                }
            }
            
            #endregion

            return cardEffects;
        }
    }
}