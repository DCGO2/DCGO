using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Proximamon
namespace DCGO.CardEffects.EX12
{
    public class EX12_077 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasText("Gammamon")
                        && targetPermanent.TopCard.EqualsTraits("VB");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 5,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 6)
                );
            }
            #endregion

            #region DNA Digivolution
            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect("DNA Digivolution", CanUseCondition, card);
                addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
                addJogressConditionClass.SetNotShowUI(true);
                cardEffects.Add(addJogressConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                JogressCondition GetJogress(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        bool PermanentCondition1(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                && (permanent.TopCard.CardColors.Contains(CardColor.Red)
                                    || permanent.TopCard.CardColors.Contains(CardColor.Blue))
                                && permanent.Levels_ForJogress(card).Contains(6);
                        }

                        bool PermanentCondition2(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                && (permanent.TopCard.CardColors.Contains(CardColor.Black)
                                    || permanent.TopCard.CardColors.Contains(CardColor.Purple))
                                && permanent.Levels_ForJogress(card).Contains(6);
                        }

                        JogressConditionElement[] elements = new JogressConditionElement[]
                        {
                        new JogressConditionElement(PermanentCondition1, "a level 6 Red or Blue Digimon"),

                        new JogressConditionElement(PermanentCondition2, "a level 6 Black or Purple Digimon"),
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Security A. +1
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName1 = "Place 2 cards with [Gammamon] text/[VB] under 1 of your Digimon to delete 1 enemy Digimon/Tamer";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName1,
                SharedActivateCoroutine1,
                SharedEffectDescription1,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            string SharedEffectDescription1(string tag)
            {
                return $"[{tag}] By placing 2 cards with [Gammamon] in their texts or the [VB] trait from your hand or trash as 1 of your Digimon's top or bottom digivolution cards, delete 1 of your opponent's Digimon or Tamers.";
            }

            bool CanSelectCardCondition1(CardSource cardSource)
            {
                return cardSource.HasText("Gammamon")
                    || cardSource.EqualsTraits("VB");
            }

            bool CanSelectPermanentPlaceCondition1(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
            }

            bool CanSelectPermanentDeletionCondition1(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon
                        || permanent.IsTamer);
            }

            IEnumerator SharedActivateCoroutine1(Hashtable hashtable, ActivateClass activateClass)
            {
                if (card.Owner.HandCards.Count(CanSelectCardCondition1) + card.Owner.TrashCards.Count(CanSelectCardCondition1) >= 2)
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentPlaceCondition1,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: selectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass
                        );
                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to place sources under.", "The opponent is selecting 1 Digimon to place sources under.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator selectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        int toSelect = 2;
                        List<CardSource> selectedCards = new List<CardSource>();

                        while (toSelect > 0)
                        {
                            bool CanSelectFilteredCardCondtion(CardSource cardSource)
                            {
                                return CanSelectCardCondition1(cardSource)
                                    && !selectedCards.Contains(cardSource);
                            }

                            int validHandCardCount = card.Owner.HandCards.Count(CanSelectFilteredCardCondtion);
                            int validTrashCardCount = card.Owner.TrashCards.Count(CanSelectFilteredCardCondtion);

                            List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                            if (validHandCardCount > 0)
                            {
                                selectionElements.Add(new(message: "from Hand", value: 1, spriteIndex: 0));
                            }
                            if (validTrashCardCount > 0)
                            {
                                selectionElements.Add(new(message: "from Trash", value: 2, spriteIndex: 0));
                            }
                            selectionElements.Add(new(message: "Do not place", value: 3, spriteIndex: 1));

                            GManager.instance.userSelectionManager.SetIntSelection(
                                selectionElements: selectionElements,
                                selectPlayer: card.Owner,
                                selectPlayerMessage: "From which area will you select a card?",
                                notSelectPlayerMessage: "The opponent is choosing from which area to select card.");

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            if (GManager.instance.userSelectionManager.SelectedIntValue == 3)
                            {
                                break;
                            }
                            if (GManager.instance.userSelectionManager.SelectedIntValue == 1)
                            {
                                int maxCount = Math.Min(toSelect, card.Owner.HandCards.Count(CanSelectCardCondition1));
                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectFilteredCardCondtion,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: true,
                                    canEndNotMax: true,
                                    isShowOpponent: true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                string messagePluralize = maxCount > 1 ? "Select one or more cards to place under this Digimon. You will be able to select a second card if you only choose 1." : "Select a card to place under this Digimon.";

                                selectHandEffect.SetUpCustomMessage(
                                    messagePluralize,
                                    $"The opponent is selecting cards to place.");

                                yield return StartCoroutine(selectHandEffect.Activate());
                            }
                            else
                            {
                                int maxCount = Math.Min(toSelect, card.Owner.TrashCards.Count(CanSelectCardCondition1));
                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectFilteredCardCondtion,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select link card(s)",
                                    maxCount: maxCount,
                                    canEndNotMax: true,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                string messagePluralize = maxCount > 1 ? "Select one or more cards to place under this Digimon. You will be able to select a second card if you only choose 1." : "Select a card to place under this Digimon.";

                                selectCardEffect.SetUpCustomMessage(
                                    messagePluralize,
                                    $"The opponent is selecting cards to place.");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                            }

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                if (cardSource != null)
                                {
                                    selectedCards.Add(cardSource);
                                    toSelect--;
                                }

                                yield return null;
                            }
                        }

                        if (selectedCards.Count == 2)
                        {

                            List<SelectionElement<bool>> selectionElements1 = new List<SelectionElement<bool>>()
                            {
                                new SelectionElement<bool>(message: $"To Top", value : true, spriteIndex: 0),
                                new SelectionElement<bool>(message: $"To Bottom", value : false, spriteIndex: 1),
                            };

                            string selectPlayerMessage1 = "What order do you place the cards?";
                            string notSelectPlayerMessage1 = "The opponent is choosing what order to place the cards.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage1, notSelectPlayerMessage: notSelectPlayerMessage1);

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            List<CardSource> fixedCards = new List<CardSource>();

                            bool ToTop = GManager.instance.userSelectionManager.SelectedBoolValue;

                            selectCardEffect.SetUp(
                                canTargetCondition: _ => true,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => false,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                message: "Specify the order to place the cards in the digivolution cards\n(cards will be placed so that cards with lower numbers are on top).",
                                maxCount: selectedCards.Count,
                                canEndNotMax: false,
                                isShowOpponent: false,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: selectedCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Cards");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                            {
                                fixedCards = cardSources.Clone();
                                fixedCards.Reverse();

                                if (ToTop)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsTop(fixedCards, activateClass));
                                }
                                else
                                {
                                    yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(fixedCards, activateClass));
                                }

                                yield return null;
                            }

                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentDeletionCondition1))
                            {
                                SelectPermanentEffect selectPermanentEffect2 = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect2.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentDeletionCondition1,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Destroy,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect2.Activate());
                            }
                        }
                    }
                }
            }
            #endregion

            #region Shared OP/WD/WA/C
            string SharedEffectName2 = "May play/use 1 10 cost or lower [Gammamon] text/[VB] from sources for free";

            string SharedEffectHash2 = "EX12_077_OP_WD_WA_C";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName2,
                SharedActivateCoroutine2,
                SharedEffectDescription2,
                additionalActivateCondition: AdditionalActivateCondition2,
                optional: false,
                isSkippable: true,
                maxCountPerTurn: 1,
                hashValue: SharedEffectHash2,
                onPlay: true,
                whenDigivolving: true,
                whenAttacking: true,
                counter: true);

            string SharedEffectDescription2(string tag)
            {
                return $"[{tag}] [Once Per Turn] You may play or use 1 play or use cost 10 or lower card with [Gammamon] in its text or the [VB] trait from any of your Digimon's digivolution cards without paying the cost.";
            }

            bool AdditionalActivateCondition2(Hashtable hashtable, ActivateClass activateClass)
            {
                return CardEffectCommons.HasMatchConditionPermanent(permanent => CanSelectPermanentCondition2(permanent, activateClass));
            }

            bool CanSelectCardCondition2(CardSource cardSource, ActivateClass activateClass)
            {
                return (cardSource.HasText("Gammamon")
                        || cardSource.EqualsTraits("VB"))
                    && cardSource.GetCostItself <= 10
                    && ((cardSource.IsOption
                        && !cardSource.CanNotPlayThisOption)
                    || (cardSource.HasPlayCost
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass)));
            }

            bool CanSelectPermanentCondition2(Permanent permanent, ActivateClass activateClass)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.DigivolutionCards.Some(cardSource => CanSelectCardCondition2(cardSource, activateClass));
            }

            IEnumerator SharedActivateCoroutine2(Hashtable hashtable, ActivateClass activateClass)
            {
                Permanent selectedPermanent = null;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: permanent => CanSelectPermanentCondition2(permanent, activateClass),
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: selectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass
                    );
                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to play/use a source from.", "The opponent is selecting 1 Digimon to play/use a source from.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator selectPermanentCoroutine(Permanent permanent)
                {
                    selectedPermanent = permanent;
                    yield return null;
                }

                if (selectedPermanent != null)
                {
                    CardSource selectedCard = null;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: cardSource => CanSelectCardCondition2(cardSource, activateClass),
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 card to play/use",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.DigivolutionCards,
                        customRootCardList: selectedPermanent.DigivolutionCards,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 card to play/use.", "The opponent is selecting 1 card to play/use.");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (selectedCard != null)
                    {
                        if (selectedCard.IsOption)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                                cardSources: new List<CardSource>() { selectedCard },
                                activateClass: activateClass,
                                payCost: false,
                                root: SelectCardEffect.Root.Hand));
                        }
                        else
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                new List<CardSource>() { selectedCard },
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.Hand,
                                activateETB: true));
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
