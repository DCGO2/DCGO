using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.BT16
{
    public class Patamon_BT16_016 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            switch (timing)
            {
                case EffectTiming.None:
                {
                    #region Alternate Digivolution Requirement
                    
                    bool PermanentCondition(Permanent targetPermanent)
                    {
                        return targetPermanent.TopCard.ContainsCardName("Tokomon");
                    }
                    
                    cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                        permanentCondition: PermanentCondition,
                        digivolutionCost: 0,
                        ignoreDigivolutionRequirement: false,
                        card: card,
                        condition: null));
                    
                    #endregion
                    
                    
                    #region Inherited Effect
                    
                    ActivateClass activateClass = new ActivateClass();
                    activateClass.SetUpICardEffect("Trash 1 digivolution card", CanUseCondition, card);
                    activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                        EffectDiscription());
                    activateClass.SetIsInheritedEffect(true);
                    cardEffects.Add(activateClass);
                    
                    string EffectDiscription()
                    {
                        return "[When Attacking] Trash the top digivolution card of 1 of your opponent's Digimon.";
                    }
                    
                    bool CanSelectPermanentCondition(Permanent permanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                               && permanent.DigivolutionCards.Count(cardSource =>
                                   !cardSource.CanNotTrashFromDigivolutionCards(activateClass)) >= 1;
                    }
                    
                    bool CanUseCondition(Hashtable hashtable)
                    {
                        return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                    }
                    
                    bool CanActivateCondition(Hashtable hashtable)
                    {
                        return CardEffectCommons.IsExistOnBattleArea(card) &&
                               CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                    }
                    
                    IEnumerator ActivateCoroutine(Hashtable hashtable)
                    {
                        var maxCount = Math.Min(1,
                            CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));
                        var selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);
                        
                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will trash digivolution cards.",
                            "The opponent is selecting 1 Digimon that will trash digivolution cards.");
                        
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        yield break;
                        
                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanent,
                                    trashCount: 1, isFromTop: true, activateClass: activateClass));
                        }
                    }
                    
                    #endregion
                    
                    break;
                }
                case EffectTiming.OnEnterFieldAnyone:
                case EffectTiming.OnStartMainPhase:
                {
                    #region Start of Main Phase Effect
                    var activateClass = new ActivateClass();
                    activateClass.SetUpICardEffect("1 of your Digimon may Digivolve", CanUseCondition, card);
                    activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                    cardEffects.Add(activateClass);

                    string EffectDiscription()
                    {
                        return "[Start of Main Phase] If it's your turn, 1 of your Digimon may digivolve into a level 4 Digimon card with the [Angel] or [Free] trait from your trash with the digivolution cost reduced by 1.";
                    }


                    bool CanSelectCardCondition(CardSource cardSource)
                    {
                        return (cardSource.CardTraits.Contains("Angel") || cardSource.CardTraits.Contains("Free"))
                               && cardSource.HasLevel && cardSource.Level == 4;
                    }

                    bool CanSelectPermanentCondition(Permanent permanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                               && card.Owner.HandCards.Select(cardSource => 
                                   CanSelectCardCondition(cardSource) 
                                   && cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, false, activateClass)
                                   ).FirstOrDefault();
                    }

                    bool CanUseCondition(Hashtable hashtable)
                    {
                        return CardEffectCommons.IsExistOnBattleArea(card) && CardEffectCommons.IsOwnerTurn(card);
                    }

                    bool CanActivateCondition(Hashtable hashtable)
                    {
                        return CardEffectCommons.IsExistOnBattleArea(card)
                               && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition)
                               && card.Owner.TrashCards.Count >= 1;
                    }

                    IEnumerator ActivateCoroutine(Hashtable hashtable)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            Permanent selectedPermanent = null;

                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

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

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will digivolve.", "The opponent is selecting 1 Digimon that will digivolve.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;

                                yield return null;
                            }

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                    targetPermanent: selectedPermanent,
                                    cardCondition: CanSelectCardCondition,
                                    payCost: true,
                                    reduceCostTuple: (reduceCost: 1, reduceCostCardCondition: null),
                                    fixedCostTuple: null,
                                    ignoreDigivolutionRequirementFixedCost: -1,
                                    isHand: false,
                                    activateClass: activateClass,
                                    successProcess: null));
                            }
                        }
                    }
                    #endregion
                    
                    break;
                }
            }

            return cardEffects;
        }
    }
}