using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX6
{
    public class EX6_042 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 4)
                    {
                        return targetPermanent.TopCard.ContainsTraits("Legend-Arms");
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region Hand - Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("One of your opponents Digimon must attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsDigimonEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Hand] [Main] By paying 2 cost and placing this card as the bottom digivolution card of 1 of your Digimon that's level 5 or has the [Legend-Arms] trait, until the end of your opponent's turn, 1 of their Digimon gains \"[Start of Your Main Phase] This Digimon attacks.\"";
                }

                bool IsLevel5OrHasLegendArmsTrait(Permanent targetPermanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(targetPermanent, card))
                    {
                        if (targetPermanent.TopCard.HasLevel && targetPermanent.Level == 5)
                            return true;

                        if (targetPermanent.TopCard.ContainsTraits("Legend-Arms"))
                            return true;
                    }

                    return false;
                }

                bool IsEnemyPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnHand(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsLevel5OrHasLegendArmsTrait))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;
                    Permanent selectedEnemy = null;

                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(-2, activateClass));

                    if (CardEffectCommons.HasMatchConditionPermanent(IsLevel5OrHasLegendArmsTrait))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsLevel5OrHasLegendArmsTrait));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsLevel5OrHasLegendArmsTrait,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to add bottom digivolution source.", "The opponent is selecting 1 Digimon to add bottom digivolution source.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }
                    }

                    if (selectedPermanent != null)
                    {
                        Permanent selectedPermanent1 = null;

                        if (CardEffectCommons.HasMatchConditionPermanent(IsEnemyPermanent))
                        {
                            int enemyCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsEnemyPermanent));

                            SelectPermanentEffect selectEnemyEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectEnemyEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsEnemyPermanent,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: enemyCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectEnemyEffect.SetUpCustomMessage("Select 1 Digimon to gain effect.", "The opponent is selecting 1 Digimon gain effect.");

                            yield return ContinuousController.instance.StartCoroutine(selectEnemyEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent1 = permanent;

                                yield return null;
                            }

                            if (selectedPermanent1 != null)
                            {
                                ActivateClass activateClass1 = new ActivateClass();
                                activateClass1.SetUpICardEffect("Attack with this Digimon", CanUseCondition1, selectedPermanent1.TopCard);
                                activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                                activateClass1.SetEffectSourcePermanent(selectedPermanent1);
                                selectedPermanent1.UntilOwnerTurnEndEffects.Add(GetCardEffect);

                                if (!selectedPermanent1.TopCard.CanNotBeAffected(activateClass))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent1));
                                }

                                string EffectDiscription1()
                                {
                                    return "[Start of Your Main Phase] Attack with this Digimon.";
                                }

                                bool CanUseCondition1(Hashtable hashtable1)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent1))
                                    {
                                        if (selectedPermanent1.TopCard.Owner.GetBattleAreaDigimons().Contains(selectedPermanent1))
                                        {
                                            if (GManager.instance.turnStateMachine.gameContext.TurnPlayer == selectedPermanent1.TopCard.Owner)
                                            {
                                                return true;
                                            }
                                        }
                                    }

                                    return false;
                                }

                                bool CanActivateCondition1(Hashtable hashtable1)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent1))
                                    {
                                        if (!selectedPermanent1.TopCard.CanNotBeAffected(activateClass))
                                        {
                                            if (selectedPermanent1.CanAttack(activateClass1))
                                            {
                                                return true;
                                            }
                                        }
                                    }

                                    return false;
                                }

                                IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent1))
                                    {
                                        if (selectedPermanent1.CanAttack(activateClass1))
                                        {
                                            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                            selectAttackEffect.SetUp(
                                                attacker: selectedPermanent1,
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

                        yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(
                                new List<CardSource>() { card },
                                activateClass));
                    }
                }
            }
            #endregion

            #region Your Turn - Place Digivolution
            if (timing == EffectTiming.OnAddDigivolutionCards)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain <Blocker> and <Reboot>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("GainEffects_EX6_042");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] [Once Per Turn] When an effect places a digivolution card under this Digimon, it gains <Blocker> and <Reboot> until the end of your opponent's turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnAddDigivolutionCard(
                                hashtable: hashtable,
                                permanentCondition: permanent => permanent == card.PermanentOfThisCard(),
                                cardEffectCondition: cardEffect => cardEffect.EffectSourceCard != null,
                                cardCondition: null))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(
                                targetPermanent: card.PermanentOfThisCard(),
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainReboot(
                                targetPermanent: card.PermanentOfThisCard(),
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));
                }
            }
            #endregion

            #region All Turns - ESS
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Prevent Deletion", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Protection_EX6_042");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon would be deleted other than by one of your effects, by trashing 1 card with the [Legend-Arms] trait in this Digimon's digivolution cards, prevent that deletion.";
                }

                bool HasLegendArmsTrait(CardSource cardSource)
                {
                    if (!cardSource.CanNotTrashFromDigivolutionCards(activateClass))
                    {
                        return cardSource.ContainsTraits("Legend-Arms");
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card))
                        {
                            if (!CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOwnerEffect(cardEffect, card)))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count(HasLegendArmsTrait) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    if (selectedPermanent.DigivolutionCards.Count(HasLegendArmsTrait) >= 1)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = Math.Min(1, selectedPermanent.DigivolutionCards.Count(HasLegendArmsTrait));

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: HasLegendArmsTrait,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select cards to discard.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: selectedPermanent.DigivolutionCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetNotShowCard();
                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        if (selectedCards.Count >= 1)
                        {
                            selectedPermanent.willBeRemoveField = false;

                            selectedPermanent.HideDeleteEffect();
                            selectedPermanent.HideHandBounceEffect();
                            selectedPermanent.HideDeckBounceEffect();

                            yield return ContinuousController.instance.StartCoroutine(new ITrashDigivolutionCards(selectedPermanent, selectedCards, activateClass).TrashDigivolutionCards());
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}