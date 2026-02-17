using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

// Icemon
namespace DCGO.CardEffects.P
{
    public class P_215 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel3 &&
                        (targetPermanent.TopCard.EqualsTraits("Ice-Snow") ||
                        targetPermanent.TopCard.HasRockMineralTraits);
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

            #region Shared WM / OP / WD

            string SharedEffectDescription(string tag)
                => $"[{tag}] By placing 1 level 4 or lower [Ice-Snow], [Mineral] or [Rock] trait card from your hand or trash as this Digimon's bottom digivolution card, until your opponent's turn ends, their effects can't return 1 of your [Ice-Snow], [Mineral] or [Rock] trait Digimon to hands or decks or affect it with <De-Digivolve> effects.";

            bool AdditionalActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.HasMatchConditionOwnersHand(card, CardSelectCondition)
                    || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CardSelectCondition);
            }

            bool CardSelectCondition(CardSource cardSource)
            {
                return cardSource.HasLevel 
                    && cardSource.Level <= 4 
                    && (cardSource.EqualsTraits("Ice-Snow")
                        || cardSource.HasRockMineralTraits);
            }

            bool PermanentSelectCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && (permanent.TopCard.EqualsTraits("Ice-Snow")
                        || permanent.TopCard.HasRockMineralTraits);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                Permanent thisPermament = card.PermanentOfThisCard();

                bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CardSelectCondition);
                bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CardSelectCondition);

                if (canSelectHand || canSelectTrash)
                {
                    if (canSelectHand && canSelectTrash)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "From which area do you select a card?";
                        string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                    }
                    else
                    {
                        GManager.instance.userSelectionManager.SetBool(canSelectHand);
                    }

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                    CardSource selectedCard = null;

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (fromHand)
                    {
                        int maxCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CardSelectCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                        yield return StartCoroutine(selectHandEffect.Activate());
                    }
                    else
                    {
                        int maxCount = Math.Min(1, card.Owner.TrashCards.Count(CardSelectCondition));

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CardSelectCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message:"Select 1 card to place as digivolution source",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }

                    if (selectedCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                            new List<CardSource> { selectedCard },
                            activateClass));

                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, PermanentSelectCondition))
                        {
                            var selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: PermanentSelectCondition,
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

                            IEnumerator SelectPermanentCoroutine(Permanent selectedPermanent)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotReturnToHand(
                                    targetPermanent: selectedPermanent,
                                    cardEffectCondition: CardEffectCondition,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                    activateClass: activateClass,
                                    effectName: "Can't return to hand by opponent's effects"));

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotReturnToDeck(
                                    targetPermanent: selectedPermanent,
                                    cardEffectCondition: CardEffectCondition,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                    activateClass: activateClass,
                                    effectName: "Can't return to deck by opponent's effects"));

                                ImmuneFromDeDigivolveClass immuneFromDeDigivolveClass = new ImmuneFromDeDigivolveClass();
                                immuneFromDeDigivolveClass.SetUpICardEffect("Isn't affected by <De-Digivolve>", CanUseCondition1, selectedPermanent.TopCard);
                                immuneFromDeDigivolveClass.SetUpImmuneFromDeDigivolveClass(PermanentCondition: PermanentCondition);
                                selectedPermanent.UntilOpponentTurnEndEffects.Add((_timing) => immuneFromDeDigivolveClass);

                                bool CanUseCondition1(Hashtable hashtable1)
                                {
                                    if (selectedPermanent.TopCard != null)
                                    {
                                        return true;
                                    }

                                    return false;
                                }

                                bool PermanentCondition(Permanent permanent)
                                {
                                    if (permanent == selectedPermanent)
                                    {
                                        return true;
                                    }

                                    return false;
                                }
                            }

                            bool CardEffectCondition(ICardEffect cardEffect)
                            {
                                return CardEffectCommons.IsOpponentEffect(cardEffect, card);
                            }
                        }
                    }
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects(ref cardEffects, timing, card, 
                                                "By placing 1 [Ice-Snow], [Mineral] or [Rock] Digimon from hand or trash under this, 1 such digimon cannot be returned to deck or de-digivolved.",
                                                SharedActivateCoroutine,
                                                SharedEffectDescription,
                                                optional: true,
                                                whenMoving: true,
                                                onPlay: true,
                                                whenDigivolving: true,
                                                additionalActivateCondition: AdditionalActivateCondition);

            #endregion

            #region ESS

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(
                    isInheritedEffect: true,
                    card: card,
                    condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}
