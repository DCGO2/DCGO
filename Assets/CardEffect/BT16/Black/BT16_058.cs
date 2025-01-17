using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT16
{
    public class BT16_058 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.CardNames.Contains("Dorumon"))
                    {
                        return true;
                    }

                    if (targetPermanent.TopCard.Level == 3 && targetPermanent.TopCard.ContainsTraits("SoC"))
                    {
                        return true;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }

            #endregion

            #region Collision

            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.CollisionSelfStaticEffect(false, card, null));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By trashing 1 card in your hand, <Draw 1>. Then, if this Digimon has a Tamer with the [SoC] trait in its digivolution cards, 1 of your opponent's Digimon gains \"[Start of Your Main Phase] This Digimon attacks.\" until the end of their turn.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return true;
                }

                bool HasTamerCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("SoC"))
                    {
                        if (cardSource.IsTamer)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool discarded = false;
                    Permanent selectedPermanent = null;

                    if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                    {
                        int discardCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: discardCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1)
                            {
                                discarded = true;

                                yield return null;
                            }
                        }
                    }

                    if (discarded)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                        #region Has Tamer with [SoC] Trait - grant Start of Your Main Phase Effect

                        if (card.PermanentOfThisCard().DigivolutionCards.Count(HasTamerCardCondition) >= 1)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

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

                                selectPermanentEffect.SetUpCustomMessage("Selcect 1 Digimon that will get effects.", "The opponent is selecting 1 Digimon that will get effects.");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    selectedPermanent = permanent;

                                    yield return null;
                                }

                                if (selectedPermanent != null)
                                {
                                    ActivateClass activateClass1 = new ActivateClass();
                                    activateClass1.SetUpICardEffect("Attack with this Digimon", CanUseCondition1, selectedPermanent.TopCard);
                                    activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                                    activateClass1.SetEffectSourcePermanent(selectedPermanent);
                                    selectedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

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
                                }
                            }
                        }

                        #endregion
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By trashing 1 card in your hand, <Draw 1>. Then, if this Digimon has a Tamer with the [SoC] trait in its digivolution cards, 1 of your opponent's Digimon gains \"[Start of Your Main Phase] This Digimon attacks.\" until the end of their turn.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return true;
                }

                bool HasTamerCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("SoC"))
                    {
                        if (cardSource.IsTamer)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool discarded = false;
                    Permanent selectedPermanent = null;

                    if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                    {
                        int discardCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: discardCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1)
                            {
                                discarded = true;

                                yield return null;
                            }
                        }
                    }

                    if (discarded)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                        #region Has Tamer with [SoC] Trait - grant Start of Your Main Phase Effect

                        if (card.PermanentOfThisCard().DigivolutionCards.Count(HasTamerCardCondition) >= 1)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

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

                                selectPermanentEffect.SetUpCustomMessage("Selcect 1 Digimon that will get effects.", "The opponent is selecting 1 Digimon that will get effects.");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    selectedPermanent = permanent;

                                    yield return null;
                                }

                                if (selectedPermanent != null)
                                {
                                    ActivateClass activateClass1 = new ActivateClass();
                                    activateClass1.SetUpICardEffect("Attack with this Digimon", CanUseCondition1, selectedPermanent.TopCard);
                                    activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                                    activateClass1.SetEffectSourcePermanent(selectedPermanent);
                                    selectedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

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
                                }
                            }
                        }

                        #endregion
                    }
                }
            }

            #endregion

            #region Inherited Effect

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect<Func<int>>(
                    changeValue: () => 1000,
                    isInheritedEffect: true,
                    card: card,
                    condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}