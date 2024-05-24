using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class Angemon_BT16_019 : CEntity_Effect
    {
        
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            var cardEffects = new List<ICardEffect>();
            
            switch (timing)
            {
                case EffectTiming.None:
                    #region Alternate Digivolution Requirement
                    
                    bool PermanentCondition(Permanent targetPermanent)
                    {
                        return targetPermanent.TopCard.CardNames.Contains("Patamon");
                    }
                    
                    cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                        permanentCondition: PermanentCondition,
                        digivolutionCost: 2,
                        ignoreDigivolutionRequirement: false,
                        card: card,
                        condition: null)
                    );
                    
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
                case EffectTiming.OnAllyAttack:
                    cardEffects.Add(CardEffectFactory.RaidSelfEffect(
                        isInheritedEffect: false,
                        card: card,
                        condition: null)
                    );
                    break;
                case EffectTiming.OnEnterFieldAnyone:
                    var activateClass = new ActivateClass();
                    activateClass.SetUpICardEffect("Select 1 of your Digimon to gain effects", CanUseCondition, card);
                    activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                    cardEffects.Add(activateClass);

                    string EffectDiscription()
                    {
                        return "[On Play] [When Digivolving] 1 of your Digimon can't be deleted in battle until the end of your opponent's turn.";
                    }

                    bool CanUseCondition(Hashtable hashtable)
                    {
                        return CardEffectCommons.CanTriggerOnPlay(hashtable, card)
                               || CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                    }

                    bool CanActivateCondition(Hashtable hashtable)
                    {
                        return CardEffectCommons.IsExistOnBattleArea(card);
                    }
                    
                    bool CanSelectPermanentCondition(Permanent permanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                    }

                    IEnumerator ActivateCoroutine(Hashtable hashtable)
                    {
                        var selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);
                        
                        selectPermanentEffect.SetUpCustomMessage(
                            "Select 1 Digimon that will get effects.",
                            "The opponent is selecting 1 Digimon that will get effects.");
                        
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        yield break;
                        
                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotBeDeletedByBattle(
                                targetPermanent: permanent,
                                canNotBeDestroyedByBattleCondition: CanNotBeDestroyedByBattleCondition,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass,
                                effectName: "Can't be deleted in battle"));
                            yield break;
                            
                            bool CanNotBeDestroyedByBattleCondition(Permanent permanent1, Permanent attackingPermanent, Permanent defendingPermanent, CardSource defendingCard)
                            {
                                return permanent == attackingPermanent || permanent == defendingPermanent;
                            }
                        }
                    }
                    break;
            }

            return cardEffects;
        }
    }
}