using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using UnityEngine;

namespace DCGO.CardEffects.BT18
{
    public class Sephirothmon_BT18_066 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolve Condition
            // Any black tamer
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return card.Owner.HandCards.Contains(card);
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.IsTamer && (targetPermanent.TopCard.CardColors.Contains(CardColor.Black));
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false,
                    card: card, condition: Condition));
            }

            //Mercurymon
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Mercurymon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 1, ignoreDigivolutionRequirement: false,
                    card: card, condition: null));
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 level 4 or lower as bottom digivolution card.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By placing 1 level 4 or lower card with the [Hybrid] trait other that [Sephirothmon] from your hand or trash as this Digimon's bottom digivolution card, you may activate 1 [On Play] effect on the placed card as an effect of this Digimon.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("Hybrid"))
                    {
                        if (!cardSource.CardNames.Contains("Sephirothmon"))
                        {
                            if (cardSource.HasLevel)
                            {
                                if (cardSource.Level <= 4)
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
                    return CardEffectCommons.CanTriggerOnPlay(hashtable,card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            return true;
                        }

                        if(CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        bool canSelectHand = card.Owner.HandCards.Count(CanSelectCardCondition) >= 1;
                        bool canSelectTrash = card.Owner.TrashCards.Count(CanSelectCardCondition) >= 1;

                        if (canSelectHand || canSelectTrash)
                        {
                            if (canSelectHand && canSelectTrash)
                            {
                                List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                        };

                                string selectPlayerMessage = "From which area do you choose a card?";
                                string notSelectPlayerMessage = "The opponent is choosing from which area to choose a card.";

                                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                            }

                            else
                            {
                                GManager.instance.userSelectionManager.SetBool(canSelectHand);
                            }

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                            List<CardSource> selectedCards = new List<CardSource>();
                        

                        if (!fromHand)
                        {
                            int maxCount = Math.Min(1, card.Owner.TrashCards.Count((cardSource) => CanSelectCardCondition(cardSource)));

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

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
                                    .Filter(cardEffect => cardEffect != null && cardEffect is ActivateICardEffect && !cardEffect.IsSecurityEffect && cardEffect.IsOnPlay);

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

                                        SelectCardEffect selectSourceCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                        selectSourceCardEffect.SetUp(
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

                                        selectSourceCardEffect.SetNotShowCard();
                                        selectSourceCardEffect.SetUpSkillInfos(skillInfos);
                                        selectSourceCardEffect.SetUpAfterSelectIndexCoroutine(AfterSelectIndexCoroutine);

                                        yield return ContinuousController.instance.StartCoroutine(selectSourceCardEffect.Activate());

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
                                        Hashtable effectHashtable = CardEffectCommons.OnPlayCheckHashtableOfCard(card);

                                            if (selectedEffect.CanUse(effectHashtable))
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(
                                                    ((ActivateICardEffect)selectedEffect).Activate_Optional_Effect_Execute(effectHashtable));
                                            }
                                        }
                                    }
                                }
                            }

                            if (fromHand)
                            {
                                int maxCount = Math.Min(1, card.Owner.HandCards.Count((cardSource) => CanSelectCardCondition(cardSource)));

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCards.Add(cardSource);

                                    yield return null;
                                }

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
                                    root: SelectCardEffect.Root.Hand,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage(
                                    "Select 1 card to place on bottom of digivolution cards.",
                                    "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                                selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

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
                                        .Filter(cardEffect => cardEffect != null && cardEffect is ActivateICardEffect && !cardEffect.IsSecurityEffect && cardEffect.IsOnPlay);

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

                                            SelectCardEffect selectSourceCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                            selectSourceCardEffect.SetUp(
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

                                            selectSourceCardEffect.SetNotShowCard();
                                            selectSourceCardEffect.SetUpSkillInfos(skillInfos);
                                            selectSourceCardEffect.SetUpAfterSelectIndexCoroutine(AfterSelectIndexCoroutine);

                                            yield return ContinuousController.instance.StartCoroutine(selectSourceCardEffect.Activate());

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
                                            Hashtable effectHashtable = CardEffectCommons.OnPlayCheckHashtableOfCard(card);

                                            if (selectedEffect.CanUse(effectHashtable))
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(
                                                    ((ActivateICardEffect)selectedEffect).Activate_Optional_Effect_Execute(effectHashtable));
                                            }
                                        }
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
                activateClass.SetUpICardEffect("Place 1 level 4 or lower as bottom digivolution card.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By placing 1 level 4 or lower card with the [Hybrid] trait other that [Sephirothmon] from your hand or trash as this Digimon's bottom digivolution card, you may activate 1 [On Play] effect on the placed card as an effect of this Digimon.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("Hybrid"))
                    {
                        if (!cardSource.CardNames.Contains("Sephirothmon"))
                        {
                            if (cardSource.HasLevel)
                            {
                                if (cardSource.Level <= 4)
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
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            return true;
                        }

                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        bool canSelectHand = card.Owner.HandCards.Count(CanSelectCardCondition) >= 1;
                        bool canSelectTrash = card.Owner.TrashCards.Count(CanSelectCardCondition) >= 1;

                        if (canSelectHand || canSelectTrash)
                        {
                            if (canSelectHand && canSelectTrash)
                            {
                                List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                        };

                                string selectPlayerMessage = "From which area do you choose a card?";
                                string notSelectPlayerMessage = "The opponent is choosing from which area to choose a card.";

                                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                            }

                            else
                            {
                                GManager.instance.userSelectionManager.SetBool(canSelectHand);
                            }

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                            List<CardSource> selectedCards = new List<CardSource>();


                            if (!fromHand)
                            {
                                int maxCount = Math.Min(1, card.Owner.TrashCards.Count((cardSource) => CanSelectCardCondition(cardSource)));

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCards.Add(cardSource);

                                    yield return null;
                                }

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
                                        .Filter(cardEffect => cardEffect != null && cardEffect is ActivateICardEffect && !cardEffect.IsSecurityEffect && cardEffect.IsOnPlay);

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

                                            SelectCardEffect selectSourceCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                            selectSourceCardEffect.SetUp(
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

                                            selectSourceCardEffect.SetNotShowCard();
                                            selectSourceCardEffect.SetUpSkillInfos(skillInfos);
                                            selectSourceCardEffect.SetUpAfterSelectIndexCoroutine(AfterSelectIndexCoroutine);

                                            yield return ContinuousController.instance.StartCoroutine(selectSourceCardEffect.Activate());

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
                                            Hashtable effectHashtable = CardEffectCommons.OnPlayCheckHashtableOfCard(card);

                                            if (selectedEffect.CanUse(effectHashtable))
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(
                                                    ((ActivateICardEffect)selectedEffect).Activate_Optional_Effect_Execute(effectHashtable));
                                            }
                                        }
                                    }
                                }
                            }

                            if (fromHand)
                            {
                                int maxCount = Math.Min(1, card.Owner.HandCards.Count((cardSource) => CanSelectCardCondition(cardSource)));

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCards.Add(cardSource);

                                    yield return null;
                                }

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
                                    root: SelectCardEffect.Root.Hand,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage(
                                    "Select 1 card to place on bottom of digivolution cards.",
                                    "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                                selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

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
                                        .Filter(cardEffect => cardEffect != null && cardEffect is ActivateICardEffect && !cardEffect.IsSecurityEffect && cardEffect.IsOnPlay);

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

                                            SelectCardEffect selectSourceCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                            selectSourceCardEffect.SetUp(
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

                                            selectSourceCardEffect.SetNotShowCard();
                                            selectSourceCardEffect.SetUpSkillInfos(skillInfos);
                                            selectSourceCardEffect.SetUpAfterSelectIndexCoroutine(AfterSelectIndexCoroutine);

                                            yield return ContinuousController.instance.StartCoroutine(selectSourceCardEffect.Activate());

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
                                            Hashtable effectHashtable = CardEffectCommons.OnPlayCheckHashtableOfCard(card);

                                            if (selectedEffect.CanUse(effectHashtable))
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(
                                                    ((ActivateICardEffect)selectedEffect).Activate_Optional_Effect_Execute(effectHashtable));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region ESS - Opponent's Turn
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 2000, isInheritedEffect: true, card: card, condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}