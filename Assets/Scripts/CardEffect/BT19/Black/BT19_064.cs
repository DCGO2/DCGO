using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT19
{
    public class BT19_064 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Justimon: Accel Arm") || targetPermanent.TopCard.EqualsCardName("Justimon: Critical Arm");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 1, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            
            #endregion

            #region Blast Digivolve
            
            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }
            
            #endregion
            
            #region On Play
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Digimon gains Blocker and isn't affected by digimon effects", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] This Digimon gains <Blocker> and isn't affected by your opponent's Digimon's effects until the end of your opponent's turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                    canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's digimon's effects", CanUseCondition1, card);
                    canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                    selectedPermanent.UntilOpponentTurnEndEffects.Add((_timing) => canNotAffectedClass);

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent);
                    }

                    bool CardCondition(CardSource cardSource)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                        {
                            if (cardSource == selectedPermanent.TopCard)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    bool SkillCondition(ICardEffect cardEffect)
                    {
                        if (CardEffectCommons.IsOpponentEffect(cardEffect, card))
                        {
                            if (cardEffect.IsDigimonEffect)
                            {
                                return true;
                            }
                        }

                        return false;
                    }
                    
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(
                        targetPermanent: card.PermanentOfThisCard(),
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass));
                    
                }
            }
            
            #endregion
            
            #region When Digivolving - Immunity
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Digimon gains Blocker and isn't affected by digimon effects", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] This Digimon gains <Blocker> and isn't affected by your opponent's Digimon's effects until the end of your opponent's turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                    canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's digimon's effects", CanUseCondition1, card);
                    canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                    selectedPermanent.UntilOpponentTurnEndEffects.Add((_timing) => canNotAffectedClass);

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent);
                    }

                    bool CardCondition(CardSource cardSource)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                        {
                            if (cardSource == selectedPermanent.TopCard)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    bool SkillCondition(ICardEffect cardEffect)
                    {
                        if (CardEffectCommons.IsOpponentEffect(cardEffect, card))
                        {
                            if (cardEffect.IsDigimonEffect)
                            {
                                return true;
                            }
                        }

                        return false;
                    }
                    
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(
                        targetPermanent: card.PermanentOfThisCard(),
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass));
                    
                }
            }
            
            #endregion
            
            #region When Digivolving - Unsuspend
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 1 option card, unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("TrashUnsuspend_BT19_062");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] (Once Per Turn) By trashing 1 Option card in the battle area, unsuspend this Digimon.";
                }

                bool IsOption(Permanent permanent)
                {
                    return permanent.IsOption;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(IsOption);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsOption,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Option to trash.",
                        "The opponent is selecting 1 Option to trash.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent>() { selectedPermanent },
                            activateClass: activateClass,
                            successProcess: permanents => SuccessProcess(),
                            failureProcess: null));

                        IEnumerator SuccessProcess()
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                                new List<Permanent> { card.PermanentOfThisCard() },
                                activateClass).Unsuspend());
                        }
                    }
                }
            }
            
            #endregion
            
            #region When Attacking
            
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 1 option card, unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("TrashUnsuspend_BT19_062");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] (Once Per Turn) By trashing 1 Option card in the battle area, unsuspend this Digimon.";
                }

                bool IsOption(Permanent permanent)
                {
                    return permanent.IsOption;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(IsOption);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsOption,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Option to trash.",
                        "The opponent is selecting 1 Option to trash.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent>() { selectedPermanent },
                            activateClass: activateClass,
                            successProcess: permanents => SuccessProcess(),
                            failureProcess: null));

                        IEnumerator SuccessProcess()
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                                new List<Permanent> { card.PermanentOfThisCard() },
                                activateClass).Unsuspend());
                        }
                    }
                }
            }
            
            #endregion

            return cardEffects;
        }
    }
}