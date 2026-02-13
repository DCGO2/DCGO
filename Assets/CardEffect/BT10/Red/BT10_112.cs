using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.BT10
{
    public class BT10_112 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasRoyalKnightTraits && targetPermanent.TopCard.HasLevel && targetPermanent.Level == 6;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 5,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 card to digivolution cards to activate effects and Blitz", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may place 1 card with [Royal Knight] in its traits and a play cost of 13 or less from your hand or trash under this Digimon as its bottom digivolution card. Activate 1 of that Digimon's [When Digivolving] effects as an effect of this Digimon. Then, <Blitz>.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.HasRoyalKnightTraits)
                    {
                        if (cardSource.GetCostItself <= 13)
                        {
                            if (cardSource.HasPlayCost)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }

                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            return true;
                        }

                        if (CardEffectCommons.CanActivateBlitz(card, activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
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

                            List<CardSource> selectedCards = new List<CardSource>();

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

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

                                selectHandEffect.SetUpCustomMessage(
                                    "Select 1 card to place on bottom of digivolution cards.",
                                    "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

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
                                    message: "Select 1 card to place on bottom of digivolution cards.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage(
                                    "Select 1 card to place on bottom of digivolution cards.",
                                    "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                                selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                            }

                            CardSource selectedCard = null;

                            if (selectedCards.Count >= 1)
                            {
                                if (CardEffectCommons.IsExistOnBattleArea(card))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                                        selectedCards,
                                        activateClass));

                                    selectedCard = selectedCards[0];
                                }
                            }

                            if (selectedCard != null)
                            {
                                List<ICardEffect> candidateEffects = selectedCard.EffectList_ForCard(EffectTiming.OnEnterFieldAnyone, card)
                                    .Clone()
                                    .Filter(cardEffect => cardEffect != null && cardEffect is ActivateICardEffect && !cardEffect.IsSecurityEffect && cardEffect.IsWhenDigivolving);

                                if (candidateEffects.Count >= 1)
                                {
                                    ICardEffect selectedEffect = null;

                                    if (candidateEffects.Count == 1)
                                    {
                                        selectedEffect = candidateEffects[0];
                                    }
                                    else
                                    {
                                        List<SkillInfo> skillInfos = candidateEffects
                                            .Map(cardEffect => new SkillInfo(cardEffect, null, EffectTiming.None));

                                        List<CardSource> cardSources = candidateEffects
                                            .Map(cardEffect => cardEffect.EffectSourceCard);

                                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                        selectCardEffect.SetUp(
                                            canTargetCondition: (cardSource) => true,
                                            canTargetCondition_ByPreSelecetedList: null,
                                            canEndSelectCondition: null,
                                            canNoSelect: () => false,
                                            selectCardCoroutine: null,
                                            afterSelectCardCoroutine: null,
                                            message: "Select 1 effect to activate.",
                                            maxCount: 1,
                                            canEndNotMax: false,
                                            isShowOpponent: false,
                                            mode: SelectCardEffect.Mode.Custom,
                                            root: SelectCardEffect.Root.Custom,
                                            customRootCardList: cardSources,
                                            canLookReverseCard: true,
                                            selectPlayer: card.Owner,
                                            cardEffect: activateClass);

                                        selectCardEffect.SetNotShowCard();
                                        selectCardEffect.SetUpSkillInfos(skillInfos);
                                        selectCardEffect.SetUpAfterSelectIndexCoroutine(AfterSelectIndexCoroutine);

                                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                        IEnumerator AfterSelectIndexCoroutine(List<int> selectedIndexes)
                                        {
                                            if (selectedIndexes.Count == 1)
                                            {
                                                selectedEffect = candidateEffects[selectedIndexes[0]];
                                                yield return null;
                                            }
                                        }
                                    }

                                    if (selectedEffect != null)
                                    {
                                        Hashtable effectHashtable = CardEffectCommons.WhenDigivolvingCheckHashtableOfCard(card);

                                        if (selectedEffect.CanUse(effectHashtable))
                                        {
                                            selectedEffect.SetIsDigimonEffect(true);
                                            yield return ContinuousController.instance.StartCoroutine(
                                                ((ActivateICardEffect)selectedEffect).Activate_Optional_Effect_Execute(effectHashtable));
                                        }
                                    }
                                }
                            }
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.BlitzProcess(card, activateClass));
                    }
                }
            }

            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.HasRoyalKnightTraits) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: Condition));
            }

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.HasRoyalKnightTraits) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: Condition));
            }

            if (timing == EffectTiming.None)
            {
                int count()
                {
                    int count = 0;

                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        count = card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.HasRoyalKnightTraits);
                    }

                    return count;
                }

                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (count() >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect<Func<int>>(
                    changeValue: () => count(),
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition));
            }

            return cardEffects;
        }
    }
}