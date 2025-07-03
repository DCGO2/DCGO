using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Machinedramon
namespace DCGO.CardEffects.EX9
{
    public class EX9_073 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel5 && targetPermanent.TopCard.EqualsTraits("DM");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region Assembly
            if (timing == EffectTiming.None)
            {
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect($"Assembly", CanUseCondition, card);
                addAssemblyConditionClass.SetUpAddAssemblyConditionClass(getAssemblyCondition: GetAssembly);
                addAssemblyConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAssemblyConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        AssemblyConditionElement element = new AssemblyConditionElement(CanSelectCardCondition);

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            if (cardSource != null)
                            {
                                if (cardSource.Owner == card.Owner)
                                {
                                    if (cardSource.IsDigimon)
                                    {
                                        if (cardSource.IsLevel5)
                                        {
                                            if (cardSource.EqualsTraits("Cyborg"))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                        {
                            List<string> cardNames = new List<string>();

                            foreach (CardSource cardSource1 in cardSources)
                            {
                                foreach (string cardName in cardSource1.CardNames)
                                {
                                    if (!cardNames.Contains(cardName))
                                    {
                                        cardNames.Add(cardName);
                                    }
                                }
                            }

                            if (cardSource.CardNames.Count((cardName) => cardNames.Contains(cardName)) >= 1)
                            {
                                return false;
                            }

                            return true;
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            element: element,
                            CanTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                            selectMessage: "4 level 5 [Cyborg] trait Digimon cards w/different name",
                            elementCount: 4,
                            reduceCost: 6);

                        return assemblyCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region On Play/ When Digivolving/ When Attacking Shared

            bool CanSelectCardConditionShared(CardSource cardSource)
            {
                return cardSource.IsLevel5 &&
                       (cardSource.EqualsTraits("Cyborg") || cardSource.EqualsTraits("Ver.5"));
            }

            bool CanActivateConditionShared(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                       (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardConditionShared) ||
                        CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardConditionShared));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 card in hand or trash as source, to activate its [On Play] effect",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("AddSource_EX9_073");
                cardEffects.Add(activateClass);

                string EffectDescription() =>
                    "[On Play] [Once Per Turn] By placing 1 level 5 [Cyborg] or [Ver.5] trait card from your hand or trash as this Digimon's top digivolution card, active 1 [On Play] effect on the placed card as an effect of this Digimon.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardConditionShared);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardConditionShared);

                    if (canSelectHand && canSelectTrash)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>
                        {
                            new(message: "From hand", value: true, spriteIndex: 0),
                            new(message: "From trash", value: false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "From which area do you choose a card?";
                        string notSelectPlayerMessage = "The opponent is choosing from which area to choose a card.";

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
                            canTargetCondition: CanSelectCardConditionShared,
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

                        selectHandEffect.SetUpCustomMessage("Select 1 card to place on top of digivolution cards.",
                            "The opponent is selecting 1 card to place on top of digivolution cards.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Placed Card");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                    }
                    else
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardConditionShared,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to place on top of digivolution cards.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to place on top of digivolution cards.",
                            "The opponent is selecting 1 card to place on top of digivolution cards.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Placed Card");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }

                    if (selectedCard)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard()
                            .AddDigivolutionCardsTop(new List<CardSource>() { selectedCard }, activateClass));

                        List<ICardEffect> candidateEffects = selectedCard.cEntity_EffectController.GetCardEffects_ExceptAddedEffects(EffectTiming.OnEnterFieldAnyone, card)
                            .Clone()
                            .Filter(cardEffect =>
                                cardEffect is ActivateICardEffect && !cardEffect.IsSecurityEffect &&
                                cardEffect.IsOnPlay);

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
                                    canTargetCondition: _ => true,
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
                                Hashtable effectHashtable = CardEffectCommons.OnPlayCheckHashtableOfCard(card);

                                if (selectedEffect.CanUse(effectHashtable))
                                {
                                    selectedEffect.SetIsDigimonEffect(true);

                                    yield return ContinuousController.instance.StartCoroutine(
                                        ((ActivateICardEffect)selectedEffect)
                                        .Activate_Optional_Effect_Execute(effectHashtable));
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
                activateClass.SetUpICardEffect("Place 1 card in hand or trash as source, to activate its [On Play] effect",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, 1, true,
                    EffectDescription());
                activateClass.SetHashString("AddSource_EX9_073");
                cardEffects.Add(activateClass);

                string EffectDescription() =>
                    "[When Digivolving] [Once Per Turn] By placing 1 level 5 [Cyborg] or [Ver.5] trait card from your hand or trash as this Digimon's top digivolution card, active 1 [On Play] effect on the placed card as an effect of this Digimon.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardConditionShared);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardConditionShared);

                    if (canSelectHand && canSelectTrash)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>
                        {
                            new(message: "From hand", value: true, spriteIndex: 0),
                            new(message: "From trash", value: false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "From which area do you choose a card?";
                        string notSelectPlayerMessage = "The opponent is choosing from which area to choose a card.";

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
                            canTargetCondition: CanSelectCardConditionShared,
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

                        selectHandEffect.SetUpCustomMessage("Select 1 card to place on top of digivolution cards.",
                            "The opponent is selecting 1 card to place on top of digivolution cards.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Placed Card");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                    }
                    else
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardConditionShared,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to place on top of digivolution cards.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to place on top of digivolution cards.",
                            "The opponent is selecting 1 card to place on top of digivolution cards.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Placed Card");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }

                    if (selectedCard)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard()
                            .AddDigivolutionCardsTop(new List<CardSource>() { selectedCard }, activateClass));

                        List<ICardEffect> candidateEffects = selectedCard.cEntity_EffectController.GetCardEffects_ExceptAddedEffects(EffectTiming.OnEnterFieldAnyone, card)
                            .Clone()
                            .Filter(cardEffect =>
                                cardEffect is ActivateICardEffect && !cardEffect.IsSecurityEffect &&
                                cardEffect.IsOnPlay);

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
                                    canTargetCondition: _ => true,
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
                                Hashtable effectHashtable = CardEffectCommons.OnPlayCheckHashtableOfCard(card);

                                if (selectedEffect.CanUse(effectHashtable))
                                {
                                    selectedEffect.SetIsDigimonEffect(true);

                                    yield return ContinuousController.instance.StartCoroutine(
                                        ((ActivateICardEffect)selectedEffect)
                                        .Activate_Optional_Effect_Execute(effectHashtable));
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 card in hand or trash as source, to activate its [On Play] effect",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, 1, true,
                    EffectDescription());
                activateClass.SetHashString("AddSource_EX9_073");
                cardEffects.Add(activateClass);

                string EffectDescription() =>
                    "[When Attacking] [Once Per Turn] By placing 1 level 5 [Cyborg] or [Ver.5] trait card from your hand or trash as this Digimon's top digivolution card, active 1 [On Play] effect on the placed card as an effect of this Digimon.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardConditionShared);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardConditionShared);

                    if (canSelectHand && canSelectTrash)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>
                        {
                            new(message: "From hand", value: true, spriteIndex: 0),
                            new(message: "From trash", value: false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "From which area do you choose a card?";
                        string notSelectPlayerMessage = "The opponent is choosing from which area to choose a card.";

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
                            canTargetCondition: CanSelectCardConditionShared,
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

                        selectHandEffect.SetUpCustomMessage("Select 1 card to place on top of digivolution cards.",
                            "The opponent is selecting 1 card to place on top of digivolution cards.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Placed Card");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                    }
                    else
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardConditionShared,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to place on top of digivolution cards.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to place on top of digivolution cards.",
                            "The opponent is selecting 1 card to place on top of digivolution cards.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Placed Card");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }

