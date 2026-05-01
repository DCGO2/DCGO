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
                if (CardEffectCommons.IsOwnerTurn(card))
                {
                    bool hasPaidCost = false;
                    bool canAddSecurityToHand = card.Owner.SecurityCards.Any();
                    bool canTrashBottomFaceDownCard = CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsTamerWithFaceDownCard);

                    if (canAddSecurityToHand)
                    {
                        #region Select to paid Security Cost
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                        if (card.Owner.SecurityCards.Count >= 2)
                        {
                            selectionElements.Add(new SelectionElement<int>(message: $"Add top security card to hand", value: 1, spriteIndex: 0));
                            selectionElements.Add(new SelectionElement<int>(message: $"Add bottom security card to hand", value: 2, spriteIndex: 0));
                        }
                        else if (card.Owner.SecurityCards.Count == 1)
                        {
                            selectionElements.Add(new SelectionElement<int>(message: $"Add security card to hand", value: 3, spriteIndex: 0));
                        }
                        selectionElements.Add(new SelectionElement<int>(message: $"Don't add security card to hand", value: 4, spriteIndex: 1));

                        string selectPlayerMessage = "Will you add the top security card to your hand?";
                        string notSelectPlayerMessage = "The opponent is to add the top security card to their hand.";

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                        int selectedValue = GManager.instance.userSelectionManager.SelectedIntValue;

                        if (selectedValue != 4)
                        {
                            CardSource topCard = null;
                            if (selectedValue == 1 || selectedValue == 3) topCard = card.Owner.SecurityCards[0];
                            else if (selectedValue == 2) topCard = card.Owner.SecurityCards[-1];

                            if (topCard != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { topCard }, false, activateClass));
                                yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(player: card.Owner, refSkillInfos: ref ContinuousController.instance.nullSkillInfos, activateClass).ReduceSecurity());
                                if (card.Owner.HandCards.Contains(topCard)) hasPaidCost = true;
                            }

                        }
                        #endregion
                    }

                    if (!hasPaidCost && canTrashBottomFaceDownCard)
                    {
                        #region Select to paid by Trashing bottom face down card
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                        };
                        string selectPlayerMessage = "Will you trash the bottom face-down card under any of your Tamers?";
                        string notSelectPlayerMessage = "The opponent is to trash the bottom face-down card under any of their Tamers.";
                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                        bool trashBottomCard = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (trashBottomCard)
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
                        #endregion
                    }

                    if (hasPaidCost) isUsed = true;
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, isGlowingDawnCard))
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
                    whenAttacking: true);
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
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsTamerWithFaceDownCard))
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
