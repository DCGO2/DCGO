using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Cougarmon
namespace DCGO.CardEffects.BT26
{
    public class BT26_026 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsTraits("Glowing Dawn");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 3));
            }
            #endregion

            #region Barrier
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing a Tamer's bottom face-down card or top security, use 1 [Glowing Dawn] Option for 2 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT26_026_WA");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] [Once Per Turn] By trashing the bottom face-down card from under any of your Tamers or your top security card, you may use 1 Option card with the [Glowing Dawn] trait from your hand with the cost reduced by 2.";

                bool IsTamerWithFaceDownCard(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.HasFaceDownDigivolutionCards
                        && !permanent.ImmuneFromStackTrashing(activateClass);

                bool FaceDownCards(CardSource cardSource) => cardSource.IsFaceDown;

                bool CanSelectOptionCardCondition(CardSource cardSource)
                    => cardSource.IsOption
                        && cardSource.ContainsTraits("Glowing Dawn")
                        && !cardSource.CanNotPlayThisOption;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && (card.Owner.SecurityCards.Count >= 1 || CardEffectCommons.HasMatchConditionPermanent(IsTamerWithFaceDownCard));

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canTrashTamerFaceDown = CardEffectCommons.HasMatchConditionPermanent(IsTamerWithFaceDownCard);
                    bool canTrashSecurity = card.Owner.SecurityCards.Count >= 1;

                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                    if (canTrashTamerFaceDown) selectionElements.Add(new SelectionElement<int>(message: "Trash 1 Tamer's bottom face-down card", value: 1, spriteIndex: 0));
                    if (canTrashSecurity) selectionElements.Add(new SelectionElement<int>(message: "Trash top security card", value: 2, spriteIndex: 0));
                    selectionElements.Add(new SelectionElement<int>(message: "Don't pay the cost", value: 3, spriteIndex: 1));

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: "Will you pay the cost?", notSelectPlayerMessage: "The opponent is choosing to pay the cost.");
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                    int selected = GManager.instance.userSelectionManager.SelectedIntValue;

                    bool hasPaidCost = false;

                    if (selected == 1)
                    {
                        Permanent selectedTamer = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsTamerWithFaceDownCard,
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

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer to trash its bottom face-down card.", "The opponent is selecting 1 Tamer to trash its bottom face-down card.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedTamer != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedTamer, trashCount: 1, isFromTop: false, activateClass: activateClass, cardCondition: FaceDownCards));
                            hasPaidCost = true;
                        }
                    }
                    else if (selected == 2)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(player: card.Owner, destroySecurityCount: 1, cardEffect: activateClass, fromTop: true).DestroySecurity());
                        hasPaidCost = true;
                    }

                    if (hasPaidCost && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectOptionCardCondition))
                    {
                        CardSource selectedCard = null;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOptionCardCondition,
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
                            selectedCard = cardSource;
                            yield return null;
                        }

                        selectHandEffect.SetUpCustomMessage("Select 1 [Glowing Dawn] Option card to use.", "The opponent is selecting 1 [Glowing Dawn] Option card to use.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

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
                                root: SelectCardEffect.Root.Hand));

                            card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);
                        }
                    }
                }
            }
            #endregion

            #region Inherit - Barrier
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
