using System;
using System.Collections;
using System.Collections.Generic;

// Alphamon
namespace DCGO.CardEffects.EX13
{
    public class EX13_060 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - [Grademon]
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Grademon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Alternate Digivolution Requirement - Lv.5 w/[Chronicle] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasChronicleTraits;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null,
                    level: 5));
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DP -8000, then if opponent has 5 or more memory, gain 2 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetHashString("EX13_060_WD");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Digivolving] 1 of your opponent's Digimon gets -8000 DP until their turn ends. Then, if they have 5 or more memory, gain 2 memory.";

                bool CanSelectPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

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
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -8000.", "The opponent is selecting 1 Digimon that will get DP -8000.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: permanent,
                                changeValue: -8000,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));
                        }
                    }

                    if (card.Owner.Enemy.MemoryForPlayer >= 5
                        && card.Owner.CanAddMemory(activateClass))
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(2, activateClass));
                    }
                }
            }
            #endregion

            #region Your Turn - OPT
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 of your Digimon may attack, then may activate 1 [When Digivolving] effect", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("EX13_060_YT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Your Turn] [Once Per Turn] When any of your [Chronicle] trait Digimon or Tamers are played, 1 of your Digimon may attack. Then, you may activate 1 of this Digimon's [When Digivolving] effects.";

                bool PlayedPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && (permanent.IsDigimon || permanent.IsTamer)
                        && permanent.TopCard.HasChronicleTraits;

                bool CanAttackPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.CanAttack(activateClass);

                bool CanBeEffectCandidate(ICardEffect cardEffect)
                {
                    if (cardEffect != null
                        && cardEffect is ActivateICardEffect
                        && !cardEffect.IsSecurityEffect
                        && cardEffect.IsWhenDigivolving)
                    {
                        Hashtable digivolvingHashtable = CardEffectCommons.WhenDigivolvingCheckHashtableOfCard(cardEffect.EffectSourceCard);

                        return cardEffect.CanUse(digivolvingHashtable);
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PlayedPermanentCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanAttackPermanentCondition)
                            || card.PermanentOfThisCard().EffectList(EffectTiming.OnEnterFieldAnyone).Some(CanBeEffectCandidate));

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool didSomething = false;

                    #region May Attack
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanAttackPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanAttackPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectAttackerCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Attack,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will attack.", "The opponent is selecting 1 Digimon that will attack.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectAttackerCoroutine(Permanent permanent)
                        {
                            didSomething = true;

                            yield return null;
                        }
                    }
                    #endregion

                    #region May Activate [When Digivolving]
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        List<ICardEffect> candidateEffects = card.PermanentOfThisCard().EffectList(EffectTiming.OnEnterFieldAnyone).Clone().Filter(CanBeEffectCandidate);

                        if (candidateEffects.Count >= 1)
                        {
                            GManager.instance.userSelectionManager.SetBoolSelection(
                                selectionElements: new List<SelectionElement<bool>>()
                                {
                                    new(message: "Activate", value: true, spriteIndex: 0),
                                    new(message: "Don't activate", value: false, spriteIndex: 1),
                                },
                                selectPlayer: card.Owner,
                                selectPlayerMessage: "Will you activate 1 of this Digimon's [When Digivolving] effects?",
                                notSelectPlayerMessage: "The opponent is choosing whether to activate a [When Digivolving] effect.");

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            if (GManager.instance.userSelectionManager.SelectedBoolValue)
                            {
                                ICardEffect selectedEffect = null;

                                if (candidateEffects.Count == 1)
                                {
                                    selectedEffect = candidateEffects[0];
                                }
                                else
                                {
                                    List<SkillInfo> skillInfos = candidateEffects
                                        .Map(cardEffect => new SkillInfo(cardEffect, null, EffectTiming.None));

                                    List<CardSource> cardSources = candidateEffects
                                        .Map(cardEffect => cardEffect.EffectSourceCard);

                                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                    selectCardEffect.SetUp(
                                        canTargetCondition: (cardSource) => true,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => false,
                                        selectCardCoroutine: null,
                                        afterSelectCardCoroutine: null,
                                        message: "Select 1 effect to activate.",
                                        maxCount: 1,
                                        canEndNotMax: false,
                                        isShowOpponent: false,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Custom,
                                        customRootCardList: cardSources,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                                    selectCardEffect.SetNotShowCard();
                                    selectCardEffect.SetUpSkillInfos(skillInfos);
                                    selectCardEffect.SetUpAfterSelectIndexCoroutine(AfterSelectIndexCoroutine);

                                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                    IEnumerator AfterSelectIndexCoroutine(List<int> selectedIndexes)
                                    {
                                        if (selectedIndexes.Count == 1)
                                        {
                                            selectedEffect = candidateEffects[selectedIndexes[0]];
                                        }

                                        yield return null;
                                    }
                                }

                                if (selectedEffect != null
                                    && selectedEffect.EffectSourceCard != null
                                    && selectedEffect.EffectSourceCard.PermanentOfThisCard() != null)
                                {
                                    selectedEffect.SetIsDigimonEffect(true);
                                    Hashtable digivolvingHashtable = CardEffectCommons.WhenDigivolvingCheckHashtableOfCard(selectedEffect.EffectSourceCard);

                                    if (selectedEffect.CanUse(digivolvingHashtable))
                                    {
                                        didSomething = true;

                                        yield return ContinuousController.instance.StartCoroutine(((ActivateICardEffect)selectedEffect).Activate_Optional_Effect_Execute(digivolvingHashtable));
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    if (!didSomething) activateClass.RemoveUse();
                }
            }
            #endregion

            #region End of Your Turn - OPT
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May play 1 [Chronicle] trait card from hand for 6 less, it gains <Rush>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("EX13_060_EOT");
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[End of Your Turn] [Once Per Turn] You may play 1 [Chronicle] trait card without [Alphamon] in its name from your hand with the cost reduced by 6. It gains <Rush> for the turn.";

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.HasChronicleTraits
                        && !cardSource.ContainsCardName("Alphamon")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, true, activateClass, fixedCost: Math.Max(0, cardSource.GetCostItself - 6));

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                        canTargetCondition: CanSelectCardCondition,
                        root: SelectCardEffect.Root.Hand,
                        cardEffect: activateClass,
                        payCost: true,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        reduceCostTuple: (reduceCost: 6, reduceCostCardCondition: null)));

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        selectedCards.AddRange(cardSources);

                        yield return null;
                    }

                    if (selectedCards.Count == 0)
                    {
                        activateClass.RemoveUse();
                        yield break;
                    }

                    foreach (CardSource selectedCard in selectedCards)
                    {
                        Permanent playedPermanent = selectedCard.PermanentOfThisCard();

                        if (playedPermanent != null
                            && playedPermanent.IsDigimon
                            && CardEffectCommons.IsPermanentExistsOnBattleArea(playedPermanent))
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRush(
                                targetPermanent: playedPermanent,
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass));
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
