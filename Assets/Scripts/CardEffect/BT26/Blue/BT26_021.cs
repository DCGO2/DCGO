using System;
using System.Collections;
using System.Collections.Generic;

// Gekomon
namespace DCGO.CardEffects.BT26
{
    public class BT26_021 : CEntity_Effect
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

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 3));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName()
                => "1 of your [TS] Digimon's attack target can't change for the turn";

            string SharedEffectDescription(string tag)
                => $"[{tag}] 1 of your Digimon with the [TS] trait's attack target can't change for the turn.";

            bool CanSelectPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.HasTSTraits;

            bool SharedAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 [TS] Digimon whose attack target can't change.", "The opponent is selecting 1 [TS] Digimon whose attack target can't change.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        permanent.UntilEachTurnEndEffects.Add(_timing => PermanentEffectFactory.CanNotSwitchAttackTargetEffect(permanent, activateClass));

                        yield return null;
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
                additionalActivateCondition: SharedAdditionalActivateCondition,
                onPlay: true,
                whenDigivolving: true);

            #region Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [TS] Tamer card from trash for 2 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT26_021_Main");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] [Once Per Turn] You may play 1 Tamer card with the [TS] trait from your trash with the cost reduced by 2.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.IsTamer
                        && cardSource.HasTSTraits
                        && cardSource.HasPlayCost
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, true, activateClass, fixedCost: cardSource.GetCostItself - 2);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool isUsed = false;

                    CardSource selectedCard = null;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 [TS] Tamer card to play.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    selectCardEffect.SetUpCustomMessage("Select 1 [TS] Tamer card to play.", "The opponent is selecting 1 [TS] Tamer card to play.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    if (selectedCard != null)
                    {
                        isUsed = true;

                        #region reduce cost
                        int reduceCost = 2;

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect($"Play Cost -{reduceCost}", CanUseCondition1, card);
                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: PlayCondition, rootCondition: RootCondition, isUpDown: IsUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                        Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                        card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                        ICardEffect GetCardEffect(EffectTiming _timing)
                            => _timing == EffectTiming.None ? changeCostClass : null;

                        bool CanUseCondition1(Hashtable _hashtable) => true;

                        bool PlayCondition(CardSource cardSource) => cardSource == selectedCard;

                        bool RootCondition(SelectCardEffect.Root root) => true;

                        bool IsUpDown() => true;

                        int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                        {
                            if (PlayCondition(cardSource))
                            {
                                cost -= reduceCost;
                            }

                            return cost;
                        }
                        #endregion

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: new List<CardSource> { selectedCard },
                            activateClass: activateClass,
                            payCost: true,
                            isTapped: false,
                            root: SelectCardEffect.Root.Trash,
                            activateETB: true));

                        #region release reduction
                        card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);
                        #endregion
                    }

                    if (!isUsed) activateClass.RemoveUse();
                }
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 1 hand card, trash bottom 2 digivolution cards of 1 opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT26_021_Inherit");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When a Digimon attacks, by trashing 1 card in your hand, trash the bottom 2 digivolution cards of 1 of your opponent's Digimon.";

                bool AnyPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);

                bool CanSelectDeleteTargetCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.DigivolutionCards.Count >= 1;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, AnyPermanentCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.Owner.HandCards.Count >= 1
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectDeleteTargetCondition);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
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
                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDeleteTargetCondition))
                                {
                                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectDeleteTargetCondition));

                                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectDeleteTargetCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: false,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: SelectPermanentCoroutine,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will trash its bottom 2 digivolution cards.", "The opponent is selecting 1 Digimon that will trash its bottom 2 digivolution cards.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanent, trashCount: 2, isFromTop: false, activateClass: activateClass));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
