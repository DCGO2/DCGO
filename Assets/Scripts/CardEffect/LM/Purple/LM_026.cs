using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.LM
{
    public class LM_026 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Growlmon") && targetPermanent.TopCard.HasLevel &&
                           targetPermanent.TopCard.Level == 5;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition,
                    digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Ace - Blast Digivolve

            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 of your opponent's Digimon with 11000 DP or less.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] Delete 1 of your opponent's Digimon with 11000 DP or less.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           permanent.DP <= card.Owner.MaxDP_DeleteEffect(11000, activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
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
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 of your opponent's Digimon with 11000 DP or less.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] Delete 1 of your opponent's Digimon with 11000 DP or less.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           permanent.DP <= card.Owner.MaxDP_DeleteEffect(11000, activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
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
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play a [Guilmon] from digivolution cards or from trash.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[All Turns] When this Digimon would leave the battle area, by playing 1 [Guilmon] from this Digimon's digivolution cards or from your trash, place this Digimon as that Digimon's bottom digivolution card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card) &&
                           !activateClass.IsSameEffect(CardEffectCommons.GetCardEffectFromHashtable(hashtable));
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Guilmon") &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           (card.PermanentOfThisCard().DigivolutionCards.Some(CanSelectCardCondition) ||
                            CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectDigivolutionCards = card.PermanentOfThisCard().DigivolutionCards.Some(CanSelectCardCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    if (canSelectTrash || canSelectDigivolutionCards)
                    {
                        if (canSelectTrash && canSelectDigivolutionCards)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                            {
                                new(message: "From digivolution cards", value: true, spriteIndex: 0),
                                new(message: "From trash", value: false, spriteIndex: 1),
                            };

                            string selectPlayerMessage = "From which area will you play a [Guilmon]?";
                            string notSelectPlayerMessage = "The opponent is choosing from which area to play a [Lucemon].";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements,
                                selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                                notSelectPlayerMessage: notSelectPlayerMessage);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectDigivolutionCards);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                            .WaitForEndSelect());

                        bool fromDigivolution = GManager.instance.userSelectionManager.SelectedBoolValue;

                        CardSource selectedCard = null;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select a [Guilmon] to play.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: false,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: fromDigivolution ? card.PermanentOfThisCard().DigivolutionCards : card.Owner.TrashCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        if (selectedCard)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: new List<CardSource> { selectedCard },
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: fromDigivolution ? SelectCardEffect.Root.DigivolutionCards : SelectCardEffect.Root.Trash,
                                activateETB: true));

                            yield return ContinuousController.instance.StartCoroutine(selectedCard.PermanentOfThisCard()
                                .AddDigivolutionCardsBottom(new List<CardSource> { card }, activateClass));
                        }
                    }
                }
            }

            #endregion

            #region Rule Text

            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("Also treated as [ChaosGallantmon]", _ => true, card);
                changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: ChangeCardNames);
                cardEffects.Add(changeCardNamesClass);

                List<string> ChangeCardNames(CardSource cardSource, List<string> cardNames)
                {
                    if (cardSource == card)
                    {
                        cardNames.Add("ChaosGallantmon");
                    }

                    return cardNames;
                }
            }

            #endregion

            #region All Turns - ESS

            if (timing == EffectTiming.None)
            {
                ChangeDPDeleteEffectMaxDPClass changeDPDeleteEffectMaxDPClass = new ChangeDPDeleteEffectMaxDPClass();
                changeDPDeleteEffectMaxDPClass.SetUpICardEffect("Maximum DP of DP-based deletion effects gets +5000 DP",
                    CanUseCondition, card);
                changeDPDeleteEffectMaxDPClass.SetUpChangeDPDeleteEffectMaxDPClass(changeMaxDP: ChangeMaxDP);
                changeDPDeleteEffectMaxDPClass.SetIsInheritedEffect(true);
                cardEffects.Add(changeDPDeleteEffectMaxDPClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard());
                }

                int ChangeMaxDP(int maxDP, ICardEffect cardEffect)
                {
                    if (cardEffect != null &&
                        cardEffect.EffectSourceCard &&
                        cardEffect.EffectSourceCard.Owner == card.Owner &&
                        cardEffect.EffectSourceCard.PermanentOfThisCard() == card.PermanentOfThisCard())
                    {
                        maxDP += 5000;
                    }

                    return maxDP;
                }
            }

            #endregion

            return cardEffects;
        }
    }
}