                    if (selectedCard)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard()
                            .AddDigivolutionCardsTop(new List<CardSource>() { selectedCard }, activateClass));

                        List<ICardEffect> candidateEffects = selectedCard.cEntity_EffectController.GetCardEffects_ExceptAddedEffects(EffectTiming.OnEnterFieldAnyone, card)
                            .Clone()
                            .Filter(cardEffect =>
                                cardEffect is ActivateICardEffect && !cardEffect.IsSecurityEffect &&
                                cardEffect.IsOnPlay);

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
                                    canTargetCondition: _ => true,
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
                                Hashtable effectHashtable = CardEffectCommons.OnPlayCheckHashtableOfCard(card);

                                if (selectedEffect.CanUse(effectHashtable))
                                {
                                    selectedEffect.SetIsDigimonEffect(true);

                                    yield return ContinuousController.instance.StartCoroutine(
                                        ((ActivateICardEffect)selectedEffect)
                                        .Activate_Optional_Effect_Execute(effectHashtable));
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Trash 2 bottom digivolution cards to prevent this Digimon from leaving the battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription() =>
                    "[All Turns] When this Digimon would leave the battle area, by trashing its bottom 2 face-down or [Cyborg] trait digivolution cards, it doesn't leave.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                }

                bool CanSelectTrashSourceCardCondition(CardSource cardSource)
                {
                    return (cardSource.IsFlipped || cardSource.EqualsTraits("Cyborg")) &&
                           !cardSource.CanNotTrashFromDigivolutionCards(activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectTrashSourceCardCondition) >= 2;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();

                    int startingSources = thisPermanent.DigivolutionCards.Count;
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: thisPermanent, trashCount: 2, isFromTop: false, activateClass: activateClass, CanSelectTrashSourceCardCondition));

                    if (thisPermanent.DigivolutionCards.Count == startingSources - 2)
                    {
                        thisPermanent.willBeRemoveField = false;
                        thisPermanent.HideDeleteEffect();
                        thisPermanent.HideHandBounceEffect();
                        thisPermanent.HideDeckBounceEffect();
                        thisPermanent.HideWillRemoveFieldEffect();
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}