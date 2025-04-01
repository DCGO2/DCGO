using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.EX6
{
    public class EX6_024 : CEntity_Effect
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
                            "[Sanzomon] or [Gokuumon] or [Cho-Hakkaimon]");
                        
                        bool CanSelectCardCondition(CardSource xrosCardSource)
                        {
                            if (xrosCardSource != null)
                            {
                                if (xrosCardSource.Owner == card.Owner)
                                {
                                    if (xrosCardSource.IsDigimon)
                                    {
                                        if (xrosCardSource.ContainsCardName("Sanzomon") ||
                                            xrosCardSource.ContainsCardName("Gokuumon") ||
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
            
            #endregion
            
            #region On Play
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "1 Digimon may gain [Security Attack -1] until the end of your opponent's turn. Then, if DigiXrosing, 1 of your opponent's Digimon or Tamers can't suspend until the end of their turn.",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false,
                    EffectDescription());
                activateClass.SetHashString("SecurityAttack-1CantSuspend_EX6-024");
                cardEffects.Add(activateClass);
            
                string EffectDescription()
                {
                    return
                        "[On Play] [Once Per Turn] 1 Digimon may gain [Security Attack -1] until the end of your opponent's turn. Then, if DigiXrosing, 1 of your opponent's Digimon or Tamers can't suspend until the end of their turn.";
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
                
                bool CanSelectOpponentPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        return permanent.IsDigimon || permanent.IsTamer;
                    }
                    
                    return false;
                }
                
                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSecMinusPermanentSharedCondition))
                        {
                            return true;
                        }
                        
                        if (CardEffectCommons.IsDijiXros(hashtable, count => count >= 1))
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentCondition))
                            {
                                return true;
                            }
                        }
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
                    
                    if (CardEffectCommons.IsDijiXros(hashtable, count => count >= 1))
                    {
                        if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectOpponentPermanentCondition))
                        {
                            int maxCount = Math.Min(1,
                                CardEffectCommons.MatchConditionPermanentCount(CanSelectOpponentPermanentCondition));
                            
                            SelectPermanentEffect selectPermanentEffect =
                                GManager.instance.GetComponent<SelectPermanentEffect>();
                            
                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition_ByPreSelecetedList: null,
                                canTargetCondition: CanSelectOpponentPermanentCondition,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);
                            
                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon/Tamer that will be unable to suspend.",
                                "The opponent is selecting 1 Digimon/Tamer that will be unable to suspend.");
                            
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                            
                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                Permanent selectedPermanent = permanent;
                                
                                if (selectedPermanent != null)
                                {
                                    CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                                    canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCanNotSuspendCondition, card);
                                    canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCanNotSuspendCondition);
                                    selectedPermanent.UntilOwnerTurnEndEffects.Add(_ => canNotSuspendClass);
                                    
                                    if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                                    }
                                    
                                    bool CanUseCanNotSuspendCondition(Hashtable hashtableCanNotSuspend)
                                    {
                                        if (selectedPermanent.TopCard != null)
                                        {
                                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                            {
                                                return true;
                                            }
                                        }
                                        
                                        return false;
                                    }
                                    
                                    bool PermanentCanNotSuspendCondition(Permanent permanentCanNotSuspend)
                                    {
                                        if (permanentCanNotSuspend == selectedPermanent)
                                        {
                                            return true;
                                        }
                                        
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            #endregion
            
            #region When Attacking
            
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "1 Digimon may gain [Security Attack -1] until the end of your opponent's turn",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false,
                    EffectDescription());
                activateClass.SetHashString("SecurityAttack-1_EX6-024");
                cardEffects.Add(activateClass);
            
                string EffectDescription()
                {
                    return
                        "[When Attacking] [Once Per Turn] 1 Digimon may gain [Security Attack -1] until the end of your opponent's turn. Then, if DigiXrosing, 1 of your opponent's Digimon or Tamers can't suspend until the end of their turn.";
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }
                
                bool CanSelectOpponentPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        return permanent.IsDigimon || permanent.IsTamer;
                    }
                    
                    return false;
                }
                
                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSecMinusPermanentSharedCondition))
                        {
                            return true;
                        }
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
            
            #region All Turns
            
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Return 1 yellow Digimon card from this Digimon's digivolution cards to the hand",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                activateClass.SetHashString("AllTurns_EX6_024");
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
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false,
                    EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("SecurityAttack-1_EX6-024");
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