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
                    return targetPermanent.TopCard.EqualsTraits("Insectoid") || targetPermanent.TopCard.HasTSTraits;
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
                                && (cardSource.EqualsTraits("Larva") || cardSource.EqualsTraits("Insectoid") || cardSource.EqualsTraits("Titan"));
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

            bool SharedAdditionalActivateConditionA(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionPermanent(CanSelectBattleTargetCondition);

            IEnumerator SharedActivateCoroutineA(Hashtable hashtable, ActivateClass activateClass)
            {
                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectBattleTargetCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
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

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectNameA(),
                SharedActivateCoroutineA,
                SharedEffectDescriptionA,
                optional: false,
                isSkippable: true,
                additionalActivateCondition: SharedAdditionalActivateConditionA,
                onPlay: true,
                whenDigivolving: true);

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
                    && (permanent.TopCard.EqualsTraits("Insectoid") || permanent.TopCard.EqualsTraits("Titan"));

            bool SharedAdditionalActivateConditionB(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendCondition);

            IEnumerator SharedActivateCoroutineB(Hashtable hashtable, ActivateClass activateClass)
            {
                SelectPermanentEffect selectSuspendEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectSuspendEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectSuspendCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: null,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Tap,
                    cardEffect: activateClass);

                selectSuspendEffect.SetUpCustomMessage("Select 1 Digimon to suspend.", "The opponent is selecting 1 Digimon to suspend.");

                yield return ContinuousController.instance.StartCoroutine(selectSuspendEffect.Activate());

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDPPlayerEffect(
                    permanentCondition: IsSuspendedInsectoidOrTitan,
                    changeValue: 3000,
                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                    activateClass: activateClass));

                CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's Options' effects", CanUseConditionImmunity, card);
                canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                card.Owner.UntilOpponentTurnEndEffects.Add(GetCardEffect);
                card.Owner.UntilOpponentTurnEndEffects.Add(GetDetailEffect);

                bool CanUseConditionImmunity(Hashtable hashtable)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleArea(card.PermanentOfThisCard())
                        && card == card.PermanentOfThisCard().TopCard;
                }

                bool CardCondition(CardSource cardSource)
                {
                    return IsSuspendedInsectoidOrTitan(cardSource.PermanentOfThisCard());
                }

                bool SkillCondition(ICardEffect cardEffect)
                {
                    return CardEffectCommons.IsOpponentEffect(cardEffect, card)
                        && cardEffect.IsOptionEffect;
                }

                ICardEffect GetCardEffect(EffectTiming _timing)
                {
                    if (_timing == EffectTiming.None)
                    {
                        return canNotAffectedClass;
                    }

                    return null;
                }

                ICardEffect GetDetailEffect(EffectTiming timing)
                {
                    if (timing == EffectTiming.None)
                    {
                        return CardEffectFactory.AddDetailClass(CanUseConditionImmunity, IsSuspendedInsectoidOrTitan, "Isn't affected by opponent's Options' effects", false, card);
                    }
                    return null;
                }
            }
            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectNameB(),
                SharedActivateCoroutineB,
                SharedEffectDescriptionB,
                optional: false,
                isSkippable: true,
                additionalActivateCondition: SharedAdditionalActivateConditionB,
                startOfYourMainPhase: true,
                onPlay: true,
                whenDigivolving: true);

            return cardEffects;
        }
    }
}
