using System;
using System.Collections;
using System.Collections.Generic;

// Kuwagamon
namespace DCGO.CardEffects.BT26
{
    public class BT26_038 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 3));
            }
            #endregion

            #region Shared When Moving / On Play / When Digivolving

            string SharedEffectName()
                => "May suspend 1 Digimon, then 1 of your [Insectoid]/[Titan] Digimon gets +3000 DP";

            string SharedEffectDescription(string tag)
                => $"[{tag}] You may suspend 1 Digimon. Then, 1 of your Digimon with the [Insectoid] or [Titan] trait gets +3000 DP until your opponent's turn ends.";

            bool CanSelectSuspendCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent)
                    && !permanent.IsSuspended && permanent.CanSuspend;

            bool CanSelectBuffTargetCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && (permanent.TopCard.ContainsTraits("Insectoid") || permanent.TopCard.ContainsTraits("Titan"));

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendCondition))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectSuspendCondition));

                    SelectPermanentEffect selectSuspendEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectSuspendEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectSuspendCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    selectSuspendEffect.SetUpCustomMessage("Select 1 Digimon to suspend.", "The opponent is selecting 1 Digimon to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectSuspendEffect.Activate());
                }

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectBuffTargetCondition))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectBuffTargetCondition));

                    SelectPermanentEffect selectBuffEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectBuffEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectBuffTargetCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectBuffEffect.SetUpCustomMessage("Select 1 [Insectoid]/[Titan] Digimon to get +3000 DP.", "The opponent is selecting 1 [Insectoid]/[Titan] Digimon to get +3000 DP.");

                    yield return ContinuousController.instance.StartCoroutine(selectBuffEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: 3000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                    }
                }
            }

            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                whenMoving: true,
                onPlay: true,
                whenDigivolving: true);

            #region Inherit - Win Battle
            if (timing == EffectTiming.OnEndBattle)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve 1 [Insectoid]/[Titan] Digimon into hand card for 1 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT26_038_Inherit");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Your Turn] [Once Per Turn] When this Digimon wins a battle, 1 of your [Insectoid] or [Titan] trait Digimon may digivolve into an [Insectoid] or [Titan] trait Digimon card in the hand with the cost reduced by 1.";

                bool WinnerCondition(Permanent permanent) => permanent.cardSources.Contains(card);

                bool CanSelectTargetPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.ContainsTraits("Insectoid") || permanent.TopCard.ContainsTraits("Titan"));

                bool CardCondition(CardSource cardSource)
                    => cardSource.IsDigimon && (cardSource.ContainsTraits("Insectoid") || cardSource.ContainsTraits("Titan"));

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerWhenWinBattle(hashtable, WinnerCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectTargetPermanentCondition);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool isUsed = false;

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTargetPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectTargetPermanentCondition));

                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectTargetPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 [Insectoid]/[Titan] Digimon to digivolve.", "The opponent is selecting 1 [Insectoid]/[Titan] Digimon to digivolve.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedPermanent != null)
                        {
                            isUsed = true;

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: selectedPermanent,
                                cardCondition: CardCondition,
                                payCost: true,
                                reduceCostTuple: (1, CardCondition),
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: true,
                                activateClass: activateClass,
                                successProcess: null,
                                isOptional: true));
                        }
                    }

                    if (!isUsed) activateClass.RemoveUse();
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
