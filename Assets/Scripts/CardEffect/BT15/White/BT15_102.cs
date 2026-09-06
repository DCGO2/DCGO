using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Apocalymon
namespace DCGO.CardEffects.BT15
{
    public class BT15_102 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Before Pay Cost - Condition Effect
            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Placing 1 [Dark Masters] to get Play Cost -4", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetHashString("PlayCost-12_BT15_102");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "When this card would be played, by placing up to 3 [Dark Masters] trait cards with different names from your battle area or trash under it, reduce the play cost by 4 for each one.";
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool CanNoSelect(CardSource cardSource, int placed)
                {
                    if (cardSource != null
                    && cardSource.PayingCost(SelectCardEffect.Root.Hand, null, checkAvailability: false) - (4 * placed) > cardSource.Owner.MaxMemoryCost)
                    {
                        return false;
                    }

                    return true;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition);
                }

                bool IsDarkMaster(CardSource cardSource) => cardSource.EqualsTraits("Dark Masters");

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsDarkMaster)
                            || CardEffectCommons.HasMatchConditionOwnersPermanent(card, permanent => IsDarkMaster(permanent.TopCard));
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    #region Shared bool and stuff
                    List<CardSource> digivolutionCards = new List<CardSource>();

                    bool CanSelectCardCondition(CardSource cardSource)
                    {
                        if (IsDarkMaster(cardSource))
                        {
                            List<CardSource> chosenCards = digivolutionCards.Clone();
                            chosenCards.Add(cardSource);

                            return Combinations.GetUniqueNameCardCount(chosenCards) == chosenCards.Count;// If the highest unique names does not match the number of cards in the list, something shares a name
                        }

                        return false;
                    }

                    bool CanSelectPermanentCondition(Permanent permanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                            && CanSelectCardCondition(permanent.TopCard);
                    }

                    int validPermanentCount = CardEffectCommons.MatchConditionOwnersPermanentCount(card, CanSelectPermanentCondition);
                    int validTrashCardCount = card.Owner.TrashCards.Count(CanSelectCardCondition);

                    bool noSelect() => CanNoSelect(card, digivolutionCards.Count);

                    bool ValidPermanentCombination(List<Permanent> permanents, Permanent permanent)
                    {
                        List<CardSource> chosenCards = permanents.Map(perm => perm.TopCard);
                        chosenCards.Add(permanent.TopCard);

                        return ValidCardCombination(chosenCards);
                    }

                    bool ValidCardSourceCombination(List<CardSource> cardSources, CardSource cardSource)
                    {
                        List<CardSource> chosenCards = cardSources.Clone();
                        chosenCards.Add(cardSource);

                        return ValidCardCombination(chosenCards);
                    }

                    bool ValidCardCombination(List<CardSource> cardSources)
                    {
                        List<CardSource> chosenCards = digivolutionCards.Clone();
                        chosenCards.AddRange(cardSources);

                        return Combinations.GetUniqueNameCardCount(chosenCards) == chosenCards.Count;// If the highest unique names does not match the number of cards in the list, something shares a name
                    }
                    #endregion

                    while (digivolutionCards.Count < 3
                    && validPermanentCount + validTrashCardCount != 0)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                        if (validPermanentCount > 0)
                        {
                            selectionElements.Add(new(message: "from Battle Area", value: 1, spriteIndex: 0));
                        }

                        if (validTrashCardCount > 0)
                        {
                            selectionElements.Add(new(message: "from Trash", value: 2, spriteIndex: 0));
                        }

                        if (noSelect())
                        {
                            string message = digivolutionCards.Count > 0 ? "Finish Placing" : "Do not place";
                            selectionElements.Add(new(message: message, value: 3, spriteIndex: 1));
                        }

                        GManager.instance.userSelectionManager.SetIntSelection(
                            selectionElements: selectionElements,
                            selectPlayer: card.Owner,
                            selectPlayerMessage: "From which area will you select a card?",
                            notSelectPlayerMessage: "The opponent is choosing from which area to select a card.");

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedIntValue == 3)
                        {
                            break;
                        }

                        if (GManager.instance.userSelectionManager.SelectedIntValue == 1)
                        {
                            bool canNoSelect = noSelect() && validTrashCardCount > 0;
                            int maxCount = Math.Min(3 - digivolutionCards.Count, validPermanentCount);
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: ValidPermanentCombination,
                                canEndSelectCondition: CanEndSelectCondition,
                                maxCount: maxCount,
                                canNoSelect: canNoSelect, //if need to select cards and no valid trash cards
                                canEndNotMax: true,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select Digimon to place.", "The opponent is selecting Digimon to place.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            bool CanEndSelectCondition(List<Permanent> permanents)
                            {
                                List<CardSource> chosenCards = permanents.Map(perm => perm.TopCard);

                                return ValidCardCombination(chosenCards);
                            }

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                digivolutionCards.Add(permanent.TopCard);

                                yield return null;
                            }
                            validPermanentCount = CardEffectCommons.MatchConditionOwnersPermanentCount(card, CanSelectPermanentCondition);
                        }
                        else
                        {
                            bool canNoSelect = noSelect() && validPermanentCount > 0;
                            int maxCount = Math.Min(3 - digivolutionCards.Count, validTrashCardCount);
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: ValidCardSourceCombination,
                                canEndSelectCondition: ValidCardCombination,
                                canNoSelect: () => canNoSelect,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select cards to place in Digivolution cards.",
                                maxCount: maxCount,
                                canEndNotMax: true,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                digivolutionCards.Add(cardSource);

                                yield return null;
                            }
                            validTrashCardCount = card.Owner.TrashCards.Count(CanSelectCardCondition);
                        }
                    }

                    if (digivolutionCards.Count >= 1)
                    {
                        GManager.instance.GetComponent<SelectDigiXrosClass>().AddDigivolutionCardInfos(new AddDigivolutionCardsInfo(activateClass, digivolutionCards));

                        if (card.Owner.CanReduceCost(null, card))
                        {
                            ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                        }

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect($"Play Cost {-4 * digivolutionCards.Count}", CanUseCondition1, card);
                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                        card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            return true;
                        }

                        int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                        {
                            if (CardSourceCondition(cardSource)
                            && RootCondition(root)
                            && PermanentsCondition(targetPermanents))
                            {
                                Cost -= digivolutionCards.Count * 4;
                            }

                            return Cost;
                        }

                        bool PermanentsCondition(List<Permanent> targetPermanents)
                        {
                            return targetPermanents == null
                                    || targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0;
                        }

                        bool CardSourceCondition(CardSource cardSource)
                        {
                            return cardSource == card;
                        }

                        bool RootCondition(SelectCardEffect.Root root)
                        {
                            return true;
                        }

                        bool isUpDown()
                        {
                            return true;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));
                    }
                }
            }
            #endregion

            #region Reduce Play Cost - Not Shown
            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -4", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => true, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("Dark Masters");
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CanSelectCardCondition(permanent.TopCard);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    ICardEffect activateClass = card.EffectList(EffectTiming.BeforePayCost).Find(cardEffect => cardEffect.EffectName == "Placing 1 [Dark Masters] to get Play Cost -4");

                    return activateClass != null
                        && (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition)
                            || CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanentCondition));
                }

                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource)
                    && RootCondition(root)
                    && PermanentsCondition(targetPermanents))
                    {
                        List<CardSource> validCards = card.Owner.GetBattleAreaDigimons().Filter(CanSelectPermanentCondition).Map(permanent => permanent.TopCard);
                        validCards.AddRange(card.Owner.TrashCards.Filter(CanSelectCardCondition));
                        int targetCount = Math.Min(3, Combinations.GetUniqueNameCardCount(validCards));

                        Cost -= targetCount * 4;
                    }

                    return Cost;
                }

                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    return targetPermanents == null
                            || targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource != null
                        && cardSource == card;
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                bool isUpDown()
                {
                    return true;
                }
            }
            #endregion

            #region End of Your Turn
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 digimon from trash under this Digimon's digivolution cards and activate one of it's [On Play] effects", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsDigimonEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[End of Your Turn] [Once Per Turn] By placing 1 level 6 or lower card from your trash as this Digimon's bottom digivolution card, activate 1 [On Play] effect on that card as an effect. Then, trash the top 2 cards of your opponent's deck for each of this Digimon's level 6 digivolution cards.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.HasLevel
                        && cardSource.Level <= 6;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 card to place on bottom of digivolution cards.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage(
                        "Select 1 card to place on bottom of digivolution cards.",
                        "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    CardSource selectedCard = null;

                    if (selectedCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                            selectedCards,
                            activateClass));

                        selectedCard = selectedCards[0];
                    }

                    if (selectedCard != null)
                    {
                        Permanent thisPermanent = card.PermanentOfThisCard();

                        #region Add other card's effects to this card for "as effect of this card"
                        AddSkillClass addSkillClass = new AddSkillClass();
                        addSkillClass.SetUpICardEffect("Copy Digivolution Card Effects", _ => true, card);

                        ICardEffect GetAddSkillEffect(EffectTiming _timing) => addSkillClass;

                        card.Owner.UntilCalculateFixedCostEffect.Add(GetAddSkillEffect);

                        List<ICardEffect> candidateEffects = selectedCard.EffectList(EffectTiming.OnEnterFieldAnyone)
                            .Filter(cardEffect => cardEffect != null && cardEffect is ActivateICardEffect && !cardEffect.IsSecurityEffect && cardEffect.IsOnPlay);

                        List<ICardEffect> GetEffects(CardSource sourceCard, List<ICardEffect> getCardEffects, EffectTiming _timing)
                        {
                            getCardEffects ??= new List<ICardEffect>();

                            if (sourceCard == null || _timing is not EffectTiming.OnEnterFieldAnyone)
                                return getCardEffects;

                            foreach (ICardEffect cardEffect in candidateEffects)
                            {
                                if (cardEffect.IsInheritedEffect || cardEffect.IsLinkedEffect)
                                {
                                    continue;
                                }

                                cardEffect.SetOriginalEffectSourceCard(selectedCard);

                                getCardEffects.Add(cardEffect);
                            }

                            return getCardEffects;
                        }

                        addSkillClass.SetUpAddSkillClass(
                            cardSourceCondition: cardSource => cardSource == card,
                            getEffects: GetEffects,
                            limitTiming: EffectTiming.OnEnterFieldAnyone
                        );
                        #endregion

                        #region Activate one of the added effects
                        List<ICardEffect> activatableEffects = thisPermanent.EffectList(EffectTiming.OnEnterFieldAnyone)
                            .Filter(cardEffect => cardEffect != null && cardEffect.OriginalEffectSourceCard == selectedCard && cardEffect is ActivateICardEffect && !cardEffect.IsSecurityEffect && cardEffect.IsOnPlay);

                        if (activatableEffects.Count >= 1)
                        {
                            ICardEffect selectedEffect = null;

                            if (activatableEffects.Count == 1)
                            {
                                selectedEffect = activatableEffects[0];
                            }
                            else
                            {
                                List<SkillInfo> skillInfos = activatableEffects
                                    .Map(cardEffect => new SkillInfo(cardEffect, null, EffectTiming.None));

                                List<CardSource> cardSources = activatableEffects
                                    .Map(cardEffect => cardEffect.OriginalEffectSourceCard);

                                SelectCardEffect selectSourceCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectSourceCardEffect.SetUp(
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

                                selectSourceCardEffect.SetNotShowCard();
                                selectSourceCardEffect.SetUpSkillInfos(skillInfos);
                                selectSourceCardEffect.SetUpAfterSelectIndexCoroutine(AfterSelectIndexCoroutine);

                                yield return ContinuousController.instance.StartCoroutine(selectSourceCardEffect.Activate());

                                IEnumerator AfterSelectIndexCoroutine(List<int> selectedIndexes)
                                {
                                    if (selectedIndexes.Count == 1)
                                    {
                                        selectedEffect = candidateEffects[selectedIndexes[0]];
                                        yield return null;
                                    }
                                }
                            }

                            if (selectedEffect != null)
                            {
                                Hashtable effectHashtable = CardEffectCommons.OnPlayCheckHashtableOfCard(card);

                                if (selectedEffect.CanUse(effectHashtable))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(
                                        ((ActivateICardEffect)selectedEffect).Activate_Optional_Effect_Execute(effectHashtable));
                                }
                            }
                        }
                        #endregion

                        #region Remove effects
                        card.Owner.UntilCalculateFixedCostEffect.Remove(GetAddSkillEffect);
                        #endregion

                        int trashCount = 2 * card.PermanentOfThisCard().DigivolutionCards.Count(cardSource => cardSource.IsFaceUp && cardSource.IsLevel6);

                        yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(trashCount, card.Owner.Enemy, activateClass).AddTrashCardsFromLibraryTop());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
