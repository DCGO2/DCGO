using System;
using System.Collections;
using System.Collections.Generic;

// Murasamemon // Gonozan: Murashigure
namespace DCGO.CardEffects.BT26
{
    public class BT26_031 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Effects

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsTraits("Glowing Dawn");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 4));
            }
            #endregion

            #region When Digivolving - Trash most security's top card
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing top security of player with most, opponent's Digimon/Tamer can't suspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Digivolving] By trashing the top security card of 1 player with the most security cards, 1 of your opponent's Digimon or Tamers can't suspend until their turn ends.";

                bool CanSelectDebuffTargetCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                        && (permanent.IsDigimon || permanent.IsTamer);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectDebuffTargetCondition)
                        && (card.Owner.SecurityCards.Count > 0 || card.Owner.Enemy.SecurityCards.Count > 0);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool validOwnerSecurity = card.Owner.SecurityCards.Count > 0 && card.Owner.SecurityCards.Count >= card.Owner.Enemy.SecurityCards.Count;
                    bool validEnemySecurity = card.Owner.Enemy.SecurityCards.Count > 0 && card.Owner.Enemy.SecurityCards.Count >= card.Owner.SecurityCards.Count;

                    if (validOwnerSecurity || validEnemySecurity)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                        if (validOwnerSecurity) selectionElements.Add(new SelectionElement<int>(message: "Trash your own top security card", value: 1, spriteIndex: 0));
                        if (validEnemySecurity) selectionElements.Add(new SelectionElement<int>(message: "Trash your opponent's top security card", value: 2, spriteIndex: 0));
                        selectionElements.Add(new SelectionElement<int>(message: "Don't trash security", value: 3, spriteIndex: 1));

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: "Will you trash 1 player's top security card?", notSelectPlayerMessage: "The opponent is choosing whether to trash a security card.");
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool doTrash = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                        bool ownSecurity = GManager.instance.userSelectionManager.SelectedIntValue == 1;

                        if (doTrash)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashSecurityAndProcessAccordingToResult(
                                player: ownSecurity ? card.Owner : card.Owner.Enemy,
                                trashAmount: 1,
                                activateClass: activateClass,
                                fromTop: true,
                                successProcess: SuccessProcess,
                                failureProcess: null));

                            IEnumerator SuccessProcess(List<CardSource> cardSources)
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDebuffTargetCondition))
                                {
                                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectDebuffTargetCondition));

                                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectDebuffTargetCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: false,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: SelectPermanentCoroutine,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or Tamer that can't suspend.", "The opponent is selecting 1 Digimon or Tamer that can't suspend.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCantSuspendUntilOpponentTurnEnd(permanent, activateClass));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Shared When Digivolving / When Attacking - Recovery

            string SharedEffectName()
                => "By trashing a Tamer's bottom face-down card, Recovery +1";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] By trashing the bottom face-down card from under any of your Tamers, <Recovery +1>.";

            bool IsTamerWithFaceDownCard(ICardEffect activateClass, Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.HasFaceDownDigivolutionCards
                    && !permanent.ImmuneFromStackTrashing(activateClass);

            bool FaceDownCards(CardSource cardSource) => cardSource.IsFaceDown;

            bool SharedAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionPermanent(permanent => IsTamerWithFaceDownCard(activateClass, permanent));

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool IsTamerWithFaceDownCardBound(Permanent permanent) => IsTamerWithFaceDownCard(activateClass, permanent);

                if (CardEffectCommons.HasMatchConditionPermanent(IsTamerWithFaceDownCardBound))
                {
                    Permanent selectedTamer = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsTamerWithFaceDownCardBound,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedTamer = permanent;
                        yield return null;
                    }

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer to trash its bottom face-down card.", "The opponent is selecting 1 Tamer to trash its bottom face-down card.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    if (selectedTamer != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedTamer, trashCount: 1, isFromTop: false, activateClass: activateClass, cardCondition: FaceDownCards));
                        yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                    }
                }
            }

            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                maxCountPerTurn: 1,
                hashValue: "BT26_031_WD_WA",
                additionalActivateCondition: SharedAdditionalActivateCondition,
                whenDigivolving: true,
                whenAttacking: true);

            #endregion

            #region Option Effects

            #region Use Req.
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource)
                    => cardSource.ContainsTraits("Glowing Dawn");
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 opponent's Digimon -8000 DP, then by trashing top security, further -5000 DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] 1 of your opponent's Digimon gets -8000 DP until their turn ends. By trashing your top security card, it further gets -5000 DP.";

                bool CanSelectDebuffTargetCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDebuffTargetCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectDebuffTargetCondition));

                        Permanent selectedTarget = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDebuffTargetCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedTarget = permanent;
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: -8000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -8000.", "The opponent is selecting 1 Digimon that will get DP -8000.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedTarget != null && card.Owner.SecurityCards.Count >= 1)
                        {
                            CardSource topSecurityCard = card.Owner.SecurityCards[0];

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: _ => true,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                message: "Trash your top security card for a further -5000 DP?",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Discard,
                                root: SelectCardEffect.Root.Security,
                                customRootCardList: new List<CardSource>() { topSecurityCard },
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Trash your top security card for a further -5000 DP?", "The opponent is deciding whether to trash their top security card.");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                            {
                                if (cardSources.Count >= 1 && CardEffectCommons.IsPermanentExistsOnBattleArea(selectedTarget))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: selectedTarget, changeValue: -5000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Arts Digivolution
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            }
            #endregion

            #endregion

            return cardEffects;
        }
    }
}
