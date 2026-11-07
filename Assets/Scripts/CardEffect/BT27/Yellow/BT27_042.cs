using System.Collections;
using System.Collections.Generic;

// KingChessmon
namespace DCGO.CardEffects.BT27
{
    public class BT27_042 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - Lv.5 w/[Chessmon] in text or [DATA SQUAD] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasText("Chessmon")
                        || targetPermanent.TopCard.HasDataSquadTraits;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName = "May play 1 play cost 7 or lower [Chessmon] in name Digimon from hand";

            string SharedEffectDescription(string tag)
                => $"[{tag}] You may play 1 play cost 7 or lower Digimon card with [Chessmon] in its name from your hand without paying the cost.";

            bool CanSelectCardCondition(CardSource cardSource, ActivateClass activateClass)
                => cardSource.IsDigimon
                    && cardSource.ContainsCardName("Chessmon")
                    && cardSource.HasPlayCost
                    && cardSource.GetCostItself <= 7
                    && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, SelectCardEffect.Root.Hand);

            bool SharedCanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => CanSelectCardCondition(cardSource, activateClass));

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                    canTargetCondition: cardSource => CanSelectCardCondition(cardSource, activateClass),
                    root: SelectCardEffect.Root.Hand,
                    cardEffect: activateClass,
                    payCost: false));
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                isSkippable: true,
                additionalActivateCondition: SharedCanActivateCondition,
                onPlay: true,
                whenDigivolving: true);

            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<Recovery +1>, then by suspending 1 [QueenChessmon], may play this card to breeding", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Deletion] <Recovery +1>. Then, by suspending 1 of your [QueenChessmon], you may play this card from your trash to the breeding area without paying the cost.";

                bool CanSelectQueenCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsCardName("QueenChessmon")
                        && !permanent.IsSuspended
                        && permanent.CanSuspend;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.CanActivateOnDeletion(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());

                    if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectQueenCondition)) yield break;

                    Permanent selectedQueen = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectQueenCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 [QueenChessmon] to suspend.", "The opponent is selecting 1 [QueenChessmon] to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedQueen = permanent;
                        yield return null;
                    }

                    if (selectedQueen == null) yield break;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { selectedQueen },
                        activateClass: activateClass,
                        successProcess: SuccessProcess,
                        failureProcess: null));

                    IEnumerator SuccessProcess(List<Permanent> _permanents)
                    {
                        if (CardEffectCommons.IsExistOnTrash(card)
                            && CardEffectCommons.CanPlayAsNewPermanent(card, false, activateClass, SelectCardEffect.Root.Trash, isBreedingArea: true))
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: new List<CardSource>() { card },
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.Trash,
                                activateETB: true,
                                isBreedingArea: true));
                        }
                    }
                }
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

                static bool HasChessmonName(CardSource assemblyCard)
                    => assemblyCard.ContainsCardName("Chessmon");

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource != card) return null;

                    AssemblyConditionElement level5Element = new AssemblyConditionElement(
                        assemblyCard => HasChessmonName(assemblyCard) && assemblyCard.Level_Assembly.Contains(5),
                        selectMessage: "1 level 5 card with [Chessmon] in its name",
                        elementCount: 1);
                    AssemblyConditionElement level4Element = new AssemblyConditionElement(
                        assemblyCard => HasChessmonName(assemblyCard) && assemblyCard.Level_Assembly.Contains(4),
                        selectMessage: "1 level 4 card with [Chessmon] in its name",
                        elementCount: 1);
                    AssemblyConditionElement level3Element = new AssemblyConditionElement(
                        assemblyCard => HasChessmonName(assemblyCard) && assemblyCard.Level_Assembly.Contains(3),
                        selectMessage: "1 level 3 card with [Chessmon] in its name",
                        elementCount: 1);

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
