using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT18
{
    public class BT18_096 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Ignore color requirements

            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);
                cardEffects.Add(ignoreColorConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionOwnersPermanent(card,
                        permanent =>
                            permanent.IsDigimon &&
                            permanent.TopCard.HasHybridTenWarriorsTraits);
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }
            }

            #endregion

            #region Main Effect

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Main] 1 of your Digimon or Tamers may digivolve into [Susanoomon] in the hand or trash without paying the cost. Then, by placing up to 4 of your Tamers with different colors under 1 of your [Susanoomon] as its bottom digivolution cards, gain 1 memory for each placed card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool DigivolveFromPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card) &&
                           (permanent.IsDigimon || permanent.IsTamer) &&
                           (card.Owner.HandCards.Where(DigivolveToCardCondition).Any(cardSource =>
                                cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, false, activateClass)) ||
                            card.Owner.TrashCards.Where(DigivolveToCardCondition).Any(cardSource =>
                                cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, false, activateClass)));
                }

                bool DigivolveToCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Susanoomon");
                }

                bool CanSelectOwnTamerPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card) && permanent.IsTamer;
                }

                bool CanSelectSusanoomonPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.EqualsCardName("Susanoomon");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    // Digivolve into Susanoomon
                    if (CardEffectCommons.HasMatchConditionPermanent(DigivolveFromPermanentCondition))
                    {
                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: DigivolveFromPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will digivolve.",
                            "The opponent is selecting 1 Digimon that will digivolve.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, DigivolveToCardCondition);
                            bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, DigivolveToCardCondition);

                            if (canSelectHand || canSelectTrash)
                            {
                                if (canSelectHand && canSelectTrash)
                                {
                                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>
                                    {
                                        new(message: "From hand", value: true, spriteIndex: 0),
                                        new(message: "From trash", value: false, spriteIndex: 1),
                                    };

                                    string selectPlayerMessage = "From which area do you digivolve?";
                                    string notSelectPlayerMessage = "The opponent is choosing from which area to digivolve.";

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

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                    targetPermanent: selectedPermanent,
                                    cardCondition: DigivolveToCardCondition,
                                    payCost: false,
                                    reduceCostTuple: null,
                                    fixedCostTuple: null,
                                    ignoreDigivolutionRequirementFixedCost: -1,
                                    isHand: fromHand,
                                    activateClass: activateClass,
                                    successProcess: null));
                            }
                        }
                    }

                    // Place Tamers
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSusanoomonPermanentCondition) &&
                        CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnTamerPermanentCondition))
                    {
                        Permanent susanoomonPermanent = null;

                        SelectPermanentEffect selectPermanentEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectSusanoomonPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine1,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage(
                            "Select 1 of your Susanoomon.",
                            "The opponent is selecting 1 of their Susanoomon.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine1(Permanent selectedPermanent)
                        {
                            susanoomonPermanent = selectedPermanent;

                            yield return null;
                        }

                        if (susanoomonPermanent != null)
                        {
                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectOwnTamerPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: CanTargetConditionByPreSelectedList,
                                canEndSelectCondition: CanEndSelectCondition,
                                maxCount: Math.Min(4, CardEffectCommons.MatchConditionPermanentCount(CanSelectOwnTamerPermanentCondition)),
                                canNoSelect: true,
                                canEndNotMax: true,
                                selectPermanentCoroutine: SelectPermanentCoroutine2,
                                afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage(
                                "Select up to 4 of your Tamers with different colors to place on bottom of digivolution cards of your Susanoomon.",
                                "The opponent is selecting up to 4 of your Tamers with different colors to place on bottom of digivolution cards of your Susanoomon.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            bool CanTargetConditionByPreSelectedList(List<Permanent> permanents, Permanent permanent)
                            {
                                List<Permanent> concat = permanents.Concat(new List<Permanent>() { permanent }).ToList();

                                return Combinations.GetDifferenetColorCardCount(concat.Map(permanent1 => permanent1.TopCard)) >=
                                       concat.Count;
                            }

                            bool CanEndSelectCondition(List<Permanent> permanents)
                            {
                                return permanents.Count > 0 && Combinations.GetDifferenetColorCardCount(
                                    permanents.Map(permanent1 => permanent1.TopCard)) >= permanents.Count;
                            }

                            IEnumerator SelectPermanentCoroutine2(Permanent selectedPermanent)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(
                                    new List<Permanent[]> { new[] { selectedPermanent, susanoomonPermanent } }, false,
                                    activateClass).PlacePermanentToDigivolutionCards());
                            }

                            IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                            {
                                yield return ContinuousController.instance.StartCoroutine(
                                    card.Owner.AddMemory(permanents.Count, activateClass));
                            }
                        }
                    }
                }
            }

            #endregion

            #region Security Effect

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Play 1 Tamer card with an inherited effect from hand or trash and add this card to hand",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Security] You may play 1 Tamer card with an inherited effect from your hand or trash without paying the cost. Then, add this card to the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                    {
                        return cardSource.IsTamer && cardSource.HasInheritedEffect;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    if (canSelectHand || canSelectTrash)
                    {
                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>
                            {
                                new(message: "From hand", value: true, spriteIndex: 0),
                                new(message: "From trash", value: false, spriteIndex: 1),
                            };

                            string selectPlayerMessage = "From which area do you play a card?";
                            string notSelectPlayerMessage = "The opponent is choosing from which area to play a card.";

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

                        List<CardSource> selectedCards = new List<CardSource>();

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        if (fromHand)
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
                        }

                        else
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
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }

                        SelectCardEffect.Root root = SelectCardEffect.Root.Hand;

                        if (!fromHand)
                        {
                            root = SelectCardEffect.Root.Trash;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: root,
                            activateETB: true));
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}