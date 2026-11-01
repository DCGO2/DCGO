using System;
using System.Collections;
using System.Collections.Generic;

// Breakdramon
namespace DCGO.CardEffects.EX13
{
    public class EX13_044 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Groundramon") || targetPermanent.TopCard.EqualsCardName("Wingdramon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
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
                        && (assemblyCard.HasText("Dracomon") || assemblyCard.HasText("Examon"));

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

            #region Shared On Play / When Digivolving

            string SharedEffectName = "May suspend up to 2 Digimon/Tamers, then 2 opponent's Digimon/Tamers can't unsuspend";

            string SharedEffectDescription(string tag)
                => $"[{tag}] You may suspend up to 2 Digimon or Tamers. Then, 2 of your opponent's Digimon or Tamers can't unsuspend until their turn ends.";

            bool CanSelectSuspendPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnBattleArea(permanent)
                    && (permanent.IsDigimon || permanent.IsTamer)
                    && !permanent.IsSuspended;

            bool CanSelectOpponentPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                SelectPermanentEffect suspendSelectEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                int suspendCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(CanSelectSuspendPermanentCondition));

                suspendSelectEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectSuspendPermanentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: suspendCount,
                    canNoSelect: true,
                    canEndNotMax: true,
                    selectPermanentCoroutine: null,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Tap,
                    cardEffect: activateClass);

                suspendSelectEffect.SetUpCustomMessage("Select up to 2 Digimon or Tamers to suspend.", "The opponent is selecting up to 2 Digimon or Tamers to suspend.");

                yield return ContinuousController.instance.StartCoroutine(suspendSelectEffect.Activate());

                int canNotUnsuspendCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(CanSelectOpponentPermanentCondition));

                SelectPermanentEffect canNotUnsuspendSelectEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                canNotUnsuspendSelectEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectOpponentPermanentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: canNotUnsuspendCount,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectCanNotUnsuspendCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                canNotUnsuspendSelectEffect.SetUpCustomMessage(
                    "Select 2 Digimon or Tamers that can't unsuspend until their turn ends.",
                    "The opponent is selecting 2 Digimon or Tamers that can't unsuspend until your turn ends.");

                yield return ContinuousController.instance.StartCoroutine(canNotUnsuspendSelectEffect.Activate());

                IEnumerator SelectCanNotUnsuspendCoroutine(Permanent permanent)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotUnsuspend(
                        targetPermanent: permanent,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass,
                        condition: null,
                        effectName: "Can't unsuspend"));
                }
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

            #region All Turns / All Turns - ESS
            if (timing == EffectTiming.OnTappedAnyone)
            {
                foreach (bool isInheritedEffect in new[] { false, true })
                {
                    ActivateClass activateClass = new ActivateClass();
                    activateClass.SetUpICardEffect("1 [Dracomon]/[Examon] text Digimon may battle 1 opponent's Digimon", CanUseCondition, card);
                    activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                    activateClass.SetIsSkippable(true);
                    activateClass.SetIsInheritedEffect(isInheritedEffect);
                    activateClass.SetHashString(isInheritedEffect ? "EX13_044_ESS_AT" : "EX13_044_AT");
                    cardEffects.Add(activateClass);

                    string EffectDescription()
                        => "[All Turns] [Once Per Turn] When any of your Digimon suspend, 1 of your Digimon with [Dracomon] or [Examon] in its text may battle 1 of your opponent's Digimon.";

                    bool IsOwnerDigimon(Permanent permanent)
                        => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                    bool IsBattlerCondition(Permanent permanent)
                        => IsOwnerDigimon(permanent)
                            && (permanent.TopCard.HasText("Dracomon") || permanent.TopCard.HasText("Examon"));

                    bool IsDefenderCondition(Permanent permanent)
                        => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                    bool CanUseCondition(Hashtable hashtable)
                        => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                            && CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, IsOwnerDigimon);

                    bool CanActivateCondition(Hashtable hashtable)
                        => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                            && CardEffectCommons.HasMatchConditionPermanent(IsBattlerCondition)
                            && CardEffectCommons.HasMatchConditionPermanent(IsDefenderCondition);

                    IEnumerator ActivateCoroutine(Hashtable hashtable)
                    {
                        bool isBattled = false;
                        Permanent selectedAttacker = null;

                        SelectPermanentEffect attackerSelectEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        attackerSelectEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsBattlerCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectAttackerCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        attackerSelectEffect.SetUpCustomMessage("Select 1 of your [Dracomon]/[Examon] text Digimon to battle.", "The opponent is selecting 1 Digimon to battle.");

                        yield return ContinuousController.instance.StartCoroutine(attackerSelectEffect.Activate());

                        IEnumerator SelectAttackerCoroutine(Permanent permanent)
                        {
                            selectedAttacker = permanent;
                            yield return null;
                        }

                        if (selectedAttacker != null && CardEffectCommons.HasMatchConditionPermanent(IsDefenderCondition))
                        {
                            Permanent selectedDefender = null;

                            SelectPermanentEffect defenderSelectEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            defenderSelectEffect.SetUp(
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

                            defenderSelectEffect.SetUpCustomMessage("Select 1 of your opponent's Digimon to battle.", "The opponent is selecting 1 Digimon to battle.");

                            yield return ContinuousController.instance.StartCoroutine(defenderSelectEffect.Activate());

                            IEnumerator SelectDefenderCoroutine(Permanent permanent)
                            {
                                selectedDefender = permanent;
                                yield return null;
                            }

                            if (selectedDefender != null)
                            {
                                isBattled = true;
                                yield return ContinuousController.instance.StartCoroutine(new IBattle(selectedAttacker, selectedDefender, null, true).Battle());
                            }
                        }

                        if (!isBattled) activateClass.RemoveUse();
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
