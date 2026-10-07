using System.Collections;
using System.Collections.Generic;

// Leopardmon
namespace DCGO.CardEffects.EX13
{
    public class EX13_043 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Leopardmon: Leopard Mode");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 1, ignoreDigivolutionRequirement: false, card: card, condition: null));
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
                        && assemblyCard.HasDigimonColor(CardColor.Green)
                        && (assemblyCard.EqualsTraits("Mammal")
                            || assemblyCard.EqualsTraits("Beast")
                            || assemblyCard.EqualsTraits("Beastkin"));

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

            #region Shared On Play / When Digivolving - Suspend and return to deck

            string SuspendEffectName = "May suspend 1 Digimon, then may return 1 opponent's lowest DP Digimon to deck bottom";

            string SuspendEffectDescription(string tag)
                => $"[{tag}] You may suspend 1 Digimon. Then, you may return 1 of your opponent's lowest DP Digimon to the bottom of the deck.";

            bool CanSelectSuspendCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent)
                    && !permanent.IsSuspended;

            bool IsOpponentLowestDPDigimon(Permanent permanent)
                => CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy);

            IEnumerator SuspendActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendCondition))
                {
                    SelectPermanentEffect selectSuspendEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectSuspendEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectSuspendCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    selectSuspendEffect.SetUpCustomMessage("Select 1 Digimon to suspend.", "The opponent is selecting 1 Digimon to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectSuspendEffect.Activate());
                }

                if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentLowestDPDigimon))
                {
                    SelectPermanentEffect selectReturnEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectReturnEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsOpponentLowestDPDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                        cardEffect: activateClass);

                    selectReturnEffect.SetUpCustomMessage("Select 1 Digimon to return to the bottom of the deck.", "The opponent is selecting 1 Digimon to return to the bottom of the deck.");

                    yield return ContinuousController.instance.StartCoroutine(selectReturnEffect.Activate());
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SuspendEffectName,
                SuspendActivateCoroutine,
                SuspendEffectDescription,
                optional: false,
                hashValue: "EX13_043_OP_WD",
                onPlay: true,
                whenDigivolving: true);

            #endregion

            #region Shared When Digivolving / When Attacking - Play or use

            string PlayOrUseEffectName = "May play or use 1 [Mammal]/[Beast]/[Beastkin]/[Royal Knight] card from hand for 4 less, further reduced per suspended Digimon";

            string PlayOrUseEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] You may play or use 1 [Mammal], [Beast], [Beastkin] or [Royal Knight] trait card from your hand with the cost reduced by 4. For each suspended Digimon, further reduce it by 1.";

            bool IsSuspendedDigimon(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent)
                    && permanent.IsSuspended;

            bool CanSelectPlayOrUseCardCondition(CardSource cardSource)
                => cardSource.EqualsTraits("Mammal")
                    || cardSource.EqualsTraits("Beast")
                    || cardSource.EqualsTraits("Beastkin")
                    || cardSource.HasRoyalKnightTraits;

            bool PlayOrUseAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectPlayOrUseCardCondition);

            IEnumerator PlayOrUseActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool isUsed = false;

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOrUseByEffect(
                    canTargetCondition: CanSelectPlayOrUseCardCondition,
                    root: SelectCardEffect.Root.Hand,
                    cardEffect: activateClass,
                    payCost: true,
                    reducedCost: 4 + CardEffectCommons.MatchConditionPermanentCount(IsSuspendedDigimon),
                    afterSelectCardCoroutine: AfterSelectCardCoroutine));

                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                {
                    if (cardSources.Count > 0) isUsed = true;

                    yield return null;
                }

                if (!isUsed) activateClass.RemoveUse();
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                PlayOrUseEffectName,
                PlayOrUseActivateCoroutine,
                PlayOrUseEffectDescription,
                optional: false,
                isSkippable: true,
                additionalActivateCondition: PlayOrUseAdditionalActivateCondition,
                maxCountPerTurn: 1,
                hashValue: "EX13_043_WD_WA",
                whenDigivolving: true,
                whenAttacking: true);

            #endregion

            #region All Turns - OPT
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By unsuspending 1 of your Digimon, your suspended Digimon don't leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("EX13_043_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When any of your suspended Digimon would leave the battle area other than by your effects, by unsuspending 1 of your Digimon, they don't leave.";

                bool IsOwnerSuspendedDigimon(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.IsSuspended;

                bool CanSelectUnsuspendCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && CardEffectCommons.CanUnsuspend(permanent);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, IsOwnerSuspendedDigimon)
                        && !CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOwnerEffect(cardEffect, card));

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectUnsuspendCondition);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    // Taken before the cost is paid, since the unsuspended Digimon may be one of those leaving.
                    List<Permanent> protectedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable)
                        .Filter(IsOwnerSuspendedDigimon);

                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectUnsuspendCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to unsuspend.", "The opponent is selecting 1 Digimon to unsuspend.");

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

                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                        new List<Permanent>() { selectedPermanent },
                        activateClass).Unsuspend());

                    if (selectedPermanent.TopCard != null && !selectedPermanent.IsSuspended)
                    {
                        foreach (Permanent permanent in protectedPermanents)
                        {
                            permanent.willBeRemoveField = false;
                            permanent.HideHandBounceEffect();
                            permanent.HideDeckBounceEffect();
                            permanent.HideDeleteEffect();
                            permanent.HideWillRemoveFieldEffect();
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
