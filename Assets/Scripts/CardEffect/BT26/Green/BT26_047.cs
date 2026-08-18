using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// TyrantKabuterimon
namespace DCGO.CardEffects.BT26
{
    public class BT26_047 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsTraits("Insectoid") || targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Assembly
            if (timing == EffectTiming.None)
            {
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect($"Assembly", CanUseCondition, card);
                addAssemblyConditionClass.SetUpAddAssemblyConditionClass(getAssemblyCondition: GetAssembly);
                addAssemblyConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAssemblyConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        AssemblyConditionElement element = new AssemblyConditionElement(CanSelectCardCondition);

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            return cardSource != null
                                && cardSource.Owner == card.Owner
                                && cardSource.IsDigimon
                                && cardSource.HasLevel
                                && (cardSource.ContainsTraits("Larva") || cardSource.ContainsTraits("Insectoid") || cardSource.ContainsTraits("Titan"));
                        }

                        bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                        {
                            List<int> cardLevels = new List<int>();

                            foreach (CardSource cardSource1 in cardSources)
                            {
                                if (!cardLevels.Contains(cardSource1.Level))
                                {
                                    cardLevels.Add(cardSource1.Level);
                                }
                                foreach (int level in cardSource1.Level_Assembly)
                                {
                                    if (!cardLevels.Contains(level))
                                    {
                                        cardLevels.Add(level);
                                    }
                                }
                            }

                            if (cardSource.Level_Assembly.Count((level) => cardLevels.Contains(level)) >= 1 || cardLevels.Contains(cardSource.Level))
                            {
                                return false;
                            }

                            return true;
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            element: element,
                            CanTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                            selectMessage: "4 [Larva]/[Insectoid]/[Titan] trait Digimon cards w/different levels",
                            elementCount: 4,
                            reduceCost: 6);

                        return assemblyCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Shared On Play / When Digivolving - May Battle

            string SharedEffectNameA()
                => "This Digimon may battle 1 of your opponent's Digimon";

            string SharedEffectDescriptionA(string tag)
                => $"[{tag}] This Digimon may battle 1 of your opponent's Digimon.";

            bool CanSelectBattleTargetCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.HasDP;

            bool SharedCanActivateConditionA(Hashtable hashtable, ICardEffect activateClass)
                => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                    && CardEffectCommons.HasMatchConditionPermanent(CanSelectBattleTargetCondition);

            IEnumerator SharedActivateCoroutineA(Hashtable hashtable, ActivateClass activateClass)
            {
                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectBattleTargetCondition));

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectBattleTargetCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to battle.", "The opponent is selecting 1 Digimon to battle.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IBattle(card.PermanentOfThisCard(), permanent, null, true).Battle());
                }
            }

            #endregion

            #region On Play A
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectNameA(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateConditionA(hash, activateClass), (hash) => SharedActivateCoroutineA(hash, activateClass), -1, false, SharedEffectDescriptionA("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
            }
            #endregion

            #region When Digivolving A
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectNameA(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateConditionA(hash, activateClass), (hash) => SharedActivateCoroutineA(hash, activateClass), -1, false, SharedEffectDescriptionA("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }
            #endregion

            #region Shared Start of Your Main Phase / On Play / When Digivolving - Suspend to buff

            string SharedEffectNameB()
                => "By suspending 1 Digimon, suspended [Insectoid]/[Titan] Digimon get +3000 DP and immune to opponent's Options";

            string SharedEffectDescriptionB(string tag)
                => $"[{tag}] By suspending 1 Digimon, until your opponent's turn ends, none of your suspended Digimon with the [Insectoid] or [Titan] trait are affected by your opponent's Option effects, and they get +3000 DP.";

            bool CanSelectSuspendCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent)
                    && !permanent.IsSuspended && permanent.CanSuspend;

            bool IsSuspendedInsectoidOrTitan(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.IsSuspended
                    && (permanent.TopCard.ContainsTraits("Insectoid") || permanent.TopCard.ContainsTraits("Titan"));

            bool SharedCanActivateConditionB(Hashtable hashtable, ICardEffect activateClass)
                => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                    && CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendCondition);

            IEnumerator SharedActivateCoroutineB(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendCondition))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectSuspendCondition));

                    SelectPermanentEffect selectSuspendEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectSuspendEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectSuspendCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    selectSuspendEffect.SetUpCustomMessage("Select 1 Digimon to suspend.", "The opponent is selecting 1 Digimon to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectSuspendEffect.Activate());

                    // +3000 DP dynamically re-applies to whichever of your [Insectoid]/[Titan] Digimon are
                    // suspended for the rest of the duration (ChangeDigimonDPPlayerEffect re-checks
                    // permanentCondition continuously, it's not a one-time snapshot).
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDPPlayerEffect(
                        permanentCondition: IsSuspendedInsectoidOrTitan,
                        changeValue: 3000,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass));

                    // No player-wide "isn't affected by opponent's Option effects" helper exists (only
                    // self-targeted CanNotAffected-based keywords like Progress), so this is built here via
                    // AddSkillClass mirroring how other player-wide dynamic grants (e.g. Progress) work.
                    AddSkillClass addSkillClass = new AddSkillClass();
                    addSkillClass.SetUpICardEffect("Isn't affected by opponent's Option effects", CanUseSkillCondition, card);
                    addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                    CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilOpponentTurnEnd, card: card, cardEffect: addSkillClass, timing: EffectTiming.None);

                    bool CanUseSkillCondition(Hashtable _hashtable) => true;

                    bool CardSourceCondition(CardSource cardSource)
                        => cardSource.PermanentOfThisCard() != null
                            && IsSuspendedInsectoidOrTitan(cardSource.PermanentOfThisCard())
                            && cardSource == cardSource.PermanentOfThisCard().TopCard;

                    List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> effects, EffectTiming _timing)
                    {
                        if (_timing == EffectTiming.None)
                        {
                            CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                            canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's Option effects", CanUseInner, cardSource);
                            canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: SelfCardCondition, SkillCondition: SkillCondition);
                            effects.Add(canNotAffectedClass);

                            bool CanUseInner(Hashtable _hashtable) => true;

                            bool SelfCardCondition(CardSource cs) => cs == cardSource;

                            bool SkillCondition(ICardEffect cardEffect)
                                => cardEffect != null
                                    && cardEffect.EffectSourceCard != null
                                    && cardEffect.EffectSourceCard.Owner == card.Owner.Enemy
                                    && cardEffect.EffectSourceCard.IsOption;
                        }

                        return effects;
                    }
                }
            }

            #endregion

            #region Start of Your Main Phase B
            if (timing == EffectTiming.OnStartMainPhase)
            {
                cardEffects.Add(CardEffectFactory.StartOfYourMainPhaseClass(
                    card,
                    SharedEffectNameB(),
                    (hash, activateClass) => SharedActivateCoroutineB(hash, activateClass),
                    SharedEffectDescriptionB("Start of Your Main Phase"),
                    additionalActivateCondition: AdditionalActivateCondition,
                    optional: false,
                    isSkippable: true
                ));

                bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                    => CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendCondition);
            }
            #endregion

            #region On Play B
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectNameB(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateConditionB(hash, activateClass), (hash) => SharedActivateCoroutineB(hash, activateClass), -1, false, SharedEffectDescriptionB("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
            }
            #endregion

            #region When Digivolving B
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectNameB(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateConditionB(hash, activateClass), (hash) => SharedActivateCoroutineB(hash, activateClass), -1, false, SharedEffectDescriptionB("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }
            #endregion

            return cardEffects;
        }
    }
}
