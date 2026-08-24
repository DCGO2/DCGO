using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public partial class CardEffectCommons
{
    #region Play By effect
    public static IEnumerator PlayByEffect(Func<CardSource, bool> canTargetCondition,
        SelectCardEffect.Root root,
        ICardEffect cardEffect,
        bool payCost,
        Func<List<CardSource>, CardSource, bool> canTargetCondition_ByPreSelecetedList = null,
        Func<List<CardSource>, bool> canEndSelectCondition = null,
        Func<List<CardSource>, IEnumerator> afterSelectCardCoroutine = null,
        int maxCount = 1,
        bool canNoSelect = true,
        bool canEndNotMax = false,
        (int reduceCost, Func<CardSource, bool> reduceCostCardCondition)? reduceCostTuple = null,
        (int fixedCost, Func<CardSource, bool> fixedCostCardCondition)? fixedCostTuple = null,
        Permanent targetPermanent = null)
    {
        if (cardEffect == null) yield break;

        Player owner = cardEffect.EffectSourceCard.Owner;

        List<CardSource> selectedCards = new List<CardSource>();

        IEnumerator SelectCardCoroutine(CardSource card)
        {
            selectedCards.Add(card);
            yield return null;
        }
        switch (root)
        {
            case SelectCardEffect.Root.Hand:
            {
                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: owner,
                    canTargetCondition: canTargetCondition,
                    canTargetCondition_ByPreSelecetedList: canTargetCondition_ByPreSelecetedList,
                    canEndSelectCondition: canEndSelectCondition,
                    maxCount: maxCount,
                    canNoSelect: canNoSelect,
                    canEndNotMax: canEndNotMax,
                    isShowOpponent: true,
                    selectCardCoroutine: SelectCardCoroutine,
                    afterSelectCardCoroutine: afterSelectCardCoroutine,
                    mode: SelectHandEffect.Mode.Custom,
                    cardEffect: cardEffect);

                selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                break;
            }

            case SelectCardEffect.Root.DigivolutionCards:
            {
                if (targetPermanent == null) yield break;

                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                selectCardEffect.SetUp(
                    canTargetCondition: canTargetCondition,
                    canTargetCondition_ByPreSelecetedList: canTargetCondition_ByPreSelecetedList,
                    canEndSelectCondition: canEndSelectCondition,
                    canNoSelect: () => canNoSelect,
                    selectCardCoroutine: SelectCardCoroutine,
                    afterSelectCardCoroutine: afterSelectCardCoroutine,
                    message: "Select 1 card to play.",
                    maxCount: maxCount,
                    canEndNotMax: canEndNotMax,
                    isShowOpponent: true,
                    mode: SelectCardEffect.Mode.Custom,
                    root: root,
                    customRootCardList: targetPermanent.DigivolutionCards,
                    canLookReverseCard: true,
                    selectPlayer: owner,
                    cardEffect: cardEffect);

                selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");

                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                break;
            }

            case SelectCardEffect.Root.LinkedCards:
            {
                if (targetPermanent == null) yield break;
                
                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                selectCardEffect.SetUp(
                    canTargetCondition: canTargetCondition,
                    canTargetCondition_ByPreSelecetedList: canTargetCondition_ByPreSelecetedList,
                    canEndSelectCondition: canEndSelectCondition,
                    canNoSelect: () => canNoSelect,
                    selectCardCoroutine: SelectCardCoroutine,
                    afterSelectCardCoroutine: afterSelectCardCoroutine,
                    message: "Select 1 card to play.",
                    maxCount: maxCount,
                    canEndNotMax: canEndNotMax,
                    isShowOpponent: true,
                    mode: SelectCardEffect.Mode.Custom,
                    root: root,
                    customRootCardList: targetPermanent.LinkedCards,
                    canLookReverseCard: true,
                    selectPlayer: owner,
                    cardEffect: cardEffect);

                selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");

                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                break;
            }

            default:
            {
                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                selectCardEffect.SetUp(
                    canTargetCondition: canTargetCondition,
                    canTargetCondition_ByPreSelecetedList: canTargetCondition_ByPreSelecetedList,
                    canEndSelectCondition: canEndSelectCondition,
                    canNoSelect: () => canNoSelect,
                    selectCardCoroutine: SelectCardCoroutine,
                    afterSelectCardCoroutine: afterSelectCardCoroutine,
                    message: "Select 1 card to play.",
                    maxCount: maxCount,
                    canEndNotMax: canEndNotMax,
                    isShowOpponent: true,
                    mode: SelectCardEffect.Mode.Custom,
                    root: root,
                    customRootCardList: null,
                    canLookReverseCard: true,
                    selectPlayer: owner,
                    cardEffect: cardEffect);

                selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");

                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                break;
            }
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

        bool SharedCardCondition(CardSource cardSource) => selectedCards.Contains(cardSource);
        bool RootCondition(SelectCardEffect.Root root) => true;
        bool CanUseCondition(Hashtable hashtable) => true;

        #region reduce cost

        Func<EffectTiming, ICardEffect> getChangeCostEffect = null;

        if (reduceCostTuple != null)
        {
            bool CardCondition(CardSource cardSource)
            {
                return SharedCardCondition(cardSource)
                    && (reduceCostTuple.Value.reduceCostCardCondition == null || reduceCostTuple.Value.reduceCostCardCondition(cardSource));
            }

            int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
            {
                if (PermanentsCondition(targetPermanents))
                {
                    Cost -= reduceCostTuple.Value.reduceCost;
                }

                return Cost;
            }

            bool isUpDown() => true;

            ChangeCostClass changeCostClass = new ChangeCostClass();
            changeCostClass.SetUpICardEffect($"Play Cost -{reduceCostTuple.Value.reduceCost}", CanUseCondition, cardEffect.EffectSourceCard);
            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
            getChangeCostEffect = GetCardEffect;

            ICardEffect GetCardEffect(EffectTiming _timing)
            {
                if (_timing == EffectTiming.None)
                {
                    return changeCostClass;
                }

                return null;
            }

            if (getChangeCostEffect != null)
            {
                owner.UntilCalculateFixedCostEffect.Add(getChangeCostEffect);
            }
        }

        #endregion

        #region set fixed cost

        Func<EffectTiming, ICardEffect> getFixedCostEffect = null;

        if (fixedCostTuple != null)
        {
            bool CardCondition(CardSource cardSource)
            {
                return SharedCardCondition(cardSource)
                    && (fixedCostTuple.Value.fixedCostCardCondition == null || fixedCostTuple.Value.fixedCostCardCondition(cardSource));
            }

            int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
            {
                if (PermanentsCondition(targetPermanents))
                {
                    Cost = fixedCostTuple.Value.fixedCost;
                }

                return Cost;
            }

            bool isUpDown() => false;

            ChangeCostClass changeCostClass = new ChangeCostClass();
            changeCostClass.SetUpICardEffect($"Play Cost {fixedCostTuple.Value.fixedCost}", CanUseCondition, cardEffect.EffectSourceCard);
            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
            getFixedCostEffect = GetCardEffect;

            ICardEffect GetCardEffect(EffectTiming _timing)
            {
                if (_timing == EffectTiming.None)
                {
                    return changeCostClass;
                }

                return null;
            }

            if (getFixedCostEffect != null)
            {
                owner.UntilCalculateFixedCostEffect.Add(getFixedCostEffect);
            }
        }

        #endregion

        yield return ContinuousController.instance.StartCoroutine(
            CardEffectCommons.PlayPermanentCards(
                cardSources: selectedCards, 
                activateClass: cardEffect, 
                payCost: payCost, 
                isTapped: false, 
                root: root, 
                activateETB: true));
        
        #region release effect
        if (getChangeCostEffect != null) owner.UntilCalculateFixedCostEffect.Remove(getChangeCostEffect);
        #endregion

        #region release effect
        if (getFixedCostEffect != null) owner.UntilCalculateFixedCostEffect.Remove(getFixedCostEffect);
        #endregion
    }
    #endregion

    #region CanPlayOrUse
    public static bool CanPlayOrUse(CardSource cardSource, ICardEffect activateClass, SelectCardEffect.Root root = SelectCardEffect.Root.Hand, int fixedCost = -1)
    {
        return cardSource != null
            && (cardSource.IsOption
                && !cardSource.CanNotPlayThisOption
                && cardSource.Owner.MaxMemoryCost >= fixedCost)
            || (cardSource.HasPlayCost
                && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: fixedCost > 0, cardEffect: activateClass, root: root, fixedCost: fixedCost));
    }
    #endregion

    //TODO: UseByEffect

    //TODO: PlayOrUseByEffect
}