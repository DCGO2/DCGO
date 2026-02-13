using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DCGO.CardEffects.EX6
{
    public class EX6_027 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Alternate Digivolution
            
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Angewomon");
                }
                
                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }
            
            #endregion
            
            #region Ace - Blast Digivolve
            
            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }
            
            #endregion
            
            #region On Play/ When Digivolving Shared
            
                        
            bool CanSelectPermanentSharedCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }
            
            bool CanActivateSharedCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.SecurityCards.Count >= 1)
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
                    "By trashing the top or bottom card of your security stack, 1 of your opponent's Digimon gets -8000 DP until the end of your opponents turn.",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, true,
                    EffectSharedDescription());
                cardEffects.Add(activateClass);

                string EffectSharedDescription()
                {
                    return
                        "[On Play] By trashing the top or bottom card of your security stack, 1 of your opponent's Digimon gets -8000 DP until the end of your opponents turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new(message: "Security Top", value: 0, spriteIndex: 0),
                            new(message: "Security Bottom", value: 1, spriteIndex: 0),
                            new(message: "No Selection", value: 2, spriteIndex: 1),
                        };
                        
                        string selectPlayerMessage = "Trash the top or bottom card of the security stack";
                        string notSelectPlayerMessage =
                            "The opponent is selecting whether to trash the top or bottom card of the security stack.";
                        
                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements,
                            selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                            notSelectPlayerMessage: notSelectPlayerMessage);
                        
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                            .WaitForEndSelect());
                        
                        int selectionValue = GManager.instance.userSelectionManager.SelectedIntValue;
                        
                        if (selectionValue != 2)
                        {
                            bool fromTop = selectionValue.Equals(0);
                            
                            yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                player: card.Owner,
                                destroySecurityCount: 1,
                                cardEffect: activateClass,
                                fromTop: fromTop).DestroySecurity());
                            
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentSharedCondition))
                            {
                                int maxCount = Math.Min(1,
                                    CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentSharedCondition));
                                
                                SelectPermanentEffect selectPermanentEffect =
                                    GManager.instance.GetComponent<SelectPermanentEffect>();
                                
                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentSharedCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);
                                
                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -8000.",
                                    "The opponent is selecting 1 Digimon that will get DP -8000.");
                                
                                yield return ContinuousController.instance.StartCoroutine(
                                    selectPermanentEffect.Activate());
                                
                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(
                                        CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent,
                                            changeValue: -8000, effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                            activateClass: activateClass));
                                }
                            }
                        }
                    }
                }
            }
            
            #endregion
            
            #region When Digivolving
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    " By trashing the top or bottom card of your security stack, 1 of your opponent's Digimon gets -8000 DP until the end of your opponents turn.",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, true,
                    EffectSharedDescription());
                cardEffects.Add(activateClass);

                string EffectSharedDescription()
                {
                    return
                        "[When Digivolving] By trashing the top or bottom card of your security stack, 1 of your opponent's Digimon gets -8000 DP until the end of your opponents turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new(message: "Security Top", value: 0, spriteIndex: 0),
                            new(message: "Security Bottom", value: 1, spriteIndex: 0),
                            new(message: "No Selection", value: 2, spriteIndex: 1),
                        };
                        
                        string selectPlayerMessage = "Trash the top or bottom card of the security stack";
                        string notSelectPlayerMessage =
                            "The opponent is selecting whether to trash the top or bottom card of the security stack.";
                        
                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements,
                            selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                            notSelectPlayerMessage: notSelectPlayerMessage);
                        
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                            .WaitForEndSelect());
                        
                        int selectionValue = GManager.instance.userSelectionManager.SelectedIntValue;
                        
                        if (selectionValue != 2)
                        {
                            bool fromTop = selectionValue.Equals(0);
                            
                            yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                player: card.Owner,
                                destroySecurityCount: 1,
                                cardEffect: activateClass,
                                fromTop: fromTop).DestroySecurity());
                            
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentSharedCondition))
                            {
                                int maxCount = Math.Min(1,
                                    CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentSharedCondition));
                                
                                SelectPermanentEffect selectPermanentEffect =
                                    GManager.instance.GetComponent<SelectPermanentEffect>();
                                
                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentSharedCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);
                                
                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -8000.",
                                    "The opponent is selecting 1 Digimon that will get DP -8000.");
                                
                                yield return ContinuousController.instance.StartCoroutine(
                                    selectPermanentEffect.Activate());
                                
                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(
                                        CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent,
                                            changeValue: -8000, effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                            activateClass: activateClass));
                                }
                            }
                        }
                    }
                }
            }
            
            #endregion
            
            #region All Turns - Your Turn
            
            if (timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "If it's your turn, this Digimon may gain [Security Attack +1] for the turn and attack",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true,
                    EffectDescription());
                activateClass.SetHashString("AllTurnsYourTurn_EX6_027");
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return
                        "[All Turns][Once Per Turn] When a card is removed from your security stack, if it's your turn, this Digimon may gain [Security Attack +1] for the turn and attack. If it's your opponent's turn, [Recovery +1 <Deck>].";
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, player => player == card.Owner))
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
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        Permanent cardPermanent = card.PermanentOfThisCard();
                        
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.ChangeDigimonSAttack(
                                targetPermanent: cardPermanent,
                                changeValue: 1,
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass));
                        
                        if (cardPermanent.CanAttack(activateClass))
                        {
                            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                            selectAttackEffect.SetUp(
                                attacker: cardPermanent,
                                canAttackPlayerCondition: () => true,
                                defenderCondition: (permanent) => true,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                        }
                    }
                }
            }
            
            #endregion
            
            #region All Turns - Opponent's Turn
            
            if (timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "If it's your opponent's turn, [Recovery +1 <Deck>]",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false,
                    EffectDescription());
                activateClass.SetHashString("AllTurnsOpponentTurn_EX6_027");
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return
                        "[All Turns][Once Per Turn] When a card is removed from your security stack, if it's your turn, this Digimon may gain [Security Attack +1] for the turn and attack. If it's your opponent's turn, [Recovery +1 <Deck>].";
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, player => player == card.Owner))
                            {
                                return true;
                            }
                        }
                    }
                    
                    return false;
                }
                
                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            if (card.Owner.CanAddSecurity(activateClass))
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
                        new IRecovery(card.Owner, 1, activateClass).Recovery());
                }
            }
            
            #endregion
            
            return cardEffects;
        }
    }
}