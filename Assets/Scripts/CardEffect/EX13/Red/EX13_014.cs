using System.Collections;
using System.Collections.Generic;

// Jesmon
namespace DCGO.CardEffects.EX13
{
    public class EX13_014 : CEntity_Effect
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
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 5));
            }
            #endregion

            #region Shared When Digivolving / When Attacking

            string UseOptionEffectName = "May use 1 cost 5 or lower [Huckmon] text Option from hand or digivolution cards for free";

            string UseOptionEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] You may use 1 use cost 5 or lower [Huckmon] text Option card from your hand or this Digimon's digivolution cards without paying the cost.";

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

            bool UseOptionAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionOwnersHand(card, IsUsableHuckmonOption)
                    || HasUsableDigivolutionCard();

            IEnumerator UseOptionActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, IsUsableHuckmonOption);
                bool canSelectDigivolutionCards = HasUsableDigivolutionCard();

                List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                if (canSelectHand)
                    selectionElements.Add(new SelectionElement<int>(message: "From hand", value: 1, spriteIndex: 0));
                if (canSelectDigivolutionCards)
                    selectionElements.Add(new SelectionElement<int>(message: "From digivolution cards", value: 2, spriteIndex: 0));
                selectionElements.Add(new SelectionElement<int>(message: "Don't use an Option", value: 3, spriteIndex: 1));

                GManager.instance.userSelectionManager.SetIntSelection(
                    selectionElements: selectionElements,
                    selectPlayer: card.Owner,
                    selectPlayerMessage: "From which area will you use an Option?",
                    notSelectPlayerMessage: "The opponent is choosing from which area to use an Option.");

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                int selectedValue = GManager.instance.userSelectionManager.SelectedIntValue;

                CardSource selectedCard = null;

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCard = cardSource;
                    yield return null;
                }

                if (selectedValue == 1)
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
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectHandEffect.SetUpCustomMessage("Select 1 Option card to use.", "The opponent is selecting 1 Option card to use.");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                }
                else if (selectedValue == 2)
                {
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: IsUsableHuckmonOption,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
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

                if (selectedCard == null)
                {
                    activateClass.RemoveUse();
                    yield break;
                }

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                    cardSources: new List<CardSource> { selectedCard },
                    activateClass: activateClass,
                    payCost: false,
                    root: selectedValue == 1 ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.DigivolutionCards));
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                UseOptionEffectName,
                UseOptionActivateCoroutine,
                UseOptionEffectDescription,
                optional: false,
                isSkippable: true,
                additionalActivateCondition: UseOptionAdditionalActivateCondition,
                maxCountPerTurn: 1,
                hashValue: "EX13_014_WD_WA",
                whenDigivolving: true,
                whenAttacking: true);

            #endregion

            #region All Turns
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May delete 1 of opponent's lowest DP Digimon, then may play 1 [Atho, Rene & Por] Token", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("EX13_014_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When any of your Digimon are played, you may delete 1 of your opponent's lowest DP Digimon. Then, if you don't have [Atho, Rene & Por], you may play 1 [Atho, Rene & Por] Token. (Digimon/White/6000 DP/<Reboot>/<Blocker>/<Decoy (Red)/(Black)>)";

                bool IsOwnerDigimon(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool IsOpponentLowestDPDigimon(Permanent permanent)
                    => CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy);

                // Compared against the token's own data so the accented name can't drift out of sync.
                bool IsAthoRenePor(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && permanent.TopCard.EqualsCardName(ContinuousController.instance.AthoRenePorToken.CardName_ENG);

                bool CanPlayToken()
                    => !CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsAthoRenePor);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, IsOwnerDigimon);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsOpponentLowestDPDigimon)
                            || CanPlayToken());

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool isUsed = false;

                    #region May delete
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsOpponentLowestDPDigimon))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOpponentLowestDPDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            if (permanents.Count > 0) isUsed = true;
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                    #endregion

                    #region May play token
                    if (CanPlayToken())
                    {
                        GManager.instance.userSelectionManager.SetBoolSelection(
                            selectionElements: new List<SelectionElement<bool>>()
                            {
                                new SelectionElement<bool>(message: "Play Token", value: true, spriteIndex: 0),
                                new SelectionElement<bool>(message: "Don't play", value: false, spriteIndex: 1),
                            },
                            selectPlayer: card.Owner,
                            selectPlayerMessage: "Will you play 1 [Atho, Rene & Por] Token?",
                            notSelectPlayerMessage: "The opponent is choosing whether to play a Token.");

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedBoolValue)
                        {
                            isUsed = true;
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayAthoRenePorToken(activateClass));
                        }
                    }
                    #endregion

                    if (!isUsed) activateClass.RemoveUse();
                }
            }
            #endregion

            #region Assembly
            if (timing == EffectTiming.None)
            {
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect("Assembly", CanUseCondition, card);
                addAssemblyConditionClass.SetUpAddAssemblyConditionClass(getAssemblyCondition: GetAssembly);
                addAssemblyConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAssemblyConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                    => true;

                bool HasHuckmonText(CardSource assemblyCard)
                    => assemblyCard.HasText("Huckmon");

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource != card) return null;

                    AssemblyConditionElement level5Element = new AssemblyConditionElement(
                        assemblyCard => HasHuckmonText(assemblyCard) && assemblyCard.Level_Assembly.Contains(5),
                        selectMessage: "1 level 5 card with [Huckmon] in its text",
                        elementCount: 1);
                    AssemblyConditionElement level4Element = new AssemblyConditionElement(
                        assemblyCard => HasHuckmonText(assemblyCard) && assemblyCard.Level_Assembly.Contains(4),
                        selectMessage: "1 level 4 card with [Huckmon] in its text",
                        elementCount: 1);
                    AssemblyConditionElement level3Element = new AssemblyConditionElement(
                        assemblyCard => HasHuckmonText(assemblyCard) && assemblyCard.Level_Assembly.Contains(3),
                        selectMessage: "1 level 3 card with [Huckmon] in its text",
                        elementCount: 1);

                    return new AssemblyCondition(
                        elements: new List<AssemblyConditionElement>() { level5Element, level4Element, level3Element },
                        reduceCost: 5);
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
