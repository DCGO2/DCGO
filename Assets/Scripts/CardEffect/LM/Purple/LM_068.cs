using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// HeavyMetaldramon // Black Sabbath
namespace DCGO.CardEffects.LM
{
    public class LM_068 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Effects
            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Dark Dragon")
                            || targetPermanent.TopCard.EqualsTraits("Evil Dragon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 4,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 5));
            }
            #endregion

            #region Security +1
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared WD / EoA
            string SharedEffectName1 = "May play 1 lvl 4 or lower [Evil]/[Dark Dragon]/[Evil Dragon] Digimon from trash for free";
            string SharedEffectDescription1(string tag) => $"[{tag}] If your hand has 4 or fewer cards, you may play 1 level 4 or lower Digimon card with the [Evil], [Dark Dragon] or [Evil Dragon] trait from your trash to the field without paying the cost.";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName1,
                    SharedActivateCoroutine1,
                    SharedEffectDescription1,
                    additionalActivateCondition: (hashtable, effect) => card.Owner.HandCards.Count <= 4,
                    optional: false,
                    isSkippable:true,
                    whenDigivolving: true,
                    endOfAttack: true);

            bool CanSelectCardCondition1(CardSource cardSource)
            {
                return cardSource.IsDigimon
                    && cardSource.HasLevel
                    && cardSource.Level <= 4
                    && (cardSource.EqualsTraits("Evil")
                        || cardSource.EqualsTraits("Dark Dragon")
                        || cardSource.EqualsTraits("Evil Dragon"));
            }

            IEnumerator SharedActivateCoroutine1(Hashtable hashtable, ActivateClass activateClass)
            {
                bool isBreedingArea = false;
                bool hasEmptyBattleAreaFrame = card.Owner.fieldCardFrames.Some((frame) => frame.IsEmptyFrame() && frame.IsBattleAreaFrame());

                if (card.Owner.GetBreedingAreaPermanents().Count == 0 && hasEmptyBattleAreaFrame)
                {
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>
                    {
                        new(message: $"Battle Area", value: 1, spriteIndex: 0),
                        new(message: $"Breeding Area", value: 2, spriteIndex: 0),
                        new(message: $"Do not play", value: 3, spriteIndex: 1)
                    };

                    string selectPlayerMessage = "Where will you play a card to?";
                    string notSelectPlayerMessage = "The opponent is choosing what location to play a card to.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool doPlay = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                    isBreedingArea = GManager.instance.userSelectionManager.SelectedIntValue == 2;
                }
                else if (card.Owner.GetBreedingAreaPermanents().Count == 0 && !hasEmptyBattleAreaFrame)
                {
                    isBreedingArea = true;
                }
                else if (!card.Owner.fieldCardFrames.Some((frame) => frame.IsEmptyFrame()))
                {
                    yield break;
                }

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                    canTargetCondition: CanSelectCardCondition1,
                    root: SelectCardEffect.Root.Trash,
                    cardEffect: activateClass,
                    payCost: false,
                    isBreedingArea: isBreedingArea));
            }
            #endregion

            #region Shared WD / OD
            string SharedEffectName2 = "Trash 2, then may activate Option effect";
            string SharedEffectDescription2(string tag) => $"[{tag}] Trash 2 cards in your hand. Then, you may activate 1 [Main] effect on this card's Option side.";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName2,
                    SharedActivateCoroutine2,
                    SharedEffectDescription2,
                    optional: false,
                    whenDigivolving: true,
                    onDeletion: true);

            IEnumerator SharedActivateCoroutine2(Hashtable hashtable, ActivateClass activateClass)
            {
                if (card.Owner.HandCards.Count >= 1)
                {
                    int discardCount = Math.Min(2, card.Owner.HandCards.Count);

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: (cardSource) => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: discardCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    yield return StartCoroutine(selectHandEffect.Activate());
                }

                List<SelectionElement<bool>> command_SelectCommands = new List<SelectionElement<bool>>()
                {
                    new SelectionElement<bool>(message: $"Yes", value: true, spriteIndex: 0),
                    new SelectionElement<bool>(message: $"No", value: false, spriteIndex: 1),
                };

                string selectPlayerMessage = "Will you use Option side Main effect?";
                string notSelectPlayerMessage = "The opponent is choosing if they will use Option side Main effect.";

                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: command_SelectCommands, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                bool use = GManager.instance.userSelectionManager.SelectedBoolValue;

                if (use) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ActivateMainOfOptionSide(card, activateClass));
            }
            #endregion
            #endregion

            #region Option Effects
            #region Trash Main
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May use this card with the cost reduced by 3", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Trash] [Your Turn] When of your [Evil], [Dark Dragon] or [Evil Dragon] trait Digimon attacks, if your hand has 4 or fewer cards, you may use this card with the cost reduced by 3.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.IsExistOnTrash(card)
                        && CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return card.Owner.HandCards.Count <= 4
                        && CanUseOption(card);
                }

                bool CanUseOption(CardSource card)
                {
                    return !card.CanNotPlayThisOption
                        && card.Owner.MaxMemoryCost >= card.GetCostItself - 3;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.IsDigimon
                        && (permanent.TopCard.EqualsTraits("Evil")
                            || permanent.TopCard.EqualsTraits("Evil Dragon")
                            || permanent.TopCard.EqualsTraits("Dark Dragon"));
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    int reducedCost = 3;

                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect($"Reduce cost by {reducedCost}", CanUseCondition1, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: _ => true, isUpDown: () => true, isCheckAvailability: () => false, isChangePayingCost: () => true);
                    card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return true;
                    }

                    int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                    {
                        if (CardSourceCondition(cardSource) &&
                            PermanentsCondition(targetPermanents))
                        {
                            cost -= reducedCost;
                        }

                        return cost;
                    }

                    bool PermanentsCondition(List<Permanent> targetPermanents)
                    {
                        return targetPermanents == null || targetPermanents.Count(targetPermanent => targetPermanent != null) == 0;
                    }

                    bool CardSourceCondition(CardSource cardSource)
                    {
                        return cardSource != null
                            && cardSource.Owner == card.Owner;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                        cardSources: new List<CardSource>() { card },
                        activateClass: activateClass,
                        payCost: true,
                        root: SelectCardEffect.Root.Trash
                    ));
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 enemy Digimon with level equal or higher than hand count", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] Delete 1 of your opponent's Digimon with a level as high as the number of cards in your hand or higher.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool CanSelectEnemyPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasLevel
                        && permanent.TopCard.Level >= card.Owner.HandCards.Count;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectEnemyPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectEnemyPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
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
