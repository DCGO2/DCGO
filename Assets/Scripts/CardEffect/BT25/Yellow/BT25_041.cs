using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Murasamemon 
namespace DCGO.CardEffects.BT25
{
    public class BT25_041 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.HasGlowingDawnTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, false, card, null, level: 4));
            }
            #endregion

            #region Alliance
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared WD / WA

            string SharedHashString = "BT25_041_WD_WA";

            string SharedEffectName = "By adding top security to hand or trashing bottom face down of a tamer, play or use 1 [Glowing Dawn] trait card from hand for 3 reduced cost";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] If it's your turn, by adding your top security card to the hand or trashing the bottom face-down card under any of your Tamers, you may play or use 1 card with the [Glowing Dawn] trait from your hand with the cost reduced by 3.";

            bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass) => CardEffectCommons.IsOwnerTurn(card);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Conditions

                bool IsTamerWithFaceDownCard(Permanent permanent) =>
                    CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card) &&
                    permanent.HasFaceDownDigivolutionCards &&
                    !permanent.ImmuneFromStackTrashing(activateClass);

                bool isGlowingDawnCard(CardSource cardSource) =>
                    cardSource.HasGlowingDawnTraits;

                bool FaceDownCards(CardSource cardSource) => cardSource.IsFaceDown;
                #endregion

                bool isUsed = false;
                bool hasPaidCost = false;
                bool canAddSecurityToHand = card.Owner.SecurityCards.Any();
                bool canTrashBottomFaceDownCard = CardEffectCommons.HasMatchConditionPermanent(IsTamerWithFaceDownCard);

                #region Select to pay Cost
                List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                if (canAddSecurityToHand) selectionElements.Add(new SelectionElement<int>(message: $"Add top security card to hand", value: 1, spriteIndex: 0));
                if (canTrashBottomFaceDownCard) selectionElements.Add(new SelectionElement<int>(message: $"Trash bottom face down card from 1 tamer", value: 2, spriteIndex: 0));
                selectionElements.Add(new SelectionElement<int>(message: $"Dont pay the cost", value: 3, spriteIndex: 1));

                string selectPlayerMessage = "Will you pay the cost?";
                string notSelectPlayerMessage = "The opponent is choosing to pay the cost.";

                GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                bool payCost = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                bool isSecurity = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                #endregion

                #region Pay Cost
                if (payCost)
                {
                    if (isSecurity)
                    {
                        CardSource topCard = card.Owner.SecurityCards[0];
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { topCard }, false, activateClass));
                        yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(player: card.Owner, refSkillInfos: ref ContinuousController.instance.nullSkillInfos, activateClass).ReduceSecurity());
                        if (card.Owner.HandCards.Contains(topCard))
                        {
                            hasPaidCost = true;
                            isUsed = true;
                        }
                    }
                    else
                    {
                        bool trash = false;
                        SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect1.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsTamerWithFaceDownCard,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);


                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanents[0], trashCount: 1, isFromTop: false, activateClass: activateClass, FaceDownCards));
                            trash = true;
                        }

                        selectPermanentEffect1.SetUpCustomMessage("Select 1 Tamer to trash 1 bottom face-down card from", "The opponent is selecting 1 Tamer to trash 1 bottom face-down card from");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());
                        if (trash)
                        {
                            hasPaidCost = true;
                            isUsed = true;
                        }
                    }
                }
                #endregion

                if (hasPaidCost && CardEffectCommons.HasMatchConditionOwnersHand(card, isGlowingDawnCard))
                {
                    CardSource selectedCard = null;

                    #region Selected Glowing Dawn Card in Hand to play or use

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, isGlowingDawnCard));

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: isGlowingDawnCard,
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

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    selectHandEffect.SetUpCustomMessage("Select 1 [Glowing Dawn] card to play/use.", "The opponent is selecting 1 [Glowing Dawn] card to play/use.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");
                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                    #endregion

                    if (selectedCard != null)
                    {

                        #region Reduce Cost

                        IEnumerator ReduceCost(string type)
                        {
                            if (card.Owner.CanReduceCost(null, card))
                            {
                                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                            }

                            Hashtable hashtable = new Hashtable
                                {
                                    { "CardEffect", activateClass }
                                };

                            ChangeCostClass changeCostClass = new ChangeCostClass();
                            changeCostClass.SetUpICardEffect($"{type} cost: -3", CanUseCondition1, card);
                            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                            card.Owner.UntilCalculateFixedCostEffect.Add(_ => changeCostClass);
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(hashtable));

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                return true;
                            }

                            int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                            {
                                if (CardSourceCondition(cardSource) &&
                                    RootCondition(root) &&
                                    PermanentsCondition(targetPermanents))
                                {
                                    cost -= 3;
                                }

                                return cost;
                            }

                            bool PermanentsCondition(List<Permanent> targetPermanents)
                            {
                                return targetPermanents == null || targetPermanents.Count(targetPermanent => targetPermanent != null) == 0;
                            }

                            bool CardSourceCondition(CardSource cardSource)
                            {
                                return cardSource != null
                                    && cardSource.Owner == card.Owner
                                    && cardSource.EqualsTraits("Glowing Dawn");
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

                        if (selectedCard.IsOption)
                        {
                            yield return ContinuousController.instance.StartCoroutine(ReduceCost("Use"));
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                                cardSources: new List<CardSource>() { selectedCard },
                                activateClass: activateClass,
                                payCost: true,
                                root: SelectCardEffect.Root.Hand));

                        }
                        else
                        {
                            yield return ContinuousController.instance.StartCoroutine(ReduceCost("Play"));
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: new List<CardSource>() { selectedCard },
                                activateClass: activateClass,
                                payCost: true,
                                isTapped: false,
                                root: SelectCardEffect.Root.Hand,
                                activateETB: true));
                        }
                    }

                }

                if (!isUsed) activateClass.RemoveUse();
            }

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    isSkippable: true,
                    maxCountPerTurn: 1,
                    hashValue: SharedHashString,
                    whenDigivolving: true,
                    whenAttacking: true,
                    additionalActivateCondition: AdditionalActivateCondition);
            #endregion

            #region Inherit End of Attack OPT

            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing bottom face down of a tamer, this digimon with [Glowing Dawn] trait unsuspends", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT25_041_EndAttack");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription() => "[End of Attack] [Once Per Turn] By trashing the bottom face-down card from under any of your Tamers, this Digimon with the [Glowing Dawn] trait unsuspends.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnEndAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsTamerWithFaceDownCard);
                }

                bool IsTamerWithFaceDownCard(Permanent permanent) =>
                    CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card) &&
                    permanent.HasFaceDownDigivolutionCards &&
                    !permanent.ImmuneFromStackTrashing(activateClass);

                bool FaceDownCards(CardSource cardSource) => cardSource.IsFaceDown;

                bool IsGlowingDawnDigimon(Permanent permanent) => permanent.TopCard.HasGlowingDawnTraits;

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool isUsed = false;
                    bool hasPaidCost = false;
                    if (CardEffectCommons.HasMatchConditionPermanent(IsTamerWithFaceDownCard))
                    {
                        bool trash = false;
                        SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect1.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsTamerWithFaceDownCard,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect1.SetUpCustomMessage("Select 1 Tamer to trash 1 bottom face-down card from", "The opponent is selecting 1 Tamer to trash 1 bottom face-down card from");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanents[0], trashCount: 1, isFromTop: false, activateClass: activateClass, FaceDownCards));
                            trash = true;
                        }

                        if (trash) hasPaidCost = true;
                    }

                    if (hasPaidCost) isUsed = true;
                    if (hasPaidCost && IsGlowingDawnDigimon(card.PermanentOfThisCard())) yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());

                    if (!isUsed) activateClass.RemoveUse();
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
