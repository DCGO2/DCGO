using System.Collections;
using System.Collections.Generic;
using System.Linq;

// KingSukamon
namespace DCGO.CardEffects.EX13
{
    public class EX13_031 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared OP/WD/OD
            string SharedEffectName()
                => "Trash 1 [Chuumon]/[Sukamon] in name from hand/sources to turn 1 enemy Digimon into Sukamon";

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                additionalActivateCondition: AdditionalActivateCondition,
                optional: false,
                isSkippable: true,
                onPlay: true,
                whenDigivolving: true,
                onDeletion: true);

            string SharedEffectDescription(string tag)
                => $"[{tag}] By trashing 1 card with [Chuumon] or [Sukamon] in its name from your hand or your Digimon's digivolution cards, you may change the base name, color and DP of 1 of your opponent's Digimon to [Sukamon], white and 3000 until their turn ends.";

            bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
            {
                return CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectTrashCardCondition)
                    || CardEffectCommons.HasMatchConditionPermanentDigivolutionCards(card, CanSelectTrashCardCondition);
            }

            bool CanSelectTrashCardCondition(CardSource cardSource)
            {
                return cardSource.ContainsCardName("Chuumon")
                    || cardSource.ContainsCardName("Sukamon");
            }

            bool CanSelectTrashPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.DigivolutionCards.Any(CanSelectTrashCardCondition);
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectTrashCardCondition);
                bool canSelectSource = CardEffectCommons.HasMatchConditionPermanentDigivolutionCards(card, CanSelectTrashCardCondition);

                if (canSelectHand || canSelectSource)
                {
                    bool selectedLocation = false;

                    if (canSelectHand && canSelectSource)
                    {
                        List<SelectionElement<int>> selectionElements1 = new List<SelectionElement<int>>()
                            {
                                new(message: "From hand", value: 1, spriteIndex: 0),
                                new(message: "From digivolution cards", value: 2, spriteIndex: 0),
                                new(message: "Don't play", value: 3, spriteIndex: 1),
                            };

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: "From which area will you select a card?", notSelectPlayerMessage: "The opponent is choosing from which area to select a card.");
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedIntValue == 3) yield break;

                        selectedLocation = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                    }
                    else
                    {
                        selectedLocation = canSelectHand;
                    }

                    if (selectedLocation)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectTrashCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return StartCoroutine(selectHandEffect.Activate());
                    }
                    else
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                            permanentCondition: CanSelectTrashPermanentCondition,
                            cardCondition: CanSelectTrashCardCondition,
                            maxCount: 1,
                            canNoTrash: false,
                            isFromOnly1Permanent: false,
                            activateClass: activateClass,
                            afterSelectionCoroutine: AfterTrashedCards
                        ));

                        IEnumerator AfterTrashedCards(Permanent permanent, List<CardSource> cards)
                        {
                            if (cards.Count > 0)
                            {
                                yield return ContinuousController.instance.StartCoroutine(SelectCardCoroutine(cards[0]));
                            }

                            yield return null;
                        }
                    }

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
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
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage(
                                "Select 1 Digimon that will get effects.",
                                "The opponent is selecting 1 Digimon that will get effects.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                Permanent selectedPermanent = permanent;

                                if (selectedPermanent != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeBaseDigimonDP(
                                        targetPermanent: permanent,
                                        changeValue: 3000,
                                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                        activateClass: activateClass));
                                }

                                if (selectedPermanent != null)
                                {
                                    ChangeBaseCardNameClass changeBaseCardNameClass = new ChangeBaseCardNameClass();
                                    changeBaseCardNameClass.SetUpICardEffect("Original card name is [Sukamon]", CanUseCondition1, card);
                                    changeBaseCardNameClass.SetUpChangeBaseCardNamesClass(changeBaseCardNames: ChangeBaseCardNames);
                                    selectedPermanent.UntilOwnerTurnEndEffects.Add((_timing) => changeBaseCardNameClass);

                                    bool CanUseCondition1(Hashtable hashtable)
                                    {
                                        return selectedPermanent.TopCard != null
                                            && !selectedPermanent.TopCard.CanNotBeAffected(activateClass);
                                    }

                                    List<string> ChangeBaseCardNames(CardSource cardSource, List<string> CardNames)
                                    {
                                        if (cardSource == selectedPermanent.TopCard)
                                        {
                                            CardNames = new List<string>() { "Sukamon" };
                                        }

                                        return CardNames;
                                    }
                                }

                                if (selectedPermanent != null)
                                {
                                    ChangeBaseCardColorClass changeBaseCardNameClass = new ChangeBaseCardColorClass();
                                    changeBaseCardNameClass.SetUpICardEffect("Original card color is white", CanUseCondition1, card);
                                    changeBaseCardNameClass.SetUpChangeBaseCardColorClass(ChangeBaseCardColors: ChangeBaseCardColors);
                                    selectedPermanent.UntilOwnerTurnEndEffects.Add((_timing) => changeBaseCardNameClass);

                                    bool CanUseCondition1(Hashtable hashtable)
                                    {
                                        return selectedPermanent.TopCard != null
                                            && !selectedPermanent.TopCard.CanNotBeAffected(activateClass);
                                    }

                                    List<CardColor> ChangeBaseCardColors(CardSource cardSource, List<CardColor> CardColors)
                                    {
                                        if (cardSource == selectedPermanent.TopCard)
                                        {
                                            CardColors = new List<CardColor>() { CardColor.White };
                                        }

                                        return CardColors;
                                    }
                                }
                            }
                        }

                        yield return null;
                    }
                }
            }
            #endregion

            #region Assembly
            if (timing == EffectTiming.None)
            {
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect("Assembly", CanUseCondition, card);
                addAssemblyConditionClass.SetUpAddAssemblyConditionClass(getAssemblyCondition: GetAssembly);
                addAssemblyConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAssemblyConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                    => true;

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        AssemblyConditionElement element = new AssemblyConditionElement(CanSelectCardCondition);

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            return cardSource != null
                                && cardSource.Owner == card.Owner
                                && cardSource.IsDigimon
                                && cardSource.HasLevel
                                && cardSource.Level <= 4
                                && cardSource.ContainsCardName("Sukamon");
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            element: element,
                            CanTargetCondition_ByPreSelecetedList: null,
                            selectMessage: "3 Lv.4 or lower Digimon cards w/[Sukamon] in name",
                            elementCount: 3,
                            reduceCost: 4);

                        return assemblyCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Inherited All Turns
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal top 3, play 1 3 cost or lower [Chuumon]/[Sukamon] in name Digimon for free, trash rest", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When any other Digimon with [Sukamon] in their names are deleted, reveal the top 3 cards of your deck. You may play 1 play cost 3 or lower Digimon card with [Chuumon] or [Sukamon] in its name among them without paying the cost. Trash the rest.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermanentCondition, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);

                bool PermanentCondition(Permanent permanent)
                    => permanent.IsDigimon
                        && permanent.TopCard.ContainsCardName("Sukamon")
                        && permanent != card.PermanentOfThisCard();

                bool CanPlayCondition(CardSource cardSource)
                {
                    return cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 3
                        && (cardSource.ContainsCardName("Chuumon")
                            || cardSource.ContainsCardName("Sukamon"))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedCard = null;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition:CanPlayCondition,
                                message: "Select 1 Digimon card to Play.",
                                mode: SelectCardEffect.Mode.Custom,
                                maxCount: 1,
                                selectCardCoroutine: SelectCardCoroutine),
                        },
                        remainingCardsPlace: RemainingCardsPlace.Trash,
                        activateClass: activateClass,
                        canNoSelect: true
                    ));

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        new List<CardSource>() { selectedCard },
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Library,
                        activateETB: true));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
