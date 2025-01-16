using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX4
{
    public class EX4_066 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] Activate 1 of the effects below. -If you have [CresGarurumon] in play, 1 of your [Agumon] or [Greymon] may digivolve into [BlitzGreymon] in your hand, ignoring digivolution requirements and without paying the cost. -If you have [BlitzGreymon] in play, 1 of your [Gabumon] or [Garurumon] may digivolve into [CresGarurumon] in your hand, ignoring digivolution requirements and without paying the cost.";
                }
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return GManager.instance.photonWaitController.StartWait("GoldBeast_Select");

                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new SelectionElement<int>(message: $"Digivolve into [BlitzGreymon]", value : 0, spriteIndex: 0),
                            new SelectionElement<int>(message: $"Digivolve into [CresGarurumon]", value : 1, spriteIndex: 1),
                        };

                    string selectPlayerMessage = "Which effect will you activate?";
                    string notSelectPlayerMessage = "The opponent is choosing which effect to activate.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    int actionID = GManager.instance.userSelectionManager.SelectedIntValue;

                    switch (actionID)
                    {
                        case 0:
                            if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.TopCard.CardNames.Contains("CresGarurumon")))
                            {
                                bool CanSelectPermanentCondition(Permanent permanent)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                                    {
                                        if (permanent.TopCard.CardNames.Contains("Agumon") || permanent.TopCard.CardNames.Contains("Greymon"))
                                        {
                                            foreach (CardSource cardSource in card.Owner.HandCards)
                                            {
                                                if (CanSelectCardCondition(cardSource))
                                                {
                                                    if (!cardSource.CanNotEvolve(permanent))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    return false;
                                }

                                bool CanSelectCardCondition(CardSource cardSource)
                                {
                                    return cardSource.ContainsCardName("BlitzGreymon");
                                }

                                if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                                {
                                    Permanent selectedPermanent = null;

                                    int maxCount = Math.Min(1, card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

                                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectPermanentCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: true,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: SelectPermanentCoroutine,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will digivolve.", "The opponent is selecting 1 Digimon that will digivolve.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        selectedPermanent = permanent;

                                        yield return null;
                                    }

                                    if (selectedPermanent != null)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                            targetPermanent: selectedPermanent,
                                            cardCondition: CanSelectCardCondition,
                                            payCost: false,
                                            reduceCostTuple: null,
                                            fixedCostTuple: null,
                                            ignoreDigivolutionRequirementFixedCost: 0,
                                            isHand: true,
                                            activateClass: activateClass,
                                            successProcess: null));
                                    }
                                }
                            }
                            break;

                        case 1:
                            if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.TopCard.CardNames.Contains("BlitzGreymon")))
                            {
                                bool CanSelectPermanentCondition(Permanent permanent)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                                    {
                                        if (permanent.TopCard.CardNames.Contains("Gabumon") || permanent.TopCard.CardNames.Contains("Garurumon"))
                                        {
                                            foreach (CardSource cardSource in card.Owner.HandCards)
                                            {
                                                if (CanSelectCardCondition(cardSource))
                                                {
                                                    if (!cardSource.CanNotEvolve(permanent))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    return false;
                                }

                                bool CanSelectCardCondition(CardSource cardSource)
                                {
                                    return cardSource.ContainsCardName("CresGarurumon");
                                }

                                if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                                {
                                    Permanent selectedPermanent = null;

                                    int maxCount = Math.Min(1, card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

                                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectPermanentCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: true,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: SelectPermanentCoroutine,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will digivolve.", "The opponent is selecting 1 Digimon that will digivolve.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        selectedPermanent = permanent;

                                        yield return null;
                                    }

                                    if (selectedPermanent != null)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                            targetPermanent: selectedPermanent,
                                            cardCondition: CanSelectCardCondition,
                                            payCost: false,
                                            reduceCostTuple: null,
                                            fixedCostTuple: null,
                                            ignoreDigivolutionRequirementFixedCost: 0,
                                            isHand: true,
                                            activateClass: activateClass,
                                            successProcess: null));
                                    }
                                }
                            }
                            break;
                    }
                }
            }


            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Play 1 [Gabumon] or [Agumon] from hand or trash and add this card to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] You may play 1 [Gabumon] or [Agumon] from your hand or trash without paying the cost. Then, add this card to your hand.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.CardNames.Contains("Gabumon") || cardSource.CardNames.Contains("Agumon"))
                            {
                                if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canSelectHand = card.Owner.HandCards.Count(CanSelectCardCondition) >= 1;
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    if (canSelectHand || canSelectTrash)
                    {
                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                        };

                            string selectPlayerMessage = "From which area do you play a card?";
                            string notSelectPlayerMessage = "The opponent is choosing from which area to play a card.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        }

                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectHand);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        List<CardSource> selectedCards = new List<CardSource>();

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (fromHand)
                        {
                            int maxCount = 1;

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCardCondition,
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

                            selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                            yield return StartCoroutine(selectHandEffect.Activate());
                        }

                        else
                        {
                            int maxCount = 1;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to play.",
                                maxCount: maxCount,
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

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: root, activateETB: true));
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
                }
            }

            return cardEffects;
        }
    }
}