using System.Collections;
using System.Collections.Generic;
using System.Linq;

// LordKnightmon (X Antibody) // Ultimate Thrusting Sword Gran Rozerio
namespace DCGO.CardEffects.BT27
{
    public class BT27_084 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Effects
            #region Alternate Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.EqualsCardName("LordKnightmon");
                }
            
                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 2, true, card, null));
            }
            #endregion

            #region Alliance
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Collision
            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.CollisionSelfStaticEffect(false, card, null));
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Add top sec to hand, may play/use 1 [Iliad]/[TS] from hand cost -5", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "When Digivolving] You may play up to 8 play cost's total worth of cards with [Knightmon] in their texts from your hand without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 8
                        && cardSource.HasText("Knightmon")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int totalCost = 8;
                    bool stop = false;
                    List<CardSource> allselectedcards = new List<CardSource>();

                    bool CanSelectCardCondition(CardSource cardSource)
                        => cardSource.HasText("Knightmon")
                            && cardSource.HasPlayCost
                            && cardSource.GetCostItself <= totalCost
                            && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass)
                            && !allselectedcards.Contains(cardSource);

                    bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                    {
                        int sumCost = cardSource.GetCostItself;
                        foreach (CardSource cardSource1 in cardSources) sumCost += cardSource1.GetCostItself;
                        return sumCost <= totalCost;
                    }

                    bool CanEndSelectCardCondition(List<CardSource> cards)
                    {
                        int sumCost = 0;
                        foreach (CardSource source in cards) sumCost += source.GetCostItself;
                        return sumCost <= totalCost;
                    }

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        allselectedcards.Add(cardSource);
                        List<CardSource> cardSources = new List<CardSource> { cardSource };

                        foreach (CardSource cardSource1 in cardSources)
                        {
                            totalCost -= cardSource1.GetCostItself;
                        }

                        yield return null;
                    }

                    IEnumerator afterSelectCardCoroutine(List<CardSource> selectedCards)
                    {
                        if (selectedCards.Count == 0)
                        {
                            stop = true;
                        }

                        yield return null;
                    }

                    while (!stop)
                    {
                        int maxCount = CardEffectCommons.MatchConditionOwnersCardCountInHand(card, CanSelectCardCondition);

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                            canEndSelectCondition: CanEndSelectCardCondition,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: true,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: afterSelectCardCoroutine,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage($"Select up to {totalCost} play cost worth of cards to play from your hand.", $"The opponent is selecting up to {totalCost} play cost worth of cards to play from their hand.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Selected cards");
                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                    }

                    if (allselectedcards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: allselectedcards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Hand,
                            activateETB: true));
                    }
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnEnterFieldAnyone || timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By suspending this Tamer, 1 [DATA SQUAD] Digimon gains Execute", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT27_084_AT");
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When any of your other Digimon are played or deleted, by trashing 1 [LordKnightmon] or [X Antibody] from this Digimon's digivolution cards, activate 1 [Main] effect on this card's Option side.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && ((timing == EffectTiming.OnEnterFieldAnyone
                            && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, IsDigimonCondition))
                        || (timing == EffectTiming.OnDestroyedAnyone
                            && CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, IsDigimonCondition, activateClass)));

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.PermanentOfThisCard().DigivolutionCards.Any(TrashConditionCard);

                bool IsDigimonCondition(Permanent permanent)
                {
                    return permanent != card.PermanentOfThisCard()
                        && CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool TrashConditionCard(CardSource cardSource)
                {
                    return (cardSource.EqualsCardName("LordKnightmon")
                            || cardSource.EqualsCardName("X Antibody"))
                        && !cardSource.CanNotTrashFromDigivolutionCards(activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanment = card.PermanentOfThisCard();
                    CardSource selectedCard = null;
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: TrashConditionCard,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 digivolution card to discard.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.DigivolutionCards,
                        customRootCardList: selectedPermanment.DigivolutionCards,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: null);

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to discard.", "The opponent is selecting 1 digivolution card to discard.");
                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsAndProcessAccordingToResult(
                        targetPermanent: selectedPermanment,
                        targetDigivolutionCards: new List<CardSource>() { selectedCard },
                        activateClass: activateClass,
                        successProcess: SuccessProcess,
                        failureProcess: null));

                    IEnumerator SuccessProcess(List<CardSource> cardSources)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ActivateMainOfOptionSide(card, activateClass));
                    }
                }
            }
            #endregion
            #endregion

            #region Option Effects
            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 lowest level enemy Digimon, if none deleted all your [Knightmon] text Digimon gain <Blocker>, Digimon immunity, and 3K DP until your turn ends.", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] Delete 1 of your opponent's lowest level Digimon. If this effect didn't delete, until your turn ends, all of your Digimon with [Knightmon] in their texts gain <Blocker>, your opponent's Digimon effects don't affect them and they get +3000 DP.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectDeletePermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<Permanent> deleteTargetPermanents = new List<Permanent>();
                    bool failedToDelete = true;

                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectDeletePermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDeletePermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            deleteTargetPermanents = permanents.Clone();

                            yield return null;
                        }
                    }

                    if (deleteTargetPermanents.Count > 0)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: deleteTargetPermanents, activateClass: activateClass, successProcess: SuccessDeleteProcess, failureProcess: null));

                        IEnumerator SuccessDeleteProcess(List<Permanent> permanents)
                        {
                            if (permanents.Count > 0)
                                failedToDelete = false;

                            yield return null;
                        }
                    }

                    if (failedToDelete)
                    {
                        CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                        canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's Digimon's effects", CanUseConditionImmunity, card);
                        canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                        card.Owner.UntilOwnerTurnEndEffects.Add(GetCardEffect);
                        card.Owner.UntilOwnerTurnEndEffects.Add(GetDetailEffect);

                        bool CanUseConditionImmunity(Hashtable hashtable)
                        {
                            return true;
                        }

                        bool CardCondition(CardSource cardSource)
                        {
                            return CardEffectCommons.IsExistOnBattleAreaDigimon(cardSource)
                                && card.Owner == cardSource.Owner
                                && cardSource.HasText("Knightmon");
                        }

                        bool SkillCondition(ICardEffect cardEffect)
                        {
                            return CardEffectCommons.IsOpponentEffect(cardEffect, card)
                                && ((!cardEffect.EffectSourceCard.IsDualCard && (cardEffect.EffectSourceCard.IsDigimon || cardEffect.IsDigimonEffect))
                                    || (cardEffect.EffectSourceCard.IsDualCard && !cardEffect.IsOptionEffect));
                        }

                        ICardEffect GetCardEffect(EffectTiming _timing)
                        {
                            if (_timing == EffectTiming.None)
                            {
                                return canNotAffectedClass;
                            }

                            return null;
                        }

                        bool PermanentCondition(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                && permanent.TopCard.HasText("Knightmon");
                        }

                        ICardEffect GetDetailEffect(EffectTiming timing)
                        {
                            if (timing == EffectTiming.None)
                            {
                                return CardEffectFactory.AddDetailClass(CanUseConditionImmunity, PermanentCondition, "Isn't affected by opponent's Digimon's effects", false, card);
                            }
                            return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlockerPlayerEffect(
                            permanentCondition: PermanentCondition,
                            effectDuration: EffectDuration.UntilOwnerTurnEnd,
                            activateClass: activateClass));

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDPPlayerEffect(
                            permanentCondition: PermanentCondition,
                            changeValue: 3000,
                            effectDuration: EffectDuration.UntilOwnerTurnEnd,
                            activateClass: activateClass));
                    }
                }
            }
            #endregion

            #region Arts Digivolution
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            }
            #endregion
            #endregion

            return cardEffects;
        }
    }
}
