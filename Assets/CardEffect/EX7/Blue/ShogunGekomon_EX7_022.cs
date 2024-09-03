using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects
{
    public class ShogunGekomon_EX7_022 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardTraits.Contains("NSp");
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
            
            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Opponent's 1 Digimon or Tamer can't suspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] 1 of your opponent's Digimon or Tamers can't suspend until the end of their turn.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                    {
                        int maxCount = Math.Min(1, card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

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

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or Tamer that will be unable to suspend.", "The opponent is selecting 1 Digimon that will get unable to suspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            if (selectedPermanent != null)
                            {
                                CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                                canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCondition1, card);
                                canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCondition);
                                selectedPermanent.UntilOwnerTurnEndEffects.Add((_timing) => canNotSuspendClass);

                                if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                                }

                                bool CanUseCondition1(Hashtable hashtable)
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

                                bool PermanentCondition(Permanent permanent)
                                {
                                    if (permanent == selectedPermanent)
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
            
            #endregion
            
            #region Owner's Turn Effect

            if (timing == EffectTiming.None)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("NSp digimon can't have attack targets switched", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return
                        "[Your Turn] All of your Digimon with the [NSp] trait trait can't have their attack targets switched.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return CardEffectCommons.IsOwnerTurn(card);
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        return true;
                    
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<Permanent> validCards = new List<Permanent>();

                    foreach (Permanent toCheck in card.Owner.GetBattleAreaPermanents())
                    {
                        if (toCheck.TopCard.CardTraits.Contains("NSp"))
                            validCards.Add(toCheck);
                    }

                    if (validCards.Count > 0)
                    {
                        foreach (Permanent validCard in validCards)
                        {
                            CanNotSwitchAttackTargetClass canNotSwitchAttackTargetClass =
                                new CanNotSwitchAttackTargetClass();
                            canNotSwitchAttackTargetClass.SetUpICardEffect("This Digimon's attack target can't be switched", CanUseCondition1,
                                validCard.TopCard);
                            canNotSwitchAttackTargetClass.SetUpCanNotSwitchAttackTargetClass(PermanentCondition: PermanentCondition);
                            validCard.UntilEachTurnEndEffects.Add((_timing) => canNotSwitchAttackTargetClass);

                            yield return ContinuousController.instance.StartCoroutine(
                                GManager.instance.GetComponent<Effects>().CreateBuffEffect(validCard));

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                if (validCard != null)
                                {
                                    return true;
                                }

                                return false;
                            }

                            bool PermanentCondition(Permanent permanent)
                            {
                                if (permanent != null)
                                {
                                    if (permanent.TopCard != null)
                                    {
                                        if (permanent == validCard)
                                        {
                                            return true;
                                        }
                                    }
                                }

                                return false;
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