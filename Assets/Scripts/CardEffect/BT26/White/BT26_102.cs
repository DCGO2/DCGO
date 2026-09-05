using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Seven Code PAD
namespace DCGO.CardEffects.BT26
{
    public class BT26_102 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Use Req.
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource) => cardSource.EqualsTraits("Seven Code");
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 6 [Seven Code] cards as digivolution material, that Digimon may digivolve into [Dantemon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] By placing 6 [Seven Code] trait Digimon cards from your battle area, link cards or trash as 1 of your [Seven Code] trait Digimon's bottom digivolution cards, that Digimon may digivolve into [Dantemon] in the hand, ignoring digivolution requirements and without paying the cost.";

                bool CanSelectTargetCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsTraits("Seven Code");

                bool CanSelectDantemonCondition(CardSource cardSource) => cardSource.EqualsCardName("Dantemon");

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectTargetCondition)) yield break;

                    Permanent selectedTarget = null;

                    List<CardSource> materialCards = new List<CardSource>();

                    bool CanSelectOtherBattleAreaCondition(Permanent permanent)
                        =>  CanSelectTargetCondition(permanent) && !materialCards.Contains(permanent.TopCard) && permanent != selectedTarget;

                    bool CanSelectLinkedOrTrashCondition(CardSource cardSource)
                        => cardSource.IsDigimon && cardSource.EqualsTraits("Seven Code") && !materialCards.Contains(cardSource);

                    bool CanSelectLinkPermanentCondition(Permanent permanent)
                        => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                            && permanent.LinkedCards.Filter(CanSelectLinkedOrTrashCondition).Count > 0;

                    int availableBattleArea = CardEffectCommons.MatchConditionPermanentCount(CanSelectOtherBattleAreaCondition);
                    int availableLinked = 0;
                    int availableTrash = card.Owner.TrashCards.Filter(CanSelectLinkedOrTrashCondition).Count;

                    foreach (Permanent permanent in card.Owner.GetBattleAreaDigimons().Where((permanent) => permanent.LinkedCards.Any(CanSelectLinkedOrTrashCondition)))
                    {
                        availableLinked += permanent.LinkedCards.Count(CanSelectLinkedOrTrashCondition);
                    }

                    if (availableBattleArea - 1 + availableLinked + availableTrash < 6) yield break;

                    SelectPermanentEffect selectTargetEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectTargetEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectTargetCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectTargetCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    IEnumerator SelectTargetCoroutine(Permanent permanent)
                    {
                        selectedTarget = permanent;
                        yield return null;
                    }

                    selectTargetEffect.SetUpCustomMessage("Select 1 [Seven Code] trait Digimon to gain digivolution material.", "The opponent is selecting 1 [Seven Code] trait Digimon.");

                    yield return ContinuousController.instance.StartCoroutine(selectTargetEffect.Activate());

                    if (selectedTarget == null) yield break;

                    int remaining = 6;

                    while (remaining > 0)
                    {
                        bool canPickBattleArea = CardEffectCommons.HasMatchConditionPermanent(CanSelectOtherBattleAreaCondition);
                        bool canPickLinked = CardEffectCommons.HasMatchConditionPermanent(CanSelectLinkPermanentCondition);
                        bool canPickTrash = card.Owner.TrashCards.Exists(CanSelectLinkedOrTrashCondition);

                        List<SelectionElement<int>> locationElements = new List<SelectionElement<int>>();
                        if (canPickBattleArea) locationElements.Add(new(message: "Battle area", value: 1, spriteIndex: 0));
                        if (canPickLinked) locationElements.Add(new(message: "Link cards", value: 2, spriteIndex: 0));
                        if (canPickTrash) locationElements.Add(new(message: "Trash", value: 3, spriteIndex: 0));
                        locationElements.Add(new(message: "Cancel", value: 4, spriteIndex: 1));

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: locationElements, selectPlayer: card.Owner, selectPlayerMessage: $"Select a location to take [Seven Code] digivolution material from ({remaining} remaining).", notSelectPlayerMessage: "The opponent is selecting a location for digivolution material.");
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                        int selectedLocation = GManager.instance.userSelectionManager.SelectedIntValue;

                        if (selectedLocation == 4) yield break;

                        if (selectedLocation == 1)
                        {
                            List<Permanent> selectedSources = new List<Permanent>();

                            int battleAreaMaxCount = Math.Min(remaining, CardEffectCommons.MatchConditionPermanentCount(CanSelectOtherBattleAreaCondition));

                            SelectPermanentEffect selectSourceEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectSourceEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectOtherBattleAreaCondition,
                                canTargetCondition_ByPreSelecetedList: (preSelected, permanent) => !preSelected.Contains(permanent),
                                canEndSelectCondition: null,
                                maxCount: battleAreaMaxCount,
                                canNoSelect: true,
                                canEndNotMax: true,
                                selectPermanentCoroutine: SelectSourceCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            IEnumerator SelectSourceCoroutine(Permanent permanent)
                            {
                                selectedSources.Add(permanent);
                                yield return null;
                            }

                            selectSourceEffect.SetUpCustomMessage($"Select up to {battleAreaMaxCount} [Seven Code] trait Digimon from your battle area to place as material ({remaining} remaining).", "The opponent is selecting material.");

                            yield return ContinuousController.instance.StartCoroutine(selectSourceEffect.Activate());

                            if (selectedSources.Count == 0) continue;

                            foreach (Permanent selectedSource in selectedSources) materialCards.Add(selectedSource.TopCard);

                            remaining -= selectedSources.Count;
                        }
                        else if (selectedLocation == 2)
                        {
                            Permanent selectedLinkSource = null;

                            SelectPermanentEffect selectLinkSourceEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectLinkSourceEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectLinkPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectLinkSourceCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            IEnumerator SelectLinkSourceCoroutine(Permanent permanent)
                            {
                                selectedLinkSource = permanent;
                                yield return null;
                            }

                            selectLinkSourceEffect.SetUpCustomMessage("Select 1 Digimon to take linked [Seven Code] cards from.", "The opponent is selecting 1 Digimon to take linked cards from.");

                            yield return ContinuousController.instance.StartCoroutine(selectLinkSourceEffect.Activate());

                            if (selectedLinkSource == null) continue;

                            List<CardSource> linkPool = selectedLinkSource.LinkedCards.Filter(CanSelectLinkedOrTrashCondition);

                            List<CardSource> selectedLinkCards = new List<CardSource>();

                            int linkMaxCount = Math.Min(remaining, linkPool.Count);

                            SelectCardEffect selectLinkCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectLinkCardEffect.SetUp(
                                canTargetCondition: CanSelectLinkedOrTrashCondition,
                                canTargetCondition_ByPreSelecetedList: (preSelected, cardSource) => !preSelected.Contains(cardSource),
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectLinkCardCoroutine,
                                message: $"Select up to {linkMaxCount} [Seven Code] trait Digimon card(s) from this Digimon's link cards to place as material ({remaining} remaining).",
                                maxCount: linkMaxCount,
                                canEndNotMax: true,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.LinkedCards,
                                customRootCardList: linkPool,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            IEnumerator AfterSelectLinkCardCoroutine(List<CardSource> cardSources)
                            {
                                selectedLinkCards.AddRange(cardSources);
                                yield return null;
                            }

                            yield return ContinuousController.instance.StartCoroutine(selectLinkCardEffect.Activate());

                            if (selectedLinkCards.Count == 0) continue;

                            materialCards.AddRange(selectedLinkCards);
                            remaining -= selectedLinkCards.Count;
                        }
                        else
                        {
                            List<CardSource> trashPool = card.Owner.TrashCards.Filter(CanSelectLinkedOrTrashCondition);

                            List<CardSource> selectedTrashCards = new List<CardSource>();

                            int trashMaxCount = Math.Min(remaining, trashPool.Count);

                            SelectCardEffect selectTrashCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectTrashCardEffect.SetUp(
                                canTargetCondition: CanSelectLinkedOrTrashCondition,
                                canTargetCondition_ByPreSelecetedList: (preSelected, cardSource) => !preSelected.Contains(cardSource),
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectTrashCardCoroutine,
                                message: $"Select up to {trashMaxCount} [Seven Code] trait Digimon card(s) from your trash to place as material ({remaining} remaining).",
                                maxCount: trashMaxCount,
                                canEndNotMax: true,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: trashPool,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            IEnumerator AfterSelectTrashCardCoroutine(List<CardSource> cardSources)
                            {
                                selectedTrashCards.AddRange(cardSources);
                                yield return null;
                            }

                            yield return ContinuousController.instance.StartCoroutine(selectTrashCardEffect.Activate());

                            if (selectedTrashCards.Count == 0) continue;

                            materialCards.AddRange(selectedTrashCards);
                            remaining -= selectedTrashCards.Count;
                        }
                    }

                    if (materialCards.Count == 6)
                    {
                        yield return ContinuousController.instance.StartCoroutine(selectedTarget.AddDigivolutionCardsBottom(materialCards, activateClass));

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            selectedTarget,
                            CanSelectDantemonCondition,
                            payCost: false,
                            reduceCostTuple: null,
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: 0,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: null,
                            ignoreRequirements: CardEffectCommons.IgnoreRequirement.All
                        ));
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 play cost 5 or lower [Appmon] card from hand or trash, then add this to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Security] You may play 1 play cost 5 or lower [Appmon] trait card from your hand or trash without paying the cost. Then, add this card to the hand.";

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.HasPlayCost && cardSource.GetCostItself <= 5 && cardSource.EqualsTraits("Appmon")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);

                bool CanUseCondition(Hashtable hashtable) => CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canPlayFromHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                    bool canPlayFromTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    bool shouldPlay = canPlayFromHand || canPlayFromTrash;
                    SelectCardEffect.Root root = canPlayFromTrash && !canPlayFromHand ? SelectCardEffect.Root.Trash : SelectCardEffect.Root.Hand;

                    if (canPlayFromHand && canPlayFromTrash)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new(message: "Play from hand", value: 1, spriteIndex: 0),
                            new(message: "Play from trash", value: 2, spriteIndex: 0),
                            new(message: "Don't play a card", value: 3, spriteIndex: 1),
                        };

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: "Will you play an [Appmon] card?", notSelectPlayerMessage: "The opponent is choosing whether to play an [Appmon] card.");
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        int selected = GManager.instance.userSelectionManager.SelectedIntValue;

                        shouldPlay = selected != 3;
                        root = selected == 1 ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                    }

                    if (shouldPlay)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                            canTargetCondition: CanSelectCardCondition,
                            root: root,
                            cardEffect: activateClass,
                            payCost: false));
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
