using System;
using System.Collections;
using System.Collections.Generic;

// Cerberusmon
namespace DCGO.CardEffects.BT26
{
    public class BT26_074 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("TS");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 4));
            }
            #endregion

            #region Shared On Play / When Digivolving / When Attacking

            string SharedHashString = "BT26_074_Shared";

            string SharedEffectName() => "By trashing 1 hand card, use 1 [Titan] Option from trash for 2 less";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] If it's your turn, by trashing 1 card in your hand, you may use 1 Option card with the [Titan] trait from your trash with the cost reduced by 2.";

            bool SharedAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.IsOwnerTurn(card)
                    && card.Owner.HandCards.Count >= 1;

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool isUsed = false;

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

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    if (selectedCardToTrash != null)
                    {
                        isUsed = true;

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashHandAndProcessAccordingToResult(
                            player: card.Owner,
                            hashtable: hashtable,
                            cardToTrash: selectedCardToTrash,
                            activateClass: activateClass,
                            successProcess: SuccessProcess,
                            failureProcess: null));

                        IEnumerator SuccessProcess(CardSource cs)
                        {
                            bool CanSelectOptionCardCondition(CardSource cardSource)
                                => cardSource.EqualsTraits("Titan")
                                && cardSource.IsOption
                                && !cardSource.CanNotPlayThisOption
                                && cardSource.Owner.MaxMemoryCost >= cardSource.GetCostItself - 2;

                            if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectOptionCardCondition))
                            {
                                CardSource selectedCard = null;

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectOptionCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 [Titan] Option card to use.",
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

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                if (selectedCard != null)
                                {
                                    int reduceCost = 2;

                                    ChangeCostClass changeCostClass = new ChangeCostClass();
                                    changeCostClass.SetUpICardEffect($"Use Cost -{reduceCost}", _ => true, card);
                                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: PlayCondition, rootCondition: _ => true, isUpDown: () => true, isCheckAvailability: () => false, isChangePayingCost: () => true);
                                    Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                                    card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                                    ICardEffect GetCardEffect(EffectTiming _timing)
                                        => _timing == EffectTiming.None ? changeCostClass : null;

                                    bool PlayCondition(CardSource cs2) => cs2 == selectedCard;

                                    int ChangeCost(CardSource cs2, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                                    {
                                        if (PlayCondition(cs2))
                                        {
                                            cost -= reduceCost;
                                        }

                                        return cost;
                                    }

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                                        cardSources: new List<CardSource>() { selectedCard },
                                        activateClass: activateClass,
                                        payCost: true,
                                        root: SelectCardEffect.Root.Trash));

                                    card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);
                                }
                            }
                        }
                    }
                }

                if (!isUsed) activateClass.RemoveUse();
            }

            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                maxCountPerTurn: 1,
                hashValue: SharedHashString,
                additionalActivateCondition: SharedAdditionalActivateCondition,
                onPlay: true,
                whenDigivolving: true,
                whenAttacking: true);

            #region Inherit - On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 opponent's lowest level Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Deletion] Delete 1 of your opponent's Digimon with the lowest level.";

                bool CanSelectPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.CanActivateOnDeletion(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
