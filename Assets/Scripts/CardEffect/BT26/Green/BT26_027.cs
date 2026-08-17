using System;
using System.Collections;
using System.Collections.Generic;

// Petermon
namespace DCGO.CardEffects.BT26
{
    public class BT26_027 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsTraits("WG");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 3));
            }
            #endregion

            #region Shared

            bool CanSelectSuspendCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && !permanent.IsSuspended && permanent.CanSuspend
                    && (permanent.TopCard.ContainsTraits("Vegetation") || permanent.TopCard.ContainsTraits("Fairy") || permanent.TopCard.ContainsTraits("WG"));

            bool CanSelectDebuffTargetCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            string SharedEffectName()
                => "By suspending 1 [Vegetation]/[Fairy]/[WG] Digimon, give 1 opponent's Digimon Security A. -2";

            string SharedEffectDescription(string tag)
                => $"[{tag}] By suspending 1 of your Digimon with the [Vegetation], [Fairy] or [WG] trait, give 1 of your opponent's Digimon <Security A. -2> until their turn ends.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendCondition))
                {
                    Permanent selectedSuspendTarget = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectSuspendCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedSuspendTarget = permanent;
                        yield return null;
                    }

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to suspend.", "The opponent is selecting 1 Digimon to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    if (selectedSuspendTarget != null && CardEffectCommons.HasMatchConditionPermanent(CanSelectDebuffTargetCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectDebuffTargetCondition));

                        SelectPermanentEffect selectDebuffEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectDebuffEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDebuffTargetCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine2,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectDebuffEffect.SetUpCustomMessage("Select 1 Digimon that will get Security A. -2.", "The opponent is selecting 1 Digimon that will get Security A. -2.");

                        yield return ContinuousController.instance.StartCoroutine(selectDebuffEffect.Activate());

                        IEnumerator SelectPermanentCoroutine2(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(targetPermanent: permanent, changeValue: -2, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                        }
                    }
                }
            }

            #endregion

            #region Start of Opponent's Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                cardEffects.Add(CardEffectFactory.StartOfYourOpponentsMainPhaseClass(
                    card,
                    SharedEffectName(),
                    (hash, activateClass) => SharedActivateCoroutine(hash, activateClass),
                    SharedEffectDescription("Start of Opponent's Main Phase"),
                    additionalActivateCondition: AdditionalActivateCondition,
                    optional: false,
                    isSkippable: true
                ));

                bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                    => CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendCondition)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectDebuffTargetCondition);
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, (hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendCondition)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectDebuffTargetCondition);
            }
            #endregion

            #region Inherit - Barrier
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
