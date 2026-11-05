using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Atratusmon
namespace DCGO.CardEffects.BT27
{
    public class BT27_052 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - Lv.5 w/[Glowing Dawn] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasGlowingDawnTraits;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Alternate Digivolution Requirement - [Atratusmon] w/o play cost
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Atratusmon")
                        && !targetPermanent.TopCard.HasPlayCost;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
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

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource != card) return null;

                    AssemblyConditionElement level5Element = new AssemblyConditionElement(
                        assemblyCard => assemblyCard.HasGlowingDawnTraits && assemblyCard.Level_Assembly.Contains(5),
                        selectMessage: "1 level 5 card with the [Glowing Dawn] trait",
                        elementCount: 1);
                    AssemblyConditionElement level4Element = new AssemblyConditionElement(
                        assemblyCard => assemblyCard.HasGlowingDawnTraits && assemblyCard.Level_Assembly.Contains(4),
                        selectMessage: "1 level 4 card with the [Glowing Dawn] trait",
                        elementCount: 1);
                    AssemblyConditionElement level3Element = new AssemblyConditionElement(
                        assemblyCard => assemblyCard.HasGlowingDawnTraits && assemblyCard.Level_Assembly.Contains(3),
                        selectMessage: "1 level 3 card with the [Glowing Dawn] trait",
                        elementCount: 1);

                    return new AssemblyCondition(
                        elements: new List<AssemblyConditionElement>() { level5Element, level4Element, level3Element },
                        reduceCost: 5);
                }
            }
            #endregion

            #region Security A. +1
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
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

            #region Shared On Play / When Digivolving / When Attacking - OPT

            string BattleEffectName = "This Digimon may battle 1 opponent's Digimon";

            string BattleEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] This Digimon may battle 1 of your opponent's Digimon.";

            bool IsDefenderCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            bool BattleCanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionPermanent(IsDefenderCondition);

            IEnumerator BattleActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                Permanent selectedDefender = null;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: IsDefenderCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectDefenderCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 of your opponent's Digimon to battle.", "The opponent is selecting 1 Digimon to battle.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectDefenderCoroutine(Permanent permanent)
                {
                    selectedDefender = permanent;
                    yield return null;
                }

                Permanent thisPermanent = card.PermanentOfThisCard();

                if (selectedDefender != null && thisPermanent != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IBattle(thisPermanent, selectedDefender, null, true).Battle());
                }
                else
                {
                    activateClass.RemoveUse();
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                BattleEffectName,
                BattleActivateCoroutine,
                BattleEffectDescription,
                optional: false,
                isSkippable: true,
                additionalActivateCondition: BattleCanActivateCondition,
                maxCountPerTurn: 1,
                hashValue: "BT27_052_Battle",
                onPlay: true,
                whenDigivolving: true,
                whenAttacking: true);

            #endregion

            #region Shared On Play / When Digivolving

            string PlayEffectName = "By trashing 2 Tamer face-down cards, may play or use 1 cost 6 or lower [Glowing Dawn] card from hand or digivolution cards for free";

            string PlayEffectDescription(string tag)
                => $"[{tag}] By trashing 2 bottom face-down cards from under any of your Tamers, you may play or use 1 play or use cost 6 or lower [Glowing Dawn] trait card from your hand or your Digimon's digivolution cards without paying the cost.";

            bool FaceDownCards(CardSource cardSource) => cardSource.IsFaceDown;

            bool TamerWithOneFaceDownSource(ICardEffect activateClass, Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.DigivolutionCards.Any(FaceDownCards)
                    && !permanent.ImmuneFromStackTrashing(activateClass);

            bool CanPayFaceDownCost(ICardEffect activateClass)
                => card.Owner.GetBattleAreaPermanents()
                    .Where(permanent => TamerWithOneFaceDownSource(activateClass, permanent))
                    .Sum(permanent => permanent.DigivolutionCards.Count(FaceDownCards)) >= 2;

            bool PlayCanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CanPayFaceDownCost(activateClass);

            IEnumerator PlayActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Trash 2 bottom face-down cards
                bool paid = false;

                bool CanSelectTamerCondition(Permanent permanent)
                    => TamerWithOneFaceDownSource(activateClass, permanent);

                SelectPermanentEffect selectTamerEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(CanSelectTamerCondition));

                selectTamerEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectTamerCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: CanEndSelectCondition,
                    maxCount: maxCount,
                    canNoSelect: true,
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

                #region May play or use 1 [Glowing Dawn] card from hand or digivolution cards
                bool IsGlowingDawnCondition(CardSource cardSource)
                    => cardSource.HasGlowingDawnTraits
                        && cardSource.BasePlayCostFromEntity<= 6;

                bool CanSelectHandCondition(CardSource cardSource)
                    => IsGlowingDawnCondition(cardSource)
                        && CardEffectCommons.CanPlayOrUse(cardSource, activateClass);

                bool CanSelectSourceCondition(CardSource cardSource)
                    => !cardSource.IsFaceDown
                        && IsGlowingDawnCondition(cardSource)
                        && CardEffectCommons.CanPlayOrUse(cardSource, activateClass, SelectCardEffect.Root.DigivolutionCards);

                List<CardSource> digimonSources = card.Owner.GetBattleAreaDigimons()
                    .SelectMany(permanent => permanent.DigivolutionCards)
                    .ToList();

                bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectHandCondition);
                bool canSelectSources = digimonSources.Any(CanSelectSourceCondition);

                if (!canSelectHand && !canSelectSources) yield break;

                bool fromHand = canSelectHand;

                if (canSelectHand && canSelectSources)
                {
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                    {
                        new SelectionElement<int>(message: "From hand", value: 1, spriteIndex: 0),
                        new SelectionElement<int>(message: "From digivolution cards", value: 2, spriteIndex: 0),
                        new SelectionElement<int>(message: "Don't play or use", value: 3, spriteIndex: 1),
                    };

                    GManager.instance.userSelectionManager.SetIntSelection(
                        selectionElements: selectionElements,
                        selectPlayer: card.Owner,
                        selectPlayerMessage: "From which area do you play or use a card?",
                        notSelectPlayerMessage: "The opponent is choosing from which area to play or use a card.");

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    if (GManager.instance.userSelectionManager.SelectedIntValue == 3) yield break;

                    fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                }

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
                        canTargetCondition: CanSelectHandCondition,
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

                    selectHandEffect.SetUpCustomMessage("Select 1 card to play or use.", "The opponent is selecting 1 card to play or use.");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                }
                else
                {
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectSourceCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 card to play or use.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.DigivolutionCards,
                        customRootCardList: digimonSources,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 card to play or use.", "The opponent is selecting 1 card to play or use.");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                }

                if (selectedCard == null) yield break;

                SelectCardEffect.Root root = fromHand ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.DigivolutionCards;

                if (selectedCard.IsOption)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                        cardSources: new List<CardSource>() { selectedCard },
                        activateClass: activateClass,
                        payCost: false,
                        root: root));
                }
                else
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: new List<CardSource>() { selectedCard },
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: root,
                        activateETB: true));
                }
                #endregion
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                PlayEffectName,
                PlayActivateCoroutine,
                PlayEffectDescription,
                optional: false,
                isSkippable: true,
                additionalActivateCondition: PlayCanActivateCondition,
                onPlay: true,
                whenDigivolving: true);

            #endregion

            return cardEffects;
        }
    }
}
