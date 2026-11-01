using System.Collections;
using System.Collections.Generic;

// UlforceVeedramon
namespace DCGO.CardEffects.EX13
{
    public class EX13_023 : CEntity_Effect
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
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Evade
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.EvadeSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared On Play / When Digivolving / When Attacking - Change Orientation

            string ChangeOrientationEffectName = "1 of your Digimon may change orientation";

            string ChangeOrientationEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] 1 of your Digimon may change orientation.";

            bool CanChangeOrientationCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.CanChangeOrientation;

            bool ChangeOrientationAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionPermanent(CanChangeOrientationCondition);

            IEnumerator ChangeOrientationActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                Permanent selectedPermanent = null;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanChangeOrientationCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage(
                    "Select 1 of your Digimon to change orientation.",
                    "The opponent is selecting 1 Digimon to change orientation.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    selectedPermanent = permanent;
                    yield return null;
                }

                if (selectedPermanent == null)
                {
                    activateClass.RemoveUse();
                    yield break;
                }

                if (selectedPermanent.IsSuspended)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                        permanents: new List<Permanent>() { selectedPermanent },
                        cardEffect: activateClass).Unsuspend());
                }
                else
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new SuspendPermanentsClass(new List<Permanent>() { selectedPermanent }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                ChangeOrientationEffectName,
                ChangeOrientationActivateCoroutine,
                ChangeOrientationEffectDescription,
                optional: false,
                isSkippable: true,
                additionalActivateCondition: ChangeOrientationAdditionalActivateCondition,
                maxCountPerTurn: 1,
                hashValue: "EX13_023_OP_WD_WA",
                onPlay: true,
                whenDigivolving: true,
                whenAttacking: true);

            #endregion

            #region Shared On Play / When Digivolving - Deck Bounce

            string DeckBounceEffectName = "May return all opponent's Digimon with the fewest digivolution cards to the bottom of the deck";

            string DeckBounceEffectDescription(string tag)
                => $"[{tag}] You may return all of your opponent's Digimon with the fewest digivolution cards to the bottom of the deck.";

            bool IsFewestSourcesOpponentDigimon(Permanent permanent)
                => CardEffectCommons.IsMinDigivolutionCards(permanent, card.Owner.Enemy);

            bool DeckBounceAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionPermanent(IsFewestSourcesOpponentDigimon);

            IEnumerator DeckBounceActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                List<Permanent> bounceTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(IsFewestSourcesOpponentDigimon);

                if (bounceTargetPermanents.Count == 0) yield break;

                yield return ContinuousController.instance.StartCoroutine(
                    new DeckBottomBounceClass(bounceTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).DeckBounce());
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                DeckBounceEffectName,
                DeckBounceActivateCoroutine,
                DeckBounceEffectDescription,
                optional: true,
                additionalActivateCondition: DeckBounceAdditionalActivateCondition,
                onPlay: true,
                whenDigivolving: true);

            #endregion

            #region All Turns
            if (timing == EffectTiming.None)
            {
                bool IsThisPermanent(Permanent permanent)
                    => permanent == card.PermanentOfThisCard();

                bool IsOpponentEffectCondition(ICardEffect cardEffect)
                    => CardEffectCommons.IsOpponentEffect(cardEffect, card);

                bool IsUnsuspendedCondition()
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();

                    return thisPermanent != null && !thisPermanent.IsSuspended;
                }

                cardEffects.Add(CardEffectFactory.ImmuneFromDPMinusStaticEffect(
                    permanentCondition: IsThisPermanent,
                    cardEffectCondition: IsOpponentEffectCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: IsUnsuspendedCondition,
                    effectName: "While unsuspended, the opponent's effects can't reduce this Digimon's DP"));

                cardEffects.Add(CardEffectFactory.CanNotBeTrashedBySkillStaticEffect(
                    permanentCondition: IsThisPermanent,
                    cardEffectCondition: IsOpponentEffectCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: IsUnsuspendedCondition,
                    effectName: "While unsuspended, the opponent's effects can't trash this Digimon's stacked cards"));

                cardEffects.Add(CardEffectFactory.CanNotBeReturnedToLibraryBySkillStaticEffect(
                    permanentCondition: IsThisPermanent,
                    cardEffectCondition: IsOpponentEffectCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: IsUnsuspendedCondition,
                    effectName: "While unsuspended, the opponent's effects can't return this Digimon's stacked cards to the hand or deck"));
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

                bool HasVeemonOrVeedramonName(CardSource assemblyCard)
                    => assemblyCard.ContainsCardName("Veemon") || assemblyCard.ContainsCardName("Veedramon");

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource != card) return null;

                    AssemblyConditionElement level5Element = new AssemblyConditionElement(assemblyCard => HasVeemonOrVeedramonName(assemblyCard) && assemblyCard.Level_Assembly.Contains(5), selectMessage: "1 level 5 card with [Veemon] or [Veedramon] in its name", elementCount: 1);
                    AssemblyConditionElement level4Element = new AssemblyConditionElement(assemblyCard => HasVeemonOrVeedramonName(assemblyCard) && assemblyCard.Level_Assembly.Contains(4), selectMessage: "1 level 4 card with [Veemon] or [Veedramon] in its name", elementCount: 1);
                    AssemblyConditionElement level3Element = new AssemblyConditionElement(assemblyCard => HasVeemonOrVeedramonName(assemblyCard) && assemblyCard.Level_Assembly.Contains(3), selectMessage: "1 level 3 card with [Veemon] or [Veedramon] in its name", elementCount: 1);

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
