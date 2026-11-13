using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Jesmon (X Antibody) / Ultimate Seibaken
namespace DCGO.CardEffects.BT27
{
    public class BT27_016 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Side
            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Jesmon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 1, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Progress
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ProgressSelfStaticEffect(false, card, null));
            }
            #endregion

            #region Pierce
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(false, card, null));
            }
            #endregion

            #region Engage
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.EngageSelfStaticEffect(false, card, null));
            }
            #endregion

            #region Shared WD/WA
            string SharedEffectName = "Activate 1 [Main] effect on this card's Option side";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    maxCountPerTurn: 1,
                    hashValue: "BT27_016_WD_WA",
                    whenDigivolving: true,
                    whenAttacking: true);

            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] This Digimon gets +6000 DP until your opponent's turn ends. Then, by returning 1 of your other Digimon or 1 [Jesmon] or [X Antibody] from this Digimon's digivolution cards to the bottom of the deck, activate 1 [Main] effect on this card's Option side.";

            bool OtherDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent != card.PermanentOfThisCard();

            bool SourceCondition(CardSource cardSource)
                => cardSource.EqualsCardName("Jesmon") || cardSource.EqualsCardName("X Antibody");

            bool HasSource()
                => card.PermanentOfThisCard() != null
                    && card.PermanentOfThisCard().DigivolutionCards.Any(SourceCondition);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                Permanent thisPermanent = card.PermanentOfThisCard();

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: thisPermanent, changeValue: 6000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));

                bool canReturnDigimon = CardEffectCommons.HasMatchConditionPermanent(OtherDigimonCondition);
                bool canReturnSource = HasSource();

                int answer = canReturnDigimon ? 1 : 2;

                if (canReturnDigimon && canReturnSource)
                {
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>
                        {
                            new(message: "Return 1 of your other Digimon", value: 1, spriteIndex: 0),
                            new(message: "Return 1 digivolution card", value: 2, spriteIndex: 0),
                            new(message: "Don't return", value: 0, spriteIndex: 1)
                        };

                    GManager.instance.userSelectionManager.SetIntSelection(
                        selectionElements: selectionElements,
                        selectPlayer: card.Owner,
                        selectPlayerMessage: "Which card will you return to the bottom of the deck?",
                        notSelectPlayerMessage: "The opponent is choosing which card to return to the bottom of the deck.");

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    answer = GManager.instance.userSelectionManager.SelectedIntValue;
                }

                bool returned = false;

                if (answer == 1)
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: OtherDigimonCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 of your other Digimon to return to the bottom of the deck.", "The opponent is selecting 1 Digimon to return to the bottom of the deck.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeckBouncePeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent>() { selectedPermanent },
                            activateClass: activateClass,
                            successProcess: SuccessProcess(),
                            failureProcess: null));

                        IEnumerator SuccessProcess()
                        {
                            returned = true;
                            yield return null;
                        }
                    }
                }
                else if (answer == 2 && thisPermanent != null)
                {
                    CardSource selectedCard = null;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: SourceCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 [Gankoomon] or [X Antibody] to return to the bottom of the deck.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Custom,
                        customRootCardList: thisPermanent.DigivolutionCards,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 [Gankoomon] or [X Antibody] to return to the bottom of the deck.", "The opponent is selecting 1 digivolution card to return to the bottom of the deck.");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (selectedCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new ReturnToLibraryBottomDigivolutionCardsClass(
                            thisPermanent,
                            new List<CardSource>() { selectedCard },
                            CardEffectCommons.CardEffectHashtable(activateClass)).ReturnToLibraryBottomDigivolutionCards());

                        returned = !thisPermanent.DigivolutionCards.Contains(selectedCard);
                    }
                }

                if (returned && thisPermanent != null)
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ActivateMainOfOptionSide(card, activateClass));
            }
            #endregion
            #endregion

            #region Option Side
            #region Use Req
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, cardSource => cardSource.HasText("Huckmon")));
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 enemy lowest DP as bottom sec to trash their top sec", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] By placing 1 of your opponent's lowest DP Digimon as the bottom security card, trash their top security card.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy);


                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool returned = false;

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to return to the bottom of security.", "The opponent is selecting 1 Digimon to return to the bottom of security.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null && !selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlacePermanentInSecurityAndProcessAccordingToResult(
                                targetPermanent: selectedPermanent,
                                activateClass: activateClass,
                                toTop: false,
                                SuccessProcess
                            ));

                            IEnumerator SuccessProcess(CardSource cardSource)
                            {
                                returned = true;
                                yield return null;
                            }
                        }
                    }

                    if (returned)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                    }
                }
            }
            #endregion

            #region Arts Digivolution
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            }
            #endregion
            #endregion

            return cardEffects;
        }
    }
}
