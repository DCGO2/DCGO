using System;
using System.Collections;
using System.Collections.Generic;

// WereGarurumon
namespace DCGO.CardEffects.BT23
{
    public class BT23_056 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasCSTraits
                        && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.IsLevel4;
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

            #region Blocker

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region OP/WD Shared

            bool IsCsTamer(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                    && permanent.IsTamer
                    && permanent.TopCard.HasCSTraits;
            }

            bool SharedCanSelectPermamentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                Permanent selectedPermanent = null;

                #region Select Permament

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(SharedCanSelectPermamentCondition));

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: SharedCanSelectPermamentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    selectedPermanent = permanent;
                    yield return null;
                }

                selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to gain '[Start of Your Main Phase] This Digimon attacks'.", "The opponent is selecting 1 digimon to gain '[Start of Your Main Phase] This Digimon attacks'.");
                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                #endregion

                if (selectedPermanent != null)
                {
                    #region Setup Attack on Start of Main Phase

                    ActivateClass activateClass1 = new ActivateClass();
                    activateClass1.SetUpICardEffect("Attack with this Digimon", CanUseCondition1, selectedPermanent.TopCard);
                    activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                    activateClass1.SetEffectSourcePermanent(selectedPermanent);

                    if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                    }

                    string EffectDiscription1()
                    {
                        return "[Start of Your Main Phase] Attack with this Digimon.";
                    }

                    bool CanUseCondition1(Hashtable hashtable1)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                        {
                            if (selectedPermanent.TopCard.Owner.GetBattleAreaDigimons().Contains(selectedPermanent))
                            {
                                if (GManager.instance.turnStateMachine.gameContext.TurnPlayer == selectedPermanent.TopCard.Owner)
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
                    }

                    bool CanActivateCondition1(Hashtable hashtable1)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                        {
                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                if (selectedPermanent.CanAttack(activateClass1))
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
                    }

                    IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                        {
                            if (selectedPermanent.CanAttack(activateClass1))
                            {
                                SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                selectAttackEffect.SetUp(
                                    attacker: selectedPermanent,
                                    canAttackPlayerCondition: () => true,
                                    defenderCondition: (permanent) => true,
                                    cardEffect: activateClass1);

                                selectAttackEffect.SetCanNotSelectNotAttack();

                                yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                            }
                        }
                    }

                    ICardEffect GetCardEffect(EffectTiming _timing)
                    {
                        if (_timing == EffectTiming.OnStartMainPhase)
                        {
                            return activateClass1;
                        }

                        return null;
                    }

                    #endregion

                    selectedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Give 1 digimon '[Start of your main phase] this digimon attacks' ", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] If you have a Tamer with the [CS] trait, give 1 of your opponent's Digimon '[Start of Your Main Phase] This Digimon attacks.' until their turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionPermanent(IsCsTamer)
                        && CardEffectCommons.HasMatchConditionPermanent(SharedCanSelectPermamentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return null;
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Give 1 digimon '[Start of your main phase] this digimon attacks' ", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If you have a Tamer with the [CS] trait, give 1 of your opponent's Digimon '[Start of Your Main Phase] This Digimon attacks.' until their turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionPermanent(IsCsTamer)
                        && CardEffectCommons.HasMatchConditionPermanent(SharedCanSelectPermamentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return null;
                }
            }

            #endregion

            #region All Turns - ESS

            if (timing == EffectTiming.OnAttackTargetChanged)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<De-Digivolve 1>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("Bt23_056_AT");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When attack targets change, <De-Digivolve 1> 1 of your opponent's Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPermanentAttackTargetSwitch(hashtable, permanent => true);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(SharedCanSelectPermamentCondition))
                    {
                        Permanent selectedPermanent = null;

                        #region Select Permament

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(SharedCanSelectPermamentCondition));

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: SharedCanSelectPermamentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to <De-Digivolve>.", "The opponent is selecting 1 digimon to <De-Digivolve>.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        #endregion

                        if (selectedPermanent != null) yield return ContinuousController.instance.StartCoroutine(new IDegeneration(
                            permanent: selectedPermanent,
                            DegenerationCount: 1,
                            cardEffect: activateClass).Degeneration()
                        );
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
