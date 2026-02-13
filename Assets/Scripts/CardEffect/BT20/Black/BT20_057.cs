using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Gankoomon
namespace DCGO.CardEffects.BT20
{
    public class BT20_057 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Reduce Play Cost
            bool HasProperTraitInPlay(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                {
                    if (permanent.IsDigimon)
                    {
                        return permanent.TopCard.ContainsCardName("Huckmon") ||
                               permanent.TopCard.ContainsCardName("Jesmon") ||
                               permanent.TopCard.ContainsCardName("Sistermon");
                    }
                }

                return false;
            }

            #region Before Pay Cost - Condition Effect

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reduce the play cost by 4", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetHashString("PlayCost-4_BT20_057");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When this card would be played, if you have a Digimon with [Huckmon], [Jesmon], or [Sistermon] in its name, reduce the play cost by 4.";
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
                    return CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnHand(card))
                    {
                        return CardEffectCommons.HasMatchConditionPermanent(HasProperTraitInPlay);
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
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
                                    int targetCost = 0;

                                    if (CardEffectCommons.HasMatchConditionPermanent(HasProperTraitInPlay))
                                        targetCost += 4;

                                    Cost -= targetCost;
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

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Contains(card))
                    {
                        ICardEffect activateClass = card.EffectList(EffectTiming.BeforePayCost).Find(cardEffect => cardEffect.EffectName == "Reduce the play cost by 4");

                        if (activateClass != null)
                        {
                            return CardEffectCommons.HasMatchConditionPermanent(HasProperTraitInPlay);
                        }
                    }

                    return false;
                }

                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource))
                    {
                        if (RootCondition(root))
                        {
                            if (PermanentsCondition(targetPermanents))
                            {
                                int targetCount = 0;

                                if (CardEffectCommons.HasMatchConditionPermanent(HasProperTraitInPlay))
                                    targetCount += 4;

                                Cost -= targetCount;
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
                    if (cardSource != null)
                    {
                        if (cardSource == card)
                        {
                            return true;
                        }
                    }

                    return false;
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

            #endregion

            #region Reboot/Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));

                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into level 6", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] 1 of your Digimon may digivolve into a level 6 or lower Digimon card with [Huckmon] in its name or the [Royal Knight] trait in the hand or trash without paying the cost.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        foreach (CardSource cardSource in card.Owner.HandCards)
                        {
                            if (CanSelectCardCondition(cardSource))
                            {
                                if (cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, true, activateClass))
                                {
                                    return true;
                                }
                            }
                        }

                        foreach (CardSource cardSource in card.Owner.TrashCards)
                        {
                            if (CanSelectCardCondition(cardSource))
                            {
                                if (cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, true, activateClass))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           cardSource.HasLevel && cardSource.Level <= 6 &&
                           (cardSource.ContainsCardName("Huckmon") || cardSource.HasRoyalKnightTraits);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to digivolve.", "The opponent is selecting 1 Digimon to digivolve.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;
                    }

                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    if (canSelectHand || canSelectTrash)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                            {
                                new(message: "Digivolve", value: true, spriteIndex: 0),
                                new(message: "Do not digivolve", value: false, spriteIndex: 1),
                            };

                        string selectPlayerMessage = "Digivolve?";
                        string notSelectPlayerMessage = "The opponent is choosing whether to digivolve.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements,
                            selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                            notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                            .WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedBoolValue)
                        {
                            if (canSelectHand && canSelectTrash)
                            {
                                selectionElements = new List<SelectionElement<bool>>()
                                    {
                                        new(message: "From hand", value: true, spriteIndex: 0),
                                        new(message: "From trash", value: false, spriteIndex: 1),
                                    };

                                selectPlayerMessage = "From which area do you play a card?";
                                notSelectPlayerMessage = "The opponent is choosing from which area to play a card.";

                                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements,
                                    selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                                    notSelectPlayerMessage: notSelectPlayerMessage);
                            }
                            else
                            {
                                GManager.instance.userSelectionManager.SetBool(canSelectHand);
                            }

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                                .WaitForEndSelect());

                            yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                    targetPermanent: selectedPermanent,
                                    cardCondition: CanSelectCardCondition,
                                    payCost: false,
                                    reduceCostTuple: null,
                                    fixedCostTuple: null,
                                    ignoreDigivolutionRequirementFixedCost: -1,
                                    isHand: GManager.instance.userSelectionManager.SelectedBoolValue,
                                    activateClass: activateClass,
                                    successProcess: null));
                        }
                    }
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into level 6", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] 1 of your Digimon may digivolve into a level 6 or lower Digimon card with [Huckmon] in its name or the [Royal Knight] trait in the hand or trash without paying the cost.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        foreach (CardSource cardSource in card.Owner.HandCards)
                        {
                            if (CanSelectCardCondition(cardSource))
                            {
                                if (cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, true, activateClass))
                                {
                                    return true;
                                }
                            }
                        }

                        foreach (CardSource cardSource in card.Owner.TrashCards)
                        {
                            if (CanSelectCardCondition(cardSource))
                            {
                                if (cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, true, activateClass))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           cardSource.HasLevel && cardSource.Level <= 6 &&
                           (cardSource.ContainsCardName("Huckmon") || cardSource.HasRoyalKnightTraits);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to digivolve.", "The opponent is selecting 1 Digimon to digivolve.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;
                    }

                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    if (canSelectHand || canSelectTrash)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                            {
                                new(message: "Digivolve", value: true, spriteIndex: 0),
                                new(message: "Do not digivolve", value: false, spriteIndex: 1),
                            };

                        string selectPlayerMessage = "Digivolve?";
                        string notSelectPlayerMessage = "The opponent is choosing whether to digivolve.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements,
                            selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                            notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                            .WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedBoolValue)
                        {
                            if (canSelectHand && canSelectTrash)
                            {
                                selectionElements = new List<SelectionElement<bool>>()
                                    {
                                        new(message: "From hand", value: true, spriteIndex: 0),
                                        new(message: "From trash", value: false, spriteIndex: 1),
                                    };

                                selectPlayerMessage = "From which area do you play a card?";
                                notSelectPlayerMessage = "The opponent is choosing from which area to play a card.";

                                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements,
                                    selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                                    notSelectPlayerMessage: notSelectPlayerMessage);
                            }
                            else
                            {
                                GManager.instance.userSelectionManager.SetBool(canSelectHand);
                            }

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                                .WaitForEndSelect());

                            yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                    targetPermanent: selectedPermanent,
                                    cardCondition: CanSelectCardCondition,
                                    payCost: false,
                                    reduceCostTuple: null,
                                    fixedCostTuple: null,
                                    ignoreDigivolutionRequirementFixedCost: -1,
                                    isHand: GManager.instance.userSelectionManager.SelectedBoolValue,
                                    activateClass: activateClass,
                                    successProcess: null));
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
