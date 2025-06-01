using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.EX6
{
    public class EX6_031 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Alternate Digivolution
            
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Sanzomon") ||
                           targetPermanent.TopCard.ContainsCardName("Gokuumon") ||
                           targetPermanent.TopCard.ContainsCardName("Sagomon") ||
                           targetPermanent.TopCard.ContainsCardName("Cho-Hakkaimon");
                }
                
                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 6,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }
            
            #endregion
            
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
                        DigiXrosConditionElement elementSanzomon =
                            new DigiXrosConditionElement(CanSelectCardConditionSanzomon, "Sanzomon");
                        
                        bool CanSelectCardConditionSanzomon(CardSource conditionCardSource)
                        {
                            if (conditionCardSource != null)
                            {
                                if (conditionCardSource.Owner == card.Owner)
                                {
                                    if (conditionCardSource.IsDigimon)
                                    {
                                        if (conditionCardSource.CardNames_DigiXros.Contains("Sanzomon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                            
                            return false;
                        }
                        
                        DigiXrosConditionElement elementGokuumon =
                            new DigiXrosConditionElement(CanSelectCardConditionGokuumon, "Gokuumon");
                        
                        bool CanSelectCardConditionGokuumon(CardSource conditionCardSource)
                        {
                            if (conditionCardSource != null)
                            {
                                if (conditionCardSource.Owner == card.Owner)
                                {
                                    if (conditionCardSource.IsDigimon)
                                    {
                                        if (conditionCardSource.CardNames_DigiXros.Contains("Gokuumon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                            
                            return false;
                        }
                        
                        DigiXrosConditionElement elementSagomon =
                            new DigiXrosConditionElement(CanSelectCardConditionSagomon, "Sagomon");
                        
                        bool CanSelectCardConditionSagomon(CardSource conditionCardSource)
                        {
                            if (conditionCardSource != null)
                            {
                                if (conditionCardSource.Owner == card.Owner)
                                {
                                    if (conditionCardSource.IsDigimon)
                                    {
                                        if (conditionCardSource.CardNames_DigiXros.Contains("Sagomon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                            
                            return false;
                        }
                        
                        DigiXrosConditionElement elementChoHakkaimon =
                            new DigiXrosConditionElement(CanSelectCardConditionChoHakkaimon, "Cho-Hakkaimon");
                        
                        bool CanSelectCardConditionChoHakkaimon(CardSource conditionCardSource)
                        {
                            if (conditionCardSource != null)
                            {
                                if (conditionCardSource.Owner == card.Owner)
                                {
                                    if (conditionCardSource.IsDigimon)
                                    {
                                        if (conditionCardSource.CardNames_DigiXros.Contains("Cho-Hakkaimon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                            
                            return false;
                        }
                        
                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>()
                            { elementSanzomon, elementGokuumon, elementSagomon, elementChoHakkaimon };
                        
                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, null, 2);
                        
                        return digiXrosCondition;
                    }
                    
                    return null;
                }
            }
            
            #endregion
            
            #region On Play/ When Digivolving Shared
            
            bool CanActivateSharedCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }
            
            #endregion
            
            #region On Play
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Security Attack -1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] All Digimon gain <Security Attack -1> until the end of your opponent's turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool PermanentCondition(Permanent permanent)
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
                    
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.ChangeDigimonSAttackPlayerEffect(
                            permanentCondition: PermanentCondition,
                            changeValue: -1,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));
                }
            }
            
            #endregion
            
            #region When Digivolving
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Security Attack -1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] All Digimon gain <Security Attack -1> until the end of your opponent's turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool PermanentCondition(Permanent permanent)
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
                    
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.ChangeDigimonSAttackPlayerEffect(
                            permanentCondition: PermanentCondition,
                            changeValue: -1,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));
                }
            }
            
            #endregion
            
            #region All Turns
            
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted || timing == EffectTiming.WhenReturntoHandAnyone || timing == EffectTiming.WhenReturntoLibraryAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Play 1 [Sanzomon] and 1 [Gokuumon]/[Sagomon]/[Cho-Hakkaimon] from its digivolution cards without paying the cost.",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true,
                    EffectDescription());
                activateClass.SetHashString("PlaySourceCard_EX6_031");
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return
                        "[All Turns] When this Digimon would be deleted or returned to the hand or deck, you may play 1 [Sanzomon] and 1 [Gokuumon]/[Sagomon]/[Cho-Hakkaimon] from its digivolution cards without paying the cost.";
                }
                
                bool CanSelectDigimonSanzomonCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.ContainsCardName("Sanzomon"))
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
                
                bool CanSelectDigimonJourneyCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.ContainsCardName("Gokuumon") ||
                            cardSource.ContainsCardName("Sagomon") ||
                            cardSource.ContainsCardName("Cho-Hakkaimon"))
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
                        if (card.PermanentOfThisCard().DigivolutionCards.Count >= 1)
                        {
                            if (card.PermanentOfThisCard().DigivolutionCards
                                    .Count(CanSelectDigimonSanzomonCardCondition) >= 1)
                            {
                                return true;
                            }
                            
                            if (card.PermanentOfThisCard().DigivolutionCards
                                    .Count(CanSelectDigimonJourneyCardCondition) >= 1)
                            {
                                return true;
                            }
                        }
                    }
                    
                    return false;
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        Permanent cardPermanent = card.PermanentOfThisCard();
                        List<CardSource> selectedCards = new List<CardSource>();

                        if (cardPermanent.DigivolutionCards.Count(CanSelectDigimonSanzomonCardCondition) >= 1)
                        {
                            int maxCount = Math.Min(1,
                                cardPermanent.DigivolutionCards.Count(CanSelectDigimonSanzomonCardCondition));
                            
                            SelectCardEffect selectCardEffect =
                                GManager.instance.GetComponent<SelectCardEffect>();
                            
                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectDigimonSanzomonCardCondition,
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
                                customRootCardList: cardPermanent.DigivolutionCards,
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
                        }
                        
                        if (cardPermanent.DigivolutionCards.Count(CanSelectDigimonJourneyCardCondition) >= 1)
                        {
                            int maxCount = Math.Min(1,
                                cardPermanent.DigivolutionCards.Count(CanSelectDigimonJourneyCardCondition));
                            
                            SelectCardEffect selectCardEffect =
                                GManager.instance.GetComponent<SelectCardEffect>();
                            
                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectDigimonJourneyCardCondition,
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
                                customRootCardList: cardPermanent.DigivolutionCards,
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

            #region Your Turn
            if (timing == EffectTiming.None)
            {

                bool Condition()
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

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.HasSecurityAttackChanges)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.InvertSAttackStaticEffect(
                    permanentCondition: PermanentCondition,
                    changeValue: -1,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition));
            }
            #endregion

            #region End of Opponent's Turn

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Place 1 Digimon with [Security Attack] on top of its owner's security stack",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("EOT_EX6-031");
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return
                        "[End of Your Turn] [Once Per Turn] You may place 1 Digimon with [Security Attack] on top of its owner's security stack.";
                }
                
                bool IsPermanentWithSecurityAttackCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                    {
                        if (permanent.HasSecurityAttackChanges)
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
                        if (CardEffectCommons.IsOpponentTurn(card))
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
                        if (CardEffectCommons.HasMatchConditionPermanent(IsPermanentWithSecurityAttackCondition))
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsPermanentWithSecurityAttackCondition))
                    {
                        int maxCount = Math.Min(1,
                            CardEffectCommons.MatchConditionPermanentCount(IsPermanentWithSecurityAttackCondition));
                        
                        SelectPermanentEffect selectPermanentEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();
                        
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsPermanentWithSecurityAttackCondition,
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
                            "Select 1 Digimon to place on top of your security stack.",
                            "The opponent is selecting 1 Digimon to place on top of their security stack.");
                        
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        
                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            if (permanent.TopCard != null)
                            {
                                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(
                                        new IPutSecurityPermanent(permanent,
                                            CardEffectCommons.CardEffectHashtable(activateClass),
                                            toTop: true).PutSecurity());
                                }
                            }
                        }
                    }
                }
            }
            
            #endregion
            
            return cardEffects;
        }
    }
}