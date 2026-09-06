using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Ravemon
namespace DCGO.CardEffects.BT26
{
    public class BT26_082 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement Crowmon
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Crowmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Alternate Digivolution Requirement DATA SQUAD
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("DATA SQUAD");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Security - End of Opponent's Turn
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play this card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Security] [End of Opponent's Turn] Play this card without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsOpponentTurn(card)
                        && CardEffectCommons.IsExistInSecurityTrigger(card, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistInSecurityActivate(card, activateClass)
                        && CardEffectCommons.CanPlayAsNewPermanent(card, false, activateClass, SelectCardEffect.Root.Security);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: new List<CardSource>() { card }, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Security, activateETB: true));
                }
            }
            #endregion

            #region Shared When Digivolving / End of Attack
            string SharedEffectName() => "By deleting this Digimon or trashing 2 Tamer's bottom face-down cards, delete 1 opponent's highest DP Digimon";

            string SharedEffectDescription(string tag)
                => $"[{tag}] By deleting this Digimon or trashing 2 bottom face-down cards from under any of your Tamers, delete 1 of your opponent's highest DP Digimon.";

            bool FaceDownCards(CardSource cardSource) => cardSource.IsFaceDown;

            bool CanSelectDeleteTargetCondition(Permanent permanent)
                => CardEffectCommons.IsMaxDP(permanent, card.Owner.Enemy, null);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool TamerWithOneFaceDownSource(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.DigivolutionCards.Any(FaceDownCards)
                        && !permanent.ImmuneFromStackTrashing(activateClass);

                bool TamerWith2OrMoreFaceDownSources(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.DigivolutionCards.Count(FaceDownCards) >= 2
                        && !permanent.ImmuneFromStackTrashing(activateClass);

                bool canTrashTamerCards = CardEffectCommons.HasMatchConditionPermanent(TamerWith2OrMoreFaceDownSources)
                    || CardEffectCommons.MatchConditionPermanentCount(TamerWithOneFaceDownSource) >= 2;

                List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                {
                    new SelectionElement<int>(message: "Delete this Digimon", value: 1, spriteIndex: 0),
                };

                if (canTrashTamerCards) selectionElements.Add(new SelectionElement<int>(message: "Trash 2 bottom face-down cards from under any of your Tamers", value: 2, spriteIndex: 0));
                selectionElements.Add(new SelectionElement<int>(message: "Don't pay the cost", value: 3, spriteIndex: 1));

                GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: "Will you pay the cost?", notSelectPlayerMessage: "The opponent is choosing to pay the cost.");
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                int selected = GManager.instance.userSelectionManager.SelectedIntValue;

                bool hasPaidCost = false;

                if (selected == 1)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass: activateClass,
                        successProcess: SuccessProcess,
                        failureProcess: null));

                    IEnumerator SuccessProcess(List<Permanent> permanents)
                    {
                        hasPaidCost = true;
                        yield return null;
                    }
                }
                else if (selected == 2)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(TamerWithOneFaceDownSource));

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: TamerWithOneFaceDownSource,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: CanEndSelectCondition,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: true,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select all Tamer(s) to trash bottom face-down cards from.", "The opponent is selecting Tamer(s) to trash bottom face-down cards from.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    bool CanEndSelectCondition(List<Permanent> permanents)
                    {
                        return permanents.Count == 2
                            || (permanents.Count > 0 && permanents[0].DigivolutionCards.Count(FaceDownCards) >= 2);
                    }

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        if (permanents.Count == 1)
                        {
                            hasPaidCost = true;
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanents[0], trashCount: 2, isFromTop: false, activateClass: activateClass, cardCondition: FaceDownCards));
                        }
                        else if (permanents.Count == 2)
                        {
                            hasPaidCost = true;
                            foreach (Permanent selectedPermanent in permanents)
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedPermanent, trashCount: 1, isFromTop: false, activateClass: activateClass, cardCondition: FaceDownCards));
                        }
                    }
                }

                if (hasPaidCost && CardEffectCommons.HasMatchConditionPermanent(CanSelectDeleteTargetCondition))
                {
                    SelectPermanentEffect selectDeleteEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectDeleteEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectDeleteTargetCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    selectDeleteEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectDeleteEffect.Activate());
                }
            }
            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                isSkippable: true,
                whenDigivolving: true,
                endOfAttack: true);

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Opponent trashes 1 hand card, then may place this as bottom security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Deletion] Your opponent trashes 1 card in their hand. Then, if their hand has 7 or fewer cards, you may place this card face up as the bottom security card.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.CanActivateOnDeletion(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.Enemy.HandCards.Count >= 1)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner.Enemy,
                            canTargetCondition: _ => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                    }

                    if (card.Owner.Enemy.HandCards.Count <= 7
                        && CardEffectCommons.IsExistOnTrash(card)
                        && card.Owner.CanAddSecurity(activateClass))
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: "Yes", value: true, spriteIndex: 0),
                            new SelectionElement<bool>(message: "No", value: false, spriteIndex: 1),
                        };

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: "Place this card face up as the bottom security card?", notSelectPlayerMessage: "The opponent is choosing whether to place this card face up as the bottom security card.");

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedBoolValue)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(card, toTop: false, faceUp: true));
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
