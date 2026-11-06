using System.Collections;
using System.Collections.Generic;

// Stegomon
namespace DCGO.CardEffects.BT27
{
    public class BT27_048 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - Tamer w/[Kurata's Army] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.IsTamer
                        && targetPermanent.TopCard.EqualsTraits("Kurata's Army");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Armor Purge
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.ArmorPurgeEffect(card: card));
            }
            #endregion

            #region Shared On Play / When Digivolving / When Attacking - OPT

            string SharedEffectName = "May suspend 1 Digimon, then by deleting 1 of your Digimon, 1 opponent's Digimon/Tamer can't unsuspend";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] You may suspend 1 Digimon. Then, by choosing 1 of your Digimon, delete the chosen Digimon, and 1 of your opponent's Digimon or Tamers can't unsuspend until their turn ends.";

            bool CanSelectSuspendCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);

            bool CanSelectOwnDigimonCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

            bool CanSelectOpponentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool isUsed = false;

                #region May suspend 1 Digimon
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
                        selectPermanentCoroutine: SelectSuspendCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    selectSuspendEffect.SetUpCustomMessage("Select 1 Digimon to suspend.", "The opponent is selecting 1 Digimon to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectSuspendEffect.Activate());

                    IEnumerator SelectSuspendCoroutine(Permanent permanent)
                    {
                        if (permanent != null) isUsed = true;
                        yield return null;
                    }
                }
                #endregion

                #region By choosing 1 of your Digimon, delete it and 1 opponent's Digimon/Tamer can't unsuspend
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnDigimonCondition))
                {
                    Permanent selectedOwnDigimon = null;

                    SelectPermanentEffect selectOwnEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectOwnEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOwnDigimonCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectOwnCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectOwnEffect.SetUpCustomMessage("Select 1 of your Digimon to delete.", "The opponent is selecting 1 of their Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectOwnEffect.Activate());

                    IEnumerator SelectOwnCoroutine(Permanent permanent)
                    {
                        selectedOwnDigimon = permanent;
                        yield return null;
                    }

                    //Choosing is the cost, so the opponent's target is picked up front and not tied to the deletion succeeding
                    if (selectedOwnDigimon != null)
                    {
                        isUsed = true;

                        Permanent selectedOpponentPermanent = null;

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentCondition))
                        {
                            SelectPermanentEffect selectOpponentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectOpponentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectOpponentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectOpponentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectOpponentEffect.SetUpCustomMessage("Select 1 Digimon or Tamer that can't unsuspend.", "The opponent is selecting 1 Digimon or Tamer that can't unsuspend.");

                            yield return ContinuousController.instance.StartCoroutine(selectOpponentEffect.Activate());

                            IEnumerator SelectOpponentCoroutine(Permanent permanent)
                            {
                                selectedOpponentPermanent = permanent;
                                yield return null;
                            }
                        }

                        yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                            new List<Permanent>() { selectedOwnDigimon },
                            CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());

                        if (selectedOpponentPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCantUnsuspendUntilOpponentTurnEnd(selectedOpponentPermanent, activateClass));
                        }
                    }
                }
                #endregion

                if (!isUsed) activateClass.RemoveUse();
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                isSkippable: true,
                maxCountPerTurn: 1,
                hashValue: "BT27_048_OP_WD_WA",
                onPlay: true,
                whenDigivolving: true,
                whenAttacking: true);

            #endregion

            return cardEffects;
        }
    }
}
