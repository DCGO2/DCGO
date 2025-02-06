using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX7
{
    public class EX7_043 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel3 && targetPermanent.TopCard.HasText("Three Musketeers");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region On Play/ When Digivolving Shared

            bool CanSelectCardSharedCondition(CardSource cardSource)
            {
                return cardSource.ContainsTraits("Three Musketeers");
            }

            bool CanSelectPermanentSharedCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanActivateSharedCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return cards from trash to De-Digivolve 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Play] By returning 3 cards with the [Three Musketeers] trait from your hand or trash to the top of the deck, <De-Digivolve 1> 1 of your opponent's Digimon (Trash the top card. You can't trash past level 3 cards).";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = card.Owner.HandCards.Count(CanSelectCardSharedCondition) >= 3;
                    bool canSelectTrash = card.Owner.TrashCards.Count(CanSelectCardSharedCondition) >= 3;

                    if (canSelectHand || canSelectTrash)
                    {
                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>
                            {
                                new(message: "From hand", value: true, spriteIndex: 0),
                                new(message: "From trash", value: false, spriteIndex: 1),
                            };

                            string selectPlayerMessage = "From which area do you return cards?";
                            string notSelectPlayerMessage = "The opponent is choosing from which area to return cards.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements,
                                selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                                notSelectPlayerMessage: notSelectPlayerMessage);
                        }

                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectHand);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                            .WaitForEndSelect());

                        bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (fromHand)
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCardSharedCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 3,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: false,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectHandEffect.SetUpCustomMessage("Select 3 cards to top deck\n(cards will be placed back to the top of the deck so that cards with lower numbers are on top).",
                                "The opponent is selecting 3 cards to top deck.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Deck Top Cards");

                            yield return StartCoroutine(selectHandEffect.Activate());
                        }

                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardSharedCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                message:
                                "Select cards to place at the top of the deck\n(cards will be placed back to the top of the deck so that cards with lower numbers are on top).",
                                maxCount: 3,
                                canEndNotMax: false,
                                isShowOpponent: false,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 3 cards to top deck.",
                                "The opponent is selecting 3 cards to top deck.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Deck Top Cards");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count == 3)
                            {
                                cardSources.Reverse();

                                yield return ContinuousController.instance.StartCoroutine(
                                    CardObjectController.AddLibraryTopCards(cardSources));

                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentSharedCondition))
                                {
                                    SelectPermanentEffect selectPermanentEffect =
                                        GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectPermanentSharedCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: 1,
                                        canNoSelect: false,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: SelectPermanentCoroutine,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to De-Digivolve.",
                                        "The opponent is selecting 1 Digimon to De-Digivolve.");
                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(
                                            new IDegeneration(permanent, 1, activateClass).Degeneration());
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion
            
            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return cards from trash to De-Digivolve 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] By returning 3 cards with the [Three Musketeers] trait from your hand or trash to the top of the deck, <De-Digivolve 1> 1 of your opponent's Digimon (Trash the top card. You can't trash past level 3 cards).";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = card.Owner.HandCards.Count(CanSelectCardSharedCondition) >= 3;
                    bool canSelectTrash = card.Owner.TrashCards.Count(CanSelectCardSharedCondition) >= 3;

                    if (canSelectHand || canSelectTrash)
                    {
                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>
                            {
                                new(message: "From hand", value: true, spriteIndex: 0),
                                new(message: "From trash", value: false, spriteIndex: 1),
                            };

                            string selectPlayerMessage = "From which area do you return cards?";
                            string notSelectPlayerMessage = "The opponent is choosing from which area to return cards.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements,
                                selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                                notSelectPlayerMessage: notSelectPlayerMessage);
                        }

                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectHand);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                            .WaitForEndSelect());

                        bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (fromHand)
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCardSharedCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 3,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: false,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectHandEffect.SetUpCustomMessage("Select 3 cards to top deck\n(cards will be placed back to the top of the deck so that cards with lower numbers are on top).",
                                "The opponent is selecting 3 cards to top deck.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Deck Top Cards");

                            yield return StartCoroutine(selectHandEffect.Activate());
                        }

                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardSharedCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                message:
                                "Select cards to place at the top of the deck\n(cards will be placed back to the top of the deck so that cards with lower numbers are on top).",
                                maxCount: 3,
                                canEndNotMax: false,
                                isShowOpponent: false,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 3 cards to top deck.",
                                "The opponent is selecting 3 cards to top deck.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Deck Top Cards");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count == 3)
                            {
                                cardSources.Reverse();

                                yield return ContinuousController.instance.StartCoroutine(
                                    CardObjectController.AddLibraryTopCards(cardSources));

                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentSharedCondition))
                                {
                                    SelectPermanentEffect selectPermanentEffect =
                                        GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectPermanentSharedCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: 1,
                                        canNoSelect: false,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: SelectPermanentCoroutine,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to De-Digivolve.",
                                        "The opponent is selecting 1 Digimon to De-Digivolve.");
                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(
                                            new IDegeneration(permanent, 1, activateClass).Degeneration());
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Reboot - ESS

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(
                    isInheritedEffect: true,
                    card: card,
                    condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}