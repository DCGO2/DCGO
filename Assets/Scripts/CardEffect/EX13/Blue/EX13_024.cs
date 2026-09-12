using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Slayerdramon
namespace DCGO.CardEffects.EX13
{
    public class EX13_024 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Wingdramon") || targetPermanent.TopCard.EqualsCardName("Groundramon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Raid
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card, condition: null));
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

            string SharedEffectName = "Trash 1 opponent's digivolution card per this Digimon's digivolution card, then may bottom deck their fewest-source Digimon";

            string SharedEffectDescription(string tag)
                => $"[{tag}] For each of this Digimon's digivolution cards, trash any 1 digivolution card from your opponent's Digimon. Then, you may return all of their Digimon with the fewest digivolution cards to the bottom of the deck.";

            bool IsOpponentDigimon(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) && permanent.DigivolutionCards.Any();

            bool IsFewestSourcesOpponentDigimon(Permanent permanent)
                => IsOpponentDigimon(permanent)
                    && CardEffectCommons.IsMinDigivolutionCards(permanent, card.Owner.Enemy);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                Permanent thisPermanent = card.PermanentOfThisCard();
                int trashCount = thisPermanent != null ? thisPermanent.DigivolutionCards.Count : 0;

                if (trashCount > 0 && CardEffectCommons.HasMatchConditionPermanent(IsOpponentDigimon))
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                        permanentCondition: IsOpponentDigimon,
                        cardCondition: null,
                        maxCount: trashCount,
                        canNoTrash: false,
                        isFromOnly1Permanent: false,
                        activateClass: activateClass,
                        canEndNotMax: false));
                }

                List<Permanent> bounceTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(IsFewestSourcesOpponentDigimon);

                if (bounceTargetPermanents.Count > 0)
                {
                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                    {
                        new SelectionElement<bool>(message: "Yes", value: true, spriteIndex: 0),
                        new SelectionElement<bool>(message: "No", value: false, spriteIndex: 1),
                    };

                    GManager.instance.userSelectionManager.SetBoolSelection(
                        selectionElements: selectionElements,
                        selectPlayer: card.Owner,
                        selectPlayerMessage: "Return all of your opponent's Digimon with the fewest digivolution cards to the bottom of the deck?",
                        notSelectPlayerMessage: "The opponent is choosing whether to return Digimon to the bottom of the deck.");

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    if (GManager.instance.userSelectionManager.SelectedBoolValue)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            new DeckBottomBounceClass(bounceTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).DeckBounce());
                    }
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
            if (timing == EffectTiming.WhenRemoveField)
            {
                foreach (bool isInheritedEffect in new[] { false, true })
                {
                    List<Permanent> removedPermanents = new List<Permanent>();

                    ActivateClass activateClass = new ActivateClass();
                    activateClass.SetUpICardEffect("Suspend 1 [Dracomon]/[Examon] text Digimon to prevent leaving", CanUseCondition, card);
                    activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                    activateClass.SetIsSkippable(true);
                    activateClass.SetIsInheritedEffect(isInheritedEffect);
                    activateClass.SetHashString(isInheritedEffect ? "EX13_024_ESS_AT" : "EX13_024_AT");
                    cardEffects.Add(activateClass);

                    string EffectDescription()
                        => "[All Turns] [Once Per Turn] When any of your [Dracomon] or [Examon] text Digimon would leave the battle area, by suspending 1 of your such Digimon, they don't leave.";

                    bool IsDracomonOrExamonDigimon(Permanent permanent)
                        => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                            && (permanent.TopCard.HasText("Dracomon") || permanent.TopCard.HasText("Examon"));

                    bool CanSuspendForCostCondition(Permanent permanent)
                        => IsDracomonOrExamonDigimon(permanent)
                            && !permanent.IsSuspended
                            && permanent.CanSuspend;

                    bool CanUseCondition(Hashtable hashtable)
                        => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                            && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, IsDracomonOrExamonDigimon);

                    bool CanActivateCondition(Hashtable hashtable)
                    {
                        removedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable).Filter(IsDracomonOrExamonDigimon);

                        return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                            && removedPermanents.Count > 0
                            && CardEffectCommons.HasMatchConditionPermanent(CanSuspendForCostCondition);
                    }

                    IEnumerator ActivateCoroutine(Hashtable hashtable)
                    {
                        Permanent suspendPermanent = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSuspendForCostCondition,
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
                            "Select 1 [Dracomon]/[Examon] text Digimon to suspend.",
                            "The opponent is selecting 1 Digimon to suspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            suspendPermanent = permanent;
                            yield return null;
                        }

                        if (suspendPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                                new List<Permanent>() { suspendPermanent },
                                activateClass,
                                SuccessProcess,
                                FailureProcess));
                        }
                        else
                        {
                            activateClass.RemoveUse();
                        }

                        IEnumerator SuccessProcess(List<Permanent> suspendedPermanents)
                        {
                            foreach (Permanent removedPermanent in removedPermanents)
                            {
                                removedPermanent.willBeRemoveField = false;

                                removedPermanent.HideHandBounceEffect();
                                removedPermanent.HideDeckBounceEffect();
                                removedPermanent.HideDeleteEffect();
                                removedPermanent.HideWillRemoveFieldEffect();
                            }

                            yield return null;
                        }

                        IEnumerator FailureProcess()
                        {
                            activateClass.RemoveUse();
                            yield return null;
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
