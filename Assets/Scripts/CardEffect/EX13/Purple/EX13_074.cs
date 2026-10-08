using System.Collections;
using System.Collections.Generic;

// Rie Kishibe
namespace DCGO.CardEffects.EX13
{
    public class EX13_074 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Turn
            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }
            #endregion

            #region All Turns - OPT
            if (timing == EffectTiming.OnEnterFieldAnyone || timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 1 [Knightmon] text Digimon card from hand or trash under this Tamer, <Draw 1>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("EX13_074_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When any of your [Knightmon] text Digimon are played or deleted, by placing 1 such card from your hand or trash under this Tamer, <Draw 1>.";

                bool PlayedPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasText("Knightmon");

                bool DeletedPermanentCondition(Permanent permanent)
                    => permanent.IsDigimon
                        && permanent.TopCard.Owner == card.Owner
                        && permanent.TopCard.HasText("Knightmon");

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.IsDigimon
                        && cardSource.HasText("Knightmon");

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (!CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)) return false;

                    return timing == EffectTiming.OnEnterFieldAnyone
                        ? CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PlayedPermanentCondition)
                        : CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, DeletedPermanentCondition, activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition)
                            || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition));

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);
                    bool fromHand = canSelectHand;
                    CardSource selectedCard = null;

                    if (canSelectHand && canSelectTrash)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new SelectionElement<int>(message: "From hand", value: 1, spriteIndex: 0),
                            new SelectionElement<int>(message: "From trash", value: 2, spriteIndex: 0),
                            new SelectionElement<int>(message: "Don't place", value: 3, spriteIndex: 1),
                        };

                        GManager.instance.userSelectionManager.SetIntSelection(
                            selectionElements: selectionElements,
                            selectPlayer: card.Owner,
                            selectPlayerMessage: "From which area do you place a card?",
                            notSelectPlayerMessage: "The opponent is choosing from which area to place a card.");

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedIntValue == 3)
                        {
                            activateClass.RemoveUse();
                            yield break;
                        }

                        fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                    }

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (fromHand && canSelectHand)
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

                        selectHandEffect.SetUpCustomMessage("Select 1 card to place under this Tamer.", "The opponent is selecting 1 card to place under their Tamer.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Placed Card");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                    }
                    else if (!fromHand && canSelectTrash)
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to place under this Tamer.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to place under this Tamer.", "The opponent is selecting 1 card to place under their Tamer.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Placed Card");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }

                    if (selectedCard == null || !CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        activateClass.RemoveUse();
                        yield break;
                    }

                    yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                        new List<CardSource>() { selectedCard }, activateClass));

                    if (CardEffectCommons.IsExistOnBattleArea(card) && card.PermanentOfThisCard().DigivolutionCards.Contains(selectedCard))
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    }
                }
            }
            #endregion

            #region Main - OPT
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Tamer may digivolve into [LordKnightmon] from hand or trash for 3", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("EX13_074_Main");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] [Once Per Turn] If this Tamer has 3 or more [Knightmon] text cards under it, it may digivolve into [LordKnightmon] in the hand or trash for a digivolution cost of 3, ignoring digivolution requirements.";

                bool IsKnightmonTextCardUnderTamer(CardSource cardSource)
                    => !cardSource.IsFaceDown
                        && cardSource.HasText("Knightmon");

                bool IsLordKnightmon(CardSource cardSource)
                    => cardSource.IsDigimon
                        && cardSource.EqualsCardName("LordKnightmon");

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.PermanentOfThisCard().DigivolutionCards.Filter(IsKnightmonTextCardUnderTamer).Count >= 3
                        && (CardEffectCommons.HasMatchConditionOwnersHand(card, IsLordKnightmon)
                            || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsLordKnightmon));

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, IsLordKnightmon);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsLordKnightmon);
                    bool isHand = canSelectHand;
                    bool digivolved = false;

                    if (canSelectHand && canSelectTrash)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new SelectionElement<int>(message: "From hand", value: 1, spriteIndex: 0),
                            new SelectionElement<int>(message: "From trash", value: 2, spriteIndex: 0),
                            new SelectionElement<int>(message: "Don't digivolve", value: 3, spriteIndex: 1),
                        };

                        GManager.instance.userSelectionManager.SetIntSelection(
                            selectionElements: selectionElements,
                            selectPlayer: card.Owner,
                            selectPlayerMessage: "From which area will you digivolve?",
                            notSelectPlayerMessage: "The opponent is choosing from which area to select a card.");

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedIntValue == 3)
                        {
                            activateClass.RemoveUse();
                            yield break;
                        }

                        isHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                    }

                    IEnumerator SuccessProcess()
                    {
                        digivolved = true;
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardCondition: IsLordKnightmon,
                        payCost: true,
                        reduceCostTuple: null,
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: 3,
                        isHand: isHand,
                        activateClass: activateClass,
                        successProcess: SuccessProcess()));

                    if (!digivolved) activateClass.RemoveUse();
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}
