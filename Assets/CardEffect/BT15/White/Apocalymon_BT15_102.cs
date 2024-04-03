using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class Apocalymon_BT15_102 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        #region Before Pay Cost
        if (timing == EffectTiming.BeforePayCost)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Placing 1 [Dark Masters] to get Play Cost -4", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetHashString("PlayCost-12_BT15_102");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "When this card would be played, by placing up to 3 [Dark Masters] trait cards with different names from your battle area or trash under it, reduce the play cost by 4 for each one.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.IsDigimon)
                {
                    if (cardSource.CardTraits.Contains("Dark Masters") || cardSource.CardTraits.Contains("DarkMasters"))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CardCondition(CardSource cardSource)
            {
                if (cardSource == card)
                {
                    if (CardEffectCommons.IsExistOnHand(cardSource))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition))
                {
                    return true;
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                    int digivolutionCardsCount = 0;

                    List<CardSource> digivolutionCards = new List<CardSource>();

                    bool CanSelectCardCondition1(CardSource cardSource)
                    {
                        if (CanSelectCardCondition(cardSource))
                        {
                            if (digivolutionCards.Count((cardSource1) => cardSource1.CardID == cardSource.CardID) == 0)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition1(cardSource)))
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    int maxCount = Math.Min(3 - digivolutionCards.Count, card.Owner.TrashCards.Count((cardSource) => CanSelectCardCondition1(cardSource)));

                    if (maxCount >= 1)
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition1,
                            canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                            canEndSelectCondition: CanEndSelectCondition,
                            canNoSelect: () => true,
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

                            if (cardIDs.Contains(cardSource.CardID))
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
                            foreach (CardSource selectedCard in selectedCards)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(selectedCard));
                            }

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
                        if (CardSourceCondition(cardSource))
                        {
                            if (RootCondition(root))
                            {
                                if (PermanentsCondition(targetPermanents))
                                {
                                    Cost -= cardSources.Count * 4;
                                }
                            }
                        }

                        return Cost;
                    }

                    bool PermanentsCondition(List<Permanent> targetPermanents)
                    {
                        if (targetPermanents == null)
                        {
                            return true;
                        }

                        else
                        {
                            if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                            {
                                return true;
                            }
                        }

                        return false;
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

        #region End of Your Turn
        if (timing == EffectTiming.OnEndTurn)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Place 1 digimon from trash under this Digimon's digivolution cards", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[End of Your Turn] [Once Per Turn] By placing 1 level 6 or lower card from your trash as this Digimon's bottom digivolution card, activate 1 [On Play] effect on that card as an effect. Then, trash the top 2 cards of your opponent's deck for each of this Digimon's level 6 digivolution cards.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.IsDigimon)
                {
                    if (cardSource.HasLevel)
                    {
                        if(cardSource.Level == 6)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    if (canSelectTrash)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "From which area do you select a card?";
                        string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                    }

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                    List<CardSource> selectedCards = new List<CardSource>();

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    if (fromHand)
                    {
                        int maxCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage(
                            "Select 1 card to place on bottom of digivolution cards.",
                            "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                        yield return StartCoroutine(selectHandEffect.Activate());
                    }

                    else
                    {
                        int maxCount = 1;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to place on bottom of digivolution cards.",
                            maxCount: maxCount,
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
                    }

                    CardSource selectedCard = null;

                    if (selectedCards.Count >= 1)
                    {
                        if (CardEffectCommons.IsExistOnBattleArea(card))
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                                selectedCards,
                                activateClass));

                            selectedCard = selectedCards[0];
                        }
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
                }

                int trashCount = 2 * card.PermanentOfThisCard().cardSources.Count;

                yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(trashCount, card.Owner.Enemy, activateClass).AddTrashCardsFromLibraryTop());
            }
        }
        #endregion

        return cardEffects;
    }
}