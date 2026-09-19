using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.EX6
{
    public class EX6_065 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Ignore Color Requirements
            
            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);
                
                cardEffects.Add(ignoreColorConditionClass);
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    if (card.Owner.GetBattleAreaDigimons()
                            .Count(permanent => permanent.TopCard.CardTraits.Contains("Legend-Arms")) >= 1)
                    {
                        return true;
                    }
                    
                    return false;
                }
                
                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        return true;
                    }
                    
                    return false;
                }
            }
            
            #endregion
            
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
                        "[Main] You may place 1 Digimon card with the [Legend-Arms] trait from your trash as 1 of your Digimon’s bottom digivolution card. Then, place this card in the battle area.";
                }
                
                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.CardTraits.Contains("Legend-Arms"))
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
                        if (!permanent.IsToken)
                        {
                            return true;
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
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();
                        
                        int maxCount = 1;
                        
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                        
                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to place on bottom of digivolution cards.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);
                        
                        selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");
                        selectCardEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.",
                            "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                        
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        
                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }
                        
                        if (selectedCards.Count >= 1)
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
                                if (permanent != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(
                                        permanent.AddDigivolutionCardsBottom(selectedCards, activateClass));
                                }
                            }
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(
                                        CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }
            
            #endregion
            
            #region Delay Effect
            
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Play 1 card with the [Legend-Arms] trait from the removed Digimon's digivolution cards without paying the cost",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetHashString("PlayDigivolutionCard_EX6_065");
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return
                        "[All Turns] When one of your Digimon would leave the battle area other than by one of your effects,[Delay] • You may play 1 card with the [Legend-Arms] trait from that Digimon's digivolution cards without paying the cost.";
                }
                
                bool IsOwnerPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DigivolutionCards.Count(CanSelectLegendArmsSourceCardCondition) >= 1)
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                bool IsOwnerPermanentToBeDeletedCondition(Permanent permanent)
                {
                    if (IsOwnerPermanentCondition(permanent))
                    {
                        if (permanent.willBeRemoveField)
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                bool CanSelectLegendArmsSourceCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.CardTraits.Contains("Legend-Arms"))
                        {
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false,
                                    cardEffect: activateClass))
                            {
                                return true;
                            }
                        }
                    }
                    
                    return false;
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanDeclareOptionDelayEffect(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, IsOwnerPermanentCondition))
                        {
                            if (!CardEffectCommons.IsByEffect(hashtable,
                                    cardEffect => CardEffectCommons.IsOwnerEffect(cardEffect, card)))
                            {
                                return true;
                            }
                        }
                    }
                    
                    return false;
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                            activateClass: activateClass, successProcess: permanents => SuccessProcess(),
                            failureProcess: null));
                }
                
                IEnumerator SuccessProcess()
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsOwnerPermanentToBeDeletedCondition))
                    {
                        int maxCount = Math.Min(1,
                            CardEffectCommons.MatchConditionPermanentCount(IsOwnerPermanentToBeDeletedCondition));
                        
                        SelectPermanentEffect selectPermanentEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();
                        
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOwnerPermanentToBeDeletedCondition,
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
                            "Select 1 Digimon which has digivolution cards.",
                            "The opponent is selecting 1 Digimon which has digivolution cards.");
                        
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        
                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;
                            
                            if (selectedPermanent != null)
                            {
                                if (selectedPermanent.DigivolutionCards.Count(CanSelectLegendArmsSourceCardCondition) >= 1)
                                {
                                    maxCount = Math.Min(1,
                                        selectedPermanent.DigivolutionCards.Count(CanSelectLegendArmsSourceCardCondition));
                                    
                                    List<CardSource> selectedCards = new List<CardSource>();
                                    
                                    SelectCardEffect selectCardEffect =
                                        GManager.instance.GetComponent<SelectCardEffect>();
                                    
                                    selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectLegendArmsSourceCardCondition,
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
                                        customRootCardList: selectedPermanent.DigivolutionCards,
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
                    }
                }
            }
            
            #endregion
            
            #region Security Effect
            
            if (timing == EffectTiming.SecuritySkill)
            {
                CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects,
                    effectName: card.BaseENGCardNameFromEntity);
            }
            
            #endregion
            
            return cardEffects;
        }
    }
}