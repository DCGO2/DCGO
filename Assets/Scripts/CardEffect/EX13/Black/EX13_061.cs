using System.Collections;
using System.Collections.Generic;

// Gankoomon
namespace DCGO.CardEffects.EX13
{
    public class EX13_061 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasText("Huckmon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 4,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 5));
            }
            #endregion

            #region Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName = "May play 1 [Hinukamuy] Token, then 1 of your white Digimon isn't affected by opponent's Digimon effects";

            string SharedEffectDescription(string tag)
                => $"[{tag}] You may play 1 [Hinukamuy] Token. (Digimon/White/6000 DP/<Alliance> <Reboot> <Blocker>) Then, until your opponent's turn ends, their Digimon effects don't affect 1 of your white Digimon.";

            bool IsOwnerWhiteDigimon(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.CardColors.Contains(CardColor.White);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region May play token
                GManager.instance.userSelectionManager.SetBoolSelection(
                    selectionElements: new List<SelectionElement<bool>>()
                    {
                        new SelectionElement<bool>(message: "Play Token", value: true, spriteIndex: 0),
                        new SelectionElement<bool>(message: "Don't play", value: false, spriteIndex: 1),
                    },
                    selectPlayer: card.Owner,
                    selectPlayerMessage: "Will you play 1 [Hinukamuy] Token?",
                    notSelectPlayerMessage: "The opponent is choosing whether to play a Token.");

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                if (GManager.instance.userSelectionManager.SelectedBoolValue)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayHinukamuyToken(activateClass));
                }
                #endregion

                #region Digimon effect immunity
                if (CardEffectCommons.HasMatchConditionPermanent(IsOwnerWhiteDigimon))
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsOwnerWhiteDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 white Digimon that isn't affected by opponent's Digimon effects.", "The opponent is selecting 1 white Digimon that isn't affected by opponent's Digimon effects.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        #region Give Digimon Effect Immunity
                        selectedPermanent.UntilOpponentTurnEndEffects.Add((_timing) => PermanentEffectFactory.DigimonEffectImmunity(selectedPermanent));
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(selectedPermanent));
                        #endregion
                    }
                }
                #endregion
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            #endregion

            #region All Turns - OPT
            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May use 1 cost 5 or lower [Huckmon] text Option from hand or digivolution cards for free", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("EX13_061_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When any of your white Digimon suspend, you may use 1 use cost 5 or lower Option card with [Huckmon] in its text from your hand or this Digimon's digivolution cards without paying the cost.";

                bool IsUsableHuckmonOption(CardSource cardSource)
                    => cardSource.IsOption
                        && cardSource.HasText("Huckmon")
                        && cardSource.GetCostItself <= 5
                        && !cardSource.CanNotPlayThisOption;

                bool HasUsableDigivolutionCard()
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();

                    return thisPermanent != null && thisPermanent.DigivolutionCards.Some(IsUsableHuckmonOption);
                }

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, IsOwnerWhiteDigimon);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && (CardEffectCommons.HasMatchConditionOwnersHand(card, IsUsableHuckmonOption)
                            || HasUsableDigivolutionCard());

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool isUsed = false;
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, IsUsableHuckmonOption);
                    bool canSelectDigivolutionCards = HasUsableDigivolutionCard();
                    bool fromHand = canSelectHand;

                    // Only ask for a location when both have a usable Option; otherwise go straight to the selection.
                    if (canSelectHand && canSelectDigivolutionCards)
                    {
                        GManager.instance.userSelectionManager.SetBoolSelection(
                            selectionElements: new List<SelectionElement<bool>>()
                            {
                                new SelectionElement<bool>(message: "From hand", value: true, spriteIndex: 0),
                                new SelectionElement<bool>(message: "From digivolution cards", value: false, spriteIndex: 1),
                            },
                            selectPlayer: card.Owner,
                            selectPlayerMessage: "From which area will you use an Option?",
                            notSelectPlayerMessage: "The opponent is choosing from which area to use an Option.");

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;
                    }

                    if (fromHand)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsUsableHuckmonOption,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 Option card to use.", "The opponent is selecting 1 Option card to use.");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                    }
                    else
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: IsUsableHuckmonOption,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            message: "Select 1 Option card to use.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.DigivolutionCards,
                            customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 Option card to use.", "The opponent is selecting 1 Option card to use.");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count == 0) yield break;

                        isUsed = true;

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                            cardSources: cardSources,
                            activateClass: activateClass,
                            payCost: false,
                            root: fromHand ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.DigivolutionCards));
                    }

                    if (!isUsed) activateClass.RemoveUse();
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
