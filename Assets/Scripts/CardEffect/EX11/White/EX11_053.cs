using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Omekamon
namespace DCGO.CardEffects.EX11
{
    public class EX11_053 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Name Rule
            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("Also treated as [X Antibody]", CanUseCondition, card);
                changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: changeCardNames);
                cardEffects.Add(changeCardNamesClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                List<string> changeCardNames(CardSource cardSource, List<string> CardNames)
                {
                    if (cardSource == card)
                    {
                        CardNames.Add("X Antibody");
                    }

                    return CardNames;
                }
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing a [Royal Knight] under any of your [King Drasil_7D6]s, Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Play] By placing 1 [Royal Knight] trait Digimon card from your hand as the bottom digivolution card of any of your [King Drasil_7D6]s on the field, <Draw 1>.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) 
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition)
                        && (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanentCondition)
                            || CardEffectCommons.HasMatchConditionOwnersBreedingPermanent(card, CanSelectPermanentCondition));
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.EqualsTraits("Royal Knight");
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                            || CardEffectCommons.IsPermanentExistsOnOwnerBreedingArea(permanent, card))
                        && permanent.IsDigimon
                        && permanent.TopCard.EqualsCardName("King Drasil_7D6");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectCardCondition,
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

                    selectHandEffect.SetUpCustomMessage("Select 1 Royal Knight to place under a King Drasil_7D6.", "The opponent is selecting 1 card to place.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");

                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    if (selectedCards.Count > 0)
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

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to add the selected card under.", "The opponent is selecting 1 Digimon to add the selected card under.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(selectedCards, activateClass));

                            yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                        }
                    }
                }
            }
            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Omnimon (X Antibody)] and place this card under it.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Deletion] If you have 1 of fewer security cards, you may 1 play [Omnimon (X Antibody)] from your hand or under your [King Drasil_7D6]s on the field without paying the cost. Then, place this card as the played Digimon's bottom digivolution card.";

                bool CanUseCondition(Hashtable hashtable) => CardEffectCommons.CanTriggerOnDeletion(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card)
                        && card.Owner.SecurityCards.Count <= 1
                        && (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition)
                            || CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanentCondition)
                            || CardEffectCommons.HasMatchConditionOwnersBreedingPermanent(card, CanSelectPermanentCondition));
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.HasPlayCost
                        && cardSource.EqualsCardName("Omnimon (X Antibody)")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                            || CardEffectCommons.IsPermanentExistsOnOwnerBreedingArea(permanent, card))
                        && permanent.TopCard.EqualsCardName("King Drasil_7D6")
                        && permanent.DigivolutionCards.Any(CanSelectCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool IsValidHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                    bool IsValidField = CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanentCondition)
                            || CardEffectCommons.HasMatchConditionOwnersBreedingPermanent(card, CanSelectPermanentCondition);

                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                    if (IsValidHand)
                    {
                        selectionElements.Add(new(message: "From hand", value: 0, spriteIndex: 0));
                    }
                    if (IsValidField)
                    {
                        selectionElements.Add(new(message: "From digivolution cards", value: 1, spriteIndex: 0));
                    }
                    selectionElements.Add(new(message: "Do not play", value: 2, spriteIndex: 1));

                    string selectPlayerMessage = "From which area will you play a Omnimon (X Antibody)?";
                    string notSelectPlayerMessage = "The opponent is choosing if they will activate their effect.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements,
                        selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                        notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                        .WaitForEndSelect());

                    int selection = GManager.instance.userSelectionManager.SelectedIntValue;

                    if (selection != 2)
                    {
                        CardSource selectedCard = null;

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;

                            yield return null;
                        }

                        if (selection == 0)
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCardCondition,
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

                            selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                            yield return StartCoroutine(selectHandEffect.Activate());

                            if (selectedCard != null)
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
                        else
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

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to play a card from.", "The opponent is selecting 1 Digimon to play a card from.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;

                                yield return null;
                            }

                            if (selectedPermanent != null)
                            {
                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 card to play.",
                                    maxCount: 1,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: selectedPermanent.DigivolutionCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                if (selectedCard != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                        new List<CardSource>() { selectedCard },
                                        activateClass: activateClass,
                                        payCost: false,
                                        isTapped: false,
                                        root: SelectCardEffect.Root.DigivolutionCards,
                                        activateETB: true));
                                }
                            }
                        }

                        if (selectedCard != null)
                        {
                             yield return ContinuousController.instance.StartCoroutine(
                                selectedCard.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { card }, activateClass));
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
