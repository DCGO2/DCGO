using DCGO.CardEntities;
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

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("Dark Masters");
                }

                bool CanSelectCardConditionPermenant(Permanent permanent)
                {
                    return permanent.TopCard.EqualsTraits("Dark Masters");
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool CanNoSelect(CardSource cardSource)
                {
                    if (cardSource != null
                    && cardSource.PayingCost(SelectCardEffect.Root.Hand, null, checkAvailability: false) > cardSource.Owner.MaxMemoryCost)
                    {
                        return false;
                    }

                    return true;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition)
                            || CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectCardConditionPermenant);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<CardSource> digivolutionCards = new List<CardSource>();

                    bool CanSelectTrashCardCondition(CardSource cardSource)
                    {
                        return CanSelectCardCondition(cardSource)
                            && digivolutionCards.Count((filteredCard) => filteredCard.CardNames.Contains(cardSource.CardNames[0])) == 0;
                    }

                    bool CanSelectPermanentCondition(Permanent permanent)
                    {
                        return CanSelectCardConditionPermenant(permanent)
                            && digivolutionCards.Count((filteredCard) => filteredCard.CardNames.Contains(permanent.TopCard.CardNames[0])) == 0;
                    }

                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectTrashCardCondition(cardSource)))
                    {
                        bool noSelect = CanNoSelect(CardEffectCommons.GetCardFromHashtable(_hashtable));
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = Math.Min(3 - digivolutionCards.Count, card.Owner.TrashCards.Count((cardSource) => CanSelectTrashCardCondition(cardSource)));

                        if (maxCount >= 1)
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectTrashCardCondition,
                                canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                canEndSelectCondition: CanEndSelectCondition,
                                canNoSelect: () => noSelect,
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

                            bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                            {
                                List<string> cardNames = new List<string>();

                                foreach (CardSource cardSource1 in cardSources)
                                {
                                    foreach (string cardName in cardSource1.CardNames)
                                    {
                                        if (!cardNames.Contains(cardName))
                                        {
                                            cardNames.Add(cardName);
                                        }
                                    }
                                }

                                foreach (CardSource cardSource1 in digivolutionCards)
                                {
                                    foreach (string cardName in cardSource1.CardNames)
                                    {
                                        if (!cardNames.Contains(cardName))
                                        {
                                            cardNames.Add(cardName);
                                        }
                                    }
                                }

                                if (cardNames.Contains(cardSource.CardNames[0]))
                                {
                                    return false;
                                }

                                return true;
                            }

                            bool CanEndSelectCondition(List<CardSource> cardSources)
                            {
                                List<string> cardIDs = new List<string>();

                                foreach (CardSource cardSource1 in cardSources)
                                {
                                    if (!cardIDs.Contains(cardSource1.CardID))
                                    {
                                        cardIDs.Add(cardSource1.CardID);
                                    }
                                }

                                foreach (CardSource cardSource1 in digivolutionCards)
                                {
                                    if (!cardIDs.Contains(cardSource1.CardID))
                                    {
                                        cardIDs.Add(cardSource1.CardID);
                                    }
                                }

                                if (cardIDs.Count != cardSources.Count + digivolutionCards.Count)
                                {
                                    return false;
                                }

                                return true;
                            }

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);
                                digivolutionCards.Add(cardSource);
                                yield return null;
                            }

                            if (selectedCards.Count >= 1)
                            {
                                GManager.instance.GetComponent<SelectDigiXrosClass>().AddDigivolutionCardInfos(new AddDigivolutionCardsInfo(activateClass, selectedCards));

                                yield return StartCoroutine(AfterSelectCardCoroutine(selectedCards));
                            }
                        }
                    }

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (card.Owner.CanReduceCost(null, card))
                        {
                            ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                        }

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect("Play Cost -4", CanUseCondition1, card);
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
                                Cost -= cardSources.Count * 4;
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

                        yield return null;
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

                bool CanUseCondition(Hashtable hashtable)
                {
                    ICardEffect activateClass = card.EffectList(EffectTiming.BeforePayCost).Find(cardEffect => cardEffect.EffectName == "Placing 1 [Dark Masters] to get Play Cost -4");

                    return activateClass != null
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);
                }

                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource)
                    && RootCondition(root)
                    && PermanentsCondition(targetPermanents))
                    {
                        List<CardSource> trashSources = card.Owner.TrashCards.Filter(CanSelectCardCondition);
                        int targetCount = (from trashCard in trashSources select trashCard.CardID).Distinct().Count();

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
                activateClass.SetUpICardEffect("Place 1 digimon from trash under this Digimon's digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
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
                        List<ICardEffect> candidateEffects = selectedCard.EffectList_ForCard(EffectTiming.OnEnterFieldAnyone, card)
                            .Clone()
                            .Filter(cardEffect => cardEffect != null && cardEffect is ActivateICardEffect && !cardEffect.IsSecurityEffect && cardEffect.IsOnPlay);

                        if (candidateEffects.Count >= 1)
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
                    }

                    int trashCount = 2 * card.PermanentOfThisCard().cardSources.Filter(cardSource => cardSource != card && cardSource.HasLevel && cardSource.Level == 6).Count;

                    yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(trashCount, card.Owner.Enemy, activateClass).AddTrashCardsFromLibraryTop());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
