using System.Collections;
using System.Collections.Generic;

// Okuwamon
namespace DCGO.CardEffects.BT26
{
    public class BT26_042 : CEntity_Effect
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

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 4));
            }
            #endregion

            #region Shared A: On Play / When Digivolving - Suspend + Can't Unsuspend

            string SharedEffectNameA()
                => "Suspend 1 opponent's Digimon/Tamer, then 1 can't unsuspend";

            string SharedEffectDescriptionA(string tag)
                => $"[{tag}] Suspend 1 of your opponent's Digimon or Tamers. Then, 1 of their Digimon or Tamers can't unsuspend until their turn ends.";

            bool CanSelectPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer);

            IEnumerator SharedActivateCoroutineA(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    SelectPermanentEffect selectSuspendEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectSuspendEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    selectSuspendEffect.SetUpCustomMessage("Select 1 Digimon or Tamer to suspend.", "The opponent is selecting 1 Digimon or Tamer to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectSuspendEffect.Activate());

                    SelectPermanentEffect selectCantUnsuspendEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectCantUnsuspendEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectCantUnsuspendEffect.SetUpCustomMessage("Select 1 Digimon or Tamer that can't unsuspend.", "The opponent is selecting 1 Digimon or Tamer that can't unsuspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectCantUnsuspendEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotUnsuspend(permanent, EffectDuration.UntilOpponentTurnEnd, activateClass, null, "Can't unsuspend"));
                    }
                }
            }

            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectNameA(),
                SharedActivateCoroutineA,
                SharedEffectDescriptionA,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            #region Shared B: On Play / When Attacking - Grant Piercing + DP

            string SharedEffectNameB()
                => "1 of your [Insectoid]/[Titan] Digimon gains Piercing and +3000 DP";

            string SharedEffectDescriptionB(string tag)
                => $"[{tag}] [Once Per Turn] Until your opponent's turn ends, 1 of your Digimon with the [Insectoid] or [Titan] trait gains <Piercing> and +3000 DP.";

            bool CanSelectGrantTargetCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && (permanent.TopCard.EqualsTraits("Insectoid") || permanent.TopCard.EqualsTraits("Titan"));

            IEnumerator SharedActivateCoroutineB(Hashtable hashtable, ActivateClass activateClass)
            {
                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectGrantTargetCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 [Insectoid]/[Titan] Digimon to gain Piercing and +3000 DP.", "The opponent is selecting 1 [Insectoid]/[Titan] Digimon to gain Piercing and +3000 DP.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainPierce(permanent, EffectDuration.UntilOpponentTurnEnd, activateClass));
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: 3000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                }
            }

            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectNameB(),
                SharedActivateCoroutineB,
                SharedEffectDescriptionB,
                optional: false,
                maxCountPerTurn: 1,
                hashValue: "BT26_042_OP_WA",
                onPlay: true,
                whenAttacking: true);

            #region Inherit - Deletes in Battle
            if (timing == EffectTiming.OnEndBattle)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash opponent's top security card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT26_042_Inherit");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When this Digimon deletes your opponent's Digimon in battle, trash their top security card.";

                bool WinnerCondition(Permanent permanent) => permanent.cardSources.Contains(card);
                bool LoserCondition(Permanent permanent) => CardEffectCommons.IsOpponentPermanent(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDeleteOpponentDigimonByBattle(hashtable: hashtable, winnerCondition: WinnerCondition, loserCondition: LoserCondition, isOnlyWinnerSurvive: true);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                         player: card.Owner.Enemy,
                         destroySecurityCount: 1,
                         cardEffect: activateClass,
                         fromTop: true).DestroySecurity());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
