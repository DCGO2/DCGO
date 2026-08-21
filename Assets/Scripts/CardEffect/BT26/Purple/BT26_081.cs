using System;
using System.Collections;
using System.Collections.Generic;

// Mervamon
namespace DCGO.CardEffects.BT26
{
    public class BT26_081 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region All Turns - Grant to Iliad Digimon

            bool IsOwnIliadDigimon(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.EqualsTraits("Iliad");

            bool GrantCondition() => CardEffectCommons.IsExistOnBattleArea(card);

            #region Alliance
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceStaticEffect(IsOwnIliadDigimon, false, card, GrantCondition));
            }
            #endregion

            #region Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootStaticEffect(IsOwnIliadDigimon, false, card, GrantCondition));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerStaticEffect(IsOwnIliadDigimon, false, card, GrantCondition));
            }
            #endregion

            #region DP +2000
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeBaseDPGlobalEffect(IsOwnIliadDigimon, 2000, false, card, GrantCondition));
            }
            #endregion

            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName() => "Play up to 8 cost worth of [Iliad] cards, then -4000 DP per [Iliad]/[TS] Digimon or Tamer";

            string SharedEffectDescription(string tag)
                => $"[{tag}] You may play up to 8 play cost's total worth of [Iliad] trait cards from your hand or trash without paying the cost. Then, to 1 of your opponent's Digimon, give -4000 DP until their turn ends for each of your [Iliad] or [TS] trait Digimon or Tamers.";

            bool CanSelectDPTargetCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            bool IliadOrTSPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer)
                    && (permanent.TopCard.EqualsTraits("Iliad") || permanent.TopCard.HasTSTraits);

            IEnumerator PlayUpToBudgetCoroutine(int totalCost, SelectCardEffect.Root root, ActivateClass activateClass, Action<int> onSpent)
            {
                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.EqualsTraits("Iliad")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, root: root, isPlayOption: true);

                if (!CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition) && root == SelectCardEffect.Root.Trash) yield break;
                if (!CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition) && root == SelectCardEffect.Root.Hand) yield break;

                List<CardSource> selectedCards = new List<CardSource>();

                bool CanEndSelectCardCondition(List<CardSource> cards)
                {
                    int sumCost = 0;
                    foreach (CardSource source in cards) sumCost += source.GetCostItself;
                    return sumCost <= totalCost;
                }

                bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                {
                    int sumCost = cardSource.GetCostItself;
                    foreach (CardSource cardSource1 in cardSources) sumCost += cardSource1.GetCostItself;
                    return sumCost <= totalCost;
                }

                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                selectCardEffect.SetUp(
                    canTargetCondition: CanSelectCardCondition,
                    canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                    canEndSelectCondition: CanEndSelectCardCondition,
                    canNoSelect: () => true,
                    selectCardCoroutine: SelectCardCoroutine,
                    afterSelectCardCoroutine: null,
                    message: $"Select up to {totalCost} play cost worth of [Iliad] trait cards to play from {(root == SelectCardEffect.Root.Hand ? "your hand" : "your trash")}.",
                    maxCount: -1,
                    canEndNotMax: true,
                    isShowOpponent: true,
                    mode: SelectCardEffect.Mode.Custom,
                    root: root,
                    customRootCardList: null,
                    canLookReverseCard: true,
                    selectPlayer: card.Owner,
                    cardEffect: activateClass);

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCards.Add(cardSource);
                    yield return null;
                }

                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                if (selectedCards.Count >= 1)
                {
                    int spent = 0;
                    foreach (CardSource source in selectedCards) spent += source.GetCostItself;
                    onSpent(spent);

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: selectedCards,
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: root,
                        activateETB: true));
                }
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.EqualsTraits("Iliad");

                if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition)
                    || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                {
                    int totalBudget = 8;
                    int spentFromHand = 0;

                    yield return ContinuousController.instance.StartCoroutine(PlayUpToBudgetCoroutine(totalBudget, SelectCardEffect.Root.Hand, activateClass, spent => spentFromHand = spent));

                    int remainingBudget = totalBudget - spentFromHand;

                    if (remainingBudget > 0)
                    {
                        yield return ContinuousController.instance.StartCoroutine(PlayUpToBudgetCoroutine(remainingBudget, SelectCardEffect.Root.Trash, activateClass, _ => { }));
                    }
                }

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDPTargetCondition))
                {
                    int countIliadOrTS = card.Owner.GetFieldPermanents().Filter(IliadOrTSPermanentCondition).Count;
                    int dpChange = -4000 * countIliadOrTS;

                    if (dpChange != 0)
                    {
                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDPTargetCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage($"Select 1 Digimon to give -{Math.Abs(dpChange)} DP.", "The opponent is selecting 1 Digimon to give DP to.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: selectedPermanent,
                                changeValue: dpChange,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));
                        }
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
                onPlay: true,
                whenDigivolving: true);

            #region Assembly
            if (timing == EffectTiming.None)
            {
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect("Assembly", CanUseCondition, card);
                addAssemblyConditionClass.SetUpAddAssemblyConditionClass(getAssemblyCondition: GetAssembly);
                addAssemblyConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAssemblyConditionClass);

                bool CanUseCondition(Hashtable hashtable) => true;

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        AssemblyConditionElement element = new AssemblyConditionElement(CanSelectCardCondition);

                        bool CanSelectCardCondition(CardSource cs)
                            => cs.EqualsCardName("Minervamon");

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            element: element,
                            CanTargetCondition_ByPreSelecetedList: null,
                            selectMessage: "w/[Minervamon] in name",
                            elementCount: 1,
                            reduceCost: 5);

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
