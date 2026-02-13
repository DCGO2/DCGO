using System.Collections;
using System.Collections.Generic;

// Mulemon
namespace DCGO.CardEffects.EX11
{
    public class EX11_040 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Maquinamon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP / WD
            string SharedEffectName = "Link [Maquinamon] from hand or digivolution cards for free.";

            string SharedEffectDescription(string tag) => $"[{tag}] You may link 1 [Maquinamon] from your hand or this Digimon's digivolution cards to 1 of your Digimon without paying the cost.";

            bool SharedCanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleAreaDigimon(card);

            bool IsLinkableMaquinamonCard(CardSource cardSource)
            {
                return cardSource.EqualsCardName("Maquinamon")
                    && cardSource.CanLink(false);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool validHandCard = CardEffectCommons.HasMatchConditionOwnersHand(card, IsLinkableMaquinamonCard);
                bool validDigivolutionCard = card.PermanentOfThisCard().DigivolutionCards.Some(IsLinkableMaquinamonCard);

                if (validHandCard || validDigivolutionCard)
                {
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                    int index = 0;
                    if (validHandCard)
                    {
                        selectionElements.Add(new (message: $"Link Maquinamon from hand", value : 1, spriteIndex: index));
                        index++;
                    }
                    if (validDigivolutionCard)
                    {
                        selectionElements.Add(new (message: $"Link Maquinamon from this Digimon's Digivolution Cards", value : 2, spriteIndex: index));
                        index++;
                    };
                    selectionElements.Add(new (message: $"Don't link", value: 3, spriteIndex: index));

                    string selectPlayerMessage = "Will you link a Maquinamon?";
                    string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool doLink = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                    bool fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;

                    if (doLink)
                    {
                        CardSource selectedCard = null;

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;

                            yield return null;
                        }

                        if (fromHand)
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsLinkableMaquinamonCard,
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

                            yield return StartCoroutine(selectHandEffect.Activate());
                        }
                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                        canTargetCondition: IsLinkableMaquinamonCard,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => true,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select 1 card to link.",
                                        maxCount: 1,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Custom,
                                        customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 card to link.", "The opponent is selecting 1 card to link.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Linked Cards");

                            yield return StartCoroutine(selectCardEffect.Activate());
                        }

                        if (selectedCard != null)
                        {
                            bool CanSelectDigimon(Permanent permanent)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                                    return selectedCard.CanLinkToTargetPermanent(permanent, false);

                                return false;
                            }

                            if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectDigimon))
                            {
                                Permanent selectedPermanent = null;
                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectDigimon,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to add link", "The opponent is selecting 1 digimon to add link");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    selectedPermanent = permanent;
                                    yield return null;
                                }

                                if (selectedPermanent != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddLinkCard(selectedCard, activateClass));
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Unchained] form hand or trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("EX11_040_YT");
                cardEffects.Add(activateClass);

                string EffectDescription() => "[Your Turn] [Once Per Turn] When this Digimon gets linked, if you have 1 or fewer Tamers, you may play 1 [Unchained] from your hand or trash without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenLinked(hashtable, PermanentCondition, null);
                }

                bool PermanentCondition(Permanent permanent) => permanent == card.PermanentOfThisCard();

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.MatchConditionOwnersPermanentCount(card, permanent => permanent.IsTamer) <= 1
                        && (CardEffectCommons.HasMatchConditionOwnersHand(card, IsUnchained)
                            || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsUnchained));
                }

                bool IsUnchained(CardSource cardSource) => cardSource.EqualsCardName("Unchained");

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool isValidHand = CardEffectCommons.HasMatchConditionOwnersHand(card, IsUnchained);
                    bool isValidTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsUnchained);

                    if (isValidHand && isValidTrash)
                    {
                        List<SelectionElement<bool>> selectionElements1 = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage1 = "From which area do you select a card?";
                        string notSelectPlayerMessage1 = "The opponent is choosing from which area to select a card.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage1, notSelectPlayerMessage: notSelectPlayerMessage1);
                    }
                    else
                    {
                        GManager.instance.userSelectionManager.SetBool(isValidHand);
                    }

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                    List<CardSource> selectedCards = new List<CardSource>();

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    if(fromHand)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsUnchained,
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

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                        if (selectedCards.Count > 0)
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Hand, activateETB: true));
                    }
                    else
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: IsUnchained,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to play",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to play", "The opponent is selecting 1 card to play");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        if (selectedCards.Count > 0)
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Trash, activateETB: true));
                    }      
                }
            }
            #endregion

            #region ESS - Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}