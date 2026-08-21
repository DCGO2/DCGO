using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// NightChiropmon
namespace DCGO.CardEffects.BT26
{
    public class BT26_070 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Glowing Dawn");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 3));
            }
            #endregion

            #region Shared On Play / When Digivolving - Draw & Trash

            string SharedEffectNameA() => "Draw 1 and trash 1 card in hand";

            string SharedEffectDescriptionA(string tag) => $"[{tag}] <Draw 1> and trash 1 card in your hand.";

            IEnumerator SharedActivateCoroutineA(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                if (card.Owner.HandCards.Count >= 1)
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: _ => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                }
            }

            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectNameA(),
                SharedActivateCoroutineA,
                SharedEffectDescriptionA,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            #region Main - Trash 2 Facedown
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 2 Tamer's bottom face-down cards, use 1 [Glowing Dawn] Option from trash for 2 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT26_070_Main");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] [Once Per Turn] By trashing 2 bottom face-down cards from under any of your Tamers, you may use 1 Option card with the [Glowing Dawn] trait from your trash with the cost reduced by 2.";

                bool IsTamerWithEnoughFaceDownCards(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.DigivolutionCards.Count(cs => cs.IsFaceDown) >= 2
                        && !permanent.ImmuneFromStackTrashing(activateClass);

                bool FaceDownCards(CardSource cardSource) => cardSource.IsFaceDown;

                bool CanSelectOptionCardCondition(CardSource cardSource, ICardEffect activateClass)
                    => cardSource.IsOption
                        && cardSource.EqualsTraits("Glowing Dawn")
                        && !cardSource.CanNotPlayThisOption
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, true, activateClass, fixedCost: cardSource.GetCostItself - 2);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(IsTamerWithEnoughFaceDownCards);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool isUsed = false;

                    if (CardEffectCommons.HasMatchConditionPermanent(IsTamerWithEnoughFaceDownCards))
                    {
                        Permanent selectedTamer = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsTamerWithEnoughFaceDownCards,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedTamer = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer to trash 2 bottom face-down cards from.", "The opponent is selecting 1 Tamer to trash 2 bottom face-down cards from.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedTamer != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedTamer, trashCount: 2, isFromTop: false, activateClass: activateClass, cardCondition: FaceDownCards));

                            isUsed = true;

                            bool CanSelectOptionCardConditionBound(CardSource cs) => CanSelectOptionCardCondition(cs, activateClass);

                            if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectOptionCardConditionBound))
                            {
                                CardSource selectedCard = null;

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectOptionCardConditionBound,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 [Glowing Dawn] Option card to use.",
                                    maxCount: 1,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                IEnumerator SelectCardCoroutine(CardSource cs)
                                {
                                    selectedCard = cs;
                                    yield return null;
                                }

                                selectCardEffect.SetUpCustomMessage("Select 1 [Glowing Dawn] Option card to use.", "The opponent is selecting 1 [Glowing Dawn] Option card to use.");
                                selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");

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

                                    bool PlayCondition(CardSource cs) => cs == selectedCard;

                                    int ChangeCost(CardSource cs, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                                    {
                                        if (PlayCondition(cs))
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

                    if (!isUsed) activateClass.RemoveUse();
                }
            }
            #endregion

            #region Inherit - Retaliation
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.RetaliationSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
