using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Nanami, Kouki, & Ivan
namespace DCGO.CardEffects.BT27
{
    public class BT27_092 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 1 [Kurata's Army] hand card face down under this Tamer, <Draw 1> and gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Start of Your Main Phase] By placing 1 [Kurata's Army] trait card from your hand face down under this Tamer, <Draw 1> and gain 1 memory.";

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.EqualsTraits("Kurata's Army");

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    CardSource selectedCard = null;

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

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    if (selectedCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass, isFacedown: true));

                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                        if (card.Owner.CanAddMemory(activateClass)) yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                    }
                }
            }
            #endregion

            #region Main - OPT
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 2 Tamer face-down cards, this Tamer may digivolve into [Kurata's Army] Digimon for 2 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT27_092_Main");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] [Once Per Turn] By trashing 2 bottom face-down cards from under any of your Tamers, this Tamer may digivolve into a [Kurata's Army] trait Digimon card in the hand or trash with the cost reduced by 2.";

                bool FaceDownCards(CardSource cardSource) => cardSource.IsFaceDown;

                bool TamerWithOneFaceDownSource(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.DigivolutionCards.Any(FaceDownCards)
                        && !permanent.ImmuneFromStackTrashing(activateClass);

                bool TamerWith2OrMoreFaceDownSources(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.DigivolutionCards.Count(FaceDownCards) >= 2
                        && !permanent.ImmuneFromStackTrashing(activateClass);

                bool CanPayFaceDownCost()
                    => CardEffectCommons.HasMatchConditionPermanent(TamerWith2OrMoreFaceDownSources)
                        || CardEffectCommons.MatchConditionPermanentCount(TamerWithOneFaceDownSource) >= 2;

                bool DigivolveCardCondition(CardSource cardSource)
                    => cardSource.IsDigimon
                        && cardSource.EqualsTraits("Kurata's Army");

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CanPayFaceDownCost();

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    #region Trash 2 bottom face-down cards
                    bool paid = false;

                    SelectPermanentEffect selectTamerEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(TamerWithOneFaceDownSource));

                    selectTamerEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: TamerWithOneFaceDownSource,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: CanEndSelectCondition,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: true,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectTamerEffect.SetUpCustomMessage("Select all Tamer(s) to trash bottom face-down cards from.", "The opponent is selecting Tamer(s) to trash bottom face-down cards from.");

                    yield return ContinuousController.instance.StartCoroutine(selectTamerEffect.Activate());

                    bool CanEndSelectCondition(List<Permanent> permanents)
                    {
                        return permanents.Count == 2
                            || (permanents.Count > 0 && permanents[0].DigivolutionCards.Count(FaceDownCards) >= 2);
                    }

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        if (permanents.Count == 1)
                        {
                            paid = true;
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanents[0], trashCount: 2, isFromTop: false, activateClass: activateClass, cardCondition: FaceDownCards));
                        }
                        else if (permanents.Count == 2)
                        {
                            paid = true;
                            foreach (Permanent selectedPermanent in permanents)
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedPermanent, trashCount: 1, isFromTop: false, activateClass: activateClass, cardCondition: FaceDownCards));
                        }
                    }

                    if (!paid) yield break;
                    #endregion

                    #region This Tamer may digivolve
                    Permanent thisTamer = card.PermanentOfThisCard();

                    if (thisTamer == null) yield break;

                    bool CanDigivolveFromHandCondition(CardSource cardSource)
                        => DigivolveCardCondition(cardSource)
                            && cardSource.CanPlayCardTargetFrame(thisTamer.PermanentFrame, true, activateClass, SelectCardEffect.Root.Hand);

                    bool CanDigivolveFromTrashCondition(CardSource cardSource)
                        => DigivolveCardCondition(cardSource)
                            && cardSource.CanPlayCardTargetFrame(thisTamer.PermanentFrame, true, activateClass, SelectCardEffect.Root.Trash);

                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanDigivolveFromHandCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanDigivolveFromTrashCondition);

                    if (!canSelectHand && !canSelectTrash) yield break;

                    bool isHand = canSelectHand;

                    if (canSelectHand && canSelectTrash)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new(message: "From hand", value: 1, spriteIndex: 0),
                            new(message: "From trash", value: 2, spriteIndex: 0),
                            new(message: "Don't digivolve", value: 3, spriteIndex: 1),
                        };

                        GManager.instance.userSelectionManager.SetIntSelection(
                            selectionElements: selectionElements,
                            selectPlayer: card.Owner,
                            selectPlayerMessage: "From which area will you digivolve?",
                            notSelectPlayerMessage: "The opponent is choosing from which area to select a card.");

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedIntValue == 3) yield break;

                        isHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: thisTamer,
                        cardCondition: DigivolveCardCondition,
                        payCost: true,
                        reduceCostTuple: (reduceCost: 2, reduceCostCardCondition: null),
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: isHand,
                        activateClass: activateClass,
                        successProcess: null));
                    #endregion
                }
            }
            #endregion

            #region End of Attack - OPT - ESS
            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By choosing 1 of your Digimon, delete it and 1 opponent's Digimon with as much DP as this Digimon or less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT27_092_ESS");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[End of Attack] [Once Per Turn] By choosing 1 of your Digimon, delete it and 1 of your opponent's Digimon with as much DP as this Digimon or less.";

                bool CanSelectOwnDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnEndAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();
                    int thisDP = thisPermanent != null && thisPermanent.HasDP ? thisPermanent.DP : -1;

                    bool CanSelectOpponentCondition(Permanent permanent)
                        => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                            && permanent.HasDP
                            && permanent.DP <= thisDP;

                    Permanent selectedOwnDigimon = null;

                    SelectPermanentEffect selectOwnEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectOwnEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOwnDigimonCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectOwnCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectOwnEffect.SetUpCustomMessage("Select 1 of your Digimon to delete.", "The opponent is selecting 1 of their Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectOwnEffect.Activate());

                    IEnumerator SelectOwnCoroutine(Permanent permanent)
                    {
                        selectedOwnDigimon = permanent;
                        yield return null;
                    }

                    if (selectedOwnDigimon == null)
                    {
                        activateClass.RemoveUse();
                        yield break;
                    }

                    List<Permanent> deleteTargets = new List<Permanent>() { selectedOwnDigimon };

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentCondition))
                    {
                        SelectPermanentEffect selectOpponentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectOpponentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOpponentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectOpponentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectOpponentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectOpponentEffect.Activate());

                        IEnumerator SelectOpponentCoroutine(Permanent permanent)
                        {
                            deleteTargets.Add(permanent);
                            yield return null;
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(deleteTargets, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
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
