using System.Collections;
using System.Collections.Generic;

// Kentaurosmon
namespace DCGO.CardEffects.EX13
{
    public class EX13_036 : CEntity_Effect 
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Holy Beast")
                        || targetPermanent.TopCard.EqualsTraits("DATA SQUAD");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Shared Bool
            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }
            #endregion

            #region Shared Sec/OP
            string SharedEffectName = "1 enemy Digimon gets -7K DP, if 6 or less sec cards all their Digimon get -7K DP instead";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    security: true,
                    onPlay: true);

            string SharedEffectDescription(string tag) => $"[{tag}] 1 of your opponent's Digimon gets -7000 DP for the turn. If there are 6 or fewer total cards in both players' security stacks, instead all of their Digimon get -7000 DP for the turn.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (card.Owner.SecurityCards.Count + card.Owner.Enemy.SecurityCards.Count <= 6)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDPPlayerEffect(
                    permanentCondition: CanSelectPermanentCondition,
                    changeValue: -7000,
                    effectDuration: EffectDuration.UntilEachTurnEnd,
                    activateClass: activateClass));
                }
                else if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage(customMessageArray: CardEffectCommons.customPermanentMessageArray_ChangeDP(changeValue: -7000, maxCount: 1));

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            targetPermanent: permanent,
                            changeValue: -7000,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass));
                    }
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May trash 1 opponent's security. Then, this may unsuspend.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Digivolving] You may trash any 1 of your opponent's security cards. Then, this Digimon may unsuspend.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && (card.Owner.SecurityCards.Count > 0
                            || card.Owner.Enemy.SecurityCards.Count > 0);
                }

                bool CanBeEffectCandidate(ICardEffect cardEffect)
                {
                    if (cardEffect != null
                    && cardEffect is ActivateICardEffect
                    && cardEffect.IsSecurityEffect)
                    {
                        Hashtable securityHashtable = CardEffectCommons.SecurityCheckHashtableOfCard(cardEffect.EffectSourceCard);

                        return cardEffect.CanUse(securityHashtable);
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool validOwnerSecurity = card.Owner.SecurityCards.Count > 0 && card.Owner.SecurityCards.Count >= card.Owner.Enemy.SecurityCards.Count;
                    bool validEnemySecurity = card.Owner.Enemy.SecurityCards.Count > 0 && card.Owner.Enemy.SecurityCards.Count >= card.Owner.SecurityCards.Count;

                    if (validOwnerSecurity || validEnemySecurity)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                        if (validOwnerSecurity)
                        {
                            selectionElements.Add(new(message: $"Trash your own top security card", value: 1, spriteIndex: 0));
                        }
                        if (validEnemySecurity)
                        {
                            selectionElements.Add(new(message: $"Trash your opponent's top security card", value: 2, spriteIndex: 0));
                        }
                        selectionElements.Add(new(message: $"Don't trash security", value: 3, spriteIndex: 1));

                        string selectPlayerMessage = "Will you trash 1 player's security?";
                        string notSelectPlayerMessage = "The opponent is choosing if they will trash a security.";

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

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
                                failureProcess: null
                            ));

                            IEnumerator SuccessProcess(List<CardSource> cardSources)
                            {
                                if (card.PermanentOfThisCard().IsDigimon)
                                {
                                    List<ICardEffect> candidateEffects = card.PermanentOfThisCard().EffectList(EffectTiming.OnEnterFieldAnyone)
                                    .Clone()
                                    .Filter(CanBeEffectCandidate);

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
                                        if (selectedEffect != null
                                        && selectedEffect.EffectSourceCard != null
                                        && selectedEffect.EffectSourceCard.PermanentOfThisCard() != null)
                                        {
                                            Hashtable securityHashtable = CardEffectCommons.WhenDigivolvingCheckHashtableOfCard(selectedEffect.EffectSourceCard);

                                            if (selectedEffect.CanUse(securityHashtable))
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(((ActivateICardEffect)selectedEffect).Activate_Optional_Effect_Execute(securityHashtable));
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

            #region Shared WD/EoA/C
            string SharedEffectName2 = "Place 1 of each player's Digimon as top sec";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName2,
                    SharedActivateCoroutine2,
                    SharedEffectDescription2,
                    additionalActivateCondition: AdditionalActivateCondition,
                    hashValue: "EX13_036_WD_EoA_C",
                    maxCountPerTurn: 1,
                    optional: true,
                    whenDigivolving: true,
                    endOfAttack: true,
                    counter: true);

            string SharedEffectDescription2(string tag) => $"[{tag}] [Once Per Turn] You may place 1 of each player's Digimon as the top security cards.";

            bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnerPermanentCondition)
                    || CardEffectCommons.HasMatchConditionPermanent(CanSelectEnemyPermanentCondition);

            bool CanSelectOwnerPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card);

            bool CanSelectEnemyPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card);

            IEnumerator SharedActivateCoroutine2(Hashtable hashtable, ActivateClass activateClass)
            {
                List<Permanent> selectedPermanents = new List<Permanent>();
                bool isUsed = false;
                bool canNoSelect = true;

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnerPermanentCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOwnerPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCondition,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to place in security.", "The opponent is selecting 1 Digimon to place in security.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectEnemyPermanentCondition))
                {
                    if (canNoSelect == true) canNoSelect = !CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnerPermanentCondition);

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectEnemyPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: canNoSelect,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCondition,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to place in security.", "The opponent is selecting 1 Digimon to place in security.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }

                IEnumerator AfterSelectPermanentCondition(List<Permanent> permanents)
                {
                    if (permanents.Count == 1)
                    {
                        selectedPermanents.Add(permanents[0]);
                        isUsed = true;
                    }
                    else
                    {
                        canNoSelect = false;
                    }

                    yield return null;
                }

                if (selectedPermanents.Count > 0)
                {
                    // Code for sending multiple entities to security simultaneously
                }

                if (!isUsed) activateClass.RemoveUse();
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

                bool IsAssemblyCard(CardSource assemblyCard)
                    => assemblyCard != null
                        && assemblyCard.Owner == card.Owner
                        && assemblyCard.HasCardColor(CardColor.Yellow)
                        && assemblyCard.EqualsTraits("Holy Beast");

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource != card) return null;

                    AssemblyConditionElement level5Element = new AssemblyConditionElement(assemblyCard => IsAssemblyCard(assemblyCard) && assemblyCard.IsLevel5, elementCount: 1);
                    AssemblyConditionElement level4Element = new AssemblyConditionElement(assemblyCard => IsAssemblyCard(assemblyCard) && assemblyCard.IsLevel4, elementCount: 1);
                    AssemblyConditionElement level3Element = new AssemblyConditionElement(assemblyCard => IsAssemblyCard(assemblyCard) && assemblyCard.IsLevel3, elementCount: 1);

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
