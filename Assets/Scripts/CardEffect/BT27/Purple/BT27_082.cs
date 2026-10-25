using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Nosferamon / Rampage Bullet
namespace DCGO.CardEffects.BT27
{
    public class BT27_082 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Effects
            #region Alternate Digivolution Requirement - Lv.5 w/[Glowing Dawn] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasGlowingDawnTraits;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Execute
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.ExecuteSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Ascension
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.AscensionSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared Security / On Deletion

            string SharedEffectName = "Delete 1 opponent's lowest level Digimon, if not deleted, trash their top security card";

            string SharedEffectDescription(string tag)
                => $"[{tag}] Delete 1 of your opponent's Digimon with the lowest level. If this effect didn't delete, trash their top security card.";

            bool CanSelectLowestLevelCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                List<Permanent> deleteTargetPermanents = new List<Permanent>();

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectLowestLevelCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectLowestLevelCondition,
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

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onDeletion: true,
                security: true);

            #endregion

            #region Shared When Digivolving / When Attacking - OPT

            string ActivateOptionEffectName = "Trash 2 face-down cards from under your Tamers to activate [Main] effect on this card's Option side";

            string ActivateOptionEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] By trashing 2 bottom face-down cards from under any of your Tamers, activate 1 [Main] effect on this card's Option side.";

            bool FaceDownCards(CardSource cardSource) => cardSource.IsFaceDown;

            bool TamerWithOneFaceDownSource(Permanent permanent, ICardEffect activateClass)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.DigivolutionCards.Any(FaceDownCards)
                    && !permanent.ImmuneFromStackTrashing(activateClass);

            bool TamerWith2OrMoreFaceDownSources(Permanent permanent, ICardEffect activateClass)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.DigivolutionCards.Count(FaceDownCards) >= 2
                    && !permanent.ImmuneFromStackTrashing(activateClass);

            bool CanPayFaceDownCost(ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionPermanent(permanent => TamerWith2OrMoreFaceDownSources(permanent, activateClass))
                    || CardEffectCommons.MatchConditionPermanentCount(permanent => TamerWithOneFaceDownSource(permanent, activateClass)) >= 2;

            bool ActivateOptionCanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CanPayFaceDownCost(activateClass);

            IEnumerator ActivateOptionCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool paid = false;

                if (CanPayFaceDownCost(activateClass))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(permanent => TamerWithOneFaceDownSource(permanent, activateClass)));

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: permanent => TamerWithOneFaceDownSource(permanent, activateClass),
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: CanEndSelectCondition,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: true,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select all Tamer(s) to trash bottom face-down cards from.", "The opponent is selecting Tamer(s) to trash bottom face-down cards from.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    bool CanEndSelectCondition(List<Permanent> permanents)
                    {
                        return permanents.Count == 2
                            || (permanents.Count > 0 && permanents[0].DigivolutionCards.Count(FaceDownCards) >= 2);
                    }

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        if (permanents.Count == 1)
                        {
                            paid = true;
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanents[0], trashCount: 2, isFromTop: false, activateClass: activateClass, cardCondition: FaceDownCards));
                        }
                        else if (permanents.Count == 2)
                        {
                            paid = true;
                            foreach (Permanent selectedPermanent in permanents)
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedPermanent, trashCount: 1, isFromTop: false, activateClass: activateClass, cardCondition: FaceDownCards));
                        }
                    }
                }

                if (!paid)
                {
                    activateClass.RemoveUse();
                    yield break;
                }

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ActivateMainOfOptionSide(card, activateClass));
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                ActivateOptionEffectName,
                ActivateOptionCoroutine,
                ActivateOptionEffectDescription,
                optional: false,
                additionalActivateCondition: ActivateOptionCanActivateCondition,
                isSkippable: true,
                maxCountPerTurn: 1,
                hashValue: "BT27_082_ActivateOption",
                whenDigivolving: true,
                whenAttacking: true);

            #endregion
            #endregion

            #region Option Effects
            #region Use Req
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, cardSource => cardSource.HasGlowingDawnTraits));
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 opponent's level 6 or higher Digimon, then may play 1 play cost 4 or lower [BEATBREAK] trait card from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] Delete 1 of your opponent's level 6 or higher Digimon. Then, you may play 1 play cost 4 or lower [BEATBREAK] trait card from your trash without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectDeleteCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasLevel
                        && permanent.Level >= 6;

                bool CanSelectPlayCardCondition(CardSource cardSource)
                    => cardSource.HasBeatBreakTraits
                        && cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 4
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, SelectCardEffect.Root.Trash);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    #region Delete
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDeleteCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDeleteCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                    #endregion

                    #region May play 1 [BEATBREAK] from trash
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectPlayCardCondition))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                            canTargetCondition: CanSelectPlayCardCondition,
                            root: SelectCardEffect.Root.Trash,
                            cardEffect: activateClass,
                            payCost: false));
                    }
                    #endregion
                }
            }
            #endregion

            #region Arts Digivolution
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            }
            #endregion
            #endregion

            return cardEffects;
        }
    }
}
