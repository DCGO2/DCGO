using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Close
namespace DCGO.CardEffects.EX11
{
    public class EX11_065 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared Conditions

            bool HasMineralOrRock(CardSource source)
            {
                return source.EqualsTraits("Mineral")
                        || source.EqualsTraits("Rock");
            }

            #endregion

            #region Start of Your Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 to gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] By trashing 1 [Mineral] or [Rock] trait card from your hand or your Digimon's digivolution cards, gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && (CardEffectCommons.HasMatchConditionOwnersHand(card, HasMineralOrRock)
                            || CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectTrashTargetCondition));
                }
                
                bool CanSelectTrashTargetCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.DigivolutionCards.Count(HasMineralOrRock) >= 1;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    #region Setup Location Selection

                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, HasMineralOrRock);
                    bool canSelectDigivolutionSource = CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectTrashTargetCondition);
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                    if (canSelectHand)
                    {
                        selectionElements.Add(new(message: "From hand", value: 0, spriteIndex: 0));
                    }
                    if (canSelectDigivolutionSource)
                    {
                        selectionElements.Add(new(message: "From digivolution cards", value: 1, spriteIndex: 0));
                    }
                    selectionElements.Add(new(message: "Do not trash", value: 2, spriteIndex: 1));

                    string selectPlayerMessage = "From which area will you trash a card?";
                    string notSelectPlayerMessage = "The opponent is choosing if they will activate their effect.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements,
                        selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                        notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                        .WaitForEndSelect());

                    int selection = GManager.instance.userSelectionManager.SelectedIntValue;

                    #endregion

                    if (selection != 2)
                    {
                        bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                        bool discarded = false;

                        if (selection == 0)
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: HasMineralOrRock,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                mode: SelectHandEffect.Mode.Discard,
                                cardEffect: activateClass);

                            yield return StartCoroutine(selectHandEffect.Activate());

                            IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                            {
                                if (cardSources.Count >= 1)
                                {
                                    discarded = true;

                                    yield return null;
                                }
                            }
                        }
                        else
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                                permanentCondition: CanSelectTrashTargetCondition,
                                cardCondition: HasMineralOrRock,
                                maxCount: 1,
                                canNoTrash: true,
                                isFromOnly1Permanent: false,
                                activateClass: activateClass,
                                afterSelectionCoroutine: AfterTrashedCards
                            ));

                            IEnumerator AfterTrashedCards(Permanent permanent, List<CardSource> cards)
                            {
                                if (cards.Count >= 1)
                                {
                                    discarded = true;

                                    yield return null;
                                }
                            }
                        }

                        if (discarded)
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                        }
                    }
                }
            }

            #endregion

            #region All Turns
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 [Mineral]/[Rock] trait card from hand or trash under played/digivolved digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription() => "[All Turns] When your Digimon are played or digivolved, if any of them have the [Mineral] or [Rock] trait, by suspending this Tamer, you may place 1 [Mineral] or [Rock] trait card from your hand or trash as any of those Digimon's bottom digivolution card.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition)
                            || CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.EqualsTraits("Mineral")
                            || permanent.TopCard.EqualsTraits("Rock"));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent> { card.PermanentOfThisCard() }, hashtable).Tap());

                    #region Select Played/Digivolved Permanent

                    List<Permanent> playedPermanents = new List<Permanent>();

                    foreach (Hashtable hash in CardEffectCommons.GetHashtablesFromHashtable(hashtable))
                    {
                        playedPermanents.Add(CardEffectCommons.GetPermanentFromHashtable(hash));
                    }

                    List<Permanent> targetPermanents = playedPermanents.Filter(PermanentCondition);

                    Permanent selectedPermament = null;
                    if (targetPermanents.Count > 1)
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: permanent => targetPermanents.Contains(permanent),
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermament = permanent;
                            yield return null;
                        }
                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get the digivolution cards.", "The opponent is selecting 1 Digimon that will get the digivolution cards.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                    else selectedPermament = targetPermanents[0];

                    #endregion

                    if (selectedPermament != null)
                    {
                        #region Setup Location Selection

                        bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, HasMineralOrRock);
                        bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, HasMineralOrRock);
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                        if (canSelectHand)
                        {
                            selectionElements.Add(new(message: "From hand", value: 0, spriteIndex: 0));
                        }
                        if (canSelectTrash)
                        {
                            selectionElements.Add(new(message: "From trash", value: 1, spriteIndex: 0));
                        }
                        selectionElements.Add(new(message: "Do not add", value: 2, spriteIndex: 1));

                        string selectPlayerMessage = "From which area will you select a card?";
                        string notSelectPlayerMessage = "The opponent is choosing if they will activate their effect.";

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements,
                            selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                            notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                            .WaitForEndSelect());

                        int selection = GManager.instance.userSelectionManager.SelectedIntValue;

                        #endregion

                        if (selection != 2)
                        {
                            if (selection == 0)
                            {
                                List<CardSource> selectedCards = new List<CardSource>();

                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: HasMineralOrRock,
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

                                selectHandEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.",
                                    "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                                yield return StartCoroutine(selectHandEffect.Activate());

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCards.Add(cardSource);

                                    yield return null;
                                }

                                if (selectedCards.Count >= 1)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(selectedPermament.AddDigivolutionCardsBottom(selectedCards, activateClass));
                                }
                            }
                            else
                            {
                                List<CardSource> selectedCards = new List<CardSource>();

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: HasMineralOrRock,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 card to place on bottom of digivolution cards.",
                                    maxCount: 1,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");
                                selectCardEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCards.Add(cardSource);

                                    yield return null;
                                }

                                if (selectedCards.Count >= 1)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(selectedPermament.AddDigivolutionCardsBottom(selectedCards, activateClass));
                                }
                            }
                        }
                    }
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
