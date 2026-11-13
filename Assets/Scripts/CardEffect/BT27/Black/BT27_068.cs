using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Gankoomon (X Antibody) / Ultimate Sengi Tengyouken
namespace DCGO.CardEffects.BT27
{
    public class BT27_068 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Side
            #region Alternate Digivolution Requirement - [Gankoomon]
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Gankoomon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Collision
            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.CollisionSelfStaticEffect(false, card, null));
            }
            #endregion

            #region Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region All Turns - Would leave the battle area
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By returning 1 other Digimon or 1 [Gankoomon]/[X Antibody] source to the deck bottom, this Digimon doesn't leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] When this Digimon would leave the battle area, by returning 1 of your other Digimon or 1 [Gankoomon] or [X Antibody] from this Digimon's digivolution cards to the bottom of the deck, it doesn't leave.";

                bool OtherDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent != card.PermanentOfThisCard()
                        && !permanent.willBeRemoveField;

                bool SourceCondition(CardSource cardSource)
                    => cardSource.EqualsCardName("Gankoomon") || cardSource.EqualsCardName("X Antibody");

                bool HasSource()
                    => card.PermanentOfThisCard() != null
                        && card.PermanentOfThisCard().DigivolutionCards.Any(SourceCondition);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && (CardEffectCommons.HasMatchConditionPermanent(OtherDigimonCondition) || HasSource());

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();

                    bool canReturnDigimon = CardEffectCommons.HasMatchConditionPermanent(OtherDigimonCondition);
                    bool canReturnSource = HasSource();

                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                    if (canReturnDigimon) selectionElements.Add(new(message: "Return 1 of your other Digimon", value: 1, spriteIndex: 0));
                    if (canReturnSource) selectionElements.Add(new(message: "Return 1 digivolution card", value: 2, spriteIndex: 0));
                    selectionElements.Add(new(message: "Don't return", value: 0, spriteIndex: 1));

                    GManager.instance.userSelectionManager.SetIntSelection(
                        selectionElements: selectionElements,
                        selectPlayer: card.Owner,
                        selectPlayerMessage: "Which card will you return to the bottom of the deck so this Digimon doesn't leave?",
                        notSelectPlayerMessage: "The opponent is choosing which card to return to the bottom of the deck.");

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    int answer = GManager.instance.userSelectionManager.SelectedIntValue;
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
                            yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(
                                new List<Permanent>() { selectedPermanent },
                                CardEffectCommons.CardEffectHashtable(activateClass)).DeckBounce());

                            returned = !CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent);
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
                    {
                        thisPermanent.willBeRemoveField = false;
                        thisPermanent.HideHandBounceEffect();
                        thisPermanent.HideDeckBounceEffect();
                        thisPermanent.HideDeleteEffect();
                        thisPermanent.HideWillRemoveFieldEffect();
                    }
                }
            }
            #endregion

            #region All Turns - OPT - Your Digimon leave
            if (timing == EffectTiming.OnLeaveFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Activate 1 [Main] effect on this card's Option side", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT27_068_ActivateOption");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When any of your Digimon leave the battle area, you may activate 1 [Main] effect on this card's Option side.";

                bool OwnDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentLeave(hashtable, OwnDigimonCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ActivateMainOfOptionSide(card, activateClass));
                }
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
                activateClass.SetUpICardEffect("Return 1 opponent's 12000 DP+ Digimon to the deck top, if not, delete 1 of their lowest play cost Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] Return 1 of your opponent's 12000 DP or higher Digimon to the top of the deck. If this effect didn't return, delete 1 of your opponent's lowest play cost Digimon.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectReturnCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.HasDP
                        && permanent.DP >= 12000;

                bool CanSelectDeleteCondition(Permanent permanent)
                    => CardEffectCommons.IsMinCost(permanent, card.Owner.Enemy, true);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool returned = false;

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectReturnCondition))
                    {
                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectReturnCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to return to the top of the deck.", "The opponent is selecting 1 Digimon to return to the top of the deck.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new DeckTopBounceClass(
                                new List<Permanent>() { selectedPermanent },
                                CardEffectCommons.CardEffectHashtable(activateClass)).DeckBounce());

                            returned = !CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent);
                        }
                    }

                    if (returned || !CardEffectCommons.HasMatchConditionPermanent(CanSelectDeleteCondition)) yield break;

                    SelectPermanentEffect deleteSelectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    deleteSelectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectDeleteCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    deleteSelectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(deleteSelectPermanentEffect.Activate());
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
