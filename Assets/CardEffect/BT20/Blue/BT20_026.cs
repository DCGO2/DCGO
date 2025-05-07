using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.BT20
{
    public class BT20_026 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("MegaSeadramon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 0,
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
                activateClass.SetUpICardEffect("Return 1 opponent's Digimon to bottom deck and apply an effect to an opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Return 1 of your opponent's level 4 or lower Digimon to the bottom of the deck. Then, if [MegaSeadramon]/[X Antibody] is in this Digimon's digivolution cards, 1 of your opponent's Digimon can't suspend until the end of their turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.Level <= 4)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectOpponentDigimonCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        if (permanent.IsDigimon)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.EqualsCardName("MegaSeadramon") || cardSource.EqualsCardName("X Antibody")) >= 1)
                    {
                        if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectOpponentDigimonCondition) >= 1)
                        {
                            int maxCount = Math.Min(1, card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectOpponentDigimonCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectOpponentDigimonCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get unable to suspend.", "The opponent is selecting 1 Digimon that will get unable to suspend.");

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
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 opponent's Digimon to bottom deck and apply an effect to an opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Return 1 of your opponent's level 4 or lower Digimon to the bottom of the deck. Then, if [MegaSeadramon]/[X Antibody] is in this Digimon's digivolution cards, 1 of your opponent's Digimon can't suspend until the end of their turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);

                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.Level <= 4)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectOpponentDigimonCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        if (permanent.IsDigimon)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.EqualsCardName("MegaSeadramon") || cardSource.EqualsCardName("X Antibody")) >= 1)
                    {
                        if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectOpponentDigimonCondition) >= 1)
                        {
                            int maxCount = Math.Min(1, card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectOpponentDigimonCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectOpponentDigimonCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get unable to suspend.", "The opponent is selecting 1 Digimon that will get unable to suspend.");

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
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.None)
            {
                CanNotSwitchAttackTargetClass canNotSwitchAttackTargetClass = new CanNotSwitchAttackTargetClass();
                canNotSwitchAttackTargetClass.SetUpICardEffect("This Digimon's attack target can't change.", CanUseCondition, card);
                canNotSwitchAttackTargetClass.SetUpCanNotSwitchAttackTargetClass(PermanentCondition: PermanentCondition);
                canNotSwitchAttackTargetClass.SetIsInheritedEffect(true);
                cardEffects.Add(canNotSwitchAttackTargetClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) && CardEffectCommons.IsOwnerTurn(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent != null && permanent.TopCard && permanent == card.PermanentOfThisCard();
                }
            }
            #endregion 

            return cardEffects;
        }
    }
}