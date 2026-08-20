using System;
using System.Collections;
using System.Collections.Generic;

// Plutomon
namespace DCGO.CardEffects.BT26
{
    public class BT26_059 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Reduce Play Cost
            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reduce play cost by 6", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetHashString("BT26_059_ReducePlayCost");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "When this card would be played, if your hand has fewer cards than your opponent's, reduce the cost by 6.";

                bool CardCondition(CardSource cardSource) => cardSource == card;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => card.Owner.HandCards.Count < card.Owner.Enemy.HandCards.Count
                        && card.Owner.CanReduceCost(null, card);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect("Play Cost -6", CanUseCondition1, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardCondition, rootCondition: RootCondition, isUpDown: IsUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);

                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                    card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                    bool CanUseCondition1(Hashtable hashtable) => true;

                    bool RootCondition(SelectCardEffect.Root root) => true;

                    bool IsUpDown() => true;

                    int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                    {
                        if (CardCondition(cardSource) && card.Owner.HandCards.Count < card.Owner.Enemy.HandCards.Count)
                        {
                            cost -= 6;
                        }

                        return cost;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));
                }
            }
            #endregion

            #region Shared On Play / When Digivolving / When Attacking

            string SharedEffectName()
                => "By trashing 1 hand card, if your turn, may play 1 [Titan] Digimon from trash for 7 less (not [Plutomon])";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] By trashing 1 card in your hand, if it's your turn, you may play 1 Digimon card with the [Titan] trait from your trash with the cost reduced by 7. This effect can't play [Plutomon].";

            bool CanSelectTrashCardCondition(CardSource cardSource, ICardEffect activateClass)
                => cardSource.IsDigimon
                    && cardSource.ContainsTraits("Titan")
                    && !cardSource.EqualsCardName("Plutomon")
                    && CardEffectCommons.CanPlayAsNewPermanent(cardSource, true, activateClass, fixedCost: cardSource.GetCostItself - 7);

            bool SharedAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => card.Owner.HandCards.Count >= 1;

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (card.Owner.HandCards.Count >= 1)
                {
                    CardSource selectedCardToTrash = null;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: _ => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Custom,
                        cardEffect: activateClass);

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCardToTrash = cardSource;
                        yield return null;
                    }

                    selectHandEffect.SetUpCustomMessage("Select 1 card to trash.", "The opponent is selecting 1 card to trash.");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    if (selectedCardToTrash != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashHandAndProcessAccordingToResult(
                            player: card.Owner,
                            hashtable: hashtable,
                            cardToTrash: selectedCardToTrash,
                            activateClass: activateClass,
                            successProcess: SuccessProcess,
                            failureProcess: null));

                        IEnumerator SuccessProcess(CardSource cardSource)
                        {
                            if (CardEffectCommons.IsOwnerTurn(card))
                            {
                                bool CanSelectTrashCardConditionBound(CardSource cs) => CanSelectTrashCardCondition(cs, activateClass);

                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectTrashCardConditionBound))
                                {
                                    CardSource selectedCard = null;

                                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                    selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectTrashCardConditionBound,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => true,
                                        selectCardCoroutine: SelectCardCoroutine1,
                                        afterSelectCardCoroutine: null,
                                        message: "Select 1 [Titan] Digimon card to play.",
                                        maxCount: 1,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Trash,
                                        customRootCardList: null,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                                    IEnumerator SelectCardCoroutine1(CardSource cs)
                                    {
                                        selectedCard = cs;
                                        yield return null;
                                    }

                                    selectCardEffect.SetUpCustomMessage("Select 1 [Titan] Digimon card to play.", "The opponent is selecting 1 [Titan] Digimon card to play.");
                                    selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                    if (selectedCard != null)
                                    {
                                        int reduceCost = 7;

                                        ChangeCostClass changeCostClass = new ChangeCostClass();
                                        changeCostClass.SetUpICardEffect($"Play Cost -{reduceCost}", _ => true, card);
                                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: PlayCondition, rootCondition: _ => true, isUpDown: () => true, isCheckAvailability: () => false, isChangePayingCost: () => true);
                                        Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                                        card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                                        ICardEffect GetCardEffect(EffectTiming _timing)
                                            => _timing == EffectTiming.None ? changeCostClass : null;

                                        bool PlayCondition(CardSource cs) => cs == selectedCard;

                                        int ChangeCost(CardSource cs, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                                        {
                                            if (PlayCondition(cs))
                                            {
                                                cost -= reduceCost;
                                            }

                                            return cost;
                                        }

                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                            cardSources: new List<CardSource> { selectedCard },
                                            activateClass: activateClass,
                                            payCost: true,
                                            isTapped: false,
                                            root: SelectCardEffect.Root.Trash,
                                            activateETB: true));

                                        card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                maxCountPerTurn: 1,
                hashValue: "BT26_059_OP_WD_WA",
                additionalActivateCondition: SharedAdditionalActivateCondition,
                onPlay: true,
                whenDigivolving: true,
                whenAttacking: true);

            #region All Turns
            if (timing == EffectTiming.OnDiscardHand)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May delete all opponent's lowest level Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT26_059_AllTurns");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When hands are trashed from, you may delete all of your opponent's Digimon with the lowest level.";

                bool CanSelectDeleteTargetCondition(Permanent permanent)
                    => CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnTrashHand(hashtable, null, cardSource => true);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectDeleteTargetCondition);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDeleteTargetCondition))
                    {
                        List<Permanent> deleteTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(CanSelectDeleteTargetCondition);
                        yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(deleteTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
