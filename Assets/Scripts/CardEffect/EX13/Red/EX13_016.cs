using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Omnimon
namespace DCGO.CardEffects.EX13
{
    public class EX13_016 : CEntity_Effect 
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasCSTraits;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 5, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 6));
            }
            #endregion

            #region DNA Digivolution
            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect("DNA Digivolution", CanUseCondition, card);
                addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
                addJogressConditionClass.SetNotShowUI(true);
                cardEffects.Add(addJogressConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                    => true;

                JogressCondition GetJogress(CardSource cardSource)
                {
                    if (cardSource != card) return null;

                    bool GreenLevel6Condition(Permanent permanent)
                        => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                            && permanent.TopCard.HasGreymonName
                            && permanent.Levels_ForJogress(card).Contains(6);

                    bool BlueLevel6Condition(Permanent permanent)
                        => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                            && permanent.TopCard.HasGarurumonName
                            && permanent.Levels_ForJogress(card).Contains(6);

                    JogressConditionElement[] elements =
                    {
                        new JogressConditionElement(GreenLevel6Condition, "a level 6 with Greymon in name"),
                        new JogressConditionElement(BlueLevel6Condition, "a level 6 with Garurumon in name"),
                    };

                    return new JogressCondition(elements, 0);
                }
            }
            #endregion

            #region Raid
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(false, card, null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(false, card, null));
            }
            #endregion

            #region Shared OP/WD/WA
            string SharedEffectName1 = "2 enemy Digimon/Tamers can't suspend until their turn ends";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName1,
                    SharedActivateCoroutine1,
                    SharedEffectDescription1,
                    hashValue: "EX13_016_Shared_1",
                    maxCountPerTurn: 1,
                    optional: false,
                    onPlay: true,
                    whenDigivolving: true,
                    whenAttacking: true);

            string SharedEffectDescription1(string tag) => $"[{tag}] [Once Per Turn] 2 of your opponent's Digimon or Tamers can't suspend until their turn ends.";

            IEnumerator SharedActivateCoroutine1(Hashtable hashtable, ActivateClass activateClass)
            {
                bool CanSelectPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon
                        || permanent.IsTamer);

                int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectPermanentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 2 Digimon or Tamers that can't suspend.", "The opponent is selecting 2 Digimon or Tamers that can't suspend.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCantSuspendUntilOpponentTurnEnd(permanent, activateClass));
                }
            }
            #endregion

            #region Shared OP/WD/C
            string SharedEffectName2 = "Delete 1 enemy Digimon";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName2,
                    SharedActivateCoroutine2,
                    SharedEffectDescription2,
                    additionalActivateCondition: (hashtable, activateClass) => CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition) > 0,
                    hashValue: "EX13_016_Shared_2",
                    maxCountPerTurn: 1,
                    optional: false,
                    isSkippable: true,
                    onPlay: true,
                    whenDigivolving: true,
                    whenAttacking: true);

            string SharedEffectDescription2(string tag) => $"[{tag}] [Once Per Turn] You may delete 1 of your opponent's Digimon with as many digivolution cards as this Digimon or fewer.";

            bool CanSelectPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                && permanent.DigivolutionCards.Count <= card.PermanentOfThisCard().DigivolutionCards.Count;

            IEnumerator SharedActivateCoroutine2(Hashtable hashtable, ActivateClass activateClass)
            {
                bool isUsed = false;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectPermanentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Destroy,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    isUsed = true;
                    yield return null;
                }

                if (!isUsed) activateClass.RemoveUse();
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 2 same-level cards from source to not leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When this Digimon would leave the battle area, by trashing 2 same-level cards from its digivolution cards, it doesn't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                    && card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 2)
                    { 
                        List<CardSource> canSelectCards = new List<CardSource>();

                        foreach (CardSource cardSource in card.PermanentOfThisCard().DigivolutionCards)
                        {
                            canSelectCards.Add(cardSource);
                        }

                        if (canSelectCards.Count >= 2)
                        {
                            List<CardSource[]> cardsList = ParameterComparer.Enumerate(canSelectCards, 2).ToList();

                            foreach (CardSource[] cardSources in cardsList)
                            {
                                if (cardSources.Length == 2
                                && cardSources[0].Level == cardSources[1].Level
                                && cardSources[0].HasLevel && cardSources[1].HasLevel)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (!cardSource.CanNotTrashFromDigivolutionCards(activateClass)
                    && CardEffectCommons.IsExistOnBattleArea(card)
                    && card.PermanentOfThisCard().DigivolutionCards.Contains(cardSource))
                    {
                        foreach (CardSource cardSource1 in card.PermanentOfThisCard().DigivolutionCards)
                        {
                            if (cardSource != cardSource1
                            && cardSource.Level == cardSource1.Level
                            && !cardSource1.CanNotTrashFromDigivolutionCards(activateClass)
                            && cardSource.HasLevel
                            && cardSource1.HasLevel)
                            {
                                return true;
                            }
                             
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                        canEndSelectCondition: CanEndSelectCondition,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select cards to trash.",
                        maxCount: 2,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.DigivolutionCards,
                        customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetNotShowCard();
                    yield return StartCoroutine(selectCardEffect.Activate());

                    bool CanEndSelectCondition(List<CardSource> cardSources)
                    {
                        if (CardEffectCommons.HasNoElement(cardSources))
                        {
                            return false;
                        }

                        List<int> levels = cardSources
                        .Map(cardSource1 => cardSource1.Level)
                        .Distinct()
                        .ToList();

                        if (levels.Count > 1)
                        {
                            return false;
                        }

                        return true;
                    }

                    bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                    {
                        List<int> levels = cardSources
                        .Map(cardSource1 => cardSource1.Level)
                        .Concat(new List<int>() { cardSource.Level })
                        .Distinct()
                        .ToList();

                        if (levels.Count > 1)
                        {
                            return false;
                        }

                        return true;
                    }

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);
                        yield return null;
                    }

                    if (selectedCards.Count == 2)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsAndProcessAccordingToResult(
                            targetPermanent: card.PermanentOfThisCard(),
                            targetDigivolutionCards: selectedCards,
                            activateClass: activateClass,
                            successProcess: SuccessProcess,
                            failureProcess: null));

                        IEnumerator SuccessProcess(List<CardSource> trashedSources)
                        {
                            card.PermanentOfThisCard().willBeRemoveField = false;

                            card.PermanentOfThisCard().HideHandBounceEffect();
                            card.PermanentOfThisCard().HideDeckBounceEffect();
                            card.PermanentOfThisCard().HideWillRemoveFieldEffect();
                            card.PermanentOfThisCard().HideDeleteEffect();

                            yield return null;
                        }
                    }
                }
            }
            #endregion

            #region Assembly
            if (timing == EffectTiming.None)
            {
                string cardName1 = "WarGreymon";
                string cardName2 = "MetalGarurumon";
                string cardName3 = "Agumon";
                string cardName4 = "Gabumon";

                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect($"Assembly", CanUseCondition, card);
                addAssemblyConditionClass.SetUpAddAssemblyConditionClass(getAssemblyCondition: GetAssembly);
                addAssemblyConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAssemblyConditionClass);

                bool CanUseCondition(Hashtable hashtable) => true;

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        AssemblyConditionElement element1 = new AssemblyConditionElement(CanSelectCardCondition1, selectMessage: $"[{cardName1}]", elementCount: 1);
                        AssemblyConditionElement element2 = new AssemblyConditionElement(CanSelectCardCondition2, selectMessage: $"[{cardName2}]", elementCount: 1);
                        AssemblyConditionElement element3 = new AssemblyConditionElement(CanSelectCardCondition3, selectMessage: $"[{cardName3}]", elementCount: 1);
                        AssemblyConditionElement element4 = new AssemblyConditionElement(CanSelectCardCondition4, selectMessage: $"[{cardName4}]", elementCount: 1);

                        bool CanSelectCardCondition1(CardSource cardSource)
                        {
                            return cardSource != null
                                && cardSource.Owner == card.Owner
                                && cardSource.EqualsCardName(cardName1);
                        }

                        bool CanSelectCardCondition2(CardSource cardSource)
                        {
                            return cardSource != null
                                && cardSource.Owner == card.Owner
                                && cardSource.EqualsCardName(cardName2);
                        }

                        bool CanSelectCardCondition3(CardSource cardSource)
                        {
                            return cardSource != null
                                && cardSource.Owner == card.Owner
                                && cardSource.EqualsCardName(cardName3);
                        }

                        bool CanSelectCardCondition4(CardSource cardSource)
                        {
                            return cardSource != null
                                && cardSource.Owner == card.Owner
                                && cardSource.EqualsCardName(cardName4);
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            elements: new List<AssemblyConditionElement>() { element1, element2, element3, element4 },
                            reduceCost: 7);

                        return assemblyCondition;
                    }

                    return null;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
