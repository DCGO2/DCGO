using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Megadramon
namespace DCGO.CardEffects.EX12
{
    public class EX12_064 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Machine")
                        || targetPermanent.TopCard.EqualsTraits("ME");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 4)
                );
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "Delete 1 enemy lvl 4 or lower Digimon, if fail, <De-Digivolve 1> 1 of enemy Digimon";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] Delete 1 of your opponent's level 4 or lower Digimon. If this effect didn't delete, <De-Digivolve 1> 1 of your opponent's Digimon.";
            }

            bool CanSelectPermanentCondition1(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.HasLevel
                    && permanent.Level <= 4;
            }

            bool CanSelectPermanentCondition2(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                List<Permanent> deleteTargetPermanents = new List<Permanent>();

                bool failedToDelete = true;

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition1,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        deleteTargetPermanents = permanents.Clone();

                        yield return null;
                    }
                }

                if (deleteTargetPermanents.Count > 0)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: deleteTargetPermanents, activateClass: activateClass, successProcess: SuccessDeleteProcess, failureProcess: null));

                    IEnumerator SuccessDeleteProcess(List<Permanent> permanents)
                    {
                        if (permanents.Count > 0)
                            failedToDelete = false;

                        yield return null;
                    }
                }

                if (failedToDelete
                && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition2))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition2,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Degenerate,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 opponent's Digimon to De-Digivolve", "The opponent is selecting 1 Digimon to De-Digivolve");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May activate 1 of this Digimon's [When Digivolving] effects", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("EX12_064_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When any of your [Machine], [Cyborg] or [ME] trait Digimon are played, you may activate 1 of this Digimon's [When Digivolving] effects.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PlayedPermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.PermanentOfThisCard().EffectList(EffectTiming.OnEnterFieldAnyone).Any(CanBeEffectCandidate);
                }

                bool PlayedPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.EqualsTraits("Machine")
                            || permanent.TopCard.EqualsTraits("Cyborg")
                            || permanent.TopCard.EqualsTraits("ME"));
                }

                bool CanBeEffectCandidate(ICardEffect cardEffect)
                {
                    if (cardEffect != null
                    && cardEffect is ActivateICardEffect
                    && !cardEffect.IsSecurityEffect
                    && cardEffect.IsWhenDigivolving)
                    {
                        Hashtable digivolvingHashtable = CardEffectCommons.WhenDigivolvingCheckHashtableOfCard(cardEffect.EffectSourceCard);

                        return cardEffect.CanUse(digivolvingHashtable);
                    }
                    return false;
                }


                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<ICardEffect> candidateEffects = card.PermanentOfThisCard().EffectList(EffectTiming.OnEnterFieldAnyone).Clone().Filter(CanBeEffectCandidate);

                    if (candidateEffects.Count >= 1)
                    {
                        ICardEffect selectedEffect = null;

                        if (candidateEffects.Count == 1)
                        {
                            selectedEffect = candidateEffects[0];
                        }
                        else
                        {
                            List<SkillInfo> skillInfos = candidateEffects
                                .Map(cardEffect => new SkillInfo(cardEffect, null, EffectTiming.None));

                            List<CardSource> cardSources = candidateEffects
                                .Map(cardEffect => cardEffect.EffectSourceCard);

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: (cardSource) => true,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => false,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 effect to activate.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: false,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: cardSources,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetNotShowCard();
                            selectCardEffect.SetUpSkillInfos(skillInfos);
                            selectCardEffect.SetUpAfterSelectIndexCoroutine(AfterSelectIndexCoroutine);

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator AfterSelectIndexCoroutine(List<int> selectedIndexes)
                            {
                                if (selectedIndexes.Count == 1)
                                {
                                    selectedEffect = candidateEffects[selectedIndexes[0]];
                                    yield return null;
                                }
                            }
                        }

                        if (selectedEffect != null
                        && selectedEffect.EffectSourceCard != null
                        && selectedEffect.EffectSourceCard.PermanentOfThisCard() != null)
                        {
                            selectedEffect.SetIsDigimonEffect(true);
                            Hashtable digivolvingHashtable = CardEffectCommons.WhenDigivolvingCheckHashtableOfCard(selectedEffect.EffectSourceCard);

                            if (selectedEffect.CanUse(digivolvingHashtable))
                            {
                                yield return ContinuousController.instance.StartCoroutine(((ActivateICardEffect)selectedEffect).Activate_Optional_Effect_Execute(digivolvingHashtable));
                            }
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
                            return cardSource.HasLevel
                                && cardSource.Level <= 4
                                && (cardSource.EqualsTraits("Machine")
                                    || cardSource.EqualsTraits("Cyborg")
                                    || cardSource.EqualsTraits("Me"));
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            element: element,
                            CanTargetCondition_ByPreSelecetedList: null,
                            selectMessage: "Lv.4 or lower [Machine]/[Cyborg]/[Me] trait card",
                            elementCount: 1,
                            reduceCost: 2);

                        return assemblyCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Inherited
            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon to delete own lowest play cost digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("EX12_064_Inherited");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[End of Attack] [Once Per Turn] By unsuspending this Digimon, delete 1 of your Digimon with the lowest play cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsMinCost(permanent, card.Owner, true);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());

                    if (!card.PermanentOfThisCard().IsSuspended)
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
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select a digimon to delete.", "The opponent is selecting a digimon to delete.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
