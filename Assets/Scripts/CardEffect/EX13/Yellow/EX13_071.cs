using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Richard Sampson
namespace DCGO.CardEffects.EX13
{
    public class EX13_071 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared OP/SOMP
            string SharedEffectName = "Place top card of deck face down under this tamer, then if enemy has Digimon gain 1 memory";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    onPlay: true,
                    startOfYourMainPhase: true);

            string SharedEffectDescription(string tag) =>
                $"[{tag}] You may place the top card of your deck face down under this tamer. Then, if your opponent has a Digimon, gain 1 memory.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (card.Owner.LibraryCards.Count >= 1)
                {
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>
                    {
                        new(message: $"Yes", value: 1, spriteIndex: 0),
                        new(message: $"No", value: 2, spriteIndex: 1)
                    };

                    string selectPlayerMessage = "Will you place the top card from your deck under this Tamer face down?";
                    string notSelectPlayerMessage = "The opponent is choosing whether to place the top card from their deck under their Tamer face down.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool Yes = GManager.instance.userSelectionManager.SelectedIntValue == 1;

                    if (Yes)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                                new List<CardSource> { card.Owner.LibraryCards[0] }, activateClass, isFacedown: true));
                    }
                }

                if (card.Owner.Enemy.GetBattleAreaDigimons().Count >= 1)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 3 face-downs from tamers and place lv4 & lv5 [Holy Beast] trait yellow Digimon cards from trash to under [Kudamon] to warp it to [Kentaurosmon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("EX13_071_Main");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] [Once Per Turn] By trashing 3 bottom face-down cards from under any of your Tamers and placing 1 each of level 4 and level 5 [Holy Beast] trait yellow Digimon cards from your trash as 1 of your [Kudamon]'s bottom digivolution cards, it may digivolve into [Kentaurosmon] in the hand or trash, ignoring level and with the cost reduced by 1.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                    && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                    && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectLv4CardCondition)
                    && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectLv5CardCondition)
                    && CardEffectCommons.HasMatchConditionPermanent(CanSelectKudamonPermanentCondition))
                    {
                        var faceDownSourceCount = card.Owner.GetBattleAreaPermanents()
                            .Filter(TamerWithOneOrMoreFaceDownSource)
                            .Sum(permanent => permanent.DigivolutionCards.Count(CanSelectTrashSourceCardCondition));

                        if (faceDownSourceCount >= 3)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool TamerWithOneOrMoreFaceDownSource(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.DigivolutionCards.Any(CanSelectTrashSourceCardCondition);
                }

                bool CanSelectTrashSourceCardCondition(CardSource cardSource)
                {
                    return cardSource.IsFaceDown;
                }

                bool CanSelectLv4CardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.EqualsTraits("Holy Beast")
                        && cardSource.IsLevel4;
                }

                bool CanSelectLv5CardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.EqualsTraits("Holy Beast")
                        && cardSource.IsLevel5;
                }

                bool CanSelectKudamonPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && permanent.TopCard.EqualsCardName("Kudamon");
                }

                bool CanSelectKentaurosmonCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Kentaurosmon");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool trashed = false;
                    const int targetTrashAmount = 3;

                    SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                    int maxCount1 = Math.Min(targetTrashAmount, CardEffectCommons.MatchConditionPermanentCount(TamerWithOneOrMoreFaceDownSource));

                    selectPermanentEffect1.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: TamerWithOneOrMoreFaceDownSource,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: CanEndSelectCondition,
                        maxCount: maxCount1,
                        canNoSelect: true,
                        canEndNotMax: true,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect1.SetUpCustomMessage(
                        $"Select Tamer(s) to trash bottom face-down cards from (Total: {targetTrashAmount})",
                        $"The opponent is selecting Tamer(s) to trash bottom face-down cards from (Total: {targetTrashAmount})"
                    );

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                    bool CanEndSelectCondition(List<Permanent> permanents)
                    {
                        if (permanents.Count > 0)
                        {
                            int faceDownCount = 0;
                            foreach (Permanent permanent in permanents)
                            {
                                faceDownCount += permanent.DigivolutionCards.Count(CanSelectTrashSourceCardCondition);
                            }

                            return faceDownCount >= targetTrashAmount;
                        }

                        return false;
                    }

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        if (permanents == null || permanents.Count == 0) yield break;

                        int remainingTrashCount = targetTrashAmount;
                        List<Permanent> validTargets = new List<Permanent>(permanents);

                        while (remainingTrashCount > 0 && validTargets.Count > 0)
                        {
                            if (validTargets.Count == 1)
                            {
                                Permanent finalTarget = validTargets[0];
                                int availableCards = finalTarget.DigivolutionCards.Count(CanSelectTrashSourceCardCondition);
                                int trashThisAmount = Math.Min(remainingTrashCount, availableCards);

                                if (trashThisAmount > 0)
                                {
                                    trashed = true;

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                                        targetPermanent: finalTarget,
                                        trashCount: trashThisAmount,
                                        isFromTop: false,
                                        activateClass: activateClass,
                                        CanSelectTrashSourceCardCondition
                                    ));

                                    remainingTrashCount -= trashThisAmount;
                                }

                                break;
                            }

                            bool allTargetsHaveExactlyOneLeft = validTargets.All(p => p.DigivolutionCards.Count(CanSelectTrashSourceCardCondition) == 1);

                            if (allTargetsHaveExactlyOneLeft)
                            {
                                foreach (Permanent target in new List<Permanent>(validTargets))
                                {
                                    if (remainingTrashCount <= 0) break;

                                    trashed = true;

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                                        targetPermanent: target,
                                        trashCount: 1,
                                        isFromTop: false,
                                        activateClass: activateClass,
                                        CanSelectTrashSourceCardCondition
                                    ));

                                    remainingTrashCount--;
                                }

                                break;
                            }

                            Permanent selectedTarget = null;
                            SelectPermanentEffect selectPermanentEffectLoop = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffectLoop.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: permanent => validTargets.Contains(permanent) && permanent.DigivolutionCards.Count(CanSelectTrashSourceCardCondition) > 0,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: (p) => { selectedTarget = p; return null; },
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffectLoop.SetUpCustomMessage(
                                $"Select a Tamer to trash up to {remainingTrashCount} cards from\n(You can choose another Tamer after if max is not chosen)",
                                "The opponent is selecting a Tamer to trash cards from"
                            );

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffectLoop.Activate());

                            if (selectedTarget != null)
                            {
                                int availableCards = selectedTarget.DigivolutionCards.Count(CanSelectTrashSourceCardCondition);

                                int maxSelectableNumber = Math.Min(remainingTrashCount, availableCards);
                                int chosenTrashAmount = 1;

                                if (maxSelectableNumber > 1)
                                {
                                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                                    for (int i = 1; i <= maxSelectableNumber; i++)
                                        selectionElements.Add(new SelectionElement<int>(message: $"{i}", value: i, spriteIndex: 0));

                                    string selectPlayerMessage = "Choose how many cards to trash?";
                                    string notSelectPlayerMessage = "The opponent is choosing how many cards to trash.";

                                    if (selectionElements.Count > 1)
                                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                                    else
                                        GManager.instance.userSelectionManager.SetInt(selectionElements[0].Value);

                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                                    chosenTrashAmount = GManager.instance.userSelectionManager.SelectedIntValue;
                                }
                                else
                                {
                                    chosenTrashAmount = maxSelectableNumber;
                                }

                                if (chosenTrashAmount > 0)
                                {
                                    trashed = true;
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                                        targetPermanent: selectedTarget,
                                        trashCount: chosenTrashAmount,
                                        isFromTop: false,
                                        activateClass: activateClass,
                                        CanSelectTrashSourceCardCondition
                                    ));

                                    remainingTrashCount -= chosenTrashAmount;

                                    if (selectedTarget.DigivolutionCards.Count(CanSelectTrashSourceCardCondition) == 0)
                                    {
                                        validTargets.Remove(selectedTarget);
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        yield return null;
                    }

                    if (trashed)
                    {
                        List<CardSource> selectedSourceCards = new List<CardSource>();
                        List<CardSource> digivolutionCardsOrder = new List<CardSource>();
                        Permanent selectedKudamonPermanent = null;

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedSourceCards.Add(cardSource);
                            yield return null;
                        }

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedKudamonPermanent = permanent;

                            yield return null;
                        }

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            digivolutionCardsOrder = cardSources.Clone();

                            yield return null;
                        }

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectLv4CardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 level 4 [Holy Beast] trait yellow Digimon card to place in Digivolution cards.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 level 4 [Holy Beast] trait yellow Digimon card to place in Digivolution cards.",
                            "The opponent is selecting 1 level 4 [Holy Beast] trait yellow Digimon card to place in Digivolution cards.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                        yield return StartCoroutine(selectCardEffect.Activate());

                        SelectCardEffect selectCardEffect2 = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect2.SetUp(
                            canTargetCondition: CanSelectLv5CardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 level 5 [Holy Beast] trait yellow Digimon card to place in Digivolution cards.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect2.SetUpCustomMessage("Select 1 level 5 [Holy Beast] trait yellow Digimon card to place in Digivolution cards.",
                            "The opponent is selecting 1 level 5 [Holy Beast] trait yellow Digimon card to place in Digivolution cards.");
                        selectCardEffect2.SetUpCustomMessage_ShowCard("Digivolution Card");

                        yield return StartCoroutine(selectCardEffect2.Activate());

                        selectedSourceCards.Reverse();

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectKudamonPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 [Kudamon] to place Digivolution cards under.", "The opponent is selecting 1 [Kudamon] to place Digivolution cards under.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        SelectCardEffect selectCardEffect3 = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect3.SetUp(
                            canTargetCondition: _ => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            message: "Specify the order to place the cards in the digivolution cards\n(cards will be placed so that cards with lower numbers are on top).",
                            maxCount: selectedSourceCards.Count,
                            canEndNotMax: false,
                            isShowOpponent: false,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: selectedSourceCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect3.SetUpCustomMessage_ShowCard("Digivolution Cards");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect3.Activate());

                        yield return ContinuousController.instance.StartCoroutine(
                            selectedKudamonPermanent.AddDigivolutionCardsBottom(digivolutionCardsOrder, activateClass));

                        bool CanEvoKentaurosmonCardCondition(CardSource cardSource, bool root)
                        {
                            return CanSelectKentaurosmonCardCondition(cardSource)
                                && cardSource.CanPlayCardTargetFrame(selectedKudamonPermanent.PermanentFrame, true, activateClass, root ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash, -1, false, CardEffectCommons.IgnoreRequirement.Level);
                        }

                        bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, (cardSource) => CanEvoKentaurosmonCardCondition(cardSource, true));
                        bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanEvoKentaurosmonCardCondition(cardSource, false));

                        if (canSelectHand || canSelectTrash)
                        {
                            if (canSelectHand && canSelectTrash)
                            {
                                GManager.instance.userSelectionManager.SetBoolSelection(
                                    selectionElements: new List<SelectionElement<bool>>()
                                    {
                                    new SelectionElement<bool>(message: "From hand", value: true, spriteIndex: 0),
                                    new SelectionElement<bool>(message: "From trash", value: false, spriteIndex: 1),
                                    },
                                    selectPlayer: card.Owner,
                                    selectPlayerMessage: "From which area will you Digivolve?",
                                    notSelectPlayerMessage: "The opponent is choosing from which area to Digivolve.");
                            }
                            else
                            {
                                GManager.instance.userSelectionManager.SetBool(canSelectHand);
                            }

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            bool isHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            selectedKudamonPermanent,
                            CanSelectKentaurosmonCardCondition,
                            payCost: true,
                            reduceCostTuple: (reduceCost: 1, reduceCostCardCondition: null),
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: isHand,
                            activateClass: activateClass,
                            successProcess: null,
                            ignoreRequirements: CardEffectCommons.IgnoreRequirement.Level));
                        }
                    }

                    if (!trashed) activateClass.RemoveUse();
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
