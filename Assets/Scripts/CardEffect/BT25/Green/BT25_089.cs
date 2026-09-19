using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT25
{
    // Kazuki & Itsuki
    public class BT25_089 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                cardEffects.Add(CardEffectFactory.Gain1MemoryTamerOpponentDigimonEffect(card));
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Link for -2", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] [Once Per Turn] You may link 1 [Appmon] trait Digimon card from your hand or your Digimon's digivolution cards to 1 of your Digimon with the cost reduced by 2.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card)
                        && (CardEffectCommons.HasMatchConditionOwnersHand(card, CanLinkCardActivateCondition)
                            || CardEffectCommons.HasMatchConditionPermanent(CanTakeFromDigivolutionCardsActivateCondition));
                }

                bool CanTakeFromDigivolutionCardsActivateCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.DigivolutionCards.Any(CanLinkCardActivateCondition);
                }

                bool CanLinkCardActivateCondition(CardSource cardSource) => CanLinkCardCondition(cardSource, false);

                bool CanLinkCardEffectCondition(CardSource cardSource) => CanLinkCardCondition(cardSource, true);

                bool CanLinkCardCondition(CardSource cardSource, bool payCost)
                {
                    return cardSource.IsDigimon
                        && cardSource.EqualsTraits("Appmon")
                        && cardSource.CanLink(payCost);
                }

                bool CanTakeFromDigivolutionCardsEffectCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.DigivolutionCards.Any(CanLinkCardEffectCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                        new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass,
                        SuccessProcess,
                        null));

                    IEnumerator SuccessProcess(List<Permanent> suspendedPermaments)
                    {
                        #region Link Cost Reduction
                        ICardEffect GetCardEffect(EffectTiming _timing)
                        {
                            if (_timing == EffectTiming.None)
                            {
                                return CardEffectFactory.GrantedReduceLinkCostClass(
                                    card: card, 
                                    reducedCost: 2,
                                    cardSourceCondition: _ => true,
                                    permanentCondition: _ => true,
                                    rootCondition: _ => true
                                );
                            }

                            return null;
                        }

                        card.Owner.UntilCalculateFixedCostEffect.Add(GetCardEffect);
                        #endregion

                        bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanLinkCardEffectCondition);
                        bool canSelectSources = CardEffectCommons.HasMatchConditionPermanent(CanTakeFromDigivolutionCardsActivateCondition);

                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                        if (canSelectHand)
                        {
                            selectionElements.Add(new SelectionElement<int>(message: $"From hand", value : 1, spriteIndex: 0));
                        }
                        if (canSelectSources)
                        {
                            selectionElements.Add(new SelectionElement<int>(message: $"From digivolution cards", value : 2, spriteIndex: 0));
                        }
                        selectionElements.Add(new SelectionElement<int>(message: $"Do not Link", value : 3, spriteIndex: 1));

                        string selectPlayerMessage = "From which area will you link a card?";
                        string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool doLink = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                        bool fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                        if (doLink)
                        {
                            if (fromHand)
                            {
                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanLinkCardEffectCondition,
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

                                selectHandEffect.SetUpCustomMessage("Select 1 card to link.", "The opponent is selecting 1 card to link.");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");

                                yield return StartCoroutine(selectHandEffect.Activate());
                            }
                            else
                            {
                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanTakeFromDigivolutionCardsEffectCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to select a digivolution card from", "The opponent is selecting 1 card to link.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                    selectCardEffect.SetUp(
                                        canTargetCondition: CanLinkCardEffectCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => true,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select 1 card to add as source.",
                                        maxCount: 1,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.DigivolutionCards,
                                        customRootCardList: permanent.DigivolutionCards,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                                    selectCardEffect.SetUpCustomMessage("Select 1 card to link.", "The opponent is selecting 1 card to link.");
                                    selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");

                                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                                }
                            }

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                bool CanLinkPermanentCondition(Permanent permanent)
                                {
                                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                        && cardSource.CanLinkToTargetPermanent(permanent, true);
                                }

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanLinkPermanentCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to link to.", "The opponent is selecting 1 digimon to link.");
                                
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new ILinkCard(true, cardSource, permanent, activateClass).LinkCard());
                                }
                            }
                        }

                        #region Remove Link Cost Reduction
                        card.Owner.UntilCalculateFixedCostEffect.Remove(GetCardEffect);
                        #endregion
                    }
                }
            }
            #endregion

            #region End of turn
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("App fuse 1 digimon into digimon in hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT25_089_AppFusion");
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[End of Your Turn] [Once Per Turn] 1 of your Digimon may app fuse into a Digimon card in the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanSelectPermanent(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        foreach (CardSource hand in card.Owner.HandCards)
                        {
                            if (hand.appFusionCondition != null)
                            {
                                if (hand.CanAppFusionFromTargetPermanent(permanent, true))
                                    return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanSelectCard(CardSource card, Permanent permanent)
                {
                    if (CardEffectCommons.IsExistOnHand(card))
                    {
                        if (card.appFusionCondition != null)
                        {
                            if (card.CanAppFusionFromTargetPermanent(permanent, true))
                                return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool executed = false;
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanent))
                    {
                        Permanent selectedPermanent = null;
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, CanSelectPermanent));
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanent,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to app fuse", "The opponent is selecting 1 digimon to app fuse");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            CardSource selectedCard = null;
                            int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, handCard => CanSelectCard(handCard, selectedPermanent)));
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: handCard => CanSelectCard(handCard, selectedPermanent),
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectHandEffect.SetUpCustomMessage("Select digimon to app fuse into.", "The opponent is selecting digimon to app fuse into.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Selected digimon");
                            yield return StartCoroutine(selectHandEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCard = cardSource;
                                yield return null;
                            }

                            if (selectedCard != null && selectedCard.CanAppFusionFromTargetPermanent(selectedPermanent, true))
                            {
                                CardSource linkCard = selectedPermanent.LinkedCards.Where(x => selectedCard.appFusionCondition.linkedCondition(selectedPermanent, x)).First();

                                PlayCardClass playCardClass = new PlayCardClass(new List<CardSource> { selectedCard }, hashtable, true, selectedPermanent, false, SelectCardEffect.Root.Hand, true);
                                playCardClass.SetAppFusion(new int[] { selectedPermanent.PermanentFrame.FrameID, selectedPermanent.LinkedCards.IndexOf(linkCard) });

                                executed = true;

                                yield return ContinuousController.instance.StartCoroutine(playCardClass.PlayCard());
                            }
                        }
                    }
                    if (!executed) activateClass.RemoveUse();
                }
            }
            #endregion

            #region Security Effect
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}
