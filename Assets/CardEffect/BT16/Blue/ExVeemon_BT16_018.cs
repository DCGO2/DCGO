using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class ExVeemon_BT16_018 : CEntity_Effect
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
                        return targetPermanent.TopCard.CardNames.Contains("Veemon");
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
                    bool InheritedEffectCondition()
                    {
                        return CardEffectCommons.IsOwnerTurn(card);
                    }
                    
                    cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                        changeValue: 2000,
                        isInheritedEffect: true,
                        card: card,
                        condition: InheritedEffectCondition));
                
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
                    var activatePlayClass = new ActivateClass();
                    activatePlayClass.SetUpICardEffect("Select 1 of your Digimon to gain battle protection", CanUsePlayCondition, card);
                    activatePlayClass.SetUpActivateClass(CanActivateCondition, ActivatePlayCoroutine, -1, true, EffectDiscription());
                    cardEffects.Add(activatePlayClass);

                    var activateDigivolveClass = new ActivateClass();
                    activateDigivolveClass.SetUpICardEffect("Select 1 of your Digimon to gain battle protection", CanUseDigivolveCondition, card);
                    activateDigivolveClass.SetUpActivateClass(CanActivateCondition, ActivateDigivolveCoroutine, -1, true, EffectDiscription());
                    cardEffects.Add(activateDigivolveClass);

                    string EffectDiscription()
                    {
                        return "[On Play] [When Digivolving] 1 of your Digimon can't be deleted in battle until the end of your opponent's turn.";
                    }

                    bool CanUsePlayCondition(Hashtable hashtable)
                    {
                        return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                    }

                    bool CanUseDigivolveCondition(Hashtable hashtable)
                    {
                        return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                    }

                    bool CanActivateCondition(Hashtable hashtable)
                    {
                        return CardEffectCommons.IsExistOnBattleArea(card);
                    }
                    
                    bool CanSelectPermanentCondition(Permanent permanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                    }

                    IEnumerator ActivatePlayCoroutine(Hashtable hashtable)
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
                            cardEffect: activatePlayClass);
                        
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
                                activateClass: activatePlayClass,
                                effectName: "Can't be deleted in battle"));
                            yield break;
                            
                            bool CanNotBeDestroyedByBattleCondition(Permanent permanent1, Permanent attackingPermanent, Permanent defendingPermanent, CardSource defendingCard)
                            {
                                return permanent1 == attackingPermanent || permanent1 == defendingPermanent;
                            }
                        }
                    }

                    IEnumerator ActivateDigivolveCoroutine(Hashtable hashtable)
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
                            cardEffect: activateDigivolveClass);

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
                                activateClass: activateDigivolveClass,
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