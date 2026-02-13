using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DCGO.CardEffects.BT17
{
    public class BT17_027 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel5 && targetPermanent.TopCard.ContainsCardName("Garurumon");
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
                    return permanent.IsTamer && (permanent.TopCard.ContainsCardName("Matt Ishida") ||
                                                 permanent.TopCard.ContainsCardName("MattIshida"));
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
                    if (permanent.TopCard.EqualsCardName("Agumon"))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanSelectHandCardSharedCondition(CardSource cardSource)
            {
                return cardSource.IsDigimon && cardSource.EqualsCardName("WarGreymon");
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
                        "[On Play] Activate 1 of the effects below: ・1 of your opponent's Digimon or Tamers can't suspend until the end of their turn. ・1 of your [Agumon] may digivolve into [WarGreymon] in your hand, ignoring its digivolution requirements and without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanSelectOpponentPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        return permanent.IsDigimon || permanent.IsTamer;
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
                        yield return GManager.instance.photonWaitController.StartWait("MetalGarurumon_BT17");

                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new(message: "1 of your opponent's Digimon or Tamers can't suspend until the end of their turn. ",
                                value: 0, spriteIndex: 0),
                            new(
                                message: "1 of your [Agumon] may digivolve into [WarGreymon] in your hand",
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
                                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectOpponentPermanentCondition))
                                {
                                    int maxCount = Math.Min(1,
                                        CardEffectCommons.MatchConditionPermanentCount(CanSelectOpponentPermanentCondition));

                                    SelectPermanentEffect selectPermanentEffect =
                                        GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canTargetCondition: CanSelectOpponentPermanentCondition,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: false,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: SelectPermanentCoroutine,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon/Tamer that will be unable to suspend.",
                                        "The opponent is selecting 1 Digimon/Tamer that will be unable to suspend.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        Permanent selectedPermanent = permanent;
                                        CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                                        canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCanNotSuspendCondition, card);
                                        canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCanNotSuspendCondition);
                                        selectedPermanent.UntilOwnerTurnEndEffects.Add(_ => canNotSuspendClass);

                                        if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance
                                                .GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                                        }

                                        bool CanUseCanNotSuspendCondition(Hashtable hashtableCanNotSuspend)
                                        {
                                            if (selectedPermanent.TopCard != null)
                                            {
                                                if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                                {
                                                    return true;
                                                }
                                            }

                                            return false;
                                        }

                                        bool PermanentCanNotSuspendCondition(Permanent permanentCanNotSuspend)
                                        {
                                            if (permanentCanNotSuspend == selectedPermanent)
                                            {
                                                return true;
                                            }

                                            return false;
                                        }
                                    }
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
                        "[When Digivolving] Activate 1 of the effects below: ・1 of your opponent's Digimon or Tamers can't suspend until the end of their turn. ・1 of your [Agumon] may digivolve into [WarGreymon] in your hand, ignoring its digivolution requirements and without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanSelectOpponentPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        return permanent.IsDigimon || permanent.IsTamer;
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
                        yield return GManager.instance.photonWaitController.StartWait("MetalGarurumon_BT17");

                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new(message: "1 of your opponent's Digimon or Tamers can't suspend until the end of their turn. ",
                                value: 0, spriteIndex: 0),
                            new(
                                message: "1 of your [Agumon] may digivolve into [WarGreymon] in your hand",
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
                                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectOpponentPermanentCondition))
                                {
                                    int maxCount = Math.Min(1,
                                        CardEffectCommons.MatchConditionPermanentCount(CanSelectOpponentPermanentCondition));

                                    SelectPermanentEffect selectPermanentEffect =
                                        GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canTargetCondition: CanSelectOpponentPermanentCondition,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: false,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: SelectPermanentCoroutine,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon/Tamer that will be unable to suspend.",
                                        "The opponent is selecting 1 Digimon/Tamer that will be unable to suspend.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        Permanent selectedPermanent = permanent;
                                        CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                                        canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCanNotSuspendCondition, card);
                                        canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCanNotSuspendCondition);
                                        selectedPermanent.UntilOwnerTurnEndEffects.Add(_ => canNotSuspendClass);

                                        if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance
                                                .GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                                        }

                                        bool CanUseCanNotSuspendCondition(Hashtable hashtableCanNotSuspend)
                                        {
                                            if (selectedPermanent.TopCard != null)
                                            {
                                                if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                                {
                                                    return true;
                                                }
                                            }

                                            return false;
                                        }

                                        bool PermanentCanNotSuspendCondition(Permanent permanentCanNotSuspend)
                                        {
                                            if (permanentCanNotSuspend == selectedPermanent)
                                            {
                                                return true;
                                            }

                                            return false;
                                        }
                                    }
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
                activateClass.SetUpICardEffect("Unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Unsuspend_BT17-027");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Attacking][Once Per Turn] If this Digimon has [Omnimon] in its name, unsuspend this Digimon.";
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
                    yield return ContinuousController.instance.StartCoroutine(
                        new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() },
                            activateClass).Unsuspend());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}