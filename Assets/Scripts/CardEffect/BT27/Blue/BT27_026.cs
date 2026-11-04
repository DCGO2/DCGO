using System.Collections;
using System.Collections.Generic;

// Thunderbirdmon
namespace DCGO.CardEffects.BT27
{
    public class BT27_026 : CEntity_Effect
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

            string SharedEffectName = "May trash 2 opponent's digivolution cards, then by deleting 1 of your Digimon, 1 opponent's Digimon can't suspend";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] You may trash any 2 digivolution cards from your opponent's Digimon. Then, by choosing 1 of your Digimon, delete the chosen Digimon and 1 of their Digimon can't suspend until their turn ends.";

            bool CanSelectOpponentDigimonCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            bool CanSelectOwnDigimonCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool isUsed = false;

                #region May trash any 2 digivolution cards from opponent's Digimon
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentDigimonCondition))
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                        permanentCondition: CanSelectOpponentDigimonCondition,
                        cardCondition: _ => true,
                        maxCount: 2,
                        canNoTrash: true,
                        isFromOnly1Permanent: false,
                        activateClass: activateClass,
                        afterSelectionCoroutine: AfterSelectionCoroutine));

                    IEnumerator AfterSelectionCoroutine(Permanent permanent, List<CardSource> cardSources)
                    {
                        if (cardSources.Count > 0) isUsed = true;
                        yield return null;
                    }
                }
                #endregion

                #region By choosing 1 of your Digimon, delete it and 1 opponent's Digimon can't suspend
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

                    if (selectedOwnDigimon != null)
                    {
                        isUsed = true;

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent>() { selectedOwnDigimon },
                            activateClass: activateClass,
                            successProcess: SuccessProcess,
                            failureProcess: null));

                        IEnumerator SuccessProcess(List<Permanent> _permanents)
                        {
                            if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentDigimonCondition)) yield break;

                            SelectPermanentEffect selectOpponentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectOpponentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectOpponentDigimonCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectOpponentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectOpponentEffect.SetUpCustomMessage("Select 1 Digimon that can't suspend.", "The opponent is selecting 1 Digimon that can't suspend.");

                            yield return ContinuousController.instance.StartCoroutine(selectOpponentEffect.Activate());

                            IEnumerator SelectOpponentCoroutine(Permanent permanent)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotSuspend(
                                    targetPermanent: permanent,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                    activateClass: activateClass,
                                    condition: null,
                                    effectName: "Can't suspend"));
                            }
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
                hashValue: "BT27_026_OP_WD_WA",
                onPlay: true,
                whenDigivolving: true,
                whenAttacking: true);

            #endregion

            return cardEffects;
        }
    }
}
