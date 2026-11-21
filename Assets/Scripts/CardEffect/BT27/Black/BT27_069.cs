using System.Collections;
using System.Collections.Generic;

// QueenChessmon
namespace DCGO.CardEffects.BT27
{
    public class BT27_069 : CEntity_Effect
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

            #region Rush / Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RushSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Overclock
            if (timing == EffectTiming.OnEndTurn)
            {
                bool PermanentCondition(Permanent permanent)
                    => permanent.TopCard.ContainsCardName("Chessmon");

                cardEffects.Add(CardEffectFactory.OverclockSelfEffect(permanentCondition: PermanentCondition, isInheritedEffect: false, card: card,
                    condition: null, displayString: "[Chessmon] in name"));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName = "<De-Digivolve 3> 1 of your opponent's Digimon";

            string SharedEffectDescription(string tag)
                => $"[{tag}] <De-Digivolve 3> 1 of your opponent's Digimon.";

            bool CanSelectDeDigivolveCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectDeDigivolveCondition)) yield break;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectDeDigivolveCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: null,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Degenerate,
                    cardEffect: activateClass);

                selectPermanentEffect.SetDegenerationCount(3);
                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to De-Digivolve.", "The opponent is selecting 1 Digimon to De-Digivolve.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
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

            #region All Turns - OPT
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 opponent's lowest play cost Digimon, if not deleted, trash their top security card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT27_069_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When any of your other Digimon with [Chessmon] in their names are deleted, delete 1 of your opponent's lowest play cost Digimon. If this effect didn't delete, trash their top security card.";

                bool OtherChessmonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent != card.PermanentOfThisCard()
                        && permanent.TopCard.ContainsCardName("Chessmon");

                bool CanSelectPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsMinCost(permanent, card.Owner.Enemy, true);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, OtherChessmonCondition, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> deleteTargetPermanents = new List<Permanent>();

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            deleteTargetPermanents = permanents.Clone();
                            yield return null;
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                        targetPermanents: deleteTargetPermanents,
                        activateClass: activateClass,
                        successProcess: null,
                        failureProcess: FailureProcess));

                    IEnumerator FailureProcess()
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            player: card.Owner.Enemy,
                            destroySecurityCount: 1,
                            cardEffect: activateClass,
                            fromTop: true).DestroySecurity());
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
