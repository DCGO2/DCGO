using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DCGO.CardEffects.BT17
{
    public class BT17_015 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel5 && targetPermanent.TopCard.ContainsCardName("Greymon");
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

            #region Play Cost Reduction

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect($"Play Cost -3", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost,
                    cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: IsUpDown,
                    isCheckAvailability: () => false, isChangePayingCost: () => true);

                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Contains(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsCorrectTamer))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool IsCorrectTamer(Permanent permanent)
                {
                    return permanent.IsTamer && (permanent.TopCard.ContainsCardName("Tai Kamiya") ||
                                                 permanent.TopCard.ContainsCardName("TaiKamiya"));
                }

                int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root,
                    List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource))
                    {
                        if (RootCondition(root))
                        {
                            if (PermanentsCondition(targetPermanents))
                            {
                                cost -= 3;
                            }
                        }
                    }

                    return cost;
                }

                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    if (targetPermanents == null)
                    {
                        return true;
                    }

                    if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                    {
                        return true;
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

                bool IsUpDown()
                {
                    return true;
                }
            }

            #endregion

            #region On Play/ When Digivolving Shared

            bool CanSelectOwnPermanentSharedCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                {
                    if (permanent.TopCard.EqualsCardName("Gabumon"))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanSelectHandCardSharedCondition(CardSource cardSource)
            {
                return cardSource.IsDigimon && cardSource.EqualsCardName("MetalGarurumon");
            }

            #endregion
            
            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Choose 1 effect", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Play] Activate 1 of the effects below: ・Delete 1 of your opponent's Digimon with 8000 DP or less. ・1 of your [Gabumon] may digivolve into [MetalGarurumon] in your hand, ignoring its digivolution requirements and without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanSelectOpponentPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(8000, activateClass))
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
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentCondition))
                        {
                            return true;
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnPermanentSharedCondition) &&
                            card.Owner.HandCards.Count(CanSelectHandCardSharedCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        yield return GManager.instance.photonWaitController.StartWait("WarGreymon_BT17");

                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new(message: "Delete 1 of your opponent's Digimon with 8000 DP or less",
                                value: 0, spriteIndex: 0),
                            new(
                                message: "1 of your [Gabumon] may digivolve into [MetalGarurumon] in your hand",
                                value: 1, spriteIndex: 0),
                        };

                        string selectPlayerMessage = "Which effect will you activate?";
                        string notSelectPlayerMessage = "The opponent is choosing which effect to activate.";

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements,
                            selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                            notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                            .WaitForEndSelect());

                        int actionID = GManager.instance.userSelectionManager.SelectedIntValue;

                        switch (actionID)
                        {
                            case 0:
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentCondition))
                                {
                                    int maxCount = Math.Min(1,
                                        CardEffectCommons.MatchConditionPermanentCount(CanSelectOpponentPermanentCondition));

                                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectOpponentPermanentCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: false,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: null,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Destroy,
                                        cardEffect: activateClass);

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                                }

                                break;
                            }

                            case 1:
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnPermanentSharedCondition) &&
                                    card.Owner.HandCards.Count(CanSelectHandCardSharedCondition) >= 1)
                                {
                                    Permanent selectedPermanent = null;

                                    int maxCount = Math.Min(1,
                                        CardEffectCommons.MatchConditionPermanentCount(CanSelectOwnPermanentSharedCondition));

                                    SelectPermanentEffect selectPermanentEffect =
                                        GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectOwnPermanentSharedCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: true,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: SelectPermanentCoroutine,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will digivolve.",
                                        "The opponent is selecting 1 Digimon that will digivolve.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        selectedPermanent = permanent;
                                        yield return null;
                                    }

                                    if (selectedPermanent != null)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                            targetPermanent: selectedPermanent,
                                            cardCondition: CanSelectHandCardSharedCondition,
                                            payCost: false,
                                            reduceCostTuple: null,
                                            fixedCostTuple: null,
                                            ignoreDigivolutionRequirementFixedCost: 0,
                                            isHand: true,
                                            activateClass: activateClass,
                                            successProcess: null));
                                    }
                                }

                                break;
                            }
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Choose 1 effect", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] Activate 1 of the effects below: ・Delete 1 of your opponent's Digimon with 8000 DP or less. ・1 of your [Gabumon] may digivolve into [MetalGarurumon] in your hand, ignoring its digivolution requirements and without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanSelectOpponentPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(8000, activateClass))
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
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentCondition))
                        {
                            return true;
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnPermanentSharedCondition) &&
                            card.Owner.HandCards.Count(CanSelectHandCardSharedCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        yield return GManager.instance.photonWaitController.StartWait("WarGreymon_BT17");

                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new(message: "Delete 1 of your opponent's Digimon with 8000 DP or less",
                                value: 0, spriteIndex: 0),
                            new(
                                message: "1 of your [Gabumon] may digivolve into [MetalGarurumon] in your hand",
                                value: 1, spriteIndex: 0),
                        };

                        string selectPlayerMessage = "Which effect will you activate?";
                        string notSelectPlayerMessage = "The opponent is choosing which effect to activate.";

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements,
                            selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                            notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                            .WaitForEndSelect());

                        int actionID = GManager.instance.userSelectionManager.SelectedIntValue;

                        switch (actionID)
                        {
                            case 0:
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentCondition))
                                {
                                    int maxCount = Math.Min(1,
                                        CardEffectCommons.MatchConditionPermanentCount(CanSelectOpponentPermanentCondition));

                                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectOpponentPermanentCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: false,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: null,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Destroy,
                                        cardEffect: activateClass);

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                                }

                                break;
                            }

                            case 1:
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnPermanentSharedCondition) &&
                                    card.Owner.HandCards.Count(CanSelectHandCardSharedCondition) >= 1)
                                {
                                    Permanent selectedPermanent = null;

                                    int maxCount = Math.Min(1,
                                        CardEffectCommons.MatchConditionPermanentCount(CanSelectOwnPermanentSharedCondition));

                                    SelectPermanentEffect selectPermanentEffect =
                                        GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectOwnPermanentSharedCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: true,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: SelectPermanentCoroutine,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will digivolve.",
                                        "The opponent is selecting 1 Digimon that will digivolve.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        selectedPermanent = permanent;
                                        yield return null;
                                    }

                                    if (selectedPermanent != null)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                            targetPermanent: selectedPermanent,
                                            cardCondition: CanSelectHandCardSharedCondition,
                                            payCost: false,
                                            reduceCostTuple: null,
                                            fixedCostTuple: null,
                                            ignoreDigivolutionRequirementFixedCost: 0,
                                            isHand: true,
                                            activateClass: activateClass,
                                            successProcess: null));
                                    }
                                }

                                break;
                            }
                        }
                    }
                }
            }

            #endregion

            #region When Attacking - ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash the top card of opponent's security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("ESS_BT17_015");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Attacking][Once Per Turn] If this Digimon has [Omnimon] in its name, trash the top card of your opponent's security stack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().TopCard.ContainsCardName("Omnimon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}