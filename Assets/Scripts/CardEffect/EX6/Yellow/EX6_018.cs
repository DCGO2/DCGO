using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DCGO.CardEffects.EX6
{
    public class EX6_018 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Alternate Digivolution
            
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Cupimon");
                }
                
                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 5,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }
            
            #endregion
            
            #region Play Cost Reduction
            
            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect($"Play Cost -5", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost,
                    cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: IsUpDown,
                    isCheckAvailability: () => false, isChangePayingCost: () => true);
                
                cardEffects.Add(changeCostClass);
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Contains(card))
                    {
                        if (!CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsPermanentLevel5OrLower))
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                bool IsPermanentLevel5OrLower(Permanent permanent)
                {
                    return permanent.IsDigimon && permanent.Level <= 5;
                }
                
                int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root,
                    List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource))
                    {
                        if (RootCondition(root))
                        {
                            if (PermanentsCondition(targetPermanents))
                            {
                                cost -= 5;
                            }
                        }
                    }
                    
                    return cost;
                }
                
                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    if (targetPermanents == null)
                    {
                        return true;
                    }
                    
                    if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                    {
                        return true;
                    }
                    
                    return false;
                }
                
                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }
                
                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }
                
                bool IsUpDown()
                {
                    return true;
                }
            }
            
            #endregion
            
            #region On Play/ Start of Your Main Phase Shared
            
            bool CanSelectCardSharedCondition(CardSource cardSource)
            {
                if (cardSource.CardTraits.Contains("Angel") ||
                    cardSource.CardTraits.Contains("Archangel") ||
                    cardSource.CardTraits.Contains("Three Great Angels") ||
                    cardSource.CardTraits.Contains("ThreeGreatAngels") ||
                    cardSource.CardTraits.Contains("Seven Great Demon Lords") ||
                    cardSource.CardTraits.Contains("SevenGreatDemonLords"))
                {
                    return true;
                }
                
                return false;
            }
            
            bool CanActivateSharedCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.LibraryCards.Count >= 1)
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
                activateClass.SetUpICardEffect("Reveal the top 3 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);
            
                string EffectDescription()
                {
                    return
                        "[On Play] Reveal the top 3 cards of your deck. Add 1 card with the [Angel]/[Archangel]/[Three Great Angels]/[Seven Great Demon Lords] trait among them to your hand. Trash the rest.";
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                            revealCount: 3,
                            simplifiedSelectCardConditions:
                            new[]
                            {
                                new SimplifiedSelectCardConditionClass(
                                    canTargetCondition: CanSelectCardSharedCondition,
                                    message:
                                    "Select 1 Digimon card with the [Angel]/[Archangel]/[Three Great Angels]/[Seven Great Demon Lords] trait.",
                                    mode: SelectCardEffect.Mode.AddHand,
                                    maxCount: 1,
                                    selectCardCoroutine: null),
                            },
                            remainingCardsPlace: RemainingCardsPlace.Trash,
                            activateClass: activateClass
                        ));
                }
            }
            
            #endregion
            
            #region Start of Your Main Phase
            
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 3 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);
            
                string EffectDescription()
                {
                    return
                        "[Start of Your Main Phase] Reveal the top 3 cards of your deck. Add 1 card with the [Angel]/[Archangel]/[Three Great Angels]/[Seven Great Demon Lords] trait among them to your hand. Trash the rest.";
                }
                
                bool CanUseCondition(Hashtable hashtable)
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
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                            revealCount: 3,
                            simplifiedSelectCardConditions:
                            new[]
                            {
                                new SimplifiedSelectCardConditionClass(
                                    canTargetCondition: CanSelectCardSharedCondition,
                                    message:
                                    "Select 1 Digimon card with the [Angel]/[Archangel]/[Three Great Angels]/[Seven Great Demon Lords] trait.",
                                    mode: SelectCardEffect.Mode.AddHand,
                                    maxCount: 1,
                                    selectCardCoroutine: null),
                            },
                            remainingCardsPlace: RemainingCardsPlace.Trash,
                            activateClass: activateClass
                        ));
                }
            }
            
            #endregion
            
            #region End of Your Turn
            
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Place one of your level 6 Digimon on top of your security stack to digivolve into [Lucemon: Chaos Mode] in the trash without paying the cost.",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("EOT_EX6-018");
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return
                        "[End of Your Turn] [Once Per Turn] By placing one of your level 6 Digimon on top of your security stack, this Digimon may digivolve into [Lucemon: Chaos Mode] in the trash without paying the cost.";
                }
                
                bool IsPermanentLevel6Condition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.IsLevel6)
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                bool IsCardLucemonChaosModeCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.ContainsCardName("Lucemon: Chaos Mode") ||
                            cardSource.ContainsCardName("Lucemon:ChaosMode") ||
                            cardSource.ContainsCardName("Lucemon Chaos Mode") ||
                            cardSource.ContainsCardName("LucemonChaosMode"))
                        {
                            
                            if (cardSource.CanEvolve(card.PermanentOfThisCard(), true))
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
                        if (CardEffectCommons.IsOwnerTurn(card))
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
                        if (CardEffectCommons.HasMatchConditionPermanent(IsPermanentLevel6Condition))
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsPermanentLevel6Condition))
                    {
                        Permanent selectedPermanent = null;
                        
                        int maxCount = Math.Min(1,
                            CardEffectCommons.MatchConditionPermanentCount(IsPermanentLevel6Condition));
                        
                        SelectPermanentEffect selectPermanentEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();
                        
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsPermanentLevel6Condition,
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
                            selectedPermanent = permanent;
                            yield return null;
                        }
                        
                        if (selectedPermanent != null)
                        {
                            if (selectedPermanent.TopCard != null)
                            {
                                if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    CardSource securityCard = selectedPermanent.TopCard;
                                    
                                    yield return ContinuousController.instance.StartCoroutine(
                                        new IPutSecurityPermanent(selectedPermanent,
                                            CardEffectCommons.CardEffectHashtable(activateClass),
                                            toTop: true).PutSecurity());
                                    
                                    if (selectedPermanent.TopCard == null)
                                    {
                                        if (!CardEffectCommons.IsExistOnBattleArea(securityCard))
                                        {
                                            if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsCardLucemonChaosModeCondition))
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(
                                                CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                                    targetPermanent: card.PermanentOfThisCard(),
                                                    cardCondition: IsCardLucemonChaosModeCondition,
                                                    payCost: false,
                                                    reduceCostTuple: null,
                                                    fixedCostTuple: null,
                                                    ignoreDigivolutionRequirementFixedCost: -1,
                                                    isHand: false,
                                                    activateClass: activateClass,
                                                    successProcess: null));
                                            }
                                        }
                                    }
